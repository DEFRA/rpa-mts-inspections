using EF.Audit;
using RPA.MTSInspections.DAL;
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
    [Auditable]
    public class PlantController : Controller
    {
        MTSInspectionsContext db = new MTSInspectionsContext();

        IPlantService PlantService;

        public PlantController()
        {
            this.db = new MTSInspectionsContext();
            this.PlantService = new PlantService(db);
        }

        public PlantController(MTSInspectionsContext context, IPlantService plantService)
        {
            this.db = context;
            this.PlantService = plantService;
        }


        // GET: Plant
        public ActionResult Index(string searchstring = null, int page = 1, int pageSize = 15)
        {
            try
            {
                var plant = PlantService.GetPlant(searchstring, page, pageSize, true);

                ViewBag.searchData = searchstring;

                return View(plant);
            }
            catch (Exception ex)
            {
                throw new Exception("PL01: Unable to return Plants", ex);
            }
        }


        // GET: Details
        public ActionResult Details(Guid id, string scheme)
        {
            Plant plant = db.Plant.Find(id);

            ViewBag.currentScheme = scheme;

            ViewBag.errorList = TempData["errorList"];

            return View(plant);
        }

        //GET: Risk Score
        public ActionResult _RiskScorePartial(Guid id, string scheme)
        {
            Guid schemeId = db.Scheme.Where(x => x.Name == scheme).Select(x => x.SchemeId).FirstOrDefault();

            RiskScore riskScore = db.RiskScore.Where(x => x.PlantId == id && x.SchemeId == schemeId).FirstOrDefault();

            return PartialView(riskScore);
        }
    }
}