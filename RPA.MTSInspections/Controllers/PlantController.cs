using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.SL;
using RPA.MTSInspections.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace RPA.MTSInspections.Controllers
{
    public class PlantController : Controller
    {
        MTSInspectionsContext db;

        IPlantService PlantService;

        public PlantController(MTSInspectionsContext context, IPlantService plantService)
        {
            this.db = context;
            this.PlantService = plantService;
        }


        // GET: Plant
        public IActionResult Index(string searchstring = null, int page = 1, int pageSize = 15)
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
        public IActionResult Details(Guid id, string scheme)
        {
            Plant plant = db.Plant.Find(id);

            ViewBag.currentScheme = scheme;

            ViewBag.errorList = TempData["errorList"];

            return View(plant);
        }

    }
}