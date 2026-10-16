using Moq;
using System.Data.Entity;

namespace RPA.MTSInspections.Tests
{
    // Replaces EntityFramework.Testing (not available for .NET 10): backs a mocked DbSet with an in-memory list.
    public static class MockDbSetExtensions
    {
        public static Mock<DbSet<T>> SetupData<T>(this Mock<DbSet<T>> mock, ICollection<T> data = null, Func<object[], T> find = null) where T : class
        {
            var list = data as IList<T> ?? (data ?? new List<T>()).ToList();
            var queryable = list.AsQueryable();

            mock.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mock.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mock.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mock.As<IEnumerable<T>>().Setup(m => m.GetEnumerator()).Returns(() => list.GetEnumerator());

            mock.Setup(m => m.Include(It.IsAny<string>())).Returns(mock.Object);
            mock.Setup(m => m.AsNoTracking()).Returns(mock.Object);

            mock.Setup(m => m.Add(It.IsAny<T>())).Returns<T>(item => { list.Add(item); return item; });
            mock.Setup(m => m.AddRange(It.IsAny<IEnumerable<T>>())).Returns<IEnumerable<T>>(items => { foreach (var item in items.ToList()) { list.Add(item); } return items; });
            mock.Setup(m => m.Remove(It.IsAny<T>())).Returns<T>(item => { list.Remove(item); return item; });
            mock.Setup(m => m.RemoveRange(It.IsAny<IEnumerable<T>>())).Returns<IEnumerable<T>>(items => { foreach (var item in items.ToList()) { list.Remove(item); } return items; });
            mock.Setup(m => m.Attach(It.IsAny<T>())).Returns<T>(item => item);

            if (find != null)
            {
                mock.Setup(m => m.Find(It.IsAny<object[]>())).Returns<object[]>(keys => find(keys));
            }

            return mock;
        }
    }
}

