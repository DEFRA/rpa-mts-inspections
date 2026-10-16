using Microsoft.AspNetCore.Mvc;
using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.SL;
using RPA.MTSInspections.SL.Interfaces;
using RPA.MTSInspections.ViewModels;

namespace RPA.MTSInspections.ViewComponents
{
    // Replaces the SchemeController._SchemeEditPartial child action (Html.RenderAction) used on the plant details screen.
    public class SchemeEditViewComponent : ViewComponent
    {
        MTSInspectionsContext db;
        ISchemeService schemeService;

        public SchemeEditViewComponent(MTSInspectionsContext context, ISchemeService schemeService)
        {
            db = context;
            this.schemeService = schemeService;
        }


        public IViewComponentResult Invoke(Guid PlantId, string scheme)
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

                return View("_BLSViewPartial", BLSschemeView);
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

                return View("_BCCViewPartial", BCCschemeView);
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

                return View("_PCGViewPartial", PCGschemeView);
            }
            else if (currentScheme.Name == "SCC")
            {
                SCCSchemeView SCCschemeView;
                
                // Check if we have preserved form data from a failed submission
                if (ViewContext.TempData.Peek("SCCSchemeView") != null)
                {
                    // Use the preserved data that contains the user's input
                    SCCschemeView = (SCCSchemeView)ViewContext.TempData["SCCSchemeView"];
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

                return View("_SCCViewPartial", SCCschemeView);
            }

            else
            {
                return Content(string.Empty);
            }
        }
    }
}