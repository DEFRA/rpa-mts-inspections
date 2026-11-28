using Moq;
using NUnit.Framework;
using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.SL;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.MTSInspections.Tests.SL
{
    [TestFixture]
    [Category("Audit Service")]
   public class AuditServiceTest
    {
        TestDb db;
        MTSInspectionsContext context;
        AuditService service;

        [SetUp]

        public void Setup()
        {

            var auditData = new List<Audit>
            {

            }.AsQueryable();

            //Create mocks for teams repository and setup mocked behaviour to mimic EF using test data above


            //Setup context and set teams repository to use above mocked object.
            db = new TestDb().Seed(auditData);
            context = db.NewContext();


            //Setup Service Injecting Mocked Context
            service = new AuditService(context);
        
        }

        [TearDown]
        public void TearDown()
        {
            context?.Dispose();
            db?.Dispose();
        }

        [Test]

        public void Test_Log_Adds_To_Log()
        {
            //Arrange
            Guid plantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79");
            string user = "M123456";
            string action = "TestAction";
            string propertyName = "TestProperty";
            string value = "TestValue";
            string newValue = "newTestvalue";

            //Act

            service.Log(plantId, user, action, propertyName, value, newValue);

            //Assert

            Assert.AreEqual(1, context.ChangeTracker.Entries<Audit>().Count(x => x.State == EntityState.Added));
        }
    }
}
