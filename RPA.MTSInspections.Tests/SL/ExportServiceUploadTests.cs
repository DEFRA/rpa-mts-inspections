using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OfficeOpenXml;
using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Services;
using RPA.MTSInspections.SL;
using System.Data.Entity;

namespace RPA.MTSInspections.Tests.SL
{
    /// <summary>
    /// The bulk load reads the uploaded workbook from the request stream: it must not write to the application
    /// directory, must reject unreadable or oversized files with an error row, and must keep its three-sheet shape.
    /// </summary>
    [TestFixture]
    public class ExportServiceUploadTests
    {
        private static ExportService Service(UploadOptions options = null)
        {
            var db = new Mock<MTSInspectionsContext>();
            db.Setup(x => x.Plant).Returns(new Mock<DbSet<RPA.MTSInspections.Models.Plant>>().SetupData().Object);
            db.Setup(x => x.Scheme).Returns(new Mock<DbSet<RPA.MTSInspections.Models.Scheme>>().SetupData().Object);

            return new ExportService(db.Object, Mock.Of<ISchemeService>(), Mock.Of<IAuditService>(), new FakeUser(),
                Options.Create(options ?? new UploadOptions()));
        }

        private static IFormFile Workbook(int sheets, int dataRows, string[] names = null)
        {
            using var package = new ExcelPackage();

            for (var i = 0; i < sheets; i++)
            {
                var sheet = package.Workbook.Worksheets.Add(names?[i] ?? $"Sheet{i + 1}");
                sheet.Cells[1, 1].Value = "Licence";
                sheet.Cells[1, 2].Value = "Scheme Year";

                for (var row = 0; row < dataRows; row++)
                {
                    sheet.Cells[row + 2, 1].Value = $"L{row}";
                    sheet.Cells[row + 2, 2].Value = "2018";
                }
            }

            return Form(package.GetAsByteArray());
        }

        private static IFormFile Form(byte[] bytes, string fileName = "upload.xlsx")
        {
            var stream = new MemoryStream(bytes);

            return new FormFile(stream, 0, bytes.Length, "bulkSource", fileName);
        }

        private static string UploadsFolder => Path.Combine(AppContext.BaseDirectory, "Uploads");

        private static int FilesInUploadsFolder() =>
            Directory.Exists(UploadsFolder) ? Directory.GetFiles(UploadsFolder, "*", SearchOption.AllDirectories).Length : 0;

        [Test]
        public void A_header_only_workbook_loads_without_errors_and_without_touching_the_file_system()
        {
            var before = FilesInUploadsFolder();

            var errors = Service().RunAddList(Workbook(sheets: 3, dataRows: 0, ["BCC", "PCG", "BLS"]));

            Assert.IsEmpty(errors);
            Assert.AreEqual(before, FilesInUploadsFolder());
        }

        [Test]
        public void An_unreadable_upload_named_like_a_path_is_reported_and_writes_nothing()
        {
            var before = FilesInUploadsFolder();
            var upload = Form([], "..\\..\\evil.xlsx");

            var errors = Service().RunAddList(upload);

            Assert.AreEqual(1, errors.Count);
            Assert.IsFalse(File.Exists(Path.Combine(AppContext.BaseDirectory, "..", "evil.xlsx")));
            Assert.AreEqual(before, FilesInUploadsFolder());
        }

        [Test]
        public void Rows_are_applied_for_every_sheet_that_is_read()
        {
            // No plants exist, so every data row is reported as not found: two rows on each of the three sheets.
            var errors = Service().RunAddList(Workbook(sheets: 3, dataRows: 2, ["BCC", "PCG", "BLS"]));

            Assert.AreEqual(6, errors.Count);
            Assert.That(errors.Select(e => e.Error), Is.All.EqualTo("Plant Not Found - Please Add Plant and Try Again"));
            CollectionAssert.AreEquivalent(new[] { "BCC", "BCC", "PCG", "PCG", "BLS", "BLS" }, errors.Select(e => e.Scheme));
        }

        [Test]
        public void An_empty_upload_is_reported()
        {
            var errors = Service().RunAddList(Form([]));

            Assert.AreEqual("No workbook was provided", errors.Single().Error);
        }

        [Test]
        public void No_file_is_reported()
        {
            var errors = Service().RunAddList(null);

            Assert.AreEqual("No workbook was provided", errors.Single().Error);
        }

        [Test]
        public void A_file_that_is_not_a_workbook_is_reported_not_thrown()
        {
            var errors = Service().RunAddList(Form("this is not an xlsx file"u8.ToArray()));

            Assert.AreEqual("The file is not a valid Excel workbook", errors.Single().Error);
        }

        [Test]
        public void An_oversized_file_is_rejected_before_it_is_read()
        {
            var service = Service(new UploadOptions { MaxFileBytes = 100 });

            var errors = service.RunAddList(Workbook(sheets: 3, dataRows: 5));

            Assert.AreEqual("The workbook is larger than the maximum allowed size", errors.Single().Error);
        }

        [Test]
        public void A_workbook_with_fewer_than_three_sheets_is_rejected()
        {
            var errors = Service().RunAddList(Workbook(sheets: 2, dataRows: 1));

            Assert.AreEqual("The workbook must contain the three template worksheets", errors.Single().Error);
        }

        [Test]
        public void A_sheet_with_too_many_rows_is_rejected_before_any_row_is_applied()
        {
            var service = Service(new UploadOptions { MaxRowsPerSheet = 3 });

            var errors = service.RunAddList(Workbook(sheets: 3, dataRows: 4, ["BCC", "PCG", "BLS"]));

            Assert.AreEqual("The worksheet has more rows than the maximum allowed", errors.Single().Error);
            Assert.AreEqual("BCC", errors.Single().Scheme);
        }

        [Test]
        public void An_empty_sheet_contributes_no_rows()
        {
            using var package = new ExcelPackage();
            package.Workbook.Worksheets.Add("BCC");
            package.Workbook.Worksheets.Add("PCG");
            package.Workbook.Worksheets.Add("BLS");

            var errors = Service().RunAddList(Form(package.GetAsByteArray()));

            Assert.IsEmpty(errors);
        }

        [Test]
        public void Two_uploads_with_the_same_file_name_do_not_collide()
        {
            var first = Workbook(sheets: 3, dataRows: 1, ["BCC", "PCG", "BLS"]);
            var second = Workbook(sheets: 3, dataRows: 1, ["BCC", "PCG", "BLS"]);

            var results = Task.WhenAll(
                Task.Run(() => Service().RunAddList(first)),
                Task.Run(() => Service().RunAddList(second))).GetAwaiter().GetResult();

            // One row on each of three sheets per upload: each request reports only its own three rows.
            Assert.AreEqual(3, results[0].Count);
            Assert.AreEqual(3, results[1].Count);
        }

        [Test]
        public void The_csv_export_keeps_its_windows_line_endings_and_column_order_on_every_platform()
        {
            var plants = new Mock<DbSet<RPA.MTSInspections.Models.Plant>>().SetupData();
            var db = new Mock<MTSInspectionsContext>();
            db.Setup(x => x.Plant).Returns(plants.Object);
            db.Setup(x => x.Scheme).Returns(new Mock<DbSet<RPA.MTSInspections.Models.Scheme>>().SetupData().Object);
            db.Setup(x => x.Option).Returns(new Mock<DbSet<RPA.MTSInspections.Models.Option>>().SetupData().Object);
            var service = new ExportService(db.Object, Mock.Of<ISchemeService>(), Mock.Of<IAuditService>(), new FakeUser(),
                Options.Create(new UploadOptions()));

            var csv = service.ConvertListToString("1");

            Assert.AreEqual(
                "\"Licence No\",\"Plant Name\",\"Address Line 1\",\"Address Line 2\",\"Town/City\",\"County\",\"Post Code\",\"Scheme Year\",\"Risk Score\",\"Active\"\r\n",
                csv);
        }
    }
}
