using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RPA.MTSInspections.Controllers
{
    public class HomeController : Controller
    {
        [Authorize(Roles = "MTS Risk Analysis: Basic Access")]
        public ActionResult Index()
        {
            ViewBag.Location = "Home";

            return View();
        }
    }
}