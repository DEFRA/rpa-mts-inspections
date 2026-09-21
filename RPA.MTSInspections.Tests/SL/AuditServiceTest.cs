using Moq;
using NUnit.Framework;
using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.SL;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.MTSInspections.Tests.SL
{
    [TestFixture]
    [Category("Audit Service")]
   public class AuditServiceTest
    {
        Mock<DbSet<Audit>> mockAuditSet;
        Mock<MTSInspectionsContext> mockContext;
        AuditService service;

        [SetUp]

        public void Setup()
        {

            var auditData = new List<Audit>
            {

            }.AsQueryable();

            //Create mocks for teams repository and setup mocked behaviour to mimic EF using test data above

            mockAuditSet = new Mock<DbSet<Audit>>();
            mockAuditSet.As<IQueryable<Audit>>().Setup(x => x.Provider).Returns(auditData.Provider);
            mockAuditSet.As<IQueryable<Audit>>().Setup(x => x.Expression).Returns(auditData.Expression);
            mockAuditSet.As<IQueryable<Audit>>().Setup(x => x.ElementType).Returns(auditData.ElementType);
            mockAuditSet.As<IQueryable<Audit>>().Setup(x => x.GetEnumerator()).Returns(auditData.GetEnumerator());

            //Setup context and set teams repository to use above mocked object.
            mockContext = new Mock<MTSInspectionsContext>();
            mockContext.Setup(x => x.Audit).Returns(mockAuditSet.Object);


            //Setup Service Injecting Mocked Context
            service = new AuditService(mockContext.Object);
        
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

            mockAuditSet.Verify(x => x.Add(It.IsAny<Audit>()), Times.Once);
        }
    }
}
