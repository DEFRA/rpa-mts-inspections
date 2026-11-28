using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.Services;

namespace RPA.MTSInspections.Pages.Plants
{
    [Authorize(Policy = MtsPolicies.ReadAccess)]
    public class DetailsModel : PageModel
    {
        private readonly MTSInspectionsContext db;

        public DetailsModel(MTSInspectionsContext db)
        {
            this.db = db;
        }

        public Plant Plant { get; private set; }

        public IActionResult OnGet(Guid id, string scheme)
        {
            Plant = db.Plant.Find(id);

            ViewData["currentScheme"] = scheme;

            // TempData round-trips lists as arrays
            if (TempData["errorList"] is IEnumerable<string> errors)
            {
                ViewData["errorList"] = errors.ToList();
            }

            if (Plant == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
