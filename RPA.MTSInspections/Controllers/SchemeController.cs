using EF.Audit;
using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Helpers;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.SL;
using RPA.MTSInspections.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RPA.MTSInspections.Controllers
{
    public class SchemeController : Controller
    {

        MTSInspectionsContext db;
        IAuditService auditService;
        ISchemeService schemeService;

        public SchemeController()
        {
            db = new MTSInspectionsContext();
            schemeService = new SchemeService(db);
            auditService = new AuditService(db);
        }

        public SchemeController(MTSInspectionsContext context, ISchemeService schemeService, IAuditService auditService)
        {
            db = context;
            this.schemeService = schemeService;
            this.auditService = auditService;
        }

        // GET: Get Relevant Partial View
        public ActionResult _SchemeEditPartial(Guid PlantId, String scheme)
        {

            Scheme currentScheme = db.Scheme.Where(x => x.Name == scheme).FirstOrDefault();

            List<PlantOption> plantOptionsList = schemeService.GetPlantOptions(PlantId, currentScheme.SchemeId);


            if (currentScheme.Name == "BLS")
            {
                BLSSchemeView BLSschemeView = new BLSSchemeView
                {
                    PlantOptions = plantOptionsList,
                    LastInspection = schemeService.GetInspectionDate(PlantId, currentScheme.SchemeId),

                    //Populate options for NOP checkboxes CHECKBOX ONLY

                    OptionANoSplit = schemeService.GetCheckOption(PlantId, currentScheme.SchemeId, "no splitting/relabelling (no cutting)"),
                    OptionBSplit = schemeService.GetCheckOption(PlantId, currentScheme.SchemeId, "splitting and/or relabelling (no cutting)"),
                    OptionCCatering = schemeService.GetCheckOption(PlantId, currentScheme.SchemeId, "Catering"),
                    OptionDRetail = schemeService.GetCheckOption(PlantId, currentScheme.SchemeId, "For retail sale"),
                    OptionESomeCut = schemeService.GetCheckOption(PlantId, currentScheme.SchemeId, "some cutting and relabelling"),
                    OptionFWholesale = schemeService.GetCheckOption(PlantId, currentScheme.SchemeId, "Wholesale"),
                    OptionGCutting = schemeService.GetCheckOption(PlantId, currentScheme.SchemeId, "Cutting to primal stage only"),
                    OptionHSlaughter = schemeService.GetCheckOption(PlantId, currentScheme.SchemeId, "Slaughter for 3rd party only")

                };

                ViewBag.PlantId = PlantId;
                ViewBag.SchemeName = currentScheme.Name;
                ViewBag.Throughput = schemeService.GetDropDownOptions("Throughput", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Throughput").FirstOrDefault());
                ViewBag.NumofNonComp = schemeService.GetDropDownOptions("Number of Non-Compliance", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Number of Non-Compliance").FirstOrDefault());
                ViewBag.OperatingHours = schemeService.GetDropDownOptions("Operating Hours", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Operating Hours").FirstOrDefault());
                ViewBag.PremisesType = schemeService.GetDropDownOptions("Premises Type", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Premises Type").FirstOrDefault());
                ViewBag.TypeOfOperation = schemeService.GetDropDownOptions("Type of Operation", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Type of Operation").FirstOrDefault());
                ViewBag.SevofNonComp = schemeService.GetDropDownOptions("Severity of Non-Compliance", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Severity of Non-Compliance").FirstOrDefault());
                ViewBag.NumofInspFail = schemeService.GetDropDownOptions("Number of Inspections Resulting in Failure", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Number of Inspections Resulting in Failure").FirstOrDefault());
                ViewBag.NumYrsSinceInsp = schemeService.GetDropDownOptions("Number of Months Since Last Inspection", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Number of Months Since Last Inspection").FirstOrDefault());
                ViewBag.OriginofProducts = schemeService.GetDropDownOptions("Origin of Products Handled", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Origin of Products Handled").FirstOrDefault());
                ViewBag.SchemeYear = schemeService.GetDropDownOptions("Scheme Year", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Scheme Year").FirstOrDefault());

                return PartialView("_BLSViewPartial", BLSschemeView);
            }

            else if (currentScheme.Name == "BCC")
            {
                BCCSchemeView BCCschemeView = new BCCSchemeView
                {
                    PlantOptions = plantOptionsList,
                    LastInspection = schemeService.GetInspectionDate(PlantId, currentScheme.SchemeId),
                };

                ViewBag.PlantId = PlantId;
                ViewBag.SchemeName = currentScheme.Name;
                ViewBag.Throughput = schemeService.GetDropDownOptions("Throughput", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Throughput").FirstOrDefault());
                ViewBag.OperatingHours = schemeService.GetDropDownOptions("Operating Hours", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Operating Hours").FirstOrDefault());
                ViewBag.SignIn = schemeService.GetDropDownOptions("Pre Inspection Sign In", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Pre Inspection Sign In").FirstOrDefault());
                ViewBag.RecordKeeping = schemeService.GetDropDownOptions("Record Keeping", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Record Keeping").FirstOrDefault());
                ViewBag.LineClearing = schemeService.GetDropDownOptions("Line Clearing", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Line Clearing").FirstOrDefault());
                ViewBag.CarcassHang = schemeService.GetDropDownOptions("Carcass Hanging Method", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Carcass Hanging Method").FirstOrDefault());
                ViewBag.Trimming = schemeService.GetDropDownOptions("Trimming", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Trimming").FirstOrDefault());
                ViewBag.HidePuller = schemeService.GetDropDownOptions("Hide Puller", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Hide Puller").FirstOrDefault());
                ViewBag.CarcassDress = schemeService.GetDropDownOptions("Carcass Dressing", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Carcass Dressing").FirstOrDefault());
                ViewBag.NumofWeeks = schemeService.GetDropDownOptions("Number of Weeks Since Last Inspection", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Number of Weeks Since Last Inspection").FirstOrDefault());
                ViewBag.FailedVisit = schemeService.GetDropDownOptions("Failed/Unsatisfactory on Last Visit", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Failed/Unsatisfactory on Last Visit").FirstOrDefault());
                ViewBag.FailedLastThree = schemeService.GetDropDownOptions("Number of Failed/Unsatisfactory in Previous 3 Years", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Number of Failed/Unsatisfactory in Previous 3 Years").FirstOrDefault());
                ViewBag.MissLastThree = schemeService.GetDropDownOptions("Number of Near Misses in Previous 3 Years", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Number of Near Misses in Previous 3 Years").FirstOrDefault());
                ViewBag.SchemeYear = schemeService.GetDropDownOptions("Scheme Year", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Scheme Year").FirstOrDefault());

                return PartialView("_BCCViewPartial", BCCschemeView);
            }
            else if (currentScheme.Name == "PCG")
            {
                PCGSchemeView PCGschemeView = new PCGSchemeView
                {
                    PlantOptions = plantOptionsList,
                    LastInspection = schemeService.GetInspectionDate(PlantId, currentScheme.SchemeId),
                };

                ViewBag.PlantId = PlantId;
                ViewBag.SchemeName = currentScheme.Name;
                ViewBag.Throughput = schemeService.GetDropDownOptions("Throughput", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Throughput").FirstOrDefault());
                ViewBag.OperatingHours = schemeService.GetDropDownOptions("Operating Hours", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Operating Hours").FirstOrDefault());
                ViewBag.SignIn = schemeService.GetDropDownOptions("Pre Inspection Sign In", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Pre Inspection Sign In").FirstOrDefault());
                ViewBag.RecordKeeping = schemeService.GetDropDownOptions("Record Keeping", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Record Keeping").FirstOrDefault());
                ViewBag.LineClearing = schemeService.GetDropDownOptions("Line Clearing", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Line Clearing").FirstOrDefault());
                ViewBag.Probe = schemeService.GetDropDownOptions("Probe", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Probe").FirstOrDefault());
                ViewBag.Trimming = schemeService.GetDropDownOptions("Trimming", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Trimming").FirstOrDefault());
                ViewBag.CarcassDress = schemeService.GetDropDownOptions("Carcass Dressing", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Carcass Dressing").FirstOrDefault());
                ViewBag.NumofWeeks = schemeService.GetDropDownOptions("Number of Weeks Since Last Inspection", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Number of Weeks Since Last Inspection").FirstOrDefault());
                ViewBag.FailedVisit = schemeService.GetDropDownOptions("Failed/Unsatisfactory on Last Visit", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Failed/Unsatisfactory on Last Visit").FirstOrDefault());
                ViewBag.FailedLastThree = schemeService.GetDropDownOptions("Number of Failed/Unsatisfactory in Previous 3 Years", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Number of Failed/Unsatisfactory in Previous 3 Years").FirstOrDefault());
                ViewBag.MissLastThree = schemeService.GetDropDownOptions("Number of Near Misses in Previous 3 Years", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Number of Near Misses in Previous 3 Years").FirstOrDefault());
                ViewBag.SchemeYear = schemeService.GetDropDownOptions("Scheme Year", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Scheme Year").FirstOrDefault());

                return PartialView("_PCGViewPartial", PCGschemeView);
            }
            else if (currentScheme.Name == "SCC")
            {
                SCCSchemeView SCCschemeView;
                
                // Check if we have preserved form data from a failed submission
                if (TempData.Peek("SCCSchemeView") != null)
                {
                    // Use the preserved data that contains the user's input
                    SCCschemeView = (SCCSchemeView)TempData["SCCSchemeView"];
                    // Keep the existing PlantOptions and LastInspection data
                    SCCschemeView.PlantOptions = plantOptionsList;
                    SCCschemeView.LastInspection = schemeService.GetInspectionDate(PlantId, currentScheme.SchemeId);
                }
                else
                {
                    // Normal flow - create fresh model
                    SCCschemeView = new SCCSchemeView
                    {
                        PlantOptions = plantOptionsList,
                        LastInspection = schemeService.GetInspectionDate(PlantId, currentScheme.SchemeId),
                    };
                }

                ViewBag.PlantId = PlantId;
                ViewBag.SchemeName = currentScheme.Name;
                ViewBag.Throughput = schemeService.GetDropDownOptions("Throughput", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Throughput").FirstOrDefault());
                ViewBag.OperatingHours = schemeService.GetDropDownOptions("Operating Hours", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Operating Hours").FirstOrDefault());
                ViewBag.SignIn = schemeService.GetDropDownOptions("Sign in prior to inspection", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Sign in prior to inspection").FirstOrDefault());
                ViewBag.Trimming = schemeService.GetDropDownOptions("Trimming", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Trimming").FirstOrDefault());
                ViewBag.SkinRemoval = schemeService.GetDropDownOptions("Skin Removal", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Skin Removal").FirstOrDefault());
                ViewBag.CarcassDressing = schemeService.GetDropDownOptions("Carcass dressing", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Carcass dressing").FirstOrDefault());
                ViewBag.HeadRemoval = schemeService.GetDropDownOptions("Head Removal", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Head Removal").FirstOrDefault());
                ViewBag.FeetRemoval = schemeService.GetDropDownOptions("Feet Removal", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Feet Removal").FirstOrDefault());
                ViewBag.SchemeYear = schemeService.GetDropDownOptions("Scheme Year", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Scheme Year").FirstOrDefault());
                ViewBag.FailedUnsatisfactory = schemeService.GetDropDownOptions("Failed / Unsatisfactory", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Failed / Unsatisfactory").FirstOrDefault());
                ViewBag.FailedUnsatisfactoryInLast12Months = schemeService.GetDropDownOptions("How many fails / unsatisfactory in previous 12 Months", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "How many fails / unsatisfactory in previous 12 Months").FirstOrDefault());
                ViewBag.NearMissesInLast12Months = schemeService.GetDropDownOptions("How many near misses in previous 12 Months", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "How many near misses in previous 12 Months").FirstOrDefault());
                ViewBag.EnforcementNoticesIssuedLast12Months = schemeService.GetDropDownOptions("Number of Enforcement Notices Issued in Last 12 Months", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Number of Enforcement Notices Issued in Last 12 Months").FirstOrDefault());
                ViewBag.NumberPenaltyNoticesIssuedInLast12Months = schemeService.GetDropDownOptions("Number of Penalty Notices Issued in Last 12 Months", currentScheme.SchemeId, db.PlantOption.Where(x => x.SchemeId == currentScheme.SchemeId && x.PlantId == PlantId && x.Option.Criteria.Name == "Number of Penalty Notices Issued in Last 12 Months").FirstOrDefault());

                return PartialView("_SCCViewPartial", SCCschemeView);
            }

            else
            {
                return RedirectToAction("Index", "Plant", new { area = "" });
            }
        }


        // POST: post action for update to BLS Scheme
        public ActionResult BLSSchemePost(BLSSchemeView bLSSchemeView, string Plantid)
        {
            //Setup
            var errorList = new List<string>();

            Scheme currentScheme = db.Scheme.AsNoTracking().Where(x => x.Name == "BLS").FirstOrDefault();

            Guid plantID = Guid.Parse(Plantid);

            Plant currentPlant = db.Plant.Find(plantID);

            String currentUser = UserHelper.CurrentUser();

            //Throughput Update

            PlantOption throughputOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Throughput");

            PlantOption newThru = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bLSSchemeView.Throughput);

            if (throughputOption == null)
            {
                if (newThru.Option == null)
                {
                    errorList.Add("Throughput is required");
                }
                else
                {
                    db.PlantOption.Add(newThru);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newThru.Option.Criteria.Name, "", newThru.Option.Name);
                }

            }
            else
            {
                if (throughputOption.OptionId != bLSSchemeView.Throughput)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, throughputOption.Option.Criteria.Name, throughputOption.Option.Name, newThru.Option.Name);
                    db.PlantOption.Remove(throughputOption);
                    db.PlantOption.Add(newThru);
                }
            }

            //Number of Non-Compliance Update

            PlantOption numberofNonOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Number of Non-Compliance");

            PlantOption newNumberofNon = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bLSSchemeView.NumofNonComp);


            if (numberofNonOption == null)
            {
                if (newNumberofNon.Option == null)
                {
                    //CAN BE NULL
                    newNumberofNon = numberofNonOption;
                }
                else
                {
                    db.PlantOption.Add(newNumberofNon);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newNumberofNon.Option.Criteria.Name, "", newNumberofNon.Option.Name);
                }
            }
            else
            {
                if (numberofNonOption.OptionId != bLSSchemeView.NumofNonComp)
                {
                    if (newNumberofNon.Option != null)
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, numberofNonOption.Option.Criteria.Name, numberofNonOption.Option.Name, newNumberofNon.Option.Name);
                        db.PlantOption.Remove(numberofNonOption);
                        db.PlantOption.Add(newNumberofNon);
                    }
                    else
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, numberofNonOption.Option.Criteria.Name, numberofNonOption.Option.Name, null);
                        db.PlantOption.Remove(numberofNonOption);
                    }
                }
            }

            //Operating Hours Update

            PlantOption opHoursOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Operating Hours");

            PlantOption newOpHours = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bLSSchemeView.OperatingHours);


            if (opHoursOption == null)
            {
                if (newOpHours.Option == null)
                {
                    errorList.Add("Operating Hours is required");
                }
                else
                {
                    db.PlantOption.Add(newOpHours);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newOpHours.Option.Criteria.Name, "", newOpHours.Option.Name);
                }
            }
            else
            {
                if (opHoursOption.OptionId != bLSSchemeView.OperatingHours)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, opHoursOption.Option.Criteria.Name, opHoursOption.Option.Name, newOpHours.Option.Name);
                    db.PlantOption.Remove(opHoursOption);
                    db.PlantOption.Add(newOpHours);
                }
            }


            //Premises Type Update

            PlantOption premTypeOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Premises Type");

            PlantOption newPremType = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bLSSchemeView.PremisesType);


            if (premTypeOption == null)
            {
                if (newPremType.Option == null)
                {
                    errorList.Add("Premises Type is required");
                }
                else
                {
                    db.PlantOption.Add(newPremType);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newPremType.Option.Criteria.Name, "", newPremType.Option.Name);
                }
            }
            else
            {
                if (premTypeOption.OptionId != bLSSchemeView.PremisesType)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, premTypeOption.Option.Criteria.Name, premTypeOption.Option.Name, newPremType.Option.Name);
                    db.PlantOption.Remove(premTypeOption);
                    db.PlantOption.Add(newPremType);
                }
            }


            //Type of Operation Update

            PlantOption typeofOpOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Type of Operation");

            PlantOption newTypeofOp = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bLSSchemeView.TypeOfOperation);


            if (typeofOpOption == null)
            {
                if (newTypeofOp.Option == null)
                {
                    errorList.Add("Type of Operation is required");
                }
                else
                {
                    db.PlantOption.Add(newTypeofOp);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newTypeofOp.Option.Criteria.Name, "", newTypeofOp.Option.Name);
                }
            }
            else
            {
                if (typeofOpOption.OptionId != bLSSchemeView.TypeOfOperation)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, typeofOpOption.Option.Criteria.Name, typeofOpOption.Option.Name, newTypeofOp.Option.Name);
                    db.PlantOption.Remove(typeofOpOption);
                    db.PlantOption.Add(newTypeofOp);
                }
            }


            //Severity of Non - Compliance Update

            PlantOption sevofCompOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Severity of Non-Compliance");

            PlantOption newSevofComp = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bLSSchemeView.SevofNonComp);


            if (sevofCompOption == null)
            {
                if (newSevofComp.Option == null)
                {
                    //CAN BE NULL
                    newSevofComp = sevofCompOption;
                }
                else
                {
                    db.PlantOption.Add(newSevofComp);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newSevofComp.Option.Criteria.Name, "", newSevofComp.Option.Name);
                }
            }
            else
            {
                if (sevofCompOption.OptionId != bLSSchemeView.SevofNonComp)
                {
                    if (newSevofComp.Option != null)
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, sevofCompOption.Option.Criteria.Name, sevofCompOption.Option.Name, newSevofComp.Option.Name);
                        db.PlantOption.Remove(sevofCompOption);
                        db.PlantOption.Add(newSevofComp);
                    }
                    else
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, sevofCompOption.Option.Criteria.Name, sevofCompOption.Option.Name, null);
                        db.PlantOption.Remove(sevofCompOption);
                    }
                }
            }


            //Number of Inspections Resulting in Failure Update

            PlantOption inspFailOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Number of Inspections Resulting in Failure");

            PlantOption newinspFailOption = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bLSSchemeView.NumofInspFail);


            if (inspFailOption == null)
            {
                if (newinspFailOption.Option == null)
                {
                    //CAN BE NULL
                    newinspFailOption = inspFailOption;
                }
                else
                {
                    db.PlantOption.Add(newinspFailOption);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newinspFailOption.Option.Criteria.Name, "", newinspFailOption.Option.Name);
                }
            }
            else
            {
                if (inspFailOption.OptionId != bLSSchemeView.NumofInspFail)
                {
                    if (newinspFailOption.Option != null)
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, inspFailOption.Option.Criteria.Name, inspFailOption.Option.Name, newinspFailOption.Option.Name);
                        db.PlantOption.Remove(inspFailOption);
                        db.PlantOption.Add(newinspFailOption);
                    }
                    else
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, inspFailOption.Option.Criteria.Name, inspFailOption.Option.Name, null);
                        db.PlantOption.Remove(inspFailOption);
                    }
                }
            }

            //Number of Weeks Update

            PlantOption numWksOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Number of Months Since Last Inspection");

            PlantOption newnumwksOption = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bLSSchemeView.NumYrsSinceInsp);


            if (numWksOption == null)
            {
                if (newnumwksOption.Option == null)
                {
                    //CAN BE NULL
                    newnumwksOption = numWksOption;
                }
                else
                {
                    db.PlantOption.Add(newnumwksOption);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newnumwksOption.Option.Criteria.Name, "", newnumwksOption.Option.Name);
                }
            }
            else
            {
                if (numWksOption.OptionId != bLSSchemeView.NumYrsSinceInsp)
                {
                    if (newnumwksOption.Option != null)
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, numWksOption.Option.Criteria.Name, numWksOption.Option.Name, newnumwksOption.Option.Name);
                        db.PlantOption.Remove(numWksOption);
                        db.PlantOption.Add(newnumwksOption);
                    }
                    else
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, numWksOption.Option.Criteria.Name, numWksOption.Option.Name, null);
                        db.PlantOption.Remove(numWksOption);
                    }
                }
            }

            //Origin Update

            PlantOption originOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Origin of Products Handled");

            PlantOption newOriginOption = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bLSSchemeView.OriginofProducts);


            if (originOption == null)
            {
                if (newOriginOption.Option == null)
                {
                    errorList.Add("Origin of Products Handled is required");
                }
                else
                {
                    db.PlantOption.Add(newOriginOption);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newOriginOption.Option.Criteria.Name, "", newOriginOption.Option.Name);
                }
            }
            else
            {
                if (originOption.OptionId != bLSSchemeView.OriginofProducts)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, originOption.Option.Criteria.Name, originOption.Option.Name, newOriginOption.Option.Name);
                    db.PlantOption.Remove(originOption);
                    db.PlantOption.Add(newOriginOption);
                }
            }

            //Scheme Year Update

            PlantOption schYrOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Scheme Year");

            PlantOption newschYrOption = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bLSSchemeView.SchemeYear);


            if (schYrOption == null)
            {
                if (newschYrOption.Option == null)
                {
                    //CAN BE NULL
                    newschYrOption = schYrOption;
                }
                else
                {
                    db.PlantOption.Add(newschYrOption);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newschYrOption.Option.Criteria.Name, "", newschYrOption.Option.Name);
                }
            }
            else
            {
                if (schYrOption.OptionId != bLSSchemeView.SchemeYear)
                {
                    if (newschYrOption.Option != null)
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, schYrOption.Option.Criteria.Name, schYrOption.Option.Name, newschYrOption.Option.Name);
                        db.PlantOption.Remove(schYrOption);
                        db.PlantOption.Add(newschYrOption);
                    }
                    else
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, schYrOption.Option.Criteria.Name, schYrOption.Option.Name, null);
                        db.PlantOption.Remove(schYrOption);
                    }
                }
            }

            //Nature of Operation Update (CHECKBOX ONLY)

            //NoP Option A (no splitting/relabelling (no cutting))

            PlantOption nopA = schemeService.GetCurrentNoPOption(plantID, currentScheme.SchemeId, "no splitting/relabelling (no cutting)");

            PlantOption newNopA = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bLSSchemeView.OptionANoSplit.Option.OptionId);

            if (bLSSchemeView.OptionANoSplit.Selected == true)
            {
                if (nopA == null)
                {
                    db.PlantOption.Add(newNopA);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newNopA.Option.Criteria.Name, newNopA.Option.Name, "Checked");
                }
            }
            else
            {
                if (nopA != null)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, nopA.Option.Criteria.Name, nopA.Option.Name, "Unchecked");
                    db.PlantOption.Remove(nopA);
                }
            }


            //NoP Option B splitting and/or relabelling (no cutting))

            PlantOption nopB = schemeService.GetCurrentNoPOption(plantID, currentScheme.SchemeId, "splitting and/or relabelling (no cutting)");

            PlantOption newNopB = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bLSSchemeView.OptionBSplit.Option.OptionId);

            if (bLSSchemeView.OptionBSplit.Selected == true)
            {
                if (nopB == null)
                {
                    db.PlantOption.Add(newNopB);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newNopB.Option.Criteria.Name, newNopB.Option.Name, "Checked");
                }
            }
            else
            {
                if (nopB != null)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, nopB.Option.Criteria.Name, nopB.Option.Name, "Unchecked");
                    db.PlantOption.Remove(nopB);
                }
            }

            //NoP Option C Catering

            PlantOption nopC = schemeService.GetCurrentNoPOption(plantID, currentScheme.SchemeId, "Catering");

            PlantOption newNopC = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bLSSchemeView.OptionCCatering.Option.OptionId);

            if (bLSSchemeView.OptionCCatering.Selected == true)
            {
                if (nopC == null)
                {
                    db.PlantOption.Add(newNopC);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newNopC.Option.Criteria.Name, newNopC.Option.Name, "Checked");
                }
            }
            else
            {
                if (nopC != null)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, nopC.Option.Criteria.Name, nopC.Option.Name, "Unchecked");
                    db.PlantOption.Remove(nopC);
                }
            }


            //NoP Option D For retail sale

            PlantOption nopD = schemeService.GetCurrentNoPOption(plantID, currentScheme.SchemeId, "For retail sale");

            PlantOption newNopD = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bLSSchemeView.OptionDRetail.Option.OptionId);

            if (bLSSchemeView.OptionDRetail.Selected == true)
            {
                if (nopD == null)
                {
                    db.PlantOption.Add(newNopD);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newNopD.Option.Criteria.Name, newNopD.Option.Name, "Checked");
                }
            }
            else
            {
                if (nopD != null)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, nopD.Option.Criteria.Name, nopD.Option.Name, "Unchecked");
                    db.PlantOption.Remove(nopD);
                }
            }

            //NoP Option E some cutting and relabelling

            PlantOption nopE = schemeService.GetCurrentNoPOption(plantID, currentScheme.SchemeId, "some cutting and relabelling");

            PlantOption newNopE = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bLSSchemeView.OptionESomeCut.Option.OptionId);

            if (bLSSchemeView.OptionESomeCut.Selected == true)
            {
                if (nopE == null)
                {
                    db.PlantOption.Add(newNopE);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newNopE.Option.Criteria.Name, newNopE.Option.Name, "Checked");
                }
            }
            else
            {
                if (nopE != null)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, nopE.Option.Criteria.Name, nopE.Option.Name, "Unchecked");
                    db.PlantOption.Remove(nopE);
                }
            }

            //NoP Option F Wholesale

            PlantOption nopF = schemeService.GetCurrentNoPOption(plantID, currentScheme.SchemeId, "Wholesale");

            PlantOption newNopF = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bLSSchemeView.OptionFWholesale.Option.OptionId);

            if (bLSSchemeView.OptionFWholesale.Selected == true)
            {
                if (nopF == null)
                {
                    db.PlantOption.Add(newNopF);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newNopF.Option.Criteria.Name, newNopF.Option.Name, "Checked");
                }
            }
            else
            {
                if (nopF != null)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, nopF.Option.Criteria.Name, nopF.Option.Name, "Unchecked");
                    db.PlantOption.Remove(nopF);
                }
            }

            //NoP Option G Cutting to primal stage only

            PlantOption nopG = schemeService.GetCurrentNoPOption(plantID, currentScheme.SchemeId, "Cutting to primal stage only");

            PlantOption newNopG = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bLSSchemeView.OptionGCutting.Option.OptionId);

            if (bLSSchemeView.OptionGCutting.Selected == true)
            {
                if (nopG == null)
                {
                    db.PlantOption.Add(newNopG);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newNopG.Option.Criteria.Name, newNopG.Option.Name, "Checked");
                }
            }
            else
            {
                if (nopG != null)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, nopG.Option.Criteria.Name, nopG.Option.Name, "Unchecked");
                    db.PlantOption.Remove(nopG);
                }
            }

            //NoP Option H Slaughter for 3rd party only

            PlantOption nopH = schemeService.GetCurrentNoPOption(plantID, currentScheme.SchemeId, "Slaughter for 3rd party only");

            PlantOption newNopH = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bLSSchemeView.OptionHSlaughter.Option.OptionId);

            if (bLSSchemeView.OptionHSlaughter.Selected == true)
            {
                if (nopH == null)
                {
                    db.PlantOption.Add(newNopH);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newNopH.Option.Criteria.Name, newNopH.Option.Name, "Checked");
                }
            }
            else
            {
                if (nopH != null)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, nopH.Option.Criteria.Name, nopH.Option.Name, "Unchecked");
                    db.PlantOption.Remove(nopH);
                }
            }

            //Save Function for Last Inspection Date
            if (User.IsInRole("MTS Risk Analysis : Admin User"))
            {
                LastInspection lastInspectionDate = db.LastInspection.Where(x => x.PlantId == plantID && x.SchemeId == currentScheme.SchemeId).FirstOrDefault();

                LastInspection newLastInspectionDate = new LastInspection
                {
                    PlantId = plantID,
                    SchemeId = currentScheme.SchemeId,
                    Date = bLSSchemeView.LastInspection.Date
                };


                if (lastInspectionDate == null)
                {
                    if (newLastInspectionDate == null)
                    {
                        //CAN BE NULL
                        newLastInspectionDate = lastInspectionDate;
                    }
                    else
                    {
                        db.LastInspection.Add(newLastInspectionDate);
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, "Date of Last Initial Inspection", "", newLastInspectionDate.Date.ToShortDateString());
                    }
                }
                else
                {
                    if (lastInspectionDate.Date.Date != newLastInspectionDate.Date.Date)
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, "Date of Last Initial Inspection", lastInspectionDate.Date.ToShortDateString(), newLastInspectionDate.Date.ToShortDateString());
                        db.LastInspection.Remove(lastInspectionDate);
                        db.LastInspection.Add(newLastInspectionDate);
                    }
                }
            }
            if (errorList.Count > 0)
            {
                TempData["errorList"] = errorList;

                return RedirectToAction("Details", "Plant", new { id = plantID, scheme = currentScheme.Name });
            }
            else
            {
                db.SaveChanges();

                return RedirectToAction("RiskScoreUpdate", new { plantIDPass = plantID, schemeIDPass = currentScheme.SchemeId });
            }
        }

        // POST: post action for update to BCC Scheme
        public ActionResult BCCSchemePost(BCCSchemeView bcCSchemeView, string Plantid)
        {
            //Setup
            var errorList = new List<string>();

            Scheme currentScheme = db.Scheme.Where(x => x.Name == "BCC").FirstOrDefault();

            Guid plantID = Guid.Parse(Plantid);

            Plant currentPlant = db.Plant.Find(plantID);

            String currentUser = UserHelper.CurrentUser();

            //Throughput Update

            PlantOption throughputOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Throughput");

            PlantOption newThru = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bcCSchemeView.Throughput);

            if (throughputOption == null)
            {
                if (newThru.Option == null)
                {
                    errorList.Add("Throughput is required");
                }
                else
                {
                    db.PlantOption.Add(newThru);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newThru.Option.Criteria.Name, "", newThru.Option.Name);
                }
            }
            else
            {
                if (throughputOption.OptionId != bcCSchemeView.Throughput)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, throughputOption.Option.Criteria.Name, throughputOption.Option.Name, newThru.Option.Name);
                    db.PlantOption.Remove(throughputOption);
                    db.PlantOption.Add(newThru);
                }
            }

            //Operating Hours Update

            PlantOption opHoursOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Operating Hours");

            PlantOption newOpHours = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bcCSchemeView.OperatingHours);

            if (opHoursOption == null)
            {
                if (newOpHours.Option == null)
                {
                    errorList.Add("Operating Hours is required");
                }
                else
                {
                    db.PlantOption.Add(newOpHours);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newOpHours.Option.Criteria.Name, "", newOpHours.Option.Name);
                }
            }
            else
            {
                if (opHoursOption.OptionId != bcCSchemeView.OperatingHours)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, opHoursOption.Option.Criteria.Name, opHoursOption.Option.Name, newOpHours.Option.Name);
                    db.PlantOption.Remove(opHoursOption);
                    db.PlantOption.Add(newOpHours);
                }
            }

            //Pre Inspection Sign In Update

            PlantOption signInOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Pre Inspection Sign In");

            PlantOption newSignIn = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bcCSchemeView.SignIn);

            if (signInOption == null)
            {
                if (newSignIn.Option == null)
                {
                    errorList.Add("Pre Inspection Sign In is required");
                }
                else
                {
                    db.PlantOption.Add(newSignIn);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newSignIn.Option.Criteria.Name, "", newSignIn.Option.Name);
                }
            }
            else
            {
                if (signInOption.OptionId != bcCSchemeView.SignIn)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, signInOption.Option.Criteria.Name, signInOption.Option.Name, newSignIn.Option.Name);
                    db.PlantOption.Remove(signInOption);
                    db.PlantOption.Add(newSignIn);
                }
            }

            //Record Keeping Update

            PlantOption recKeepOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Record Keeping");

            PlantOption newRecKeep = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bcCSchemeView.RecordKeeping);

            if (recKeepOption == null)
            {
                if (newRecKeep.Option == null)
                {
                    errorList.Add("Record Keeping is required");
                }
                else
                {
                    db.PlantOption.Add(newRecKeep);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newRecKeep.Option.Criteria.Name, "", newRecKeep.Option.Name);
                }
            }
            else
            {
                if (recKeepOption.OptionId != bcCSchemeView.RecordKeeping)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, recKeepOption.Option.Criteria.Name, recKeepOption.Option.Name, newRecKeep.Option.Name);
                    db.PlantOption.Remove(recKeepOption);
                    db.PlantOption.Add(newRecKeep);
                }
            }

            //Line Clearing Update

            PlantOption lineClearOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Line Clearing");

            PlantOption newLineClear = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bcCSchemeView.LineClearing);

            if (lineClearOption == null)
            {
                if (newLineClear.Option == null)
                {
                    errorList.Add("Line Clearing is required");
                }
                else
                {
                    db.PlantOption.Add(newLineClear);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newLineClear.Option.Criteria.Name, "", newLineClear.Option.Name);
                }
            }
            else
            {
                if (lineClearOption.OptionId != bcCSchemeView.LineClearing)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, lineClearOption.Option.Criteria.Name, lineClearOption.Option.Name, newLineClear.Option.Name);
                    db.PlantOption.Remove(lineClearOption);
                    db.PlantOption.Add(newLineClear);
                }
            }

            //Carcass Hanging Method Update

            PlantOption carcassHangOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Carcass Hanging Method");

            PlantOption newCarcassHang = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bcCSchemeView.CarcassHang);

            if (carcassHangOption == null)
            {
                if (newCarcassHang.Option == null)
                {
                    errorList.Add("Carcass Hanging Method is required");
                }
                else
                {
                    db.PlantOption.Add(newCarcassHang);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newCarcassHang.Option.Criteria.Name, "", newCarcassHang.Option.Name);
                }
            }
            else
            {
                if (carcassHangOption.OptionId != bcCSchemeView.CarcassHang)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, carcassHangOption.Option.Criteria.Name, carcassHangOption.Option.Name, newCarcassHang.Option.Name);
                    db.PlantOption.Remove(carcassHangOption);
                    db.PlantOption.Add(newCarcassHang);
                }
            }

            //Trimming Update

            PlantOption trimmingOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Trimming");

            PlantOption newTrimming = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bcCSchemeView.Trimming);

            if (trimmingOption == null)
            {
                if (newTrimming.Option == null)
                {
                    errorList.Add("Trimming is required");
                }
                else
                {
                    db.PlantOption.Add(newTrimming);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newTrimming.Option.Criteria.Name, "", newTrimming.Option.Name);
                }
            }
            else
            {
                if (trimmingOption.OptionId != bcCSchemeView.Trimming)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, trimmingOption.Option.Criteria.Name, trimmingOption.Option.Name, newTrimming.Option.Name);
                    db.PlantOption.Remove(trimmingOption);
                    db.PlantOption.Add(newTrimming);
                }
            }

            //Hide Puller Update

            PlantOption hidePullerOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Hide Puller");

            PlantOption newHidePuller = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bcCSchemeView.HidePuller);

            if (hidePullerOption == null)
            {
                if (newHidePuller.Option == null)
                {
                    errorList.Add("Hide Puller is required");
                }
                else
                {
                    db.PlantOption.Add(newHidePuller);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newHidePuller.Option.Criteria.Name, "", newHidePuller.Option.Name);
                }
            }
            else
            {
                if (hidePullerOption.OptionId != bcCSchemeView.HidePuller)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, hidePullerOption.Option.Criteria.Name, hidePullerOption.Option.Name, newHidePuller.Option.Name);
                    db.PlantOption.Remove(hidePullerOption);
                    db.PlantOption.Add(newHidePuller);
                }
            }

            //Carcass Dressing Update

            PlantOption carcassDressOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Carcass Dressing");

            PlantOption newCarcassDress = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bcCSchemeView.CarcassDress);

            if (carcassDressOption == null)
            {
                if (newCarcassDress.Option == null)
                {
                    errorList.Add("Carcass Dressing is required");
                }
                else
                {
                    db.PlantOption.Add(newCarcassDress);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newCarcassDress.Option.Criteria.Name, "", newCarcassDress.Option.Name);
                }
            }
            else
            {
                if (carcassDressOption.OptionId != bcCSchemeView.CarcassDress)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, carcassDressOption.Option.Criteria.Name, carcassDressOption.Option.Name, newCarcassDress.Option.Name);
                    db.PlantOption.Remove(carcassDressOption);
                    db.PlantOption.Add(newCarcassDress);
                }
            }

            //Number of Weeks Since Last Inspection Update

            PlantOption numOfWeeksOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Number of Weeks Since Last Inspection");

            PlantOption newNumofWeeks = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bcCSchemeView.NumofWeeks);

            if (numOfWeeksOption == null)
            {
                if (newNumofWeeks.Option == null)
                {
                    //CAN BE NULL
                    newNumofWeeks = numOfWeeksOption;
                }
                else
                {
                    db.PlantOption.Add(newNumofWeeks);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newNumofWeeks.Option.Criteria.Name, "", newNumofWeeks.Option.Name);
                }
            }
            else
            {
                if (numOfWeeksOption.OptionId != bcCSchemeView.NumofWeeks)
                {
                    if (newNumofWeeks.Option != null)
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, numOfWeeksOption.Option.Criteria.Name, numOfWeeksOption.Option.Name, newNumofWeeks.Option.Name);
                        db.PlantOption.Remove(numOfWeeksOption);
                        db.PlantOption.Add(newNumofWeeks);
                    }
                    else
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, numOfWeeksOption.Option.Criteria.Name, numOfWeeksOption.Option.Name, null);
                        db.PlantOption.Remove(numOfWeeksOption);
                    }
                }
            }

            //Failed/Unsatisfactory on Last Visit Update

            PlantOption failedVisitOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Failed/Unsatisfactory on Last Visit");

            PlantOption newFailedVisit = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bcCSchemeView.FailedVisit);

            if (failedVisitOption == null)
            {
                if (newFailedVisit.Option == null)
                {
                    //CAN BE NULL
                    newFailedVisit = failedVisitOption;
                }
                else
                {
                    db.PlantOption.Add(newFailedVisit);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newFailedVisit.Option.Criteria.Name, "", newFailedVisit.Option.Name);
                }
            }
            else
            {
                if (failedVisitOption.OptionId != bcCSchemeView.FailedVisit)
                {
                    if (newFailedVisit.Option != null)
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, failedVisitOption.Option.Criteria.Name, failedVisitOption.Option.Name, newFailedVisit.Option.Name);
                        db.PlantOption.Remove(failedVisitOption);
                        db.PlantOption.Add(newFailedVisit);
                    }
                    else
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, failedVisitOption.Option.Criteria.Name, failedVisitOption.Option.Name, null);
                        db.PlantOption.Remove(failedVisitOption);
                    }
                }
            }

            //Number of Failed/Unsatisfactory in Previous 3 Years Update

            PlantOption failedLastThreeOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Number of Failed/Unsatisfactory in Previous 3 Years");

            PlantOption newFailedLastThree = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bcCSchemeView.FailedLastThree);

            if (failedLastThreeOption == null)
            {
                if (newFailedLastThree.Option == null)
                {
                    //CAN BE NULL
                    newFailedLastThree = failedLastThreeOption;
                }
                else
                {
                    db.PlantOption.Add(newFailedLastThree);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newFailedLastThree.Option.Criteria.Name, "", newFailedLastThree.Option.Name);
                }
            }
            else
            {
                if (failedLastThreeOption.OptionId != bcCSchemeView.FailedLastThree)
                {
                    if (newFailedLastThree.Option != null)
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, failedLastThreeOption.Option.Criteria.Name, failedLastThreeOption.Option.Name, newFailedLastThree.Option.Name);
                        db.PlantOption.Remove(failedLastThreeOption);
                        db.PlantOption.Add(newFailedLastThree);
                    }
                    else
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, failedLastThreeOption.Option.Criteria.Name, failedLastThreeOption.Option.Name, null);
                        db.PlantOption.Remove(failedLastThreeOption);
                    }
                }
            }

            //Number of Near Misses in Previous 3 Years Update

            PlantOption missLastThreeOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Number of Near Misses in Previous 3 Years");

            PlantOption newMissLastThree = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bcCSchemeView.MissLastThree);

            if (missLastThreeOption == null)
            {
                if (newMissLastThree.Option == null)
                {
                    //CAN BE NULL
                    newMissLastThree = missLastThreeOption;
                }
                else
                {
                    db.PlantOption.Add(newMissLastThree);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newMissLastThree.Option.Criteria.Name, "", newMissLastThree.Option.Name);
                }
            }
            else
            {
                if (missLastThreeOption.OptionId != bcCSchemeView.MissLastThree)
                {
                    if (newMissLastThree.Option != null)
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, missLastThreeOption.Option.Criteria.Name, missLastThreeOption.Option.Name, newMissLastThree.Option.Name);
                        db.PlantOption.Remove(missLastThreeOption);
                        db.PlantOption.Add(newMissLastThree);
                    }
                    else
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, missLastThreeOption.Option.Criteria.Name, missLastThreeOption.Option.Name, null);
                        db.PlantOption.Remove(missLastThreeOption);
                    }
                }
            }

            //Scheme Year Update

            PlantOption schYrOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Scheme Year");

            PlantOption newschYrOption = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, bcCSchemeView.SchemeYear);

            if (schYrOption == null)
            {
                if (newschYrOption.Option == null)
                {
                    //CAN BE NULL
                    newschYrOption = schYrOption;
                }
                else
                {
                    db.PlantOption.Add(newschYrOption);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newschYrOption.Option.Criteria.Name, "", newschYrOption.Option.Name);
                }
            }
            else
            {
                if (schYrOption.OptionId != bcCSchemeView.SchemeYear)
                {
                    if (newschYrOption.Option != null)
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, schYrOption.Option.Criteria.Name, schYrOption.Option.Name, newschYrOption.Option.Name);
                        db.PlantOption.Remove(schYrOption);
                        db.PlantOption.Add(newschYrOption);
                    }
                    else
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, schYrOption.Option.Criteria.Name, schYrOption.Option.Name, null);
                        db.PlantOption.Remove(schYrOption);
                    }
                }
            }

            //Save Function for Last Inspection Date

            LastInspection lastInspectionDate = db.LastInspection.Where(x => x.PlantId == plantID && x.SchemeId == currentScheme.SchemeId).FirstOrDefault();

            LastInspection newLastInspectionDate = new LastInspection
            {
                PlantId = plantID,
                SchemeId = currentScheme.SchemeId,
                Date = bcCSchemeView.LastInspection.Date
            };


            if (lastInspectionDate == null)
            {
                if (newLastInspectionDate == null)
                {
                    //CAN BE NULL
                    newLastInspectionDate = lastInspectionDate;
                }
                else
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, "Date of Last Inspection", "", newLastInspectionDate.Date.ToShortDateString());
                    db.LastInspection.Add(newLastInspectionDate);
                }
            }
            else
            {
                if (lastInspectionDate.Date.Date != newLastInspectionDate.Date.Date)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, "Date of Last Inspection", lastInspectionDate.Date.ToShortDateString(), newLastInspectionDate.Date.ToShortDateString());
                    db.LastInspection.Remove(lastInspectionDate);
                    db.LastInspection.Add(newLastInspectionDate);
                }
            }

            if (errorList.Count > 0)
            {
                TempData["errorList"] = errorList;

                return RedirectToAction("Details", "Plant", new { id = plantID, scheme = currentScheme.Name });
            }
            else
            {
                db.SaveChanges();

                return RedirectToAction("RiskScoreUpdate", new { plantIDPass = plantID, schemeIDPass = currentScheme.SchemeId });
            }
        }

        // POST: post action for update to PCG Scheme
        public ActionResult PCGSchemePost(PCGSchemeView pcGSchemeView, string Plantid)
        {
            //Setup
            var errorList = new List<string>();

            Scheme currentScheme = db.Scheme.Where(x => x.Name == "PCG").FirstOrDefault();

            Guid plantID = Guid.Parse(Plantid);

            Plant currentPlant = db.Plant.Find(plantID);

            String currentUser = UserHelper.CurrentUser();

            //Throughput Update

            PlantOption throughputOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Throughput");

            PlantOption newThru = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, pcGSchemeView.Throughput);

            if (throughputOption == null)
            {
                if (newThru.Option == null)
                {
                    errorList.Add("Throughput is required");
                }
                else
                {
                    db.PlantOption.Add(newThru);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newThru.Option.Criteria.Name, "", newThru.Option.Name);
                }
            }
            else
            {
                if (throughputOption.OptionId != pcGSchemeView.Throughput)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, throughputOption.Option.Criteria.Name, throughputOption.Option.Name, newThru.Option.Name);
                    db.PlantOption.Remove(throughputOption);
                    db.PlantOption.Add(newThru);
                }
            }

            //Operating Hours Update

            PlantOption opHoursOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Operating Hours");

            PlantOption newOpHours = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, pcGSchemeView.OperatingHours);

            if (opHoursOption == null)
            {
                if (newOpHours.Option == null)
                {
                    errorList.Add("Operating Hours is required");
                }
                else
                {
                    db.PlantOption.Add(newOpHours);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newOpHours.Option.Criteria.Name, "", newOpHours.Option.Name);
                }
            }
            else
            {
                if (opHoursOption.OptionId != pcGSchemeView.OperatingHours)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, opHoursOption.Option.Criteria.Name, opHoursOption.Option.Name, newOpHours.Option.Name);
                    db.PlantOption.Remove(opHoursOption);
                    db.PlantOption.Add(newOpHours);
                }
            }

            //Pre Inspection Sign In Update

            PlantOption signInOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Pre Inspection Sign In");

            PlantOption newSignIn = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, pcGSchemeView.SignIn);

            if (signInOption == null)
            {
                if (newSignIn.Option == null)
                {
                    errorList.Add("Pre Inspection Sign In is required");
                }
                else
                {
                    db.PlantOption.Add(newSignIn);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newSignIn.Option.Criteria.Name, "", newSignIn.Option.Name);
                }
            }
            else
            {
                if (signInOption.OptionId != pcGSchemeView.SignIn)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, signInOption.Option.Criteria.Name, signInOption.Option.Name, newSignIn.Option.Name);
                    db.PlantOption.Remove(signInOption);
                    db.PlantOption.Add(newSignIn);
                }
            }

            //Record Keeping Update

            PlantOption recKeepOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Record Keeping");

            PlantOption newRecKeep = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, pcGSchemeView.RecordKeeping);

            if (recKeepOption == null)
            {
                if (newRecKeep.Option == null)
                {
                    errorList.Add("Record Keeping is required");
                }
                else
                {
                    db.PlantOption.Add(newRecKeep);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newRecKeep.Option.Criteria.Name, "", newRecKeep.Option.Name);
                }
            }
            else
            {
                if (recKeepOption.OptionId != pcGSchemeView.RecordKeeping)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, recKeepOption.Option.Criteria.Name, recKeepOption.Option.Name, newRecKeep.Option.Name);
                    db.PlantOption.Remove(recKeepOption);
                    db.PlantOption.Add(newRecKeep);
                }
            }

            //Line Clearing Update

            PlantOption lineClearOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Line Clearing");

            PlantOption newLineClear = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, pcGSchemeView.LineClearing);

            if (lineClearOption == null)
            {
                if (newLineClear.Option == null)
                {
                    errorList.Add("Line Clearing is required");
                }
                else
                {
                    db.PlantOption.Add(newLineClear);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newLineClear.Option.Criteria.Name, "", newLineClear.Option.Name);
                }
            }
            else
            {
                if (lineClearOption.OptionId != pcGSchemeView.LineClearing)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, lineClearOption.Option.Criteria.Name, lineClearOption.Option.Name, newLineClear.Option.Name);
                    db.PlantOption.Remove(lineClearOption);
                    db.PlantOption.Add(newLineClear);
                }
            }

            //Probe Update

            PlantOption probeOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Probe");

            PlantOption newProbe = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, pcGSchemeView.Probe);

            if (probeOption == null)
            {
                if (newProbe.Option == null)
                {
                    errorList.Add("Probe is required");
                }
                else
                {
                    db.PlantOption.Add(newProbe);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newProbe.Option.Criteria.Name, "", newProbe.Option.Name);
                }
            }
            else
            {
                if (probeOption.OptionId != pcGSchemeView.Probe)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, probeOption.Option.Criteria.Name, probeOption.Option.Name, newProbe.Option.Name);
                    db.PlantOption.Remove(probeOption);
                    db.PlantOption.Add(newProbe);
                }
            }

            //Trimming Update

            PlantOption trimmingOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Trimming");

            PlantOption newTrimming = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, pcGSchemeView.Trimming);

            if (trimmingOption == null)
            {
                if (newTrimming.Option == null)
                {
                    errorList.Add("Trimming is required");
                }
                else
                {
                    db.PlantOption.Add(newTrimming);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newTrimming.Option.Criteria.Name, "", newTrimming.Option.Name);
                }
            }
            else
            {
                if (trimmingOption.OptionId != pcGSchemeView.Trimming)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, trimmingOption.Option.Criteria.Name, trimmingOption.Option.Name, newTrimming.Option.Name);
                    db.PlantOption.Remove(trimmingOption);
                    db.PlantOption.Add(newTrimming);
                }
            }

            //Carcass Dressing Update

            PlantOption carcassDressOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Carcass Dressing");

            PlantOption newCarcassDress = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, pcGSchemeView.CarcassDress);

            if (carcassDressOption == null)
            {
                if (newCarcassDress.Option == null)
                {
                    errorList.Add("Carcass Dressing is required");
                }
                else
                {
                    db.PlantOption.Add(newCarcassDress);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newCarcassDress.Option.Criteria.Name, "", newCarcassDress.Option.Name);
                }
            }
            else
            {
                if (carcassDressOption.OptionId != pcGSchemeView.CarcassDress)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, carcassDressOption.Option.Criteria.Name, carcassDressOption.Option.Name, newCarcassDress.Option.Name);
                    db.PlantOption.Remove(carcassDressOption);
                    db.PlantOption.Add(newCarcassDress);
                }
            }

            //Number of Weeks Since Last Inspection Update

            PlantOption numOfWeeksOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Number of Weeks Since Last Inspection");

            PlantOption newNumofWeeks = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, pcGSchemeView.NumofWeeks);

            if (numOfWeeksOption == null)
            {
                if (newNumofWeeks.Option == null)
                {
                    //CAN BE NULL
                    newNumofWeeks = numOfWeeksOption;
                }
                else
                {
                    db.PlantOption.Add(newNumofWeeks);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newNumofWeeks.Option.Criteria.Name, newNumofWeeks.Option.Name);
                }
            }
            else
            {
                if (numOfWeeksOption.OptionId != pcGSchemeView.NumofWeeks)
                {
                    if (newNumofWeeks.Option != null)
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, numOfWeeksOption.Option.Criteria.Name, numOfWeeksOption.Option.Name, newNumofWeeks.Option.Name);
                        db.PlantOption.Remove(numOfWeeksOption);
                        db.PlantOption.Add(newNumofWeeks);
                    }
                    else
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, numOfWeeksOption.Option.Criteria.Name, numOfWeeksOption.Option.Name, null);
                        db.PlantOption.Remove(numOfWeeksOption);
                    }
                }
            }

            //Failed/Unsatisfactory on Last Visit Update

            PlantOption failedVisitOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Failed/Unsatisfactory on Last Visit");

            PlantOption newFailedVisit = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, pcGSchemeView.FailedVisit);

            if (failedVisitOption == null)
            {
                if (newFailedVisit.Option == null)
                {
                    //CAN BE NULL
                    newFailedVisit = failedVisitOption;
                }
                else
                {
                    db.PlantOption.Add(newFailedVisit);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newFailedVisit.Option.Criteria.Name, "", newFailedVisit.Option.Name);
                }
            }
            else
            {
                if (failedVisitOption.OptionId != pcGSchemeView.FailedVisit)
                {
                    if (newFailedVisit.Option != null)
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, failedVisitOption.Option.Criteria.Name, failedVisitOption.Option.Name, newFailedVisit.Option.Name);
                        db.PlantOption.Remove(failedVisitOption);
                        db.PlantOption.Add(newFailedVisit);
                    }
                    else
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, failedVisitOption.Option.Criteria.Name, failedVisitOption.Option.Name, null);
                        db.PlantOption.Remove(failedVisitOption);
                    }
                }
            }

            //Number of Failed/Unsatisfactory in Previous 3 Years Update

            PlantOption failedLastThreeOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Number of Failed/Unsatisfactory in Previous 3 Years");

            PlantOption newFailedLastThree = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, pcGSchemeView.FailedLastThree);

            if (failedLastThreeOption == null)
            {
                if (newFailedLastThree.Option == null)
                {
                    //CAN BE NULL
                    newFailedLastThree = failedLastThreeOption;
                }
                else
                {
                    db.PlantOption.Add(newFailedLastThree);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newFailedLastThree.Option.Criteria.Name, "", newFailedLastThree.Option.Name);
                }
            }
            else
            {
                if (failedLastThreeOption.OptionId != pcGSchemeView.FailedLastThree)
                {
                    if (newFailedLastThree.Option != null)
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, failedLastThreeOption.Option.Criteria.Name, failedLastThreeOption.Option.Name, newFailedLastThree.Option.Name);
                        db.PlantOption.Remove(failedLastThreeOption);
                        db.PlantOption.Add(newFailedLastThree);
                    }
                    else
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, failedLastThreeOption.Option.Criteria.Name, failedLastThreeOption.Option.Name, null);
                        db.PlantOption.Remove(failedLastThreeOption);
                    }
                }
            }

            //Number of Near Misses in Previous 3 Years Update

            PlantOption missLastThreeOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Number of Near Misses in Previous 3 Years");

            PlantOption newMissLastThree = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, pcGSchemeView.MissLastThree);

            if (missLastThreeOption == null)
            {
                if (newMissLastThree.Option == null)
                {
                    //CAN BE NULL
                    newMissLastThree = missLastThreeOption;
                }
                else
                {
                    db.PlantOption.Add(newMissLastThree);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newMissLastThree.Option.Criteria.Name, "", newMissLastThree.Option.Name);
                }
            }
            else
            {
                if (missLastThreeOption.OptionId != pcGSchemeView.MissLastThree)
                {
                    if (newMissLastThree.Option != null)
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, missLastThreeOption.Option.Criteria.Name, missLastThreeOption.Option.Name, newMissLastThree.Option.Name);
                        db.PlantOption.Remove(missLastThreeOption);
                        db.PlantOption.Add(newMissLastThree);
                    }
                    else
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, missLastThreeOption.Option.Criteria.Name, missLastThreeOption.Option.Name, null);
                        db.PlantOption.Remove(missLastThreeOption);
                    }
                }
            }

            //Scheme Year Update

            PlantOption schYrOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Scheme Year");

            PlantOption newschYrOption = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, pcGSchemeView.SchemeYear);

            if (schYrOption == null)
            {
                if (newschYrOption.Option == null)
                {
                    //CAN BE NULL
                    newschYrOption = schYrOption;
                }
                else
                {
                    db.PlantOption.Add(newschYrOption);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newschYrOption.Option.Criteria.Name, "", newschYrOption.Option.Name);
                }
            }
            else
            {
                if (schYrOption.OptionId != pcGSchemeView.SchemeYear)
                {
                    if (newschYrOption.Option != null)
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, schYrOption.Option.Criteria.Name, schYrOption.Option.Name, newschYrOption.Option.Name);
                        db.PlantOption.Remove(schYrOption);
                        db.PlantOption.Add(newschYrOption);
                    }
                    else
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, schYrOption.Option.Criteria.Name, schYrOption.Option.Name, null);
                        db.PlantOption.Remove(schYrOption);
                    }
                }
            }

            //Save Function for Last Inspection Date

            LastInspection lastInspectionDate = db.LastInspection.Where(x => x.PlantId == plantID && x.SchemeId == currentScheme.SchemeId).FirstOrDefault();

            LastInspection newLastInspectionDate = new LastInspection
            {
                PlantId = plantID,
                SchemeId = currentScheme.SchemeId,
                Date = pcGSchemeView.LastInspection.Date
            };

            if (lastInspectionDate == null)
            {
                if (newLastInspectionDate == null)
                {
                    //CAN BE NULL
                    newLastInspectionDate = lastInspectionDate;
                }
                else
                {
                    db.LastInspection.Add(newLastInspectionDate);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, "Date of Last Inspection", "", newLastInspectionDate.Date.ToShortDateString());
                }
            }
            else
            {
                if (lastInspectionDate.Date.Date != newLastInspectionDate.Date.Date)
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, "Date of Last Inspection", lastInspectionDate.Date.ToShortDateString(), newLastInspectionDate.Date.ToShortDateString());
                    db.LastInspection.Remove(lastInspectionDate);
                    db.LastInspection.Add(newLastInspectionDate);

                }
            }

            if (errorList.Count > 0)
            {
                TempData["errorList"] = errorList;

                return RedirectToAction("Details", "Plant", new { id = plantID, scheme = currentScheme.Name });
            }
            else
            {
                db.SaveChanges();

                return RedirectToAction("RiskScoreUpdate", new { plantIDPass = plantID, schemeIDPass = currentScheme.SchemeId });
            }
        }

        // POST: post action for update to PCG Scheme
        public ActionResult SCCSchemePost(SCCSchemeView sccSchemeView, string Plantid)
        {
            //Setup
            var errorList = new List<string>();

            Scheme currentScheme = db.Scheme.Where(x => x.Name == "SCC").FirstOrDefault();

            Guid plantID = Guid.Parse(Plantid);

            Plant currentPlant = db.Plant.Find(plantID);

            String currentUser = UserHelper.CurrentUser();

            //Throughput Update
            PlantOption throughputOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Throughput");
            PlantOption newThru = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, sccSchemeView.Throughput);

            if (throughputOption == null)
            {
                if (newThru.Option == null)
                {
                    errorList.Add("Throughput is required");
                }
                else
                {
                    db.PlantOption.Add(newThru);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newThru.Option.Criteria.Name, "", newThru.Option.Name);
                }
            }
            else
            {
                if (newThru.Option == null)
                {
                    errorList.Add("Throughput is required");
                }
                else
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, throughputOption.Option.Criteria.Name, throughputOption.Option.Name, newThru.Option.Name);
                    db.PlantOption.Remove(throughputOption);
                    db.PlantOption.Add(newThru);
                }
            }

            //Operating hours
            PlantOption operatingHoursOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Operating hours");
            PlantOption newOperatingHours = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, sccSchemeView.OperatingHours);

            if (operatingHoursOption == null)
            {
                if (newOperatingHours.Option == null)
                {
                    errorList.Add("Operating hours are required");
                }
                else
                {
                    db.PlantOption.Add(newOperatingHours);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newOperatingHours.Option.Criteria.Name, "", newOperatingHours.Option.Name);
                }
            }
            else
            {
                if (newOperatingHours.Option == null)
                {
                    errorList.Add("Operating hours are required");
                }
                else
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, operatingHoursOption.Option.Criteria.Name, operatingHoursOption.Option.Name, newOperatingHours.Option.Name);
                    db.PlantOption.Remove(operatingHoursOption);
                    db.PlantOption.Add(newOperatingHours);
                }
            }

            //Signin prior to inspection
            PlantOption signInOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Sign in prior to inspection");
            PlantOption newSignIn = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, sccSchemeView.SignIn);

            if (signInOption == null)
            {
                if (newSignIn.Option == null)
                {
                    errorList.Add("Sign in prior to inspection is required");
                }
                else
                {
                    db.PlantOption.Add(newSignIn);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newSignIn.Option.Criteria.Name, "", newSignIn.Option.Name);
                }
            }
            else
            {
                if (newSignIn.Option == null)
                {
                    errorList.Add("Sign in prior to inspection is required");
                }
                else
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, signInOption.Option.Criteria.Name, signInOption.Option.Name, newSignIn.Option.Name);
                    db.PlantOption.Remove(signInOption);
                    db.PlantOption.Add(newSignIn);
                }
            }

            //Trimming
            PlantOption trimmingOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Trimming");
            PlantOption newTrimming = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, sccSchemeView.Trimming);

            if (trimmingOption == null)
            {
                if (newTrimming.Option == null)
                {
                    errorList.Add("Trimming is required");
                }
                else
                {
                    db.PlantOption.Add(newTrimming);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newTrimming.Option.Criteria.Name, "", newTrimming.Option.Name);
                }
            }
            else
            {
                if (newTrimming.Option == null)
                {
                    errorList.Add("Trimming is required");
                }
                else
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, trimmingOption.Option.Criteria.Name, trimmingOption.Option.Name, newTrimming.Option.Name);
                    db.PlantOption.Remove(trimmingOption);
                    db.PlantOption.Add(newTrimming);
                }
            }

            //Skin removal
            PlantOption skinRemovalOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Skin Removal");
            PlantOption newSkinRemoval = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, sccSchemeView.SkinRemoval);

            if (skinRemovalOption == null)
            {
                if (newSkinRemoval.Option == null)
                {
                    errorList.Add("Skin removal is required");
                }
                else
                {
                    db.PlantOption.Add(newSkinRemoval);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newSkinRemoval.Option.Criteria.Name, "", newSkinRemoval.Option.Name);
                }
            }
            else
            {
                if (newSkinRemoval.Option == null)
                {
                    errorList.Add("Skin removal is required");
                }
                else
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, skinRemovalOption.Option.Criteria.Name, skinRemovalOption.Option.Name, newSkinRemoval.Option.Name);
                    db.PlantOption.Remove(skinRemovalOption);
                    db.PlantOption.Add(newSkinRemoval);
                }
            }

            //Head Removal
            PlantOption headRemovalOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Head Removal");
            PlantOption newHeadRemoval = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, sccSchemeView.HeadRemoval);

            if (headRemovalOption == null)
            {
                if (newHeadRemoval.Option == null)
                {
                    errorList.Add("Head removal is required");
                }
                else
                {
                    db.PlantOption.Add(newHeadRemoval);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newHeadRemoval.Option.Criteria.Name, "", newHeadRemoval.Option.Name);
                }
            }
            else
            {
                if (newHeadRemoval.Option == null)
                {
                    errorList.Add("Head removal is required");
                }
                else
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, headRemovalOption.Option.Criteria.Name, headRemovalOption.Option.Name, newHeadRemoval.Option.Name);
                    db.PlantOption.Remove(headRemovalOption);
                    db.PlantOption.Add(newHeadRemoval);
                }
            }

            //Feet Removal
            PlantOption feetRemovalOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Feet Removal");
            PlantOption newFeetRemoval = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, sccSchemeView.FeetRemoval);

            if (feetRemovalOption == null)
            {
                if (newFeetRemoval.Option == null)
                {
                    errorList.Add("Feet removal is required");
                }
                else
                {
                    db.PlantOption.Add(newFeetRemoval);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newFeetRemoval.Option.Criteria.Name, "", newFeetRemoval.Option.Name);
                }
            }
            else
            {
                if (newFeetRemoval.Option == null)
                {
                    errorList.Add("Feet removal is required");
                }
                else
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, feetRemovalOption.Option.Criteria.Name, feetRemovalOption.Option.Name, newFeetRemoval.Option.Name);
                    db.PlantOption.Remove(feetRemovalOption);
                    db.PlantOption.Add(newFeetRemoval);
                }
            }

            //CarcassDressing
            PlantOption carcassDressingOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Carcass dressing");
            PlantOption newCarcassDressing = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, sccSchemeView.CarcassDressing);

            if (carcassDressingOption == null)
            {
                if (newCarcassDressing.Option == null)
                {
                    errorList.Add("Carcass dressing is required");
                }
                else
                {
                    db.PlantOption.Add(newCarcassDressing);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newCarcassDressing.Option.Criteria.Name, "", newCarcassDressing.Option.Name);
                }
            }
            else
            {
                if (newCarcassDressing.Option == null)
                {
                    errorList.Add("Carcass dressing is required");
                }
                else
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, carcassDressingOption.Option.Criteria.Name, carcassDressingOption.Option.Name, newCarcassDressing.Option.Name);
                    db.PlantOption.Remove(carcassDressingOption);
                    db.PlantOption.Add(newCarcassDressing);
                }
            }

            PlantOption schYrOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Scheme Year");
            PlantOption newschYrOption = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, sccSchemeView.SchemeYear);

            if (schYrOption == null)
            {
                if (newschYrOption.Option == null)
                {
                    //CAN BE NULL
                    newschYrOption = schYrOption;
                }
                else
                {
                    db.PlantOption.Add(newschYrOption);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, newschYrOption.Option.Criteria.Name, "", newschYrOption.Option.Name);
                }
            }
            else
            {
                if (schYrOption.OptionId != sccSchemeView.SchemeYear)
                {
                    if (newschYrOption.Option != null)
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, schYrOption.Option.Criteria.Name, schYrOption.Option.Name, newschYrOption.Option.Name);
                        db.PlantOption.Remove(schYrOption);
                        db.PlantOption.Add(newschYrOption);
                    }
                    else
                    {
                        auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, schYrOption.Option.Criteria.Name, schYrOption.Option.Name, null);
                        db.PlantOption.Remove(schYrOption);
                    }
                }
            }

            //FailedUnsatisfactory
            PlantOption failedUnsatisfactoryOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Failed / Unsatisfactory");
            PlantOption failedUnsatisfactory = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, sccSchemeView.FailedUnsatisfactory);

            if (failedUnsatisfactoryOption == null)
            {
                if (failedUnsatisfactory.Option == null)
                {
                    errorList.Add("Failed / Unsatisfactory is required");
                }
                else
                {
                    db.PlantOption.Add(failedUnsatisfactory);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, failedUnsatisfactory.Option.Criteria.Name, "", failedUnsatisfactory.Option.Name);
                }
            }
            else
            {
                if (failedUnsatisfactory.Option == null)
                {
                    errorList.Add("Failed / Unsatisfactory is required");
                }
                else
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, failedUnsatisfactory.Option.Criteria.Name, failedUnsatisfactory.Option.Name, failedUnsatisfactory.Option.Name);
                    db.PlantOption.Remove(failedUnsatisfactoryOption);
                    db.PlantOption.Add(failedUnsatisfactory);
                }
            }

            //failsInPrevious12Months
            PlantOption failsInPrevious12MonthsOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "How many fails / unsatisfactory in previous 12 Months");
            PlantOption failsInPrevious12Months = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, sccSchemeView.FailedUnsatisfactoryInLast12Months);

            if (failsInPrevious12MonthsOption == null)
            {
                if (failsInPrevious12Months.Option == null)
                {
                    errorList.Add("How many fails / unsatisfactory in previous 12 Months is required");
                }
                else
                {
                    db.PlantOption.Add(failsInPrevious12Months);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, failsInPrevious12Months.Option.Criteria.Name, "", failsInPrevious12Months.Option.Name);
                }
            }
            else
            {
                if (failsInPrevious12Months.Option == null)
                {
                    errorList.Add("How many fails / unsatisfactory in previous 12 Months is required");
                }
                else
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, failsInPrevious12Months.Option.Criteria.Name, failsInPrevious12Months.Option.Name, failsInPrevious12Months.Option.Name);
                    db.PlantOption.Remove(failsInPrevious12MonthsOption);
                    db.PlantOption.Add(failsInPrevious12Months);
                }
            }

            //nearMissesInPrevious12Months
            PlantOption nearMissesInPrevious12MonthsOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "How many near misses in previous 12 Months");
            PlantOption nearMissesInPrevious12Months = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, sccSchemeView.NearMissesInLast12Months);

            if (nearMissesInPrevious12MonthsOption == null)
            {
                if (nearMissesInPrevious12Months.Option == null)
                {
                    errorList.Add("How many near misses in previous 12 Months is required");
                }
                else
                {
                    db.PlantOption.Add(nearMissesInPrevious12Months);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, nearMissesInPrevious12Months.Option.Criteria.Name, "", nearMissesInPrevious12Months.Option.Name);
                }
            }
            else
            {
                if (nearMissesInPrevious12Months.Option == null)
                {
                    errorList.Add("How many near misses in previous 12 Months is required");
                }
                else
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, nearMissesInPrevious12Months.Option.Criteria.Name, nearMissesInPrevious12Months.Option.Name, nearMissesInPrevious12Months.Option.Name);
                    db.PlantOption.Remove(nearMissesInPrevious12MonthsOption);
                    db.PlantOption.Add(nearMissesInPrevious12Months);
                }
            }

            //numberOfEnforcementsInLast12Months
            PlantOption numberOfEnforcementsInLast12MonthsOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Number of Enforcement Notices Issued in Last 12 Months");
            PlantOption numberOfEnforcementsInLast12Months = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, sccSchemeView.EnforcementNoticesIssuedLast12Months);

            if (numberOfEnforcementsInLast12MonthsOption == null)
            {
                if (numberOfEnforcementsInLast12Months.Option == null)
                {
                    errorList.Add("Number of Enforcement Notices Issued in Last 12 Months is required");
                }
                else
                {
                    db.PlantOption.Add(numberOfEnforcementsInLast12Months);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, numberOfEnforcementsInLast12Months.Option.Criteria.Name, "", numberOfEnforcementsInLast12Months.Option.Name);
                }
            }
            else
            {
                if (numberOfEnforcementsInLast12Months.Option == null)
                {
                    errorList.Add("Number of Enforcement Notices Issued in Last 12 Months is required");
                }
                else
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, numberOfEnforcementsInLast12Months.Option.Criteria.Name, numberOfEnforcementsInLast12Months.Option.Name, numberOfEnforcementsInLast12Months.Option.Name);
                    db.PlantOption.Remove(numberOfEnforcementsInLast12MonthsOption);
                    db.PlantOption.Add(numberOfEnforcementsInLast12Months);
                }
            }

            //numberOfPenaltyNoticesInLast12Months
            PlantOption numberOfPenaltyNoticesInLast12MonthsOption = schemeService.GetCurrentPlantOption(plantID, currentScheme.SchemeId, "Number of Penalty Notices Issued in Last 12 Months");
            PlantOption numberOfPenaltyNoticesInLast12Months = schemeService.CreateNewPlantOption(plantID, currentScheme.SchemeId, sccSchemeView.NumberPenaltyNoticesIssuedInLast12Months);

            if (numberOfPenaltyNoticesInLast12MonthsOption == null)
            {
                if (numberOfPenaltyNoticesInLast12Months.Option == null)
                {
                    errorList.Add("Number of Penalty Notices Issued in Last 12 Months is required");
                }
                else
                {
                    db.PlantOption.Add(numberOfPenaltyNoticesInLast12Months);
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, numberOfPenaltyNoticesInLast12Months.Option.Criteria.Name, "", numberOfPenaltyNoticesInLast12Months.Option.Name);
                }
            }
            else
            {
                if (numberOfPenaltyNoticesInLast12Months.Option == null)
                {
                    errorList.Add("Number of Penalty Notices Issued in Last 12 Months is required");
                }
                else
                {
                    auditService.Log(currentPlant.PlantId, currentUser, currentScheme.Name, numberOfPenaltyNoticesInLast12Months.Option.Criteria.Name, numberOfPenaltyNoticesInLast12Months.Option.Name, numberOfPenaltyNoticesInLast12Months.Option.Name);
                    db.PlantOption.Remove(numberOfPenaltyNoticesInLast12MonthsOption);
                    db.PlantOption.Add(numberOfPenaltyNoticesInLast12Months);
                }
            }

            if (errorList.Count > 0)
            {
                TempData["errorList"] = errorList;
                
                return RedirectToAction("Details", "Plant", new { id = plantID, scheme = currentScheme.Name });
            }
            else
            {
                db.SaveChanges();

                return RedirectToAction("RiskScoreUpdate", new { plantIDPass = plantID, schemeIDPass = currentScheme.SchemeId });
            }
        }

        public ActionResult RiskScoreUpdate(Guid plantIDPass, Guid schemeIDPass)
        {

            RiskScoreUpdateCalculation(plantIDPass, schemeIDPass);


            return RedirectToAction("Index", "Plant", new { area = "" });



        }

        public ActionResult CaclulateAllRiskScores()
        {
            List<Plant> plantList = db.Plant.Where(x => x.Active == true).ToList();

            List<Scheme> schemeList = db.Scheme.Where(x => x.Active == true).ToList();

            foreach (var plant in plantList)
            {
                if (plant.BLS == true)
                {
                    RiskScoreUpdateCalculation(plant.PlantId, schemeList.Where(x => x.Name == "BLS").Select(p => p.SchemeId).FirstOrDefault());
                }
                if (plant.BCC == true)
                {
                    RiskScoreUpdateCalculation(plant.PlantId, schemeList.Where(x => x.Name == "BCC").Select(p => p.SchemeId).FirstOrDefault());
                }
                if (plant.PCG == true)
                {
                    RiskScoreUpdateCalculation(plant.PlantId, schemeList.Where(x => x.Name == "PCG").Select(p => p.SchemeId).FirstOrDefault());
                }
                if (plant.SCC == true)
                {
                    RiskScoreUpdateCalculation(plant.PlantId, schemeList.Where(x => x.Name == "SCC").Select(p => p.SchemeId).FirstOrDefault());
                }
            }

            return RedirectToAction("Index", "Admin", new { area = "" });


        }

        private void RiskScoreUpdateCalculation(Guid plantIDPass, Guid schemeIDPass)
        {
            Scheme currentScheme = db.Scheme.Find(schemeIDPass);
            String currentUser = UserHelper.CurrentUser();

            //Recalculate the Risk Score

            if (currentScheme.Name == "BLS")
            {
                BlsRiskCalculation blsRiskCalculation = new BlsRiskCalculation();
                List<PlantOption> blsPlantOps = db.PlantOption.Where(x => x.PlantId == plantIDPass && x.SchemeId == currentScheme.SchemeId).ToList();

                int riskScoreInt = blsRiskCalculation.Calculation(blsPlantOps);

                RiskScore existingRiskScore = db.RiskScore.Where(x => x.PlantId == plantIDPass && x.SchemeId == currentScheme.SchemeId).FirstOrDefault();

                RiskScore newriskScore = new RiskScore
                {
                    PlantId = plantIDPass,
                    SchemeId = currentScheme.SchemeId,
                    Score = riskScoreInt,
                    DateCalculated = DateTime.UtcNow
                };

                if (existingRiskScore == null)
                {
                    db.RiskScore.Add(newriskScore);
                    auditService.Log(plantIDPass, currentUser, currentScheme.Name, "Risk Score", "0", newriskScore.Score.ToString());
                }
                else
                {
                    auditService.Log(plantIDPass, currentUser, currentScheme.Name, "Risk Score", existingRiskScore.Score.ToString(), newriskScore.Score.ToString());
                    db.RiskScore.Remove(existingRiskScore);
                    db.RiskScore.Add(newriskScore);
                }

                db.SaveChanges();
            }

            if (currentScheme.Name == "BCC")
            {
                BccRiskCalculation bccRiskCalculation = new BccRiskCalculation();
                List<PlantOption> bccPlantOps = db.PlantOption.Where(x => x.PlantId == plantIDPass && x.SchemeId == currentScheme.SchemeId).ToList();

                int riskScoreInt = bccRiskCalculation.Calculation(bccPlantOps);

                RiskScore existingRiskScore = db.RiskScore.Where(x => x.PlantId == plantIDPass && x.SchemeId == currentScheme.SchemeId).FirstOrDefault();

                RiskScore newriskScore = new RiskScore
                {
                    PlantId = plantIDPass,
                    SchemeId = currentScheme.SchemeId,
                    Score = riskScoreInt,
                    DateCalculated = DateTime.UtcNow
                };

                if (existingRiskScore == null)
                {
                    db.RiskScore.Add(newriskScore);
                    auditService.Log(plantIDPass, currentUser, currentScheme.Name, "Risk Score", "0", newriskScore.Score.ToString());
                }
                else
                {
                    auditService.Log(plantIDPass, currentUser, currentScheme.Name, "Risk Score", existingRiskScore.Score.ToString(), newriskScore.Score.ToString());
                    db.RiskScore.Remove(existingRiskScore);
                    db.RiskScore.Add(newriskScore);
                }

                db.SaveChanges();
            }

            if (currentScheme.Name == "PCG")
            {
                PcgRiskCalculation pcgRiskCalculation = new PcgRiskCalculation();
                List<PlantOption> pcgPlantOps = db.PlantOption.Where(x => x.PlantId == plantIDPass && x.SchemeId == currentScheme.SchemeId).ToList();

                int riskScoreInt = pcgRiskCalculation.Calculation(pcgPlantOps);

                RiskScore existingRiskScore = db.RiskScore.Where(x => x.PlantId == plantIDPass && x.SchemeId == currentScheme.SchemeId).FirstOrDefault();

                RiskScore newriskScore = new RiskScore
                {
                    PlantId = plantIDPass,
                    SchemeId = currentScheme.SchemeId,
                    Score = riskScoreInt,
                    DateCalculated = DateTime.UtcNow
                };

                if (existingRiskScore == null)
                {
                    db.RiskScore.Add(newriskScore);
                    auditService.Log(plantIDPass, currentUser, currentScheme.Name, "Risk Score", "0", newriskScore.Score.ToString());
                }
                else
                {
                    auditService.Log(plantIDPass, currentUser, currentScheme.Name, "Risk Score", existingRiskScore.Score.ToString(), newriskScore.Score.ToString());
                    db.RiskScore.Remove(existingRiskScore);
                    db.RiskScore.Add(newriskScore);
                }

                db.SaveChanges();
            }

            if (currentScheme.Name == "SCC")
            {
                SccRiskCalculation calculator = new SccRiskCalculation();
                List<PlantOption> pcgPlantOps = db.PlantOption.Where(x => x.PlantId == plantIDPass && x.SchemeId == currentScheme.SchemeId).ToList();

                int riskScoreInt = calculator.Calculation(pcgPlantOps);

                RiskScore existingRiskScore = db.RiskScore.Where(x => x.PlantId == plantIDPass && x.SchemeId == currentScheme.SchemeId).FirstOrDefault();

                RiskScore newriskScore = new RiskScore
                {
                    PlantId = plantIDPass,
                    SchemeId = currentScheme.SchemeId,
                    Score = riskScoreInt,
                    DateCalculated = DateTime.UtcNow
                };

                if (existingRiskScore == null)
                {
                    db.RiskScore.Add(newriskScore);
                    auditService.Log(plantIDPass, currentUser, currentScheme.Name, "Risk Score", "0", newriskScore.Score.ToString());
                }
                else
                {
                    auditService.Log(plantIDPass, currentUser, currentScheme.Name, "Risk Score", existingRiskScore.Score.ToString(), newriskScore.Score.ToString());
                    db.RiskScore.Remove(existingRiskScore);
                    db.RiskScore.Add(newriskScore);
                }

                db.SaveChanges();
            }
        }




        public ActionResult _Errors(List<string> errorList)
        {
            return PartialView(errorList);
        }
    }
}