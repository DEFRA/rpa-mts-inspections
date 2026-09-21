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
    [Category("Plant Service")]
    public class PlantServiceTest
    {

        Mock<DbSet<Plant>> mockPlantSet;
        Mock<MTSInspectionsContext> mockContext;
        PlantService service;

        [SetUp]

        public void Setup()
        {
            var plantData = new List<Plant>
            {
                new Plant {PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), PlantName = "Beefy Mc Beef Plant", AddressOne = "Beef Street", AddressTwo = "Beef Estate", TownCity = "Beef Town", PostCode = "BEE FY1", County = "BeefHampton", Active = true, LicenceNo = "4444", BLS = true },
                new Plant {PlantId = Guid.Parse("A462AEEE-537A-4E5D-802D-9B2D6F6F979D"), PlantName = "Beefy Mc Beef Plant2", AddressOne = "Beef Street", AddressTwo = "Beef Estate", TownCity = "Beef Town", PostCode = "BEE FY1", County = "BeefHampton", Active = true, LicenceNo = "4445", BCC = true },
                new Plant {PlantId = Guid.Parse("E8B24D01-0B55-45A4-9048-EB556EBA8126"), PlantName = "Beefy Mc Beef Plant3", AddressOne = "Beef Street", AddressTwo = "Beef Estate", TownCity = "Beef Town", PostCode = "BEE FY1", County = "BeefHampton", Active = true, LicenceNo = "4446", PCG = true },
                new Plant {PlantId = Guid.Parse("7544B6CC-4FA0-4B8F-9A3C-19A168C94A44"), PlantName = "Beefy Mc Beef Plant", AddressOne = "Beef Street", AddressTwo = "Beef Estate", TownCity = "Beef Town", PostCode = "BEE FY1", County = "BeefHampton", Active = false, LicenceNo = "4447", BLS = true },
                new Plant {PlantId = Guid.Parse("8C8957C5-D5D2-4C41-BDE0-0ED0DD845EF5"), PlantName = "Beefy Mc Beef Plant2", AddressOne = "Beef Street", AddressTwo = "Beef Estate", TownCity = "Beef Town", PostCode = "BEE FY1", County = "BeefHampton", Active = false, LicenceNo = "4448", BCC = true },
                new Plant {PlantId = Guid.Parse("BACB051F-5C82-4DED-B8C1-141EDB84D8B4"), PlantName = "Beefy Mc Beef Plant3", AddressOne = "Beef Street", AddressTwo = "Beef Estate", TownCity = "Beef Town", PostCode = "BEE FY1", County = "BeefHampton", Active = false, LicenceNo = "4449", PCG = true, BLS = true },


            }.AsQueryable();


            //Create mocks for teams repository and setup mocked behaviour to mimic EF using test data above

            mockPlantSet = new Mock<DbSet<Plant>>();
            mockPlantSet.As<IQueryable<Plant>>().Setup(x => x.Provider).Returns(plantData.Provider);
            mockPlantSet.As<IQueryable<Plant>>().Setup(x => x.Expression).Returns(plantData.Expression);
            mockPlantSet.As<IQueryable<Plant>>().Setup(x => x.ElementType).Returns(plantData.ElementType);
            mockPlantSet.As<IQueryable<Plant>>().Setup(x => x.GetEnumerator()).Returns(plantData.GetEnumerator());


            //Setup context and set teams repository to use above mocked object.
            mockContext = new Mock<MTSInspectionsContext>();
            mockContext.Setup(x => x.Plant).Returns(mockPlantSet.Object);

            //Setup Service Injecting Mocked Context
            service = new PlantService(mockContext.Object);
        }

        [Test]

        public void Test_GetPlantById_GetsPlant()
        {
            //Arrange
            //Already Done in Setup

            //Act
            var result = service.GetPlantById(Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"));

            //Assert
            Assert.AreEqual("Beefy Mc Beef Plant", result.PlantName);
        }

        [Test]

        public void Test_GetPlant_GetsAllPlants()
        {
            //Arrange
            //Already Done in Setup

            //Act
            var result = service.GetPlant(null, 1, 1, false);

            //Assert

            Assert.AreEqual(6, result.TotalItemCount);
        }


        [Test]

        public void Test_GetPlant_Search_String_Works()
        {
            //Arrange
            //Already Done in Setup

            //Act
            var result = service.GetPlant("Beefy Mc Beef Plant3", 1,10,false);

            //Assert

            Assert.AreEqual("Beefy Mc Beef Plant3", result.Select(x => x.PlantName).First());

        }



    }
}
