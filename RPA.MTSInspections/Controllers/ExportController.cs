using RPA.MTSInspections.Models;
using RPA.MTSInspections.SL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RPA.MTSInspections.Controllers
{
    public class ExportController : Controller
    {

        IExportService ExportService;

        public ExportController()
        {
            this.ExportService = new ExportService();
        }

        public ExportController(IExportService exportService)
        {
            this.ExportService = exportService;
        }

        // GET: Export
        public ActionResult Index(bool? upload)
        {
            var myCollection = ExportService.GetMyCollection();

            ViewBag.MyCustomCollection = myCollection;

            if (upload == true)
            {
                ViewBag.UploadSuccess = upload;
            }
            return View();
        }

        //POST: Return Requested Export CSV

        public FileContentResult ExportCSV(string ExportListOption, string DropDownValue)
        {
            try
            {
                string result = ExportService.ConvertListToString(ExportListOption);

                return File(new System.Text.UTF8Encoding().GetBytes(result), "text/csv", DropDownValue + "_Results.csv");
            }
            catch (Exception ex)
            {
                throw new Exception("EX001: Unable to Export Requested Data", ex);
            }
        }

        // GET: Import Partial

        public ActionResult _ImportPartial()
        {
            return PartialView();
        }

        //POST: Bulk Load a File

        [HttpPost]
        [ValidateAntiForgeryToken]

        public ActionResult _ImportPartial(HttpPostedFileBase bulkSource)
        {
            List<ErrorLog> errorLog = new List<ErrorLog>();

            errorLog = ExportService.RunAddList(bulkSource);


            if (errorLog.Count == 0)
            {


                return RedirectToAction("Index", new { upload = true });
            }
            else
            {
                return View("FailList", errorLog);
            }

        }
    }
}