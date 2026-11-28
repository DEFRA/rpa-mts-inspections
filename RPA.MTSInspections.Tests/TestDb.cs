using System.Collections;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using RPA.MTSInspections.DAL;

namespace RPA.MTSInspections.Tests
{
    // Real EF Core context over an in-memory SQLite database. The connection stays open for the lifetime of the
    // instance because an in-memory SQLite database disappears when its connection closes.
    // Foreign keys are not enforced: the legacy test fixtures describe partial object graphs.
    public sealed class TestDb : IDisposable
    {
        private readonly SqliteConnection connection = new("DataSource=:memory:;Foreign Keys=False");

        public TestDb()
        {
            connection.Open();
            using var context = NewContext();
            context.Database.EnsureCreated();
        }

        public DbContextOptions<MTSInspectionsContext> Options =>
            new DbContextOptionsBuilder<MTSInspectionsContext>().UseSqlite(connection).Options;

        public MTSInspectionsContext NewContext() => new(Options);

        // Persists entity graphs. The fixtures repeat key values and hang the same child lists off several owners, so
        // the graph is flattened, foreign keys are copied from the navigations, duplicate keys are dropped (first wins)
        // and the navigations are restored on the objects afterwards.
        public TestDb Seed(params IEnumerable[] sets)
        {
            using var context = NewContext();

            var entities = new List<object>();
            var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
            var queue = new Queue<object>(sets.SelectMany(s => s.Cast<object>()));

            while (queue.Count > 0)
            {
                var entity = queue.Dequeue();
                var type = entity == null ? null : context.Model.FindEntityType(entity.GetType());

                if (type == null || !visited.Add(entity))
                {
                    continue;
                }

                entities.Add(entity);

                foreach (var navigation in type.GetNavigations())
                {
                    foreach (var target in Targets(navigation, entity))
                    {
                        queue.Enqueue(target);
                    }
                }
            }

            foreach (var entity in entities)
            {
                var type = context.Model.FindEntityType(entity.GetType());

                foreach (var navigation in type.GetNavigations())
                {
                    foreach (var target in Targets(navigation, entity))
                    {
                        CopyKey(navigation, entity, target);
                    }
                }
            }

            var saved = new HashSet<string>();
            var unique = new List<object>();

            foreach (var entity in entities)
            {
                var type = context.Model.FindEntityType(entity.GetType());
                var key = type.FindPrimaryKey().Properties.Select(p => p.PropertyInfo.GetValue(entity)).ToList();
                var generated = type.FindPrimaryKey().Properties.All(p => p.ValueGenerated == ValueGenerated.OnAdd);

                if (generated && key.All(IsDefault) || saved.Add(type.Name + ":" + string.Join("|", key)))
                {
                    unique.Add(entity);
                }
            }

            var removed = new List<(object Entity, INavigation Navigation, object Value)>();

            foreach (var entity in unique)
            {
                foreach (var navigation in context.Model.FindEntityType(entity.GetType()).GetNavigations())
                {
                    var value = navigation.PropertyInfo.GetValue(entity);

                    if (value != null)
                    {
                        removed.Add((entity, navigation, value));
                        navigation.PropertyInfo.SetValue(entity, null);
                    }
                }
            }

            try
            {
                context.AddRange(unique);
                context.SaveChanges();
            }
            finally
            {
                foreach (var (entity, navigation, value) in removed)
                {
                    navigation.PropertyInfo.SetValue(entity, value);
                }
            }

            return this;
        }

        private static IEnumerable<object> Targets(INavigation navigation, object entity)
        {
            var value = navigation.PropertyInfo.GetValue(entity);

            if (value is IEnumerable list)
            {
                return list.Cast<object>().ToList();
            }

            return value == null ? Enumerable.Empty<object>() : new[] { value };
        }

        // Copies the owner's key into the dependent's foreign key, as relationship fix-up would.
        private static void CopyKey(INavigation navigation, object owner, object target)
        {
            var foreignKey = navigation.ForeignKey;
            var (dependent, principal) = navigation.IsOnDependent ? (owner, target) : (target, owner);

            for (var i = 0; i < foreignKey.Properties.Count; i++)
            {
                var value = foreignKey.PrincipalKey.Properties[i].PropertyInfo.GetValue(principal);

                if (!IsDefault(value) && IsDefault(foreignKey.Properties[i].PropertyInfo.GetValue(dependent)))
                {
                    foreignKey.Properties[i].PropertyInfo.SetValue(dependent, value);
                }
            }
        }

        private static bool IsDefault(object value) =>
            value == null || value.GetType().IsValueType && value.Equals(Activator.CreateInstance(value.GetType()));

        public void Dispose() => connection.Dispose();
    }
}
