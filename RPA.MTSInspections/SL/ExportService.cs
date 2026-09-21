using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Data;
using OfficeOpenXml;
using RPA.MTSInspections.Helpers;
using DocumentFormat.OpenXml.Office2013.Excel;
using System.Globalization;

namespace RPA.MTSInspections.SL
{
    public class ExportService : IExportService
    {
        MTSInspectionsContext db;
        ISchemeService schemeService;
        IAuditService auditService;

        public ExportService()
        {
            db = new MTSInspectionsContext();
            schemeService = new SchemeService(db);
            auditService = new AuditService(db);
        }

        public ExportService(MTSInspectionsContext context, ISchemeService schemeService, IAuditService auditService)
        {
            db = context;
            this.schemeService = schemeService;
            this.auditService = auditService;
        }

        public List<SelectListItem> GetMyCollection()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            SelectListItem l1 = new SelectListItem { Text = "BLS - Download List of All Plants", Value = "1" };
            SelectListItem l2 = new SelectListItem { Text = "BLS - Download List of All Plants (Active Plants Only)", Value = "2" };
            SelectListItem l3 = new SelectListItem { Text = "BLS - Download List of All Plants and Data", Value = "3" };
            SelectListItem l4 = new SelectListItem { Text = "BLS - Download List of All Plants and Data (Active Plants Only)", Value = "4" };
            SelectListItem l5 = new SelectListItem { Text = "BCC - Download List of All Plants", Value = "5" };
            SelectListItem l6 = new SelectListItem { Text = "BCC - Download List of All Plants (Active Plants Only)", Value = "6" };
            SelectListItem l7 = new SelectListItem { Text = "BCC - Download List of All Plants and Data", Value = "7" };
            SelectListItem l8 = new SelectListItem { Text = "BCC - Download List of All Plants and Data (Active Plants Only)", Value = "8" };
            SelectListItem l9 = new SelectListItem { Text = "PCG - Download List of All Plants", Value = "9" };
            SelectListItem l10 = new SelectListItem { Text = "PCG - Download List of All Plants (Active Plants Only)", Value = "10" };
            SelectListItem l11 = new SelectListItem { Text = "PCG - Download List of All Plants and Data", Value = "11" };
            SelectListItem l12 = new SelectListItem { Text = "PCG - Download List of All Plants and Data (Active Plants Only)", Value = "12" };
            SelectListItem l13 = new SelectListItem { Text = "SCC - Download List of All Plants", Value = "13" };
            SelectListItem l14 = new SelectListItem { Text = "SCC - Download List of All Plants (Active Plants Only)", Value = "14" };
            SelectListItem l15 = new SelectListItem { Text = "SCC - Download List of All Plants and Data", Value = "15" };
            SelectListItem l16 = new SelectListItem { Text = "SCC - Download List of All Plants and Data (Active Plants Only)", Value = "16" };

            list.Add(l1);
            list.Add(l2);
            list.Add(l3);
            list.Add(l4);
            list.Add(l5);
            list.Add(l6);
            list.Add(l7);
            list.Add(l8);
            list.Add(l9);
            list.Add(l10);
            list.Add(l11);
            list.Add(l12);
            list.Add(l13);
            list.Add(l14);
            list.Add(l15);
            list.Add(l16);

            return list;

        }

        public string ConvertListToString(string option)
        {
            StringWriter writer = new StringWriter();

            List<Plant> plantList = new List<Plant>();
            List <Option> nopList = db.Option.Where(x => x.Criteria.Name == "Nature of Operation").OrderBy(x => x.Name).ToList();

            switch (option)
            {
                case "1": //"BLS - Download List of All Plants"

                    plantList = db.Plant.AsNoTracking().Where(x => x.BLS).Include(x => x.PlantOptions).Include(x => x.RiskScore).ToList();

                    //Write Headers

                    writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\"", "Licence No", "Plant Name", "Address Line 1", "Address Line 2", "Town/City", "County", "Post Code", "Scheme Year", "Risk Score", "Active"));

                    foreach (var item in plantList.Where(x => x.BLS == true))
                    {

                        //Get BLS Scheme ID
                        Guid schemeId = db.Scheme.AsNoTracking().Where(x => x.Name == "BLS").Select(x => x.SchemeId).FirstOrDefault();

                        //Get Scheme Year for this Plant
                        PlantOption schemeYr = item.PlantOptions.Where(x => x.SchemeId == schemeId && x.Option.Criteria.Name == "Scheme Year").FirstOrDefault();

                        //Get Risk Score for this Plant
                        int riskScore = item.RiskScore.Where(x => x.SchemeId == schemeId).Select(x => x.Score).FirstOrDefault();

                        //Write required Data
                        writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\"", item.LicenceNo ?? "", item.PlantName ?? "", item.AddressOne ?? "", item.AddressTwo ?? "", item.TownCity ?? "", item.County ?? "", item.PostCode ?? "", (schemeYr == null) ? "" : schemeYr.Option.Name ?? "", riskScore, item.Active));

                    }

                    break;

                case "2": //"BLS - Download List of All Plants (Active Plants Only)"

                    plantList = db.Plant.AsNoTracking().Where(x => x.BLS).Include(x => x.PlantOptions).Include(x => x.RiskScore).ToList();

                    //Write Headers

                    writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\"", "Licence No", "Plant Name", "Address Line 1", "Address Line 2", "Town/City", "County", "Post Code", "Scheme Year", "Risk Score", "Active"));

                    foreach (var item in plantList.Where(x => x.Active == true && x.BLS == true))
                    {


                        //Get BLS Scheme ID
                        Guid schemeId = db.Scheme.AsNoTracking().Where(x => x.Name == "BLS").Select(x => x.SchemeId).FirstOrDefault();

                        //Get Scheme Year for this Plant
                        PlantOption schemeYr = item.PlantOptions.Where(x => x.SchemeId == schemeId && x.Option.Criteria.Name == "Scheme Year").FirstOrDefault();

                        //Get Risk Score for this Plant
                        int riskScore = item.RiskScore.Where(x => x.SchemeId == schemeId).Select(x => x.Score).FirstOrDefault();

                        //Write required Data
                        writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\"", item.LicenceNo ?? "", item.PlantName ?? "", item.AddressOne ?? "", item.AddressTwo ?? "", item.TownCity ?? "", item.County ?? "", item.PostCode ?? "", (schemeYr == null) ? "" : schemeYr.Option.Name ?? "", riskScore, item.Active));

                    }

                    break;

                case "3": //"BLS - Download List of All Plants and Data"

                    plantList = db.Plant.AsNoTracking().Where(x => x.BLS).Include(x => x.PlantOptions).Include(x => x.RiskScore).ToList();

                    //Write Headers

                    writer.Write(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\",\"{16}\",\"{17}\",\"{18}\",\"{19}\"", "Licence No", "Plant Name", "Address Line 1", "Address Line 2", "Town/City", "County", "Post Code", "Active", "Risk Score", "Scheme Year", "Date of Last Inspection", "Number of Months Since Last Inspection", "Number of Failed Inspections", "Number of Non-Compliance", "Severity of Non-Compliance", "Throughput", "Operation Hours", "Premises Type", "Type of Operation", "Origin of Products Handled"));

                    foreach (var item in nopList)
                    {
                        writer.Write(string.Format(",\"Nature of Operation - {0}\"", item.Name));
                    }

                    //Write Plant Details 

                    writer.Write("\r\n");

                    foreach (var item in plantList.Where(x => x.BLS == true))
                    {

                        //Get BLS Scheme ID
                        Guid schemeId = db.Scheme.AsNoTracking().Where(x => x.Name == "BLS").Select(x => x.SchemeId).FirstOrDefault();

                        //Get Scheme Year for this Plant
                        PlantOption schemeYr = item.PlantOptions.Where(x => x.SchemeId == schemeId && x.Option.Criteria.Name == "Scheme Year").FirstOrDefault();

                        //Get Risk Score for this Plant
                        int riskScore = item.RiskScore.Where(x => x.SchemeId == schemeId).Select(x => x.Score).FirstOrDefault();

                        //Get Last Inspection Date for this Plant
                        DateTime? LastInspectionDate = item.LastInspection.Where(x => x.SchemeId == schemeId).FirstOrDefault()?.Date;

                        writer.Write(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\",\"{16}\",\"{17}\",\"{18}\",\"{19}\"",

                        item.LicenceNo ?? "",
                        item.PlantName ?? "",
                        item.AddressOne ?? "",
                        item.AddressTwo ?? "",
                        item.TownCity ?? "",
                        item.County ?? "",
                        item.PostCode ?? "",
                        item.Active,
                        riskScore,
                        item.PlantOptions.Where(x => x.Option.Criteria.Name == "Scheme Year").FirstOrDefault()?.Option?.Name ?? "",
                        LastInspectionDate.HasValue ? LastInspectionDate.Value.ToShortDateString() : "",
                        item.PlantOptions.Where(x => x.Option.Criteria.Name == "Number of Months Since Last Inspection").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Option.Criteria.Name == "Number of Inspections Resulting in Failure").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Option.Criteria.Name == "Number of Non-Compliance").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Option.Criteria.Name == "Severity of Non-Compliance").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Option.Criteria.Name == "Throughput").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Option.Criteria.Name == "Operating Hours").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Option.Criteria.Name == "Premises Type").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Option.Criteria.Name == "Type of Operation").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Option.Criteria.Name == "Origin of Products Handled").FirstOrDefault()?.Option?.Name ?? ""
                        ));

                        var nopSelectedList = item.PlantOptions.Where(x => x.SchemeId == schemeId && x.Option.Criteria.Name == "Nature of Operation").OrderBy(x => x.Option.Name).ToList();

                        foreach (var npItem in nopList)
                        {
                            if (nopSelectedList.Exists(x => x.Option.Name == npItem.Name))
                            {
                                writer.Write(",\"TRUE\"");
                            }
                            else
                            {
                                writer.Write(",\"FALSE\"");
                            }
                        }

                        writer.Write("\r\n");
                    }

                    break;

                case "4": //"BLS - Download List of All Plants and Data (Active Plants Only)"

                    //Write Headers
                    plantList = db.Plant.AsNoTracking().Where(x => x.BLS).Include(x => x.PlantOptions).Include(x => x.RiskScore).ToList();

                    writer.Write(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\",\"{16}\",\"{17}\",\"{18}\",\"{19}\"", "Licence No", "Plant Name", "Address Line 1", "Address Line 2", "Town/City", "County", "Post Code", "Active", "Risk Score", "Scheme Year", "Date of Last Inspection", "Number of Months Since Last Inspection", "Number of Failed Inspections", "Number of Non-Compliance", "Severity of Non-Compliance", "Throughput", "Operation Hours", "Premises Type", "Type of Operation", "Origin of Products Handled"));

                    foreach (var item in nopList)
                    {
                        writer.Write(string.Format(",\"Nature of Operation - {0}\"", item.Name));
                    }


                    writer.Write("\r\n");

                    foreach (var item in plantList.Where(x => x.BLS == true && x.Active == true))
                    {

                        //Get BLS Scheme ID
                        Guid schemeId = db.Scheme.AsNoTracking().Where(x => x.Name == "BLS").Select(x => x.SchemeId).FirstOrDefault();

                        //Get Scheme Year for this Plant
                        PlantOption schemeYr = item.PlantOptions.Where(x => x.SchemeId == schemeId && x.Option.Criteria.Name == "Scheme Year").FirstOrDefault();

                        //Get Risk Score for this Plant
                        int riskScore = item.RiskScore.Where(x => x.SchemeId == schemeId).Select(x => x.Score).FirstOrDefault();

                        //Get Last Inspection Date for this Plant
                        DateTime? LastInspectionDate = item.LastInspection.Where(x => x.SchemeId == schemeId).FirstOrDefault()?.Date;

                        //Write Plant Details

                        writer.Write(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\",\"{16}\",\"{17}\",\"{18}\",\"{19}\"",

                        item.LicenceNo ?? "",
                        item.PlantName ?? "",
                        item.AddressOne ?? "",
                        item.AddressTwo ?? "",
                        item.TownCity ?? "",
                        item.County ?? "",
                        item.PostCode ?? "",
                        item.Active,
                        riskScore,
                        item.PlantOptions.Where(x => x.Option.Criteria.Name == "Scheme Year").FirstOrDefault()?.Option?.Name ?? "",
                        LastInspectionDate.HasValue ? LastInspectionDate.Value.ToShortDateString() : "",
                        item.PlantOptions.Where(x => x.Option.Criteria.Name == "Number of Months Since Last Inspection").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Option.Criteria.Name == "Number of Inspections Resulting in Failure").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Option.Criteria.Name == "Number of Non-Compliance").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Option.Criteria.Name == "Severity of Non-Compliance").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Option.Criteria.Name == "Throughput").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Option.Criteria.Name == "Operating Hours").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Option.Criteria.Name == "Premises Type").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Option.Criteria.Name == "Type of Operation").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Option.Criteria.Name == "Origin of Products Handled").FirstOrDefault()?.Option?.Name ?? ""
                        ));


                        var nopSelectedList = item.PlantOptions.Where(x => x.SchemeId == schemeId && x.Option.Criteria.Name == "Nature of Operation").OrderBy(x => x.Option.Name).ToList();

                        foreach (var npItem in nopList)
                        {
                            if (nopSelectedList.Exists(x => x.Option.Name == npItem.Name))
                            {
                                writer.Write(",\"TRUE\"");
                            }
                            else
                            {
                                writer.Write(",\"FALSE\"");
                            }
                        }

                        writer.Write("\r\n");
                    }

                    break;

                case "5": //"BCC - Download List of All Plants"

                    //Write Headers
                    plantList = db.Plant.AsNoTracking().Where(x => x.BCC).Include(x => x.PlantOptions).Include(x => x.RiskScore).ToList();

                    writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\"", "Licence No", "Plant Name", "Address Line 1", "Address Line 2", "Town/City", "County", "Post Code", "Scheme Year", "Risk Score", "Active"));

                    foreach (var item in plantList.Where(x => x.BCC == true))
                    {

                        //Get BCC Scheme ID
                        Guid schemeId = db.Scheme.AsNoTracking().Where(x => x.Name == "BCC").Select(x => x.SchemeId).FirstOrDefault();

                        //Get Scheme Year for this Plant
                        PlantOption schemeYr = item.PlantOptions.Where(x => x.SchemeId == schemeId && x.Option.Criteria.Name == "Scheme Year").FirstOrDefault();

                        //Get Risk Score for this Plant
                        int riskScore = item.RiskScore.Where(x => x.SchemeId == schemeId).Select(x => x.Score).FirstOrDefault();

                        //Write required Data
                        writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\"", item.LicenceNo ?? "", item.PlantName ?? "", item.AddressOne ?? "", item.AddressTwo ?? "", item.TownCity ?? "", item.County ?? "", item.PostCode ?? "", (schemeYr == null) ? "" : schemeYr.Option.Name ?? "", riskScore, item.Active));

                    }

                    break;

                case "6": //"BCC - Download List of All Plants (Active Plants Only)"

                    //Write Headers
                    plantList = db.Plant.AsNoTracking().Where(x => x.BCC).Include(x => x.PlantOptions).Include(x => x.RiskScore).ToList();

                    writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\"", "Licence No", "Plant Name", "Address Line 1", "Address Line 2", "Town/City", "County", "Post Code", "Scheme Year", "Risk Score", "Active"));

                    foreach (var item in plantList.Where(x => x.BCC == true && x.Active == true))
                    {

                        //Get BCC Scheme ID
                        Guid schemeId = db.Scheme.AsNoTracking().Where(x => x.Name == "BCC").Select(x => x.SchemeId).FirstOrDefault();

                        //Get Scheme Year for this Plant
                        PlantOption schemeYr = item.PlantOptions.Where(x => x.SchemeId == schemeId && x.Option.Criteria.Name == "Scheme Year").FirstOrDefault();

                        //Get Risk Score for this Plant
                        int riskScore = item.RiskScore.Where(x => x.SchemeId == schemeId).Select(x => x.Score).FirstOrDefault();

                        //Write required Data
                        writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\"", item.LicenceNo ?? "", item.PlantName ?? "", item.AddressOne ?? "", item.AddressTwo ?? "", item.TownCity ?? "", item.County ?? "", item.PostCode ?? "", (schemeYr == null) ? "" : schemeYr.Option.Name ?? "", riskScore, item.Active));

                    }

                    break;

                case "7": //"BCC - Download List of All Plants and Data"

                    plantList = db.Plant.AsNoTracking().Where(x => x.BCC).Include(x => x.PlantOptions).Include(x => x.RiskScore).ToList();

                    //Write Headers

                    writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\",\"{16}\",\"{17}\",\"{18}\",\"{19}\",\"{20}\",\"{21}\",\"{22}\",\"{23}\"", "Licence No", "Plant Name", "Address Line 1", "Address Line 2", "Town/City", "County", "Post Code", "Active", "Risk Score", "Scheme Year", "Date of Last Inspection", "Number of Weeks Since Last Inspection", "Failed/Unsatisfactory on Last Visit", "Number of Failed/Unsatisfactory in Previous 3 Years", "Number of Near Misses in Previous 3 Years", "Throughput", "Operating Hours", "Pre Inspection Sign In", "Carcass Dressing", "Line Clearing", "Record Keeping", "Trimming", "Carcass Hanging Method", "Hide Puller"));

                    //Write Plant Details 

                    foreach (var item in plantList.Where(x => x.BCC == true))
                    {

                        //Get BCC Scheme ID
                        Guid schemeId = db.Scheme.AsNoTracking().Where(x => x.Name == "BCC").Select(x => x.SchemeId).FirstOrDefault();

                        //Get Scheme Year for this Plant
                        PlantOption schemeYr = item.PlantOptions.Where(x => x.SchemeId == schemeId && x.Option.Criteria.Name == "Scheme Year").FirstOrDefault();

                        //Get Risk Score for this Plant
                        int riskScore = item.RiskScore.Where(x => x.SchemeId == schemeId).Select(x => x.Score).FirstOrDefault();

                        //Get Last Inspection Date for this Plant
                        DateTime? LastInspectionDate = item.LastInspection.Where(x => x.SchemeId == schemeId).FirstOrDefault()?.Date;



                        writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\",\"{16}\",\"{17}\",\"{18}\",\"{19}\",\"{20}\",\"{21}\",\"{22}\",\"{23}\"",

                        item.LicenceNo ?? "",
                        item.PlantName ?? "",
                        item.AddressOne ?? "",
                        item.AddressTwo ?? "",
                        item.TownCity ?? "",
                        item.County ?? "",
                        item.PostCode ?? "",
                        item.Active,
                        riskScore,
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Scheme Year").FirstOrDefault()?.Option?.Name ?? "",
                        LastInspectionDate.HasValue ? LastInspectionDate.Value.ToShortDateString() : "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Number of Weeks Since Last Inspection").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Failed/Unsatisfactory on Last Visit").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Number of Failed/Unsatisfactory in Previous 3 Years").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Number of Near Misses in Previous 3 Years").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Throughput").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Operating Hours").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Pre Inspection Sign In").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Carcass Dressing").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Line Clearing").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Record Keeping").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Trimming").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Carcass Hanging Method").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Hide Puller").FirstOrDefault()?.Option?.Name ?? ""
                        ));

                    }

                    break;

                case "8": //"BCC - Download List of All Plants and Data (Active Plants Only)"

                    plantList = db.Plant.AsNoTracking().Where(x => x.BCC).Include(x => x.PlantOptions).Include(x => x.RiskScore).ToList();

                    //Write Headers

                    writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\",\"{16}\",\"{17}\",\"{18}\",\"{19}\",\"{20}\",\"{21}\",\"{22}\",\"{23}\",\"{24}\"", "Licence No", "Plant Name", "Address Line 1", "Address Line 2", "Town/City", "County", "Post Code", "Active", "Risk Score", "Scheme Year", "Date of Last Inspection", "Number of Weeks Since Last Inspection", "Failed/Unsatisfactory on Last Visit", "Number of Failed/Unsatisfactory in Previous 3 Years", "Number of Near Misses in Previous 3 Years", "Throughput", "Operating Hours", "Pre Inspection Sign In", "Carcass Dressing", "Source of Classifiers", "Line Clearing", "Record Keeping", "Trimming", "Carcass Hanging Method", "Hide Puller"));

                    //Write Plant Details 

                    foreach (var item in plantList.Where(x => x.BCC == true && x.Active == true))
                    {

                        //Get BCC Scheme ID
                        Guid schemeId = db.Scheme.AsNoTracking().Where(x => x.Name == "BCC").Select(x => x.SchemeId).FirstOrDefault();

                        //Get Scheme Year for this Plant
                        PlantOption schemeYr = item.PlantOptions.Where(x => x.SchemeId == schemeId && x.Option.Criteria.Name == "Scheme Year").FirstOrDefault();

                        //Get Risk Score for this Plant
                        int riskScore = item.RiskScore.Where(x => x.SchemeId == schemeId).Select(x => x.Score).FirstOrDefault();

                        //Get Last Inspection Date for this Plant
                        DateTime? LastInspectionDate = item.LastInspection.Where(x => x.SchemeId == schemeId).FirstOrDefault()?.Date;



                        writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\",\"{16}\",\"{17}\",\"{18}\",\"{19}\",\"{20}\",\"{21}\",\"{22}\",\"{23}\"",

                        item.LicenceNo ?? "",
                        item.PlantName ?? "",
                        item.AddressOne ?? "",
                        item.AddressTwo ?? "",
                        item.TownCity ?? "",
                        item.County ?? "",
                        item.PostCode ?? "",
                        item.Active,
                        riskScore,
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Scheme Year").FirstOrDefault()?.Option?.Name ?? "",
                        LastInspectionDate.HasValue ? LastInspectionDate.Value.ToShortDateString() : "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Number of Weeks Since Last Inspection").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Failed/Unsatisfactory on Last Visit").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Number of Failed/Unsatisfactory in Previous 3 Years").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Number of Near Misses in Previous 3 Years").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Throughput").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Operating Hours").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Pre Inspection Sign In").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Carcass Dressing").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Line Clearing").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Record Keeping").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Trimming").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Carcass Hanging Method").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "BCC" && x.Option.Criteria.Name == "Hide Puller").FirstOrDefault()?.Option?.Name ?? ""
                        ));

                    }

                    break;

                case "9": //"PCG - Download List of All Plants"

                    //Write Headers
                    plantList = db.Plant.AsNoTracking().Where(x => x.PCG).Include(x => x.PlantOptions).Include(x => x.RiskScore).ToList();

                    writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\"", "Licence No", "Plant Name", "Address Line 1", "Address Line 2", "Town/City", "County", "Post Code", "Scheme Year", "Risk Score", "Active"));

                    foreach (var item in plantList.Where(x => x.PCG == true))
                    {

                        //Get PCG Scheme ID
                        Guid schemeId = db.Scheme.AsNoTracking().Where(x => x.Name == "PCG").Select(x => x.SchemeId).FirstOrDefault();

                        //Get Scheme Year for this Plant
                        PlantOption schemeYr = item.PlantOptions.Where(x => x.SchemeId == schemeId && x.Option.Criteria.Name == "Scheme Year").FirstOrDefault();

                        //Get Risk Score for this Plant
                        int riskScore = item.RiskScore.Where(x => x.SchemeId == schemeId).Select(x => x.Score).FirstOrDefault();

                        //Write required Data
                        writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\"", item.LicenceNo ?? "", item.PlantName ?? "", item.AddressOne ?? "", item.AddressTwo ?? "", item.TownCity ?? "", item.County ?? "", item.PostCode ?? "", (schemeYr == null) ? "" : schemeYr.Option.Name ?? "", riskScore, item.Active));

                    }


                    break;

                case "10": //"PCG - Download List of All Plants (Active Plants Only)"

                    //Write Headers
                    plantList = db.Plant.AsNoTracking().Where(x => x.PCG).Include(x => x.PlantOptions).Include(x => x.RiskScore).ToList();

                    writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\"", "Licence No", "Plant Name", "Address Line 1", "Address Line 2", "Town/City", "County", "Post Code", "Scheme Year", "Risk Score", "Active"));

                    foreach (var item in plantList.Where(x => x.PCG == true && x.Active == true))
                    {

                        //Get PCG Scheme ID
                        Guid schemeId = db.Scheme.AsNoTracking().Where(x => x.Name == "PCG").Select(x => x.SchemeId).FirstOrDefault();

                        //Get Scheme Year for this Plant
                        PlantOption schemeYr = item.PlantOptions.Where(x => x.SchemeId == schemeId && x.Option.Criteria.Name == "Scheme Year").FirstOrDefault();

                        //Get Risk Score for this Plant
                        int riskScore = item.RiskScore.Where(x => x.SchemeId == schemeId).Select(x => x.Score).FirstOrDefault();

                        //Write required Data
                        writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\"", item.LicenceNo ?? "", item.PlantName ?? "", item.AddressOne ?? "", item.AddressTwo ?? "", item.TownCity ?? "", item.County ?? "", item.PostCode ?? "", (schemeYr == null) ? "" : schemeYr.Option.Name ?? "", riskScore, item.Active));

                    }

                    break;

                case "11": //"PCG - Download List of All Plants and Data"

                    //Write Headers
                    plantList = db.Plant.AsNoTracking().Where(x => x.PCG).Include(x => x.PlantOptions).Include(x => x.RiskScore).ToList();

                    writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\",\"{16}\",\"{17}\",\"{18}\",\"{19}\",\"{20}\",\"{21}\",\"{22}\",\"{23}\"", "Licence No", "Plant Name", "Address Line 1", "Address Line 2", "Town/City", "County", "Post Code", "Active", "Risk Score", "Scheme Year", "Date of Last Inspection", "Number of Weeks Since Last Inspection", "Failed/Unsatisfactory on Last Visit", "Number of Failed/Unsatisfactory in Previous 3 Years", "Number of Near Misses in Previous 3 Years", "Throughput", "Operating Hours", "Pre Inspection Sign In", "Carcass Dressing", "Source of Classifiers", "Line Clearing", "Record Keeping", "Trimming", "Probe"));

                    //Write Plant Details 

                    foreach (var item in plantList.Where(x => x.PCG == true && x.Active == true))
                    {

                        //Get PCG Scheme ID
                        Guid schemeId = db.Scheme.AsNoTracking().Where(x => x.Name == "PCG").Select(x => x.SchemeId).FirstOrDefault();

                        //Get Scheme Year for this Plant
                        PlantOption schemeYr = item.PlantOptions.Where(x => x.SchemeId == schemeId && x.Option.Criteria.Name == "Scheme Year").FirstOrDefault();

                        //Get Risk Score for this Plant
                        int riskScore = item.RiskScore.Where(x => x.SchemeId == schemeId).Select(x => x.Score).FirstOrDefault();

                        //Get Last Inspection Date for this Plant
                        DateTime? LastInspectionDate = item.LastInspection.Where(x => x.SchemeId == schemeId).FirstOrDefault()?.Date;



                        writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\",\"{16}\",\"{17}\",\"{18}\",\"{19}\",\"{20}\",\"{21}\",\"{22}\"",

                        item.LicenceNo ?? "",
                        item.PlantName ?? "",
                        item.AddressOne ?? "",
                        item.AddressTwo ?? "",
                        item.TownCity ?? "",
                        item.County ?? "",
                        item.PostCode ?? "",
                        item.Active,
                        riskScore,
                        item.PlantOptions.Where(x => x.Option.Criteria.Name == "Scheme Year").FirstOrDefault()?.Option?.Name ?? "",
                        LastInspectionDate.HasValue ? LastInspectionDate.Value.ToShortDateString() : "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "PCG" && x.Option.Criteria.Name == "Number of Weeks Since Last Inspection").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "PCG" && x.Option.Criteria.Name == "Failed/Unsatisfactory on Last Visit").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "PCG" && x.Option.Criteria.Name == "Number of Failed/Unsatisfactory in Previous 3 Years").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "PCG" && x.Option.Criteria.Name == "Number of Near Misses in Previous 3 Years").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "PCG" && x.Option.Criteria.Name == "Throughput").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "PCG" && x.Option.Criteria.Name == "Operating Hours").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "PCG" && x.Option.Criteria.Name == "Pre Inspection Sign In").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "PCG" && x.Option.Criteria.Name == "Carcass Dressing").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "PCG" && x.Option.Criteria.Name == "Line Clearing").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "PCG" && x.Option.Criteria.Name == "Record Keeping").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "PCG" && x.Option.Criteria.Name == "Trimming").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "PCG" && x.Option.Criteria.Name == "Probe").FirstOrDefault()?.Option?.Name ?? ""
                        ));

                    }

                    break;

                case "12": //"PCG - Download List of All Plants and Data (Active Plants Only)"

                    plantList = db.Plant.AsNoTracking().Where(x => x.PCG).Include(x => x.PlantOptions).Include(x => x.RiskScore).ToList();

                    //Write Headers

                    writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\",\"{16}\",\"{17}\",\"{18}\",\"{19}\",\"{20}\",\"{21}\",\"{22}\",\"{23}\"", "Licence No", "Plant Name", "Address Line 1", "Address Line 2", "Town/City", "County", "Post Code", "Active", "Risk Score", "Scheme Year", "Date of Last Inspection", "Number of Weeks Since Last Inspection", "Failed/Unsatisfactory on Last Visit", "Number of Failed/Unsatisfactory in Previous 3 Years", "Number of Near Misses in Previous 3 Years", "Throughput", "Operating Hours", "Pre Inspection Sign In", "Carcass Dressing", "Source of Classifiers", "Line Clearing", "Record Keeping", "Trimming", "Probe"));

                    //Write Plant Details 

                    foreach (var item in plantList.Where(x => x.PCG == true))
                    {

                        //Get PCG Scheme ID
                        Guid schemeId = db.Scheme.AsNoTracking().Where(x => x.Name == "PCG").Select(x => x.SchemeId).FirstOrDefault();

                        //Get Scheme Year for this Plant
                        PlantOption schemeYr = item.PlantOptions.Where(x => x.SchemeId == schemeId && x.Option.Criteria.Name == "Scheme Year").FirstOrDefault();

                        //Get Risk Score for this Plant
                        int riskScore = item.RiskScore.Where(x => x.SchemeId == schemeId).Select(x => x.Score).FirstOrDefault();

                        //Get Last Inspection Date for this Plant
                        DateTime? LastInspectionDate = item.LastInspection.Where(x => x.SchemeId == schemeId).FirstOrDefault()?.Date;

                        writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\",\"{16}\",\"{17}\",\"{18}\",\"{19}\",\"{20}\",\"{21}\",\"{22}\"",

                        item.LicenceNo ?? "",
                        item.PlantName ?? "",
                        item.AddressOne ?? "",
                        item.AddressTwo ?? "",
                        item.TownCity ?? "",
                        item.County ?? "",
                        item.PostCode ?? "",
                        item.Active,
                        riskScore,
                        item.PlantOptions.Where(x => x.Scheme.Name == "PCG" && x.Option.Criteria.Name == "Scheme Year").FirstOrDefault()?.Option?.Name ?? "",
                        LastInspectionDate.HasValue ? LastInspectionDate.Value.ToShortDateString() : "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "PCG" && x.Option.Criteria.Name == "Number of Weeks Since Last Inspection").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "PCG" && x.Option.Criteria.Name == "Failed/Unsatisfactory on Last Visit").FirstOrDefault()?.Option?.Name ?? "",                    
                        item.PlantOptions.Where(x => x.Scheme.Name == "PCG" && x.Option.Criteria.Name == "Number of Failed/Unsatisfactory in Previous 3 Years").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "PCG" && x.Option.Criteria.Name == "Number of Near Misses in Previous 3 Years").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "PCG" && x.Option.Criteria.Name == "Throughput").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "PCG" && x.Option.Criteria.Name == "Operating Hours").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "PCG" && x.Option.Criteria.Name == "Pre Inspection Sign In").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "PCG" && x.Option.Criteria.Name == "Carcass Dressing").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "PCG" && x.Option.Criteria.Name == "Line Clearing").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "PCG" && x.Option.Criteria.Name == "Record Keeping").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "PCG" && x.Option.Criteria.Name == "Trimming").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "PCG" && x.Option.Criteria.Name == "Probe").FirstOrDefault()?.Option?.Name ?? ""
                        ));
                    }
                    break;

                case "13": //"SCC - Download List of All Plants"

                    plantList = db.Plant.AsNoTracking().Where(x => x.SCC).Include(x => x.PlantOptions).Include(x => x.RiskScore).ToList();

                    //Write Headers

                    writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\"", "Licence No", "Plant Name", "Address Line 1", "Address Line 2", "Town/City", "County", "Post Code", "Scheme Year", "Risk Score", "Active"));

                    foreach (var item in plantList.Where(x => x.SCC))
                    {

                        //Get PCG Scheme ID
                        Guid schemeId = db.Scheme.AsNoTracking().Where(x => x.Name == "SCC").Select(x => x.SchemeId).FirstOrDefault();

                        //Get Scheme Year for this Plant
                        PlantOption schemeYr = item.PlantOptions.Where(x => x.SchemeId == schemeId && x.Option.Criteria.Name == "Scheme Year").FirstOrDefault();

                        //Get Risk Score for this Plant
                        int riskScore = item.RiskScore.Where(x => x.SchemeId == schemeId).Select(x => x.Score).FirstOrDefault();

                        //Write required Data
                        writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\"", item.LicenceNo ?? "", item.PlantName ?? "", item.AddressOne ?? "", item.AddressTwo ?? "", item.TownCity ?? "", item.County ?? "", item.PostCode ?? "", (schemeYr == null) ? "" : schemeYr.Option.Name ?? "", riskScore, item.Active));

                    }


                    break;

                case "14": //"SCC - Download List of All Plants (Active Plants Only)"

                    plantList = db.Plant.AsNoTracking().Where(x => x.SCC).Include(x => x.PlantOptions).Include(x => x.RiskScore).ToList();

                    //Write Headers

                    writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\"", "Licence No", "Plant Name", "Address Line 1", "Address Line 2", "Town/City", "County", "Post Code", "Scheme Year", "Risk Score", "Active"));

                    foreach (var item in plantList.Where(x => x.SCC && x.Active))
                    {

                        //Get PCG Scheme ID
                        Guid schemeId = db.Scheme.AsNoTracking().Where(x => x.Name == "SCC").Select(x => x.SchemeId).FirstOrDefault();

                        //Get Scheme Year for this Plant
                        PlantOption schemeYr = item.PlantOptions.Where(x => x.SchemeId == schemeId && x.Option.Criteria.Name == "Scheme Year").FirstOrDefault();

                        //Get Risk Score for this Plant
                        int riskScore = item.RiskScore.Where(x => x.SchemeId == schemeId).Select(x => x.Score).FirstOrDefault();

                        //Write required Data
                        writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\"", item.LicenceNo ?? "", item.PlantName ?? "", item.AddressOne ?? "", item.AddressTwo ?? "", item.TownCity ?? "", item.County ?? "", item.PostCode ?? "", (schemeYr == null) ? "" : schemeYr.Option.Name ?? "", riskScore, item.Active));

                    }

                    break;

                case "15": //"SCC - Download List of All Plants and Data"

                    plantList = db.Plant.AsNoTracking().Where(x => x.SCC).Include(x => x.PlantOptions).Include(x => x.RiskScore).ToList();

                    //Write Headers

                    writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\",\"{16}\",\"{17}\",\"{18}\",\"{19}\",\"{20}\",\"{21}\",\"{22}\"", "Licence No", "Plant Name", "Address Line 1", "Address Line 2", "Town/City", "County", "Post Code", "Active", "Risk Score", "Scheme Year", "Throughput", "Operating hours", "Sign in prior to inspection", "Trimming", "Skin Removal", "Carcass dressing", "Head Removal", "Feet Removal", "Failed / Unsatisfactory", "Fails / unsatisfactory in previous 12 Months", "Near misses in previous 12 Months", "Enforcement Notices Issued in Last 12 Months", "Penalty Notices Issued in Last 12 Months"));

                    //Write Plant Details 

                    foreach (var item in plantList.Where(x => x.SCC))
                    {

                        //Get PCG Scheme ID
                        Guid schemeId = db.Scheme.AsNoTracking().Where(x => x.Name == "SCC").Select(x => x.SchemeId).FirstOrDefault();

                        //Get Scheme Year for this Plant
                        PlantOption schemeYr = item.PlantOptions.Where(x => x.SchemeId == schemeId && x.Option.Criteria.Name == "Scheme Year").FirstOrDefault();

                        //Get Risk Score for this Plant
                        int riskScore = item.RiskScore.Where(x => x.SchemeId == schemeId).Select(x => x.Score).FirstOrDefault();

                        //Get Last Inspection Date for this Plant
                        DateTime? LastInspectionDate = item.LastInspection.Where(x => x.SchemeId == schemeId).FirstOrDefault()?.Date;

                        writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\",\"{16}\",\"{17}\",\"{18}\",\"{19}\",\"{20}\",\"{21}\",\"{22}\"",

                        item.LicenceNo ?? "",
                        item.PlantName ?? "",
                        item.AddressOne ?? "",
                        item.AddressTwo ?? "",
                        item.TownCity ?? "",
                        item.County ?? "",
                        item.PostCode ?? "",
                        item.Active,
                        riskScore,
                        item.PlantOptions.Where(x => x.Option.Criteria.Name == "Scheme Year").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "Throughput").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "Operating hours").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "Sign in prior to inspection").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "Trimming").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "Skin Removal").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "Carcass dressing").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "Head Removal").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "Feet Removal").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "Failed / Unsatisfactory").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "How many fails / unsatisfactory in previous 12 Months").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "How many near misses in previous 12 Months").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "Number of Enforcement Notices Issued in Last 12 Months").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "Number of Penalty Notices Issued in Last 12 Months").FirstOrDefault()?.Option?.Name ?? ""
                        ));

                    }

                    break;

                case "16": //"SCC - Download List of All Plants and Data (Active Plants Only)"

                    plantList = db.Plant.AsNoTracking().Where(x => x.SCC).Include(x => x.PlantOptions).Include(x => x.RiskScore).ToList();

                    //Write Headers

                    writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\",\"{16}\",\"{17}\",\"{18}\",\"{19}\",\"{20}\",\"{21}\",\"{22}\"", "Licence No", "Plant Name", "Address Line 1", "Address Line 2", "Town/City", "County", "Post Code", "Active", "Risk Score", "Scheme Year", "Throughput", "Operating hours", "Sign in prior to inspection", "Trimming", "Skin Removal", "Carcass dressing", "Head Removal", "Feet Removal", "Failed / Unsatisfactory", "Fails / unsatisfactory in previous 12 Months", "Near misses in previous 12 Months", "Enforcement Notices Issued in Last 12 Months", "Penalty Notices Issued in Last 12 Months"));

                    //Write Plant Details 

                    foreach (var item in plantList.Where(x => x.SCC && x.Active))
                    {

                        //Get PCG Scheme ID
                        Guid schemeId = db.Scheme.AsNoTracking().Where(x => x.Name == "SCC").Select(x => x.SchemeId).FirstOrDefault();

                        //Get Scheme Year for this Plant
                        PlantOption schemeYr = item.PlantOptions.Where(x => x.SchemeId == schemeId && x.Option.Criteria.Name == "Scheme Year").FirstOrDefault();

                        //Get Risk Score for this Plant
                        int riskScore = item.RiskScore.Where(x => x.SchemeId == schemeId).Select(x => x.Score).FirstOrDefault();

                        //Get Last Inspection Date for this Plant
                        DateTime? LastInspectionDate = item.LastInspection.Where(x => x.SchemeId == schemeId).FirstOrDefault()?.Date;

                        writer.WriteLine(string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\",\"{6}\",\"{7}\",\"{8}\",\"{9}\",\"{10}\",\"{11}\",\"{12}\",\"{13}\",\"{14}\",\"{15}\",\"{16}\",\"{17}\",\"{18}\",\"{19}\",\"{20}\",\"{21}\",\"{22}\"",

                        item.LicenceNo ?? "",
                        item.PlantName ?? "",
                        item.AddressOne ?? "",
                        item.AddressTwo ?? "",
                        item.TownCity ?? "",
                        item.County ?? "",
                        item.PostCode ?? "",
                        item.Active,
                        riskScore,
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "Scheme Year").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "Throughput").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "Operating hours").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "Sign in prior to inspection").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "Trimming").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "Skin Removal").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "Carcass dressing").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "Head Removal").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "Feet Removal").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "Failed / Unsatisfactory").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "How many fails / unsatisfactory in previous 12 Months").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "How many near misses in previous 12 Months").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "Number of Enforcement Notices Issued in Last 12 Months").FirstOrDefault()?.Option?.Name ?? "",
                        item.PlantOptions.Where(x => x.Scheme.Name == "SCC" && x.Option.Criteria.Name == "Number of Penalty Notices Issued in Last 12 Months").FirstOrDefault()?.Option?.Name ?? ""
                        ));
                    }
                    break;

            }

            return writer.ToString();
        }



        List<ErrorLog> errorLog = new List<ErrorLog>();

        internal void AddToErrorLog(string LicNum, string scheme, string error)
        {
            ErrorLog newError = new ErrorLog
            {
                LicenceNo = LicNum,
                Scheme = scheme,
                Error = error
            };

            errorLog.Add(newError);
        }

        //Initial method run to Bulk Load data into the application
        public List<ErrorLog> RunAddList(HttpPostedFileBase bulkSource)
        {
            //set to run 3 times to cover the three tabs in the template

            

            for (int i = 1; i < 4; i++)
            {
                DataTable table = BulkData(bulkSource, i);

                //add each row's data to the database

                foreach (DataRow row in table.Rows)
                {
                    //int i = 2;

                    AddTableData(row, table.TableName);

                    db.SaveChanges();
                }
            }

            return errorLog;
        }


        //method to convert excel template data to DataTable we can use.
        private DataTable BulkData(HttpPostedFileBase bulkSource, int i)
        {
            string filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads", Path.GetFileName(bulkSource.FileName));
            bulkSource.SaveAs(filepath);

            DataTable BulkSource = new DataTable("BulkSource");

            bool firstRowIsHeader = true;

            FileInfo existingFile = new FileInfo(filepath);

            ExcelPackage ePack = new ExcelPackage(existingFile);
            ExcelWorksheet ws = ePack.Workbook.Worksheets[i];

            //Get the number of columns and the headers of each column

            for (int xx = 1; xx <= ws.Dimension.End.Column; xx++)
            {
                string data = "";
                if (ws.Cells[1, xx].Value != null)
                {
                    data = ws.Cells[1, xx].Value.ToString();

                    string columnName = firstRowIsHeader ? data : "column" + xx.ToString();
                    BulkSource.Columns.Add(columnName);
                }
            }


            int first = firstRowIsHeader ? 2 : 1;

            //Get the data in each row of the spreadsheet
                        

            for (int excelRow = first; excelRow <= ws.Dimension.End.Row; excelRow++)
            {
                DataRow rw = BulkSource.NewRow();
                //BulkSource.Rows.Add(rw);
                                
                for (int excelCol = 1; excelCol <= BulkSource.Columns.Count; excelCol++)
                {
                    string data = "";
                    if (ws.Cells[excelRow, excelCol].Value != null)
                    {
                        data = ws.Cells[excelRow, excelCol].Value.ToString();
                    }
                    rw[excelCol - 1] = data;
                }

                if(rw.ItemArray.Where(c => c != null && !c.Equals("")).ToArray().Length == 0)
                {
                    
                }
                else
                {
                    BulkSource.Rows.Add(rw);
                }
            }

            BulkSource.TableName = ws.Name;

            File.Delete(filepath);

            return BulkSource;

        }

        public void AddTableData(DataRow row, string tableName)
        {
            //check that there is data on the spreadsheet (checking there is a licence number)

            string plant = row[0].ToString();

            if (plant != "")
            {
                //get the plant to update from the database

                Plant plantToUpdate = db.Plant.Where(x => x.LicenceNo == plant).FirstOrDefault();

                Scheme currentScheme = db.Scheme.AsNoTracking().Where(x => x.Name == tableName).FirstOrDefault();

                if (plantToUpdate != null)
                {
                    ///////
                    //BCC//
                    ///////

                    if (currentScheme.Name == "BCC")
                    {

                        //Set BCC to true for the plant (may need moving to the end)

                        if (plantToUpdate.BCC == false)
                        {
                            plantToUpdate.BCC = true;
                            db.Entry(plantToUpdate).State = EntityState.Modified;
                        }



                        //////
                        //Edit the Scheme Year
                        //////

                        //Check for Existing Scheme Year Plant Option

                        PlantOption schYrOption = schemeService.GetCurrentPlantOption(plantToUpdate.PlantId, currentScheme.SchemeId, "Scheme Year");

                        //Convert data in dataRow to a string

                        string optionName1 = row[1].ToString();

                        //Obtain the option object from the database that matches is put in on spreadsheet

                        Option newSYOption = db.Option.AsNoTracking().Where(x => x.Name == optionName1 && x.Criteria.Scheme.SchemeId == currentScheme.SchemeId && x.Criteria.Name == "Scheme Year").FirstOrDefault();

                        //if a valid option has been entered on the spreadsheet do below    

                        if (newSYOption != null)
                        {
                            //create a plant option for the new option entered on the spreadsheet

                            PlantOption newschYrPlantOption = schemeService.CreateNewPlantOption(plantToUpdate.PlantId, currentScheme.SchemeId, newSYOption.OptionId);

                            //if a plant option didn't already exist for this category add to DB

                            if (schYrOption == null)
                            {

                                db.PlantOption.Add(newschYrPlantOption);

                            }

                            //if plant option already exists - edit it and then audit

                            else
                            {
                                if (schYrOption.OptionId != newSYOption.OptionId)
                                {
                                    auditService.Log(plantToUpdate.PlantId, UserHelper.CurrentUser(), currentScheme.Name, schYrOption.Option.Criteria.Name, schYrOption.Option.Name, newSYOption.Name);
                                    db.PlantOption.Remove(schYrOption);
                                    db.PlantOption.Add(newschYrPlantOption);
                                }
                            }
                        }
                        else
                        {
                            AddToErrorLog(row[0].ToString(), row.Table.TableName.ToString(), "Invalid Scheme Year Provided");
                        }

                        /////
                        //Edit the Date of Last Inspection
                        /////

                        //locate existing last inspection date in DB

                        LastInspection lastInspectionDate = db.LastInspection.Where(x => x.PlantId == plantToUpdate.PlantId && x.SchemeId == currentScheme.SchemeId).FirstOrDefault();

                        //create a new last inspection date using data from spreadsheet uploaded

                        DateTime dt;

                        if (DateTime.TryParseExact(row[2].ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                        {

                            LastInspection newLastInspectionDate = new LastInspection
                            {
                                PlantId = plantToUpdate.PlantId,
                                SchemeId = currentScheme.SchemeId,
                                Date = dt
                            };

                            //if last inspection date doesn't currently exist then add it

                            if (lastInspectionDate == null)
                            {
                                db.LastInspection.Add(newLastInspectionDate);

                            }

                            //if it exists then update it and add to audit

                            else
                            {
                                auditService.Log(plantToUpdate.PlantId, UserHelper.CurrentUser(), currentScheme.Name, "Last Inspection Date", lastInspectionDate.Date.ToShortDateString(), newLastInspectionDate.Date.ToShortDateString());
                                db.LastInspection.Remove(lastInspectionDate);
                                db.LastInspection.Add(newLastInspectionDate);
                            }
                        }
                        else
                        {
                            AddToErrorLog(row[0].ToString(), row.Table.TableName.ToString(), "Invalid Date Entered");
                        }

                        /////
                        //Edit Failed/Unsatisfactory
                        /////

                        //find existing in database

                        PlantOption failedVisitOption = schemeService.GetCurrentPlantOption(plantToUpdate.PlantId, currentScheme.SchemeId, "Failed/Unsatisfactory on Last Visit");

                        //convert data row data to string

                        string optionName2 = row[4].ToString();

                        //find the relevant option to match data entered in spreadsheet

                        Option newFVOption = db.Option.AsNoTracking().Where(x => x.Name == optionName2 && x.Criteria.Scheme.SchemeId == currentScheme.SchemeId && x.Criteria.Name == "Failed/Unsatisfactory on Last Visit").FirstOrDefault();

                        if (newFVOption != null)
                        {

                            //create a new plant option

                            PlantOption newFVPlantOption = schemeService.CreateNewPlantOption(plantToUpdate.PlantId, currentScheme.SchemeId, newFVOption.OptionId);

                            //if a plant option didn't already exist for this category add to DB

                            if (failedVisitOption == null)
                            {

                                db.PlantOption.Add(newFVPlantOption);

                            }

                            //if it exists then update in DB

                            else
                            {
                                if (failedVisitOption.OptionId != newFVOption.OptionId)
                                {
                                    auditService.Log(plantToUpdate.PlantId, UserHelper.CurrentUser(), currentScheme.Name, failedVisitOption.Option.Criteria.Name, failedVisitOption.Option.Name, newFVOption.Name);
                                    db.PlantOption.Attach(failedVisitOption);
                                    db.PlantOption.Remove(failedVisitOption);
                                    db.PlantOption.Add(newFVPlantOption);
                                }
                            }
                        }
                        else
                        {
                            AddToErrorLog(row[0].ToString(), row.Table.TableName.ToString(), "Invalid Failed/Unsatisfactory on Last Visit Option Provided");
                        }

                        /////
                        //Edit Total Near Misses
                        /////

                        //find existing in database
                        PlantOption nearMissesOption = schemeService.GetCurrentPlantOption(plantToUpdate.PlantId, currentScheme.SchemeId, "Number of Near Misses in Previous 3 Years");

                        //convert data row data to int

                        int num;

                        if (Int32.TryParse(row[5].ToString(), out num))
                        {
                            string stringNum = "";

                            //find out which option the int fits into

                            if (num == 0)
                            {
                                stringNum = "No Near Misses";
                            }
                            else if (num <= 10)
                            {
                                stringNum = num.ToString();
                            }
                            else
                            {
                                stringNum = "Over 10";
                            }

                            //find the relevant option to match data entered in spreadsheet

                            Option newNMOption = db.Option.AsNoTracking().Where(x => x.Name == stringNum && x.Criteria.Scheme.SchemeId == currentScheme.SchemeId && x.Criteria.Name == "Number of Near Misses in Previous 3 Years").FirstOrDefault();



                            if (newNMOption != null)
                            {

                                //create a new plant option
                                PlantOption newNMPlantOption = schemeService.CreateNewPlantOption(plantToUpdate.PlantId, currentScheme.SchemeId, newNMOption.OptionId);

                                //if a plant option didn't already exist for this category add to DB

                                if (nearMissesOption == null)
                                {

                                    db.PlantOption.Add(newNMPlantOption);

                                }
                                else
                                {
                                    //if it already exists in DB update it
                                    if (nearMissesOption.OptionId != newNMOption.OptionId)
                                    {
                                        auditService.Log(plantToUpdate.PlantId, UserHelper.CurrentUser(), currentScheme.Name, nearMissesOption.Option.Criteria.Name, nearMissesOption.Option.Name, newNMOption.Name);
                                        db.PlantOption.Remove(nearMissesOption);
                                        db.PlantOption.Add(newNMPlantOption);
                                    }
                                }
                            }
                        }
                        else
                        {
                            AddToErrorLog(row[0].ToString(), row.Table.TableName.ToString(), "Invalid Number of Near Misses in Previous 3 years Option Provided");
                        }

                        /////
                        //Edit Unsatisfactory/Fails
                        /////

                        //find existing in database
                        PlantOption FailedOption = schemeService.GetCurrentPlantOption(plantToUpdate.PlantId, currentScheme.SchemeId, "Number of Failed/Unsatisfactory in Previous 3 Years");

                        //convert data row data to int
                        int Fnum;

                        if (Int32.TryParse(row[6].ToString(), out Fnum))
                        {
                            //find out which option the int fits into
                            string stringFNum = "";

                            if (Fnum == 0)
                            {
                                stringFNum = "No Fails / Unsatisfactory";
                            }
                            else if (Fnum <= 10)
                            {
                                stringFNum = Fnum.ToString();
                            }
                            else
                            {
                                stringFNum = "Over 10";
                            }

                            //find the relevant option to match data entered in spreadsheet

                            Option newFailOption = db.Option.AsNoTracking().Where(x => x.Name == stringFNum && x.Criteria.Scheme.SchemeId == currentScheme.SchemeId && x.Criteria.Name == "Number of Failed/Unsatisfactory in Previous 3 Years").FirstOrDefault();

                            if (newFailOption != null)
                            {
                                //create a new plant option
                                PlantOption newFailPlantOption = schemeService.CreateNewPlantOption(plantToUpdate.PlantId, currentScheme.SchemeId, newFailOption.OptionId);

                                //if a plant option didn't already exist for this category add to DB

                                if (FailedOption == null)
                                {

                                    db.PlantOption.Add(newFailPlantOption);

                                }
                                //if it already exists in DB update it
                                else
                                {
                                    if (FailedOption.OptionId != newFailOption.OptionId)
                                    {
                                        auditService.Log(plantToUpdate.PlantId, UserHelper.CurrentUser(), currentScheme.Name, FailedOption.Option.Criteria.Name, FailedOption.Option.Name, newFailOption.Name);
                                        db.PlantOption.Remove(FailedOption);
                                        db.PlantOption.Add(newFailPlantOption);
                                    }
                                }
                            }
                        }
                        else
                        {
                            AddToErrorLog(row[0].ToString(), row.Table.TableName.ToString(), "Invalid Number of Failed/Unsatisfactory in Previous 3 years Option Provided");
                        }



                    }

                    ///////
                    //PCG//
                    ///////


                    if (currentScheme.Name == "PCG")
                    {

                        //Set PCG to true if not already on the plant

                        if (plantToUpdate.PCG == false)
                        {
                            plantToUpdate.PCG = true;

                            db.Entry(plantToUpdate).State = EntityState.Modified;
                        }

                        //////
                        //Edit the Scheme Year
                        //////

                        //Check for Existing Scheme Year Plant Option

                        PlantOption schYrOption = schemeService.GetCurrentPlantOption(plantToUpdate.PlantId, currentScheme.SchemeId, "Scheme Year");

                        //Convert data in dataRow to a string

                        string optionName3 = row[1].ToString();

                        //Obtain the option object from the database that matches is put in on spreadsheet

                        Option newSYOption = db.Option.AsNoTracking().Where(x => x.Name == optionName3 && x.Criteria.Scheme.SchemeId == currentScheme.SchemeId && x.Criteria.Name == "Scheme Year").FirstOrDefault();

                        //if a valid option has been entered on the spreadsheet do below    

                        if (newSYOption != null)
                        {
                            //create a plant option for the new option entered on the spreadsheet

                            PlantOption newschYrPlantOption = schemeService.CreateNewPlantOption(plantToUpdate.PlantId, currentScheme.SchemeId, newSYOption.OptionId);

                            //if a plant option didn't already exist for this category add to DB

                            if (schYrOption == null)
                            {

                                db.PlantOption.Add(newschYrPlantOption);

                            }

                            //if plant option already exists - edit it and then audit

                            else
                            {
                                if (schYrOption.OptionId != newSYOption.OptionId)
                                {
                                    auditService.Log(plantToUpdate.PlantId, UserHelper.CurrentUser(), currentScheme.Name, schYrOption.Option.Criteria.Name, schYrOption.Option.Name, newSYOption.Name);
                                    db.PlantOption.Remove(schYrOption);
                                    db.PlantOption.Add(newschYrPlantOption);
                                }
                            }
                        }
                        else
                        {
                            AddToErrorLog(row[0].ToString(), row.Table.TableName.ToString(), "Invalid Scheme Year Provided");
                        }
                        /////
                        //Edit the Date of Last Inspection
                        /////

                        //locate existing last inspection date in DB

                        LastInspection lastInspectionDate = db.LastInspection.Where(x => x.PlantId == plantToUpdate.PlantId && x.SchemeId == currentScheme.SchemeId).FirstOrDefault();

                        //create a new last inspection date using data from spreadsheet uploaded

                        DateTime dt;

                        if (DateTime.TryParseExact(row[2].ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                        {

                            LastInspection newLastInspectionDate = new LastInspection
                            {
                                PlantId = plantToUpdate.PlantId,
                                SchemeId = currentScheme.SchemeId,
                                Date = dt
                            };

                            //if last inspection date doesn't currently exist then add it

                            if (lastInspectionDate == null)
                            {
                                db.LastInspection.Add(newLastInspectionDate);

                            }

                            //if it exists then update it and add to audit

                            else
                            {
                                auditService.Log(plantToUpdate.PlantId, UserHelper.CurrentUser(), currentScheme.Name, "Last Inspection Date", lastInspectionDate.Date.ToShortDateString(), newLastInspectionDate.Date.ToShortDateString());
                                db.LastInspection.Remove(lastInspectionDate);
                                db.LastInspection.Add(newLastInspectionDate);
                            }
                        }
                        else
                        {
                            AddToErrorLog(row[0].ToString(), row.Table.TableName.ToString(), "Invalid Date Entered");
                        }


                        /////
                        //Edit Failed/Unsatisfactory
                        /////

                        //find existing in database

                        PlantOption failedVisitOption = schemeService.GetCurrentPlantOption(plantToUpdate.PlantId, currentScheme.SchemeId, "Failed/Unsatisfactory on Last Visit");

                        //convert data row data to string

                        string optionName4 = row[4].ToString();

                        //find the relevant option to match data entered in spreadsheet

                        Option newFVOption = db.Option.AsNoTracking().Where(x => x.Name == optionName4 && x.Criteria.Scheme.SchemeId == currentScheme.SchemeId && x.Criteria.Name == "Failed/Unsatisfactory on Last Visit").FirstOrDefault();

                        if (newFVOption != null)
                        {

                            //create a new plant option

                            PlantOption newFVPlantOption = schemeService.CreateNewPlantOption(plantToUpdate.PlantId, currentScheme.SchemeId, newFVOption.OptionId);

                            //if a plant option didn't already exist for this category add to DB

                            if (failedVisitOption == null)
                            {

                                db.PlantOption.Add(newFVPlantOption);

                            }

                            //if it exists then update in DB

                            else
                            {
                                if (failedVisitOption.OptionId != newFVOption.OptionId)
                                {
                                    auditService.Log(plantToUpdate.PlantId, UserHelper.CurrentUser(), currentScheme.Name, failedVisitOption.Option.Criteria.Name, failedVisitOption.Option.Name, newFVOption.Name);
                                    db.PlantOption.Remove(failedVisitOption);
                                    db.PlantOption.Add(newFVPlantOption);
                                }
                            }
                        }
                       else
                        {
                            AddToErrorLog(row[0].ToString(), row.Table.TableName.ToString(), "Invalid Failed/Unsatisfactory on Last Visit Option Provided");
                        }

                        /////
                        //Edit Total Near Misses
                        /////

                        //find existing in database
                        PlantOption nearMissesOption = schemeService.GetCurrentPlantOption(plantToUpdate.PlantId, currentScheme.SchemeId, "Number of Near Misses in Previous 3 Years");

                        //convert data row data to int

                        int num;

                        if (Int32.TryParse(row[5].ToString(), out num))
                        {
                            string stringNum = "";

                            //find out which option the int fits into

                            if (num == 0)
                            {
                                stringNum = "No Near Misses";
                            }
                            else if (num <= 10)
                            {
                                stringNum = num.ToString();
                            }
                            else
                            {
                                stringNum = "Over 10";
                            }

                            //find the relevant option to match data entered in spreadsheet

                            Option newNMOption = db.Option.AsNoTracking().Where(x => x.Name == stringNum && x.Criteria.Scheme.SchemeId == currentScheme.SchemeId && x.Criteria.Name == "Number of Near Misses in Previous 3 Years").FirstOrDefault();

                            //if a valid option has been entered on the spreadsheet do below

                            if (newNMOption != null)
                            {

                                //create a new plant option
                                PlantOption newNMPlantOption = schemeService.CreateNewPlantOption(plantToUpdate.PlantId, currentScheme.SchemeId, newNMOption.OptionId);

                                //if a plant option didn't already exist for this category add to DB

                                if (nearMissesOption == null)
                                {

                                    db.PlantOption.Add(newNMPlantOption);

                                }
                                else
                                {
                                    //if it already exists in DB update it
                                    if (nearMissesOption.OptionId != newNMOption.OptionId)
                                    {
                                        auditService.Log(plantToUpdate.PlantId, UserHelper.CurrentUser(), currentScheme.Name, nearMissesOption.Option.Criteria.Name, nearMissesOption.Option.Name, newNMOption.Name);
                                        db.PlantOption.Remove(nearMissesOption);
                                        db.PlantOption.Add(newNMPlantOption);
                                    }
                                }
                            }
                        }
                        else
                        {
                            AddToErrorLog(row[0].ToString(), row.Table.TableName.ToString(), "Invalid Number of Near Misses in Previous 3 years Option Provided");
                        }

                        /////
                        //Edit Unsatisfactory/Fails
                        /////

                        //find existing in database
                        PlantOption FailedOption = schemeService.GetCurrentPlantOption(plantToUpdate.PlantId, currentScheme.SchemeId, "Number of Failed/Unsatisfactory in Previous 3 Years");

                        //convert data row data to int
                        int Fnum;

                        if (Int32.TryParse(row[6].ToString(), out Fnum))
                        {

                            //find out which option the int fits into
                            string stringFNum = "";

                            if (Fnum == 0)
                            {
                                stringFNum = "No Fails / Unsatisfactory";
                            }
                            else if (Fnum <= 10)
                            {
                                stringFNum = Fnum.ToString();
                            }
                            else
                            {
                                stringFNum = "Over 10";
                            }

                            //find the relevant option to match data entered in spreadsheet

                            Option newFailOption = db.Option.AsNoTracking().Where(x => x.Name == stringFNum && x.Criteria.Scheme.SchemeId == currentScheme.SchemeId && x.Criteria.Name == "Number of Failed/Unsatisfactory in Previous 3 Years").FirstOrDefault();

                            if (newFailOption != null)
                            {
                                //create a new plant option
                                PlantOption newFailPlantOption = schemeService.CreateNewPlantOption(plantToUpdate.PlantId, currentScheme.SchemeId, newFailOption.OptionId);

                                //if a plant option didn't already exist for this category add to DB

                                if (FailedOption == null)
                                {

                                    db.PlantOption.Add(newFailPlantOption);

                                }
                                //if it already exists in DB update it
                                else
                                {
                                    if (FailedOption.OptionId != newFailOption.OptionId)
                                    {
                                        auditService.Log(plantToUpdate.PlantId, UserHelper.CurrentUser(), currentScheme.Name, FailedOption.Option.Criteria.Name, FailedOption.Option.Name, newFailOption.Name);
                                        db.PlantOption.Remove(FailedOption);
                                        db.PlantOption.Add(newFailPlantOption);
                                    }
                                }
                            }
                        }
                        else
                        {
                            AddToErrorLog(row[0].ToString(), row.Table.TableName.ToString(), "Invalid Number of Failed/Unsatisfactory in Previous 3 years Option Provided");
                        }



                    }

                    ///////
                    //BLS//
                    ///////

                    if (currentScheme.Name == "BLS")
                    {

                        if (plantToUpdate.BLS == false)
                        {
                            plantToUpdate.BLS = true;
                            db.Entry(plantToUpdate).State = EntityState.Modified;
                        }

                        //////
                        //Edit the Scheme Year
                        //////

                        //Check for Existing Scheme Year Plant Option

                        PlantOption schYrOption = schemeService.GetCurrentPlantOption(plantToUpdate.PlantId, currentScheme.SchemeId, "Scheme Year");

                        //Convert data in dataRow to a string

                        string optionName5 = row[1].ToString();

                        //Obtain the option object from the database that matches is put in on spreadsheet

                        Option newSYOption = db.Option.AsNoTracking().Where(x => x.Name == optionName5 && x.Criteria.Scheme.SchemeId == currentScheme.SchemeId && x.Criteria.Name == "Scheme Year").FirstOrDefault();

                        //if a valid option has been entered on the spreadsheet do below    

                        if (newSYOption != null)
                        {
                            //create a plant option for the new option entered on the spreadsheet

                            PlantOption newschYrPlantOption = schemeService.CreateNewPlantOption(plantToUpdate.PlantId, currentScheme.SchemeId, newSYOption.OptionId);

                            //if a plant option didn't already exist for this category add to DB

                            if (schYrOption == null)
                            {

                                db.PlantOption.Add(newschYrPlantOption);

                            }

                            //if plant option already exists - edit it and then audit

                            else
                            {
                                if (schYrOption.OptionId != newSYOption.OptionId)
                                {
                                    auditService.Log(plantToUpdate.PlantId, UserHelper.CurrentUser(), currentScheme.Name, schYrOption.Option.Criteria.Name, schYrOption.Option.Name, newSYOption.Name);
                                    db.PlantOption.Remove(schYrOption);
                                    db.PlantOption.Add(newschYrPlantOption);
                                }
                            }
                        }
                        else
                        {
                            AddToErrorLog(row[0].ToString(), row.Table.TableName.ToString(), "Invalid Scheme Year");
                        }

                        /////
                        //Edit the Date of Last Inspection
                        /////

                        //locate existing last inspection date in DB

                        LastInspection lastInspectionDate = db.LastInspection.Where(x => x.PlantId == plantToUpdate.PlantId && x.SchemeId == currentScheme.SchemeId).FirstOrDefault();

                        //create a new last inspection date using data from spreadsheet uploaded

                        DateTime dt;

                        if (DateTime.TryParseExact(row[2].ToString(), "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                        {

                            LastInspection newLastInspectionDate = new LastInspection
                            {
                                PlantId = plantToUpdate.PlantId,
                                SchemeId = currentScheme.SchemeId,
                                Date = DateTime.Parse(row[2].ToString())
                            };

                            //if last inspection date doesn't currently exist then add it

                            if (lastInspectionDate == null)
                            {
                                db.LastInspection.Add(newLastInspectionDate);

                            }

                            //if it exists then update it and add to audit

                            else
                            {
                                auditService.Log(plantToUpdate.PlantId, UserHelper.CurrentUser(), currentScheme.Name, "Last Inspection Date", lastInspectionDate.Date.ToShortDateString(), newLastInspectionDate.Date.ToShortDateString());
                                db.LastInspection.Remove(lastInspectionDate);
                                db.LastInspection.Add(newLastInspectionDate);
                            }
                        }
                        else
                        {
                            AddToErrorLog(row[0].ToString(), row.Table.TableName.ToString(), "Invalid Date Entered");
                        }

                        /////
                        //Edit Number of Inspections Resulting in Failure
                        /////

                        //find existing in database

                        PlantOption numInspFailOption = schemeService.GetCurrentPlantOption(plantToUpdate.PlantId, currentScheme.SchemeId, "Number of Inspections Resulting in Failure");

                        //convert data row data to string

                        string optionName6 = row[3].ToString();

                        //find the relevant option to match data entered in spreadsheet

                        Option newNumInspFailOption = db.Option.AsNoTracking().Where(x => x.Name == optionName6 && x.Criteria.Scheme.SchemeId == currentScheme.SchemeId && x.Criteria.Name == "Number of Inspections Resulting in Failure").FirstOrDefault();

                        if (newNumInspFailOption != null)
                        {

                            //create a new plant option

                            PlantOption newNumInspFailPlantOption = schemeService.CreateNewPlantOption(plantToUpdate.PlantId, currentScheme.SchemeId, newNumInspFailOption.OptionId);

                            //if a plant option didn't already exist for this category add to DB

                            if (numInspFailOption == null)
                            {

                                db.PlantOption.Add(newNumInspFailPlantOption);

                            }

                            //if it exists then update in DB

                            else
                            {
                                if (numInspFailOption.OptionId != newNumInspFailOption.OptionId)
                                {
                                    auditService.Log(plantToUpdate.PlantId, UserHelper.CurrentUser(), currentScheme.Name, numInspFailOption.Option.Criteria.Name, numInspFailOption.Option.Name, newNumInspFailOption.Name);
                                    db.PlantOption.Remove(numInspFailOption);
                                    db.PlantOption.Add(newNumInspFailPlantOption);
                                }
                            }
                        }
                        else
                        {
                            AddToErrorLog(row[0].ToString(), row.Table.TableName.ToString(), "Invalid Number of Inspections Resulting in Failure Option Entered");
                        }

                        /////
                        //Edit Number of Non-Compliance
                        /////

                        //find existing in database
                        PlantOption numNonCompOption = schemeService.GetCurrentPlantOption(plantToUpdate.PlantId, currentScheme.SchemeId, "Number of Non-Compliance");

                        //convert data row data to string

                        string optionName7 = row[4].ToString();

                        //find the relevant option to match data entered in spreadsheet

                        Option newNumNonCompOption = db.Option.AsNoTracking().Where(x => x.Name == optionName7 && x.Criteria.Scheme.SchemeId == currentScheme.SchemeId && x.Criteria.Name == "Number of Non-Compliance").FirstOrDefault();

                        if (newNumNonCompOption != null)
                        {

                            //create a new plant option
                            PlantOption newNumNonCompPlantOption = schemeService.CreateNewPlantOption(plantToUpdate.PlantId, currentScheme.SchemeId, newNumNonCompOption.OptionId);

                            //if a plant option didn't already exist for this category add to DB

                            if (numNonCompOption == null)
                            {

                                db.PlantOption.Add(newNumNonCompPlantOption);

                            }
                            else
                            {
                                //if it already exists in DB update it
                                if (numNonCompOption.OptionId != newNumNonCompOption.OptionId)
                                {
                                    auditService.Log(plantToUpdate.PlantId, UserHelper.CurrentUser(), currentScheme.Name, numNonCompOption.Option.Criteria.Name, numNonCompOption.Option.Name, newNumNonCompOption.Name);
                                    db.PlantOption.Remove(numNonCompOption);
                                    db.PlantOption.Add(newNumNonCompPlantOption);
                                }
                            }
                        }
                        else
                        {
                            AddToErrorLog(row[0].ToString(), row.Table.TableName.ToString(), "Invalid Number of Non-Compliance Option Entered");
                        }

                        /////
                        //Edit Severity of Non-Compliance
                        /////

                        //find existing in database
                        PlantOption sevNonCompOption = schemeService.GetCurrentPlantOption(plantToUpdate.PlantId, currentScheme.SchemeId, "Severity of Non-Compliance");

                        //convert data row data to string

                        string optionName8 = row[5].ToString();

                        //find the relevant option to match data entered in spreadsheet

                        Option newSevNonCompOption = db.Option.AsNoTracking().Where(x => x.Name == optionName8 && x.Criteria.Scheme.SchemeId == currentScheme.SchemeId && x.Criteria.Name == "Severity of Non-Compliance").FirstOrDefault();

                        if (newSevNonCompOption != null)
                        {
                            //create a new plant option
                            PlantOption newSevNonCompPlantOption = schemeService.CreateNewPlantOption(plantToUpdate.PlantId, currentScheme.SchemeId, newSevNonCompOption.OptionId);

                            //if a plant option didn't already exist for this category add to DB

                            if (sevNonCompOption == null)
                            {

                                db.PlantOption.Add(newSevNonCompPlantOption);

                            }
                            //if it already exists in DB update it
                            else
                            {
                                if (sevNonCompOption.OptionId != newSevNonCompOption.OptionId)
                                {
                                    auditService.Log(plantToUpdate.PlantId, UserHelper.CurrentUser(), currentScheme.Name, sevNonCompOption.Option.Criteria.Name, sevNonCompOption.Option.Name, newSevNonCompOption.Name);
                                    db.PlantOption.Remove(sevNonCompOption);
                                    db.PlantOption.Add(newSevNonCompPlantOption);
                                }
                            }


                        }
                        else
                        {
                            AddToErrorLog(row[0].ToString(), row.Table.TableName.ToString(), "Invalid Severity of Non-Compliance Option Entered");
                        }


                    }

                }
                else
                {
                    AddToErrorLog(row[0].ToString(), row.Table.TableName.ToString(), "Plant Not Found - Please Add Plant and Try Again");

                }
            }

            else
            {
                AddToErrorLog(row[0].ToString(), row.Table.TableName.ToString(), "No Licence Number Entered");

            }
        }
    }


}


