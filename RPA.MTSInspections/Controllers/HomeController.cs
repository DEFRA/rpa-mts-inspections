using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using RPA.MTSInspections.Services;

namespace RPA.MTSInspections.Controllers
{
    public class HomeController : Controller
    {
        [Authorize(Policy = MtsPolicies.ReadAccess)]
        public IActionResult Index()
        {
            ViewBag.Location = "Home";

            return View();
        }
    }
}