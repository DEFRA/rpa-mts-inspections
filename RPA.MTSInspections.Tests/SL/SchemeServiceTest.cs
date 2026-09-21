using Moq;
using NUnit.Framework;
using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.SL;
using RPA.MTSInspections.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPA.MTSInspections.Tests.SL
{
    [TestFixture]
    [Category("Scheme Service")]

    class SchemeServiceTest
    {
        Mock<MTSInspectionsContext> mockContext;
        SchemeService service;
        Mock<ISchemeService> Sservice;
        Mock<IAuditService> Aservice;
        Mock<DbSet<Plant>> mockPlantSet;
        Mock<DbSet<Scheme>> mockSchemeSet;
        Mock<DbSet<Criteria>> mockCriteriaSet;
        Mock<DbSet<Option>> mockOptionSet;
        Mock<DbSet<PlantOption>> mockPlantOptionSet;
        Mock<DbSet<RiskScore>> mockRiskScoreSet;
        Mock<DbSet<LastInspection>> mockLastInspectionSet;


        [SetUp]

        public void Setup()
        {

            //Plants

            var plant1 = new Plant { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), PlantName = "Beefy Mc Beef Plant", AddressOne = "Beef Street", AddressTwo = "Beef Estate", TownCity = "Beef Town", PostCode = "BEE FY1", County = "BeefHampton", Active = true, LicenceNo = "4444", BCC = true };
            var plant2 = new Plant { PlantId = Guid.Parse("A462AEEE-537A-4E5D-802D-9B2D6F6F979D"), PlantName = "Beefy Mc Beef Plant2", AddressOne = "Beef Street", AddressTwo = "Beef Estate", TownCity = "Beef Town", PostCode = "BEE FY1", County = "BeefHampton", Active = true, LicenceNo = "4445", BLS = true };
            var plant3 = new Plant { PlantId = Guid.Parse("E8B24D01-0B55-45A4-9048-EB556EBA8126"), PlantName = "Beefy Mc Beef Plant3", AddressOne = "Beef Street", AddressTwo = "Beef Estate", TownCity = "Beef Town", PostCode = "BEE FY1", County = "BeefHampton", Active = true, LicenceNo = "4446", PCG = true };

            var plantData = new List<Plant>
            {
               plant1,plant2,plant3

            }.AsQueryable();


            //Schemes

            var schemeBLS = new Scheme { SchemeId = Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Name = "BLS", Weighting = 0, Active = true };
            var schemeBCC = new Scheme { SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Name = "BCC", Weighting = 0, Active = true };
            var schemePCG = new Scheme { SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Name = "PCG", Weighting = 0, Active = true };

            var schemeData = new List<Scheme>
            {
                schemeBLS, schemeBCC, schemePCG

            }.AsQueryable();


            //Risk Scores

            var riskScore1 = new RiskScore { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Scheme = schemeBCC, DateCalculated = DateTime.UtcNow, Score = 500 };
            var riskScore2 = new RiskScore { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant2, SchemeId = Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Scheme = schemeBLS, DateCalculated = DateTime.UtcNow, Score = 300 };
            var riskScore3 = new RiskScore { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant3, SchemeId = Guid.Parse("E8B24D01-0B55-45A4-9048-EB556EBA8126"), Scheme = schemePCG, DateCalculated = DateTime.UtcNow, Score = 800 };

            var riskScoreData = new List<RiskScore>
            {
                riskScore1, riskScore2, riskScore3

            }.AsQueryable();

            //Update Plant with Risk Score

            plant1.RiskScore = riskScoreData.ToList();
            plant2.RiskScore = riskScoreData.ToList();
            plant3.RiskScore = riskScoreData.ToList();


            //BLS Criteria
            var criteria1 = new Criteria { CriteriaId = Guid.Parse("C65CC653-C9B6-494F-9847-F7DEA02DDD43"), Name = "Number of Non-Compliance", SchemeId = Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Weighting = 10, Scheme = schemeBLS };
            var criteria2 = new Criteria { CriteriaId = Guid.Parse("D0A819B8-0E43-42B5-B6E6-6244A8D2C9F4"), Name = "Operating Hours", SchemeId = Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Weighting = 5, Scheme = schemeBLS };
            var criteria3 = new Criteria { CriteriaId = Guid.Parse("47D33F8E-DD3F-4AE5-9603-B6D4718F15B7"), Name = "Premises Type", SchemeId = Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Weighting = 5, Scheme = schemeBLS };
            var criteria4 = new Criteria { CriteriaId = Guid.Parse("F2129472-B824-4D2D-90F1-CC9ACF6B7604"), Name = "Type of Operation", SchemeId = Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Weighting = 1, Scheme = schemeBLS };
            var criteria5 = new Criteria { CriteriaId = Guid.Parse("AAF3C4F3-1018-41A6-A2B4-ED3C64C92388"), Name = "Origin of Products Handled", SchemeId = Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Weighting = 10, Scheme = schemeBLS };
            var criteria6 = new Criteria { CriteriaId = Guid.Parse("21985672-F552-4276-ABEC-A46622D11739"), Name = "Nature of Operation", SchemeId = Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Weighting = 5, Scheme = schemeBLS };
            var criteria7 = new Criteria { CriteriaId = Guid.Parse("3A82EF12-A7B4-42D2-AD4B-32848DC85A4D"), Name = "Scheme Year", SchemeId = Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Weighting = 0, Scheme = schemeBLS };
            var criteria8 = new Criteria { CriteriaId = Guid.Parse("01AF1AA3-7E0D-4E01-8C1D-007CDDB76643"), Name = "Severity of Non-Compliance", SchemeId = Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Weighting = 10, Scheme = schemeBLS };
            var criteria9 = new Criteria { CriteriaId = Guid.Parse("DCCB4EBE-287A-46E7-BE50-74D323A809A5"), Name = "Throughput", SchemeId = Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Weighting = 1, Scheme = schemeBLS };
            var criteria10 = new Criteria { CriteriaId = Guid.Parse("5AD5696F-D1AB-46AF-9049-66A948F2E3D2"), Name = "Number of Inspections Resulting in Failure", SchemeId = Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Weighting = 10, Scheme = schemeBLS };
            var criteria11 = new Criteria { CriteriaId = Guid.Parse("2B6CE3A6-687D-406E-BBAB-31701F28F33B"), Name = "Number of Months Since Last Inspection", SchemeId = Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Weighting = 0, Scheme = schemeBLS };

            //BCC Criteria

            var criteria12 = new Criteria { CriteriaId = Guid.Parse("9B225621-875E-4324-B341-1D6FC8CB5402"), Name = "Number of Near Misses in Previous 3 Years", SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Weighting = 20, Scheme = schemeBCC };
            var criteria13 = new Criteria { CriteriaId = Guid.Parse("5D370875-2B57-4B70-B44F-2F9D7524E24F"), Name = "Source of Classifiers", SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Weighting = 0, Scheme = schemeBCC };
            var criteria14 = new Criteria { CriteriaId = Guid.Parse("3E7A0FF3-1D42-4522-9421-327990F04FD1"), Name = "Scheme Year", SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Weighting = 0, Scheme = schemeBCC };
            var criteria15 = new Criteria { CriteriaId = Guid.Parse("FC0BBAC1-9F98-47C1-A4D9-44405FF045A1"), Name = "Carcass Hanging Method", SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Weighting = 10, Scheme = schemeBCC };
            var criteria16 = new Criteria { CriteriaId = Guid.Parse("8A84C06D-F965-4EE1-AE82-544D656D3A94"), Name = "Failed/Unsatisfactory on Last Visit", SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Weighting = 20, Scheme = schemeBCC };
            var criteria17 = new Criteria { CriteriaId = Guid.Parse("93161331-9649-4730-8981-679F4320791E"), Name = "Hide Puller", SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Weighting = 5, Scheme = schemeBCC };
            var criteria18 = new Criteria { CriteriaId = Guid.Parse("FF41ADF1-77BC-4A23-91BD-6AD650A6C6B0"), Name = "Pre Inspection Sign In", SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Weighting = 6, Scheme = schemeBCC };
            var criteria19 = new Criteria { CriteriaId = Guid.Parse("FF41ADF1-77BC-4A23-91BD-6AD650A6C6B0"), Name = "Operating Hours", SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Weighting = 0, Scheme = schemeBCC };
            var criteria20 = new Criteria { CriteriaId = Guid.Parse("FC46A70A-12D0-4C3F-87FD-976C4FF9616A"), Name = "Line Clearing", SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Weighting = 0, Scheme = schemeBCC };
            var criteria21 = new Criteria { CriteriaId = Guid.Parse("1F9BBA1A-77DE-4C6C-A53E-9BD738F6493A"), Name = "Throughput", SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Weighting = 20, Scheme = schemeBCC };
            var criteria22 = new Criteria { CriteriaId = Guid.Parse("6DE3685D-7BE8-4D32-8C2E-A8628CF5A3FF"), Name = "Number of Weeks Since Last Inspection", SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Weighting = 0, Scheme = schemeBCC };
            var criteria23 = new Criteria { CriteriaId = Guid.Parse("8B6F76BE-A6C5-4384-8489-AA6791A24A76"), Name = "Carcass Dressing", SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Weighting = 15, Scheme = schemeBCC };
            var criteria24 = new Criteria { CriteriaId = Guid.Parse("53453A5B-26DE-4970-BD36-ABB0687EBD4E"), Name = "Record Keeping", SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Weighting = 7, Scheme = schemeBCC };
            var criteria25 = new Criteria { CriteriaId = Guid.Parse("5706C8DE-54F2-465A-A5DD-DE0671E270F2"), Name = "Trimming", SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Weighting = 15, Scheme = schemeBCC };
            var criteria26 = new Criteria { CriteriaId = Guid.Parse("1F325828-8CB3-462E-AE8A-E34F22AF7758"), Name = "Number of Failed/Unsatisfactory in Previous 3 Years", SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Weighting = 20, Scheme = schemeBCC };

            //PCG Criteria

            var criteria27 = new Criteria { CriteriaId = Guid.Parse("70C39842-B3E2-4C99-8D04-09050102C723"), Name = "Record Keeping", SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Weighting = 7, Scheme = schemePCG };
            var criteria28 = new Criteria { CriteriaId = Guid.Parse("D1BB78C0-E8D9-47D0-978B-46D80BC5189B"), Name = "Carcass Dressing", SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Weighting = 15, Scheme = schemePCG };
            var criteria29 = new Criteria { CriteriaId = Guid.Parse("320A4287-A1FE-43A6-883E-668F07D4FCD8"), Name = "Scheme Year", SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Weighting = 0, Scheme = schemePCG };
            var criteria30 = new Criteria { CriteriaId = Guid.Parse("C80E9480-1DBB-4D8F-8645-6EE45DD1AE54"), Name = "Trimming", SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Weighting = 15, Scheme = schemePCG };
            var criteria31 = new Criteria { CriteriaId = Guid.Parse("8EA0504C-6532-4C67-B3A6-7A083436191E"), Name = "Throughput", SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Weighting = 20, Scheme = schemePCG };
            var criteria32 = new Criteria { CriteriaId = Guid.Parse("FD7B2B44-DFC4-456E-8717-80FD3B636ADD"), Name = "Line Clearing", SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Weighting = 0, Scheme = schemePCG };
            var criteria33 = new Criteria { CriteriaId = Guid.Parse("41FE59BD-BAD7-4344-A6AD-94BF16C51350"), Name = "Number of Near Misses in Previous 3 years", SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Weighting = 20, Scheme = schemePCG };
            var criteria34 = new Criteria { CriteriaId = Guid.Parse("07F52BA6-45AE-4B2B-9674-94D2AC5B3400"), Name = "Operating Hours", SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Weighting = 0, Scheme = schemePCG };
            var criteria35 = new Criteria { CriteriaId = Guid.Parse("ED6C296B-CD2F-4731-9146-986D2E054BC0"), Name = "Number of Failed/Unsatisfactory in Previous 3 years", SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Weighting = 20, Scheme = schemePCG };
            var criteria36 = new Criteria { CriteriaId = Guid.Parse("B5FCE165-3126-4B2E-89EC-A48B130705E4"), Name = "Failed/Unsatisfactory on Last Visit", SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Weighting = 20, Scheme = schemePCG };
            var criteria37 = new Criteria { CriteriaId = Guid.Parse("52859B5B-9ABC-400D-9378-A72940D2F04A"), Name = "Probe", SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Weighting = 0, Scheme = schemePCG };
            var criteria38 = new Criteria { CriteriaId = Guid.Parse("16250F46-9901-4E6B-8BAD-C635BDD81E68"), Name = "Pre Inspection Sign In", SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Weighting = 6, Scheme = schemePCG };
            var criteria39 = new Criteria { CriteriaId = Guid.Parse("93B907B1-91FA-405D-A2DE-C68A02F2EE88"), Name = "Number of Weeks Since Last Inspection", SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Weighting = 0, Scheme = schemePCG };
            var criteria40 = new Criteria { CriteriaId = Guid.Parse("7ADA4564-CB20-4776-9CE8-E69526D9DEB1"), Name = "Source of Classifiers", SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Weighting = 0, Scheme = schemePCG };

            var criteriaData = new List<Criteria>
            {
                criteria1, criteria2, criteria3, criteria4, criteria5, criteria6, criteria7, criteria8, criteria9, criteria10, criteria11, criteria12, criteria13, criteria14, criteria15, criteria16, criteria17, criteria18, criteria19, criteria20, criteria21, criteria22, criteria23, criteria24, criteria25, criteria26, criteria27, criteria28, criteria29, criteria30, criteria31, criteria32, criteria33, criteria34, criteria35, criteria36, criteria37, criteria38, criteria39, criteria40

            }.AsQueryable();

            //BLS Options

            var option1 = new Option { OptionId = Guid.Parse("3C79CCB1-FE98-48C6-8F9E-072B7B97DE6D"), Name = "2>3", CriteriaId = Guid.Parse("C65CC653-C9B6-494F-9847-F7DEA02DDD43"), Active = true, Score = 4, Criteria = criteria1 };
            var option2 = new Option { OptionId = Guid.Parse("46C8CFCC-E188-4CF7-9CA6-78B4A283B4AD"), Name = "05:00 - 17:00hrs", CriteriaId = Guid.Parse("D0A819B8-0E43-42B5-B6E6-6244A8D2C9F4"), Active = true, Score = 3, Criteria = criteria2 };
            var option3 = new Option { OptionId = Guid.Parse("5365431C-DB42-49FF-AD54-0EA864D66CEF"), Name = "FSA approved Dormant", CriteriaId = Guid.Parse("47D33F8E-DD3F-4AE5-9603-B6D4718F15B7"), Active = true, Score = 1, Criteria = criteria3 };
            var option4 = new Option { OptionId = Guid.Parse("0C91E26A-2E5F-4CE9-9F93-086D9A79BE20"), Name = "Slaughter, cutting and mincing - O12M and U12M slaughtered on site only", CriteriaId = Guid.Parse("F2129472-B824-4D2D-90F1-CC9ACF6B7604"), Active = true, Score = 10, Criteria = criteria4 };
            var option5 = new Option { OptionId = Guid.Parse("64CF8806-81C2-4427-822D-4A501C8D80F4"), Name = "Non UK Only", CriteriaId = Guid.Parse("AAF3C4F3-1018-41A6-A2B4-ED3C64C92388"), Active = true, Score = 10, Criteria = criteria5 };
            var option6 = new Option { OptionId = Guid.Parse("21985672-F552-4276-ABEC-A46622D11739"), Name = "no splitting/relabelling (no cutting)", CriteriaId = Guid.Parse("21985672-F552-4276-ABEC-A46622D11739"), Active = true, Score = 3, Criteria = criteria6 };
            var option7 = new Option { OptionId = Guid.Parse("357DB66F-F767-4B30-9CC5-7BE9D7ACAF99"), Name = "2018", CriteriaId = Guid.Parse("3A82EF12-A7B4-42D2-AD4B-32848DC85A4D"), Active = true, Score = 0, Criteria = criteria7 };
            var option8 = new Option { OptionId = Guid.Parse("7510C5EB-EC6B-46F3-9B11-17272C98612F"), Name = "4", CriteriaId = Guid.Parse("01AF1AA3-7E0D-4E01-8C1D-007CDDB76643"), Active = true, Score = 8, Criteria = criteria8 };
            var option9 = new Option { OptionId = Guid.Parse("8E777ED0-2833-4E10-BAC1-0799F05DF26F"), Name = "High || >400 Ton || >1200 Headage", CriteriaId = Guid.Parse("DCCB4EBE-287A-46E7-BE50-74D323A809A5"), Active = true, Score = 9, Criteria = criteria9 };
            var option10 = new Option { OptionId = Guid.Parse("BDCAC403-40FB-457B-A809-13CB812401DD"), Name = "2>3", CriteriaId = Guid.Parse("5AD5696F-D1AB-46AF-9049-66A948F2E3D2"), Active = true, Score = 4, Criteria = criteria10 };
            var option11 = new Option { OptionId = Guid.Parse("86554B19-8A7F-465B-910F-0461D383D9C2"), Name = "1-3 Months", CriteriaId = Guid.Parse("2B6CE3A6-687D-406E-BBAB-31701F28F33B"), Active = true, Score = 30, Criteria = criteria11 };

            //BCC Options

            var option12 = new Option { OptionId = Guid.Parse("3602988E-497D-4BB0-80AE-18791916C842"), Name = "4", CriteriaId = Guid.Parse("9B225621-875E-4324-B341-1D6FC8CB5402"), Active = true, Score = 8, Criteria = criteria12 };
            var option13 = new Option { OptionId = Guid.Parse("A6F21734-EB83-42A4-86E6-80EC62DC4EC4"), Name = "Internal", CriteriaId = Guid.Parse("5D370875-2B57-4B70-B44F-2F9D7524E24F"), Active = true, Score = 20, Criteria = criteria13 };
            var option14 = new Option { OptionId = Guid.Parse("C7530FA7-9918-4D29-99F6-C409CF850667"), Name = "2018", CriteriaId = Guid.Parse("3E7A0FF3-1D42-4522-9421-327990F04FD1"), Active = true, Score = 0, Criteria = criteria14 };
            var option15 = new Option { OptionId = Guid.Parse("6EAD4CEC-3C24-4B1C-B147-5E71DF42D807"), Name = "Conventional", CriteriaId = Guid.Parse("FC0BBAC1-9F98-47C1-A4D9-44405FF045A1"), Active = true, Score = 5, Criteria = criteria15 };
            var option16 = new Option { OptionId = Guid.Parse("68CE74D2-489E-45BA-B792-04EE67DFD8CD"), Name = "Yes", CriteriaId = Guid.Parse("8A84C06D-F965-4EE1-AE82-544D656D3A94"), Active = true, Score = 10, Criteria = criteria16 };
            var option17 = new Option { OptionId = Guid.Parse("9DBD5839-951D-424A-B51C-19058273CA01"), Name = "Downward", CriteriaId = Guid.Parse("93161331-9649-4730-8981-679F4320791E"), Active = true, Score = 20, Criteria = criteria17 };
            var option18 = new Option { OptionId = Guid.Parse("F4F2ABB8-A8A9-4EDD-B1F4-3A40668609BF"), Name = "Yes", CriteriaId = Guid.Parse("FF41ADF1-77BC-4A23-91BD-6AD650A6C6B0"), Active = true, Score = 20, Criteria = criteria18 };
            var option19 = new Option { OptionId = Guid.Parse("194A8463-3E13-49EF-AC4E-1789EA44B84E"), Name = "Out of hours", CriteriaId = Guid.Parse("FC5FB450-D147-4878-905D-7A9E7010348D"), Active = true, Score = 20, Criteria = criteria19 };
            var option20 = new Option { OptionId = Guid.Parse("686779F6-AD0C-4A98-BC41-1FB944892CB5"), Name = "Left hanging", CriteriaId = Guid.Parse("FC46A70A-12D0-4C3F-87FD-976C4FF9616A"), Active = true, Score = 20, Criteria = criteria20 };
            var option21 = new Option { OptionId = Guid.Parse("9166E6B2-5242-47F3-9C5B-09FFF2617EE2"), Name = "Up to 150 a week", CriteriaId = Guid.Parse("1F9BBA1A-77DE-4C6C-A53E-9BD738F6493A"), Active = true, Score = 0, Criteria = criteria21 };
            var option22 = new Option { OptionId = Guid.Parse("C188F55A-7901-48D2-85DE-2AF8A1A40155"), Name = "0-4 Weeks", CriteriaId = Guid.Parse("6DE3685D-7BE8-4D32-8C2E-A8628CF5A3FF"), Active = true, Score = 20, Criteria = criteria22 };
            var option23 = new Option { OptionId = Guid.Parse("D29CE433-E584-4425-B5EE-3FA5D66391D6"), Name = "All 3 Specs", CriteriaId = Guid.Parse("8B6F76BE-A6C5-4384-8489-AA6791A24A76"), Active = true, Score = 35, Criteria = criteria23 };
            var option24 = new Option { OptionId = Guid.Parse("32714101-4B82-417C-A354-303A82F9D041"), Name = "Unsatisfactory", CriteriaId = Guid.Parse("53453A5B-26DE-4970-BD36-ABB0687EBD4E"), Active = true, Score = 20, Criteria = criteria24 };
            var option25 = new Option { OptionId = Guid.Parse("86A3CF32-4866-4D87-A717-37FD6AEC0FFD"), Name = "Before scale point", CriteriaId = Guid.Parse("5706C8DE-54F2-465A-A5DD-DE0671E270F2"), Active = true, Score = 5, Criteria = criteria25 };
            var option26 = new Option { OptionId = Guid.Parse("9374DCFB-1B37-44C1-98DF-3C4423B0C767"), Name = "3", CriteriaId = Guid.Parse("1F325828-8CB3-462E-AE8A-E34F22AF7758"), Active = true, Score = 6, Criteria = criteria26 };

            //PCG Options

            var option27 = new Option { OptionId = Guid.Parse("35705372-538F-4832-BF61-6299C93C324F"), Name = "Satisfactory", CriteriaId = Guid.Parse("70C39842-B3E2-4C99-8D04-09050102C723"), Active = true, Score = 10, Criteria = criteria27 };
            var option28 = new Option { OptionId = Guid.Parse("083BC019-CAAA-4BDA-A11D-21E346BCDC40"), Name = "Standard", CriteriaId = Guid.Parse("D1BB78C0-E8D9-47D0-978B-46D80BC5189B"), Active = true, Score = 5, Criteria = criteria28 };
            var option29 = new Option { OptionId = Guid.Parse("8CE86C47-DB7D-4BEC-B517-C90DB89719C6"), Name = "2018", CriteriaId = Guid.Parse("320A4287-A1FE-43A6-883E-668F07D4FCD8"), Active = true, Score = 0, Criteria = criteria29 };
            var option30 = new Option { OptionId = Guid.Parse("3ABC3626-5C1C-457A-98FB-70183B9D1CA0"), Name = "No trimming", CriteriaId = Guid.Parse("C80E9480-1DBB-4D8F-8645-6EE45DD1AE54"), Active = true, Score = 0, Criteria = criteria30 };
            var option31 = new Option { OptionId = Guid.Parse("AECBFA21-8D8F-41DA-B820-46D0691E00EC"), Name = "Up to 2,500 a week", CriteriaId = Guid.Parse("8EA0504C-6532-4C67-B3A6-7A083436191E"), Active = true, Score = 10, Criteria = criteria31 };
            var option32 = new Option { OptionId = Guid.Parse("31C6E0F2-C6B7-4D78-82AA-4F86BC37BB11"), Name = "Left hanging", CriteriaId = Guid.Parse("FD7B2B44-DFC4-456E-8717-80FD3B636ADD"), Active = true, Score = 20, Criteria = criteria32 };
            var option33 = new Option { OptionId = Guid.Parse("95F06852-CCB4-4839-84D3-2E27AE26E75E"), Name = "4", CriteriaId = Guid.Parse("41FE59BD-BAD7-4344-A6AD-94BF16C51350"), Active = true, Score = 8, Criteria = criteria33 };
            var option34 = new Option { OptionId = Guid.Parse("2983DC60-050C-4657-8A75-9ADD62D10848"), Name = "Out of hours", CriteriaId = Guid.Parse("07F52BA6-45AE-4B2B-9674-94D2AC5B3400"), Active = true, Score = 20, Criteria = criteria34 };
            var option35 = new Option { OptionId = Guid.Parse("0FFB1AE1-032D-4615-94B8-01B85A057DE7"), Name = "over 10", CriteriaId = Guid.Parse("ED6C296B-CD2F-4731-9146-986D2E054BC0"), Active = true, Score = 30, Criteria = criteria35 };
            var option36 = new Option { OptionId = Guid.Parse("F9CF7731-97F6-41E4-9150-7C32A67BCB82"), Name = "Yes", CriteriaId = Guid.Parse("B5FCE165-3126-4B2E-89EC-A48B130705E4"), Active = true, Score = 10, Criteria = criteria36 };
            var option37 = new Option { OptionId = Guid.Parse("0A0B23C4-B6D7-40C8-9953-A1239C99A4BB"), Name = "Electrical", CriteriaId = Guid.Parse("52859B5B-9ABC-400D-9378-A72940D2F04A"), Active = true, Score = 10, Criteria = criteria37 };
            var option38 = new Option { OptionId = Guid.Parse("FE75BC04-4A71-4487-BD7E-32F6BFD43A89"), Name = "0", CriteriaId = Guid.Parse("16250F46-9901-4E6B-8BAD-C635BDD81E68"), Active = true, Score = 0, Criteria = criteria38 };
            var option39 = new Option { OptionId = Guid.Parse("5D7B8BA2-9DC9-4344-81E2-3EB090940C0E"), Name = "10-12 weeks", CriteriaId = Guid.Parse("93B907B1-91FA-405D-A2DE-C68A02F2EE88"), Active = true, Score = 50, Criteria = criteria39 };
            var option40 = new Option { OptionId = Guid.Parse("941C5E58-47F2-47AC-B463-81BECA858A3E"), Name = "External", CriteriaId = Guid.Parse("7ADA4564-CB20-4776-9CE8-E69526D9DEB1"), Active = true, Score = 0, Criteria = criteria40 };

            var optionData = new List<Option>
            {
                option1, option2, option3, option4, option5, option6, option7, option8, option9, option10, option11, option12, option13, option14, option15, option16, option17, option18, option19, option20, option21, option22, option23, option24, option25, option26, option27, option28, option29, option30, option31, option32, option33, option34, option35, option36, option37, option38, option39, option40

            }.AsQueryable();

            //BLS Plant Options

            var plantOption1 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Scheme = schemeBLS, OptionId = Guid.Parse("3C79CCB1-FE98-48C6-8F9E-072B7B97DE6D"), Option = option1 };
            var plantOption2 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Scheme = schemeBLS, OptionId = Guid.Parse("46C8CFCC-E188-4CF7-9CA6-78B4A283B4AD"), Option = option2 };
            var plantOption3 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Scheme = schemeBLS, OptionId = Guid.Parse("5365431C-DB42-49FF-AD54-0EA864D66CEF"), Option = option3 };
            var plantOption4 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Scheme = schemeBLS, OptionId = Guid.Parse("0C91E26A-2E5F-4CE9-9F93-086D9A79BE20"), Option = option4 };
            var plantOption5 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Scheme = schemeBLS, OptionId = Guid.Parse("64CF8806-81C2-4427-822D-4A501C8D80F4"), Option = option5 };
            var plantOption6 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Scheme = schemeBLS, OptionId = Guid.Parse("21985672-F552-4276-ABEC-A46622D11739"), Option = option6 };
            var plantOption7 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Scheme = schemeBLS, OptionId = Guid.Parse("357DB66F-F767-4B30-9CC5-7BE9D7ACAF99"), Option = option7 };
            var plantOption8 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Scheme = schemeBLS, OptionId = Guid.Parse("7510C5EB-EC6B-46F3-9B11-17272C98612F"), Option = option8 };
            var plantOption9 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Scheme = schemeBLS, OptionId = Guid.Parse("8E777ED0-2833-4E10-BAC1-0799F05DF26F"), Option = option9 };
            var plantOption10 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Scheme = schemeBLS, OptionId = Guid.Parse("BDCAC403-40FB-457B-A809-13CB812401DD"), Option = option10 };
            var plantOption11 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Scheme = schemeBLS, OptionId = Guid.Parse("2B6CE3A6-687D-406E-BBAB-31701F28F33B"), Option = option11 };

            //BCC Plant Options

            var plantOption12 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Scheme = schemeBCC, OptionId = Guid.Parse("3602988E-497D-4BB0-80AE-18791916C842"), Option = option12 };
            var plantOption13 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Scheme = schemeBCC, OptionId = Guid.Parse("A6F21734-EB83-42A4-86E6-80EC62DC4EC4"), Option = option13 };
            var plantOption14 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Scheme = schemeBCC, OptionId = Guid.Parse("C7530FA7-9918-4D29-99F6-C409CF850667"), Option = option14 };
            var plantOption15 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Scheme = schemeBCC, OptionId = Guid.Parse("6EAD4CEC-3C24-4B1C-B147-5E71DF42D807"), Option = option15 };
            var plantOption16 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Scheme = schemeBCC, OptionId = Guid.Parse("68CE74D2-489E-45BA-B792-04EE67DFD8CD"), Option = option16 };
            var plantOption17 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Scheme = schemeBCC, OptionId = Guid.Parse("9DBD5839-951D-424A-B51C-19058273CA01"), Option = option17 };
            var plantOption18 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Scheme = schemeBCC, OptionId = Guid.Parse("F4F2ABB8-A8A9-4EDD-B1F4-3A40668609BF"), Option = option18 };
            var plantOption19 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Scheme = schemeBCC, OptionId = Guid.Parse("194A8463-3E13-49EF-AC4E-1789EA44B84E"), Option = option19 };
            var plantOption20 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Scheme = schemeBCC, OptionId = Guid.Parse("686779F6-AD0C-4A98-BC41-1FB944892CB5"), Option = option20 };
            var plantOption21 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Scheme = schemeBCC, OptionId = Guid.Parse("9166E6B2-5242-47F3-9C5B-09FFF2617EE2"), Option = option21 };
            var plantOption22 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Scheme = schemeBCC, OptionId = Guid.Parse("C188F55A-7901-48D2-85DE-2AF8A1A40155"), Option = option22 };
            var plantOption23 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Scheme = schemeBCC, OptionId = Guid.Parse("D29CE433-E584-4425-B5EE-3FA5D66391D6"), Option = option23 };
            var plantOption24 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Scheme = schemeBCC, OptionId = Guid.Parse("32714101-4B82-417C-A354-303A82F9D041"), Option = option24 };
            var plantOption25 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Scheme = schemeBCC, OptionId = Guid.Parse("5706C8DE-54F2-465A-A5DD-DE0671E270F2"), Option = option25 };
            var plantOption26 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("5982F25B-4DDE-483C-B0D3-F2E46A5036B4"), Scheme = schemeBCC, OptionId = Guid.Parse("9374DCFB-1B37-44C1-98DF-3C4423B0C767"), Option = option26 };

            //PCG Plant Options

            var plantOption27 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Scheme = schemePCG, OptionId = option27.OptionId, Option = option27 };
            var plantOption28 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Scheme = schemePCG, OptionId = option28.OptionId, Option = option28 };
            var plantOption29 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Scheme = schemePCG, OptionId = option29.OptionId, Option = option29 };
            var plantOption30 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Scheme = schemePCG, OptionId = option30.OptionId, Option = option30 };
            var plantOption31 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Scheme = schemePCG, OptionId = option31.OptionId, Option = option31 };
            var plantOption32 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Scheme = schemePCG, OptionId = option32.OptionId, Option = option32 };
            var plantOption33 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Scheme = schemePCG, OptionId = option33.OptionId, Option = option33 };
            var plantOption34 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Scheme = schemePCG, OptionId = option34.OptionId, Option = option34 };
            var plantOption35 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Scheme = schemePCG, OptionId = option35.OptionId, Option = option35 };
            var plantOption36 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Scheme = schemePCG, OptionId = option36.OptionId, Option = option36 };
            var plantOption37 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Scheme = schemePCG, OptionId = option37.OptionId, Option = option37 };
            var plantOption38 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Scheme = schemePCG, OptionId = option38.OptionId, Option = option38 };
            var plantOption39 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Scheme = schemePCG, OptionId = option39.OptionId, Option = option39 };
            var plantOption40 = new PlantOption { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Plant = plant1, SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Scheme = schemePCG, OptionId = option40.OptionId, Option = option40 };


            var plantOptionData = new List<PlantOption>
            {
                plantOption1, plantOption2, plantOption3, plantOption4, plantOption5, plantOption6, plantOption7, plantOption8, plantOption9, plantOption10, plantOption11, plantOption12, plantOption13, plantOption14, plantOption15, plantOption16, plantOption17, plantOption18, plantOption19, plantOption20, plantOption21, plantOption22, plantOption23, plantOption24, plantOption25, plantOption26, plantOption27, plantOption28, plantOption29, plantOption30, plantOption31, plantOption32, plantOption33, plantOption34, plantOption35, plantOption36, plantOption37, plantOption38, plantOption39, plantOption40

            }.AsQueryable();

            //Last Inspection Data

            var lastInspDate1 = new LastInspection { PlantId = Guid.Parse("A462AEEE-537A-4E5D-802D-9B2D6F6F979D"), SchemeId = Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Date = new DateTime(2018, 05, 01) };
            var lastInspDate2 = new LastInspection { PlantId = Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), SchemeId = Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Date = new DateTime(2017, 05, 01) };
            var lastInspDate3 = new LastInspection { PlantId = Guid.Parse("E8B24D01-0B55-45A4-9048-EB556EBA8126"), SchemeId = Guid.Parse("F5C4F862-9798-4F5C-B64E-CB6FAD81B0EF"), Date = new DateTime(2016, 05, 01) };

            var lastInspectionDateData = new List<LastInspection>
            {

                lastInspDate1, lastInspDate2, lastInspDate3

            }.AsQueryable();


            //Update Plant with plantOptions

            plant1.PlantOptions = plantOptionData.ToList();
            plant2.PlantOptions = plantOptionData.ToList();
            plant3.PlantOptions = plantOptionData.ToList();

            //Update Plant with Last Inspections

            plant1.LastInspection = lastInspectionDateData.ToList();
            plant2.LastInspection = lastInspectionDateData.ToList();
            plant3.LastInspection = lastInspectionDateData.ToList();

            //Create mocks for teams repository and setup mocked behaviour to mimic EF using test data above

            mockPlantSet = new Mock<DbSet<Plant>>();
            mockPlantSet.As<IQueryable<Plant>>().Setup(x => x.Provider).Returns(plantData.Provider);
            mockPlantSet.As<IQueryable<Plant>>().Setup(x => x.Expression).Returns(plantData.Expression);
            mockPlantSet.As<IQueryable<Plant>>().Setup(x => x.ElementType).Returns(plantData.ElementType);
            mockPlantSet.As<IQueryable<Plant>>().Setup(x => x.GetEnumerator()).Returns(plantData.GetEnumerator());
            mockPlantSet.Setup(x => x.AsNoTracking()).Returns(mockPlantSet.Object);
            mockPlantSet.Setup(x => x.Include(It.IsAny<string>())).Returns(mockPlantSet.Object);

            mockSchemeSet = new Mock<DbSet<Scheme>>();
            mockSchemeSet.As<IQueryable<Scheme>>().Setup(x => x.Provider).Returns(schemeData.Provider);
            mockSchemeSet.As<IQueryable<Scheme>>().Setup(x => x.Expression).Returns(schemeData.Expression);
            mockSchemeSet.As<IQueryable<Scheme>>().Setup(x => x.ElementType).Returns(schemeData.ElementType);
            mockSchemeSet.As<IQueryable<Scheme>>().Setup(x => x.GetEnumerator()).Returns(schemeData.GetEnumerator());
            mockSchemeSet.Setup(x => x.AsNoTracking()).Returns(mockSchemeSet.Object);

            mockCriteriaSet = new Mock<DbSet<Criteria>>();
            mockCriteriaSet.As<IQueryable<Criteria>>().Setup(x => x.Provider).Returns(criteriaData.Provider);
            mockCriteriaSet.As<IQueryable<Criteria>>().Setup(x => x.Expression).Returns(criteriaData.Expression);
            mockCriteriaSet.As<IQueryable<Criteria>>().Setup(x => x.ElementType).Returns(criteriaData.ElementType);
            mockCriteriaSet.As<IQueryable<Criteria>>().Setup(x => x.GetEnumerator()).Returns(criteriaData.GetEnumerator());
            mockCriteriaSet.Setup(x => x.AsNoTracking()).Returns(mockCriteriaSet.Object);

            mockOptionSet = new Mock<DbSet<Option>>();
            mockOptionSet.As<IQueryable<Option>>().Setup(x => x.Provider).Returns(optionData.Provider);
            mockOptionSet.As<IQueryable<Option>>().Setup(x => x.Expression).Returns(optionData.Expression);
            mockOptionSet.As<IQueryable<Option>>().Setup(x => x.ElementType).Returns(optionData.ElementType);
            mockOptionSet.As<IQueryable<Option>>().Setup(x => x.GetEnumerator()).Returns(optionData.GetEnumerator());
            mockOptionSet.Setup(x => x.AsNoTracking()).Returns(mockOptionSet.Object);

            mockPlantOptionSet = new Mock<DbSet<PlantOption>>();
            mockPlantOptionSet.As<IQueryable<PlantOption>>().Setup(x => x.Provider).Returns(plantOptionData.Provider);
            mockPlantOptionSet.As<IQueryable<PlantOption>>().Setup(x => x.Expression).Returns(plantOptionData.Expression);
            mockPlantOptionSet.As<IQueryable<PlantOption>>().Setup(x => x.ElementType).Returns(plantOptionData.ElementType);
            mockPlantOptionSet.As<IQueryable<PlantOption>>().Setup(x => x.GetEnumerator()).Returns(plantOptionData.GetEnumerator());
            mockPlantOptionSet.Setup(x => x.AsNoTracking()).Returns(mockPlantOptionSet.Object);


            mockRiskScoreSet = new Mock<DbSet<RiskScore>>();
            mockRiskScoreSet.As<IQueryable<RiskScore>>().Setup(x => x.Provider).Returns(riskScoreData.Provider);
            mockRiskScoreSet.As<IQueryable<RiskScore>>().Setup(x => x.Expression).Returns(riskScoreData.Expression);
            mockRiskScoreSet.As<IQueryable<RiskScore>>().Setup(x => x.ElementType).Returns(riskScoreData.ElementType);
            mockRiskScoreSet.As<IQueryable<RiskScore>>().Setup(x => x.GetEnumerator()).Returns(riskScoreData.GetEnumerator());
            mockRiskScoreSet.Setup(x => x.AsNoTracking()).Returns(mockRiskScoreSet.Object);

            mockLastInspectionSet = new Mock<DbSet<LastInspection>>();
            mockLastInspectionSet.As<IQueryable<LastInspection>>().Setup(x => x.Provider).Returns(lastInspectionDateData.Provider);
            mockLastInspectionSet.As<IQueryable<LastInspection>>().Setup(x => x.Expression).Returns(lastInspectionDateData.Expression);
            mockLastInspectionSet.As<IQueryable<LastInspection>>().Setup(x => x.ElementType).Returns(lastInspectionDateData.ElementType);
            mockLastInspectionSet.As<IQueryable<LastInspection>>().Setup(x => x.GetEnumerator()).Returns(lastInspectionDateData.GetEnumerator());
            mockLastInspectionSet.Setup(x => x.AsNoTracking()).Returns(mockLastInspectionSet.Object);


            //Setup context and set teams repository to use above mocked object.
            mockContext = new Mock<MTSInspectionsContext>();
            mockContext.Setup(x => x.Plant).Returns(mockPlantSet.Object);
            mockContext.Setup(x => x.Scheme).Returns(mockSchemeSet.Object);
            mockContext.Setup(x => x.Option).Returns(mockOptionSet.Object);
            mockContext.Setup(x => x.PlantOption).Returns(mockPlantOptionSet.Object);
            mockContext.Setup(x => x.LastInspection).Returns(mockLastInspectionSet.Object);
            mockContext.Setup(x => x.Criteria).Returns(mockCriteriaSet.Object);

            //Setup Service Injecting Mocked Context
            Sservice = new Mock<ISchemeService>();
            Aservice = new Mock<IAuditService>();
            service = new SchemeService(mockContext.Object);

        }

        [Test]
        public void Test_GetPlantOptions()
        {
            //Arrange
            //Already Done in Setup

            //Act
            var result = service.GetPlantOptions(Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"));

            //Assert
            Assert.AreEqual(11, result.Count);
        }



        [Test]
        public void Test_GetCheckOption()
        {
            //Arrange
            //Already Done in Setup

            //Act

            var result = service.GetCheckOption(Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), "no splitting/relabelling (no cutting)");

            //Assert

            Assert.AreEqual(Guid.Parse("21985672-F552-4276-ABEC-A46622D11739"), result.Option.OptionId);
    
        }

        [Test]
        public void Test_GetCurrentPlantOption()
        {
            //Arrange
            //Already Done in Setup

            //Act

            var result = service.GetCurrentPlantOption(Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), "Number of Non-Compliance");

            //Assert

            Assert.AreEqual(Guid.Parse("3C79CCB1-FE98-48C6-8F9E-072B7B97DE6D"), result.OptionId);
    
        }

        [Test]
        public void Test_GetCurrentNoPOption()
        {
            //Arrange
            //Already Done in Setup

            //Act

            var result = service.GetCheckOption(Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), "no splitting/relabelling (no cutting)");

            //Assert
            Assert.AreNotEqual("splitting and/or relabelling (no cutting)", result.Option.Name);
            Assert.AreNotEqual("Catering", result.Option.Name);
            Assert.AreNotEqual("For retail sale", result.Option.Name);
            Assert.AreNotEqual("some cutting and relabelling", result.Option.Name);
            Assert.AreNotEqual("Wholesale", result.Option.Name);
            Assert.AreNotEqual("Cutting to primal stage only", result.Option.Name);
            Assert.AreNotEqual("Slaughter for 3rd party only", result.Option.Name);
            Assert.AreEqual("no splitting/relabelling (no cutting)", result.Option.Name);
        }

        [Test]
        public void Test_GetInspectionDate()
        {
            //Arrange
            //Already Done in Setup

            //Act

            var result = service.GetInspectionDate(Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"));

            //Assert

            Assert.AreEqual(new DateTime(2017, 05, 01), result.Date);
        }

        [Test]
        public void Test_GetInspectionDate_Creates_New_Date_if_NonExists()
        {
            //Arrange
            //Already Done in Setup

            //Act

            var result = service.GetInspectionDate(Guid.NewGuid(), Guid.NewGuid());

            //Assert

            Assert.IsNotEmpty(result.Date.ToShortDateString());
        }

        //[Test]
        public void Test_CreateNewPlantOption()
        {
            //Arrange
            //Already Done in Setup


            //Act

            var result = service.CreateNewPlantOption(Guid.Parse("C741904A-5C65-4D12-848D-284BF2D4AF79"), Guid.Parse("EE9597C9-9479-4486-8506-1374D7F62A16"), Guid.Parse("3C79CCB1-FE98-48C6-8F9E-072B7B97DE6D"));

            //Assert

            Assert.AreEqual("2>3", result.Option.Name);
        }
    }
}
