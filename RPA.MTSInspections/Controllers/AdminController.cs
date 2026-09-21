using RPA.MTSInspections.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using PagedList;
using RPA.MTSInspections.DAL;
using EF.Audit;
using System.Web.UI.WebControls;
using System.Web.UI;
using RPA.MTSInspections.SL;
using RPA.MTSInspections.SL.Interfaces;
using RPA.MTSInspections.ViewModels;
using RPA.MTSInspections.Helpers;
using System.Reflection;
using System.ComponentModel;

namespace RPA.MTSInspections.Controllers
{
    public class AdminController : Controller
    {
        MTSInspectionsContext db;
        ICriteriaService criteriaService;
        IPlantService plantService;
        IAuditService auditService;

        public AdminController()
        {
            db = new MTSInspectionsContext();
            plantService = new PlantService(db);
            criteriaService = new CriteriaService(db);
            auditService = new AuditService(db);

        }

        public AdminController(MTSInspectionsContext context, ICriteriaService criteriaService, IPlantService plantService, IAuditService auditService)
        {
            db = context;
            this.criteriaService = criteriaService;
            this.plantService = plantService;
            this.auditService = auditService;
        }

        // GET: Support Admin Main Page
        public ActionResult Index()
        {
            return View();
        }

        // GET: Admin Main Page
        public ActionResult AdminIndex()
        {
            return View();
        }

        // GET: BLS List of All existing Criterias
        public ActionResult BlsOptions()
        {
            List<Criteria> blsCriteriaList = criteriaService.GetBlsCriteria();

            return View("~/Views/Admin/Bls/BlsOptions.cshtml", blsCriteriaList);
        }

        // GET: BCC List of All existing Criterias
        public ActionResult BccOptions()
        {
            List<Criteria> bccCriteriaList = criteriaService.GetBccCriteria();

            return View("~/Views/Admin/Bcc/BccOptions.cshtml", bccCriteriaList);
        }

        // GET: PCG List of All existing Criterias
        public ActionResult PcgOptions()
        {
            List<Criteria> pcgCriteriaList = criteriaService.GetPcgCriteria();

            return View("~/Views/Admin/Pcg/PcgOptions.cshtml", pcgCriteriaList);
        }

        // GET: SCC List of All existing Criterias
        public ActionResult SccOptions()
        {
            List<Criteria> sccCriteriaList = criteriaService.GetSccCriteria();

            return View("~/Views/Admin/Scc/SccOptions.cshtml", sccCriteriaList);
        }


        // GET: BLS Edit an option
        [HttpGet]
        public ActionResult BlsEdit(Guid id)
        {
            Option option = db.Option.Find(id);

            return View("~/Views/Admin/Bls/BlsEdit.cshtml", option);
        }

        // GET: Bcc Edit an option
        [HttpGet]
        public ActionResult BccEdit(Guid id)
        {
            Option option = db.Option.Find(id);

            return View("~/Views/Admin/Bcc/BccEdit.cshtml", option);
        }

        // GET: Scc Edit an option
        [HttpGet]
        public ActionResult SccEdit(Guid id)
        {
            Option option = db.Option.Find(id);

            return View("~/Views/Admin/Scc/SccEdit.cshtml", option);
        }

        // GET: Pcg Edit an option
        [HttpGet]
        public ActionResult PcgEdit(Guid id)
        {
            Option option = db.Option.Find(id);

            return View("~/Views/Admin/Pcg/PcgEdit.cshtml", option);
        }


        // GET: Option BLS Create Select List
        [HttpGet]
        public ActionResult BlsCreate()
        {
            var blsCriteriaList = criteriaService.GetBlsSelectCriteria();

            ViewBag.BlsCriteriaList = blsCriteriaList;

            return View("~/Views/Admin/Bls/BlsCreate.cshtml");
        }

        // GET: Option BCC Create Select List
        [HttpGet]
        public ActionResult BccCreate()
        {
            var bccCriteriaList = criteriaService.GetBccSelectCriteria();

            ViewBag.BccCriteriaList = bccCriteriaList;

            return View("~/Views/Admin/Bcc/BccCreate.cshtml");
        }

        // GET: Option PCG Create Select List
        [HttpGet]
        public ActionResult PcgCreate()
        {
            var pcgCriteriaList = criteriaService.GetPcgSelectCriteria();

            ViewBag.PcgCriteriaList = pcgCriteriaList;

            return View("~/Views/Admin/Pcg/PcgCreate.cshtml");
        }

        // GET: Option SCC Create Select List
        [HttpGet]
        public ActionResult SccCreate()
        {
            var sccCriteriaList = criteriaService.GetSccSelectCriteria();

            ViewBag.SccCriteriaList = sccCriteriaList;

            return View("~/Views/Admin/Scc/SccCreate.cshtml");
        }

        // GET: Partial view for existing options for this criteria

        public ActionResult _OptionPartial(Guid criteriaId)
        {
            List<Option> options = db.Option.Where(x => x.CriteriaId == criteriaId && x.Active == true).ToList();

            return PartialView(options);
        }

        //GET: Partial view for weighting score of criteria
        public ActionResult _WeightingScore(Guid criteriaId)
        {
            Criteria weightingCriteria = db.Criteria.Where(x => x.CriteriaId == criteriaId).FirstOrDefault();

            return PartialView(weightingCriteria);
        }

        //GET: Edit Weighting Score
        public ActionResult EditWeightingScore(Guid criteriaId)
        {
            Criteria criteria = db.Criteria.Find(criteriaId);

            return View("~/Views/Admin/EditWeightingScore.cshtml", criteria);
        }

        //****************************//

        //POST METHODS

        //***************************//

        // POST: Edit a BLS Option
        [HttpPost]
        public ActionResult BlsEdit(Option option)
        {

            db.SetModified(option);

            try
            {
                if (ModelState.IsValid)
                {
                    db.SaveChanges();

                    return RedirectToAction("BlsOptions");
                }
            }
            catch (RetryLimitExceededException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            return RedirectToAction("BlsOptions");
        }

        // POST: Edit a BCC Option
        [HttpPost]
        public ActionResult BccEdit(Option option)
        {
            db.SetModified(option);

            try
            {
                if (ModelState.IsValid)
                {
                    db.SaveChanges();

                    return RedirectToAction("BccOptions");
                }
            }
            catch (RetryLimitExceededException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            return RedirectToAction("BccOptions");
        }

        // POST: Edit a SCC Option
        [HttpPost]
        public ActionResult SccEdit(Option option)
        {
            db.SetModified(option);

            try
            {
                if (ModelState.IsValid)
                {
                    db.SaveChanges();

                    return RedirectToAction("SccOptions");
                }
            }
            catch (RetryLimitExceededException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            return RedirectToAction("SccOptions");
        }

        // POST: Edit a PCG Option
        [HttpPost]
        public ActionResult PcgEdit(Option option)
        {
            db.SetModified(option);

            try
            {
                if (ModelState.IsValid)
                {
                    db.SaveChanges();

                    return RedirectToAction("PcgOptions");
                }
            }
            catch (RetryLimitExceededException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            return RedirectToAction("PcgOptions");
        }

        // POST: Option Create new BLS Option
        [HttpPost]
        public ActionResult BlsCreate(Option option)
        {
            db.Option.Add(option);
            db.SaveChanges();

            return RedirectToAction("BlsOptions");
        }

        // POST: Option BCC Create new BCC Option
        [HttpPost]
        public ActionResult BccCreate(Option option)
        {
            db.Option.Add(option);
            db.SaveChanges();

            return RedirectToAction("BccEdit");
        }

        // POST: Option PCG Create new PCG Option
        [HttpPost]
        public ActionResult PcgCreate(Option option)
        {
            db.Option.Add(option);
            db.SaveChanges();

            return RedirectToAction("PcgEdit");
        }

        // POST: Option SCC Create new SCC Option
        [HttpPost]
        public ActionResult SccCreate(Option option)
        {
            db.Option.Add(option);
            db.SaveChanges();

            return RedirectToAction("SccOptions");
        }

        //POST: Edit Weighting score
        [HttpPost]
        public ActionResult EditWeightingScore(Criteria criteria)
        {
            Criteria criteriaChange = db.Criteria.Where(x => x.CriteriaId == criteria.CriteriaId).FirstOrDefault();

            criteriaChange.Weighting = criteria.Weighting;

            db.SetModified(criteriaChange);
            db.SaveChanges();

            Scheme scheme = db.Scheme.Where(x => x.SchemeId == criteria.SchemeId).FirstOrDefault();

            try
            {
                if (ModelState.IsValid)
                {
                    if (scheme.Name == "BLS")
                    {
                        return RedirectToAction("BlsOptions");
                    }
                    else if (scheme.Name == "BCC")
                    {
                        return RedirectToAction("BccOptions");
                    }
                    else //if(scheme.Name == "PCG")
                    {
                        return RedirectToAction("PcgOptions");
                    }
                }
            }
            catch (RetryLimitExceededException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            if (criteria.Name == "BLS")
            {

                return RedirectToAction("BlsOptions");
            }
            else if (criteria.Name == "BCC")
            {

                return RedirectToAction("BccOptions");
            }
            else //if(criteria.Name == "PCG")
            {

                return RedirectToAction("PcgOptions");
            }
        }

        //****************************//

        //PLANT

        //***************************//

        // GET: All Plants
        public ActionResult Plants(string searchstring = null, int page = 1, int pageSize = 15)
        {
            try
            {
                var plant = plantService.GetPlant(searchstring, page, pageSize, false);

                ViewBag.searchData = searchstring;

                return View("~/Views/Admin/Plants/Plants.cshtml", plant);
            }
            catch (Exception ex)
            {
                throw new Exception("PL01: Unable to return Plants", ex);
            }
        }

        // GET: Create Plant
        [HttpGet]
        public ActionResult Create()
        {
            return View("~/Views/Admin/Plants/Create.cshtml");
        }

        // POST: Create Plant
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Plant plant)
        {
            db.Plant.Add(plant);

            db.SaveChanges();

            return RedirectToAction("Plants");
        }

        // GET: Edit Plant
        [HttpGet]
        public ActionResult Edit(Guid guid)
        {
            Plant plant = db.Plant.Find(guid);

            return View("~/Views/Admin/Plants/Edit.cshtml", plant);
        }

        // POST: Edit Plant
        [HttpPost]
        public ActionResult Edit(Plant plant, string action)
        {
            

            try
            {
                if (ModelState.IsValid)
                {
                                        
                    Plant existingPlant = db.Plant.AsNoTracking().Where(x => x.PlantId == plant.PlantId).FirstOrDefault();

                    //Add any changes to the Audit Log

                    if (plant != existingPlant)
                    {
                        if (existingPlant.LicenceNo != plant.LicenceNo)
                        {
                            auditService.Log(plant.PlantId, UserHelper.CurrentUser(), "Plant Update", "Licence Number", existingPlant.LicenceNo, plant.LicenceNo);
                        }
                        if (existingPlant.PlantName != plant.PlantName)
                        {
                            auditService.Log(plant.PlantId, UserHelper.CurrentUser(), "Plant Update", "Plant Name", existingPlant.PlantName, plant.PlantName);
                        }
                        if (existingPlant.AddressOne != plant.AddressOne)
                        {
                            auditService.Log(plant.PlantId, UserHelper.CurrentUser(), "Plant Update", "Address Line One", existingPlant.AddressOne, plant.AddressOne);
                        }
                        if (existingPlant.AddressTwo != plant.AddressTwo)
                        {
                            auditService.Log(plant.PlantId, UserHelper.CurrentUser(), "Plant Update", "Address Line Two", existingPlant.AddressTwo, plant.AddressTwo);
                        }
                        if (existingPlant.TownCity != plant.TownCity)
                        {
                            auditService.Log(plant.PlantId, UserHelper.CurrentUser(), "Plant Update", "Town/City", existingPlant.TownCity, plant.TownCity);
                        }
                        if (existingPlant.County != plant.County)
                        {
                            auditService.Log(plant.PlantId, UserHelper.CurrentUser(), "Plant Update", "Address Line Two", existingPlant.AddressTwo, plant.County);
                        }
                        if (existingPlant.PostCode != plant.PostCode)
                        {
                            auditService.Log(plant.PlantId, UserHelper.CurrentUser(), "Plant Update", "Address Line Two", existingPlant.PostCode, plant.PostCode);
                        }
                        if (existingPlant.Active != plant.Active)
                        {
                            auditService.Log(plant.PlantId, UserHelper.CurrentUser(), "Plant Update", "Plant Active", existingPlant.Active.ToString(), plant.Active.ToString());
                        }
                    }

                    db.SetModified(plant);

                    db.SaveChanges();

                    return RedirectToAction("Plants");
                }
            }
            catch
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            return RedirectToAction("Plants");
        }

        //****************************//

        //AUDIT

        //***************************//


        // GET: Plant
        public ActionResult PlantAudit(string searchstring = null, int page = 1, int pageSize = 15)
        {
            try
            {
                var plant = plantService.GetPlant(searchstring, page, pageSize, true);

                ViewBag.searchData = searchstring;

                return View("~/Views/Admin/Audit/PlantAudit.cshtml", plant);
            }
            catch (Exception ex)
            {
                throw new Exception("PL01: Unable to return Plants", ex);
            }
        }

        // GET: Audit View
        public ActionResult AuditView(Guid plantId, String schemeSelected, int page = 1, int pageSize = 15)
        {
            List<Audit> auditList = new List<Audit>();

            Plant existingPlant = db.Plant.Where(x => x.PlantId == plantId).FirstOrDefault();

            List<SelectListItem> schemeList = new List<SelectListItem>();

            schemeList.Add(new SelectListItem() { Text = "", Value = null });

            if(existingPlant.BCC == true)
            {
                schemeList.Add(new SelectListItem() { Text = "BCC", Value = "1" });
            }
            if(existingPlant.BLS == true)
            {
                schemeList.Add(new SelectListItem() { Text = "BLS", Value = "2" });
            }
            if (existingPlant.PCG == true)
            {
                schemeList.Add(new SelectListItem() { Text = "PCG", Value = "3" });
            }

            ViewBag.schemeList = schemeList;

            ViewBag.schemeSelected = schemeSelected;

            ViewBag.plantName = existingPlant.LicenceNo + " - " + existingPlant.PlantName;

            ViewBag.plantId = existingPlant.PlantId.ToString();

            if (string.IsNullOrEmpty(schemeSelected))
            {
                auditList = db.Audit.Where(x => x.PlantId == plantId).OrderByDescending(x => x.Date).ToList();
            }
            else if (schemeSelected == "1")
            {
                auditList = db.Audit.Where(x => x.PlantId == plantId && x.Action == "BCC").OrderByDescending(x => x.Date).ToList();
            }
            else if (schemeSelected == "2")
            {
                auditList = db.Audit.Where(x => x.PlantId == plantId && x.Action == "BLS").OrderByDescending(x => x.Date).ToList();
            }
            else if (schemeSelected == "3")
            {
                auditList = db.Audit.Where(x => x.PlantId == plantId && x.Action == "PCG").OrderByDescending(x => x.Date).ToList();
            }


            PagedList<Audit> pagedAuditList = new PagedList<Audit>(auditList, page, pageSize);

            return View("~/Views/Admin/Audit/AuditView.cshtml", pagedAuditList);
        }


    }
}