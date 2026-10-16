using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace RPA.MTSInspections.Controllers
{
    public class HomeController : Controller
    {
        [Authorize(Roles = "MTS Risk Analysis: Basic Access")]
        public IActionResult Index()
        {
            ViewBag.Location = "Home";

            return View();
        }
    }
}