using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.Services;
using X.PagedList;

namespace RPA.MTSInspections.Pages.Admin.AuditLog
{
    [Authorize(Policy = MtsPolicies.SupportOrAdmin)]
    public class AuditViewModel : PageModel
    {
        private readonly MTSInspectionsContext db;

        public AuditViewModel(MTSInspectionsContext db)
        {
            this.db = db;
        }

        public IPagedList<Audit> Audits { get; private set; }

        public IActionResult OnGet(Guid plantId, string schemeSelected, int page = 1, int pageSize = 15)
        {
            Plant existingPlant = db.Plant.Where(x => x.PlantId == plantId).FirstOrDefault();

            if (existingPlant == null)
            {
                return NotFound();
            }

            var schemeList = new List<SelectListItem>
            {
                new SelectListItem() { Text = "", Value = null }
            };

            if (existingPlant.BCC == true)
            {
                schemeList.Add(new SelectListItem() { Text = "BCC", Value = "1" });
            }
            if (existingPlant.BLS == true)
            {
                schemeList.Add(new SelectListItem() { Text = "BLS", Value = "2" });
            }
            if (existingPlant.PCG == true)
            {
                schemeList.Add(new SelectListItem() { Text = "PCG", Value = "3" });
            }

            ViewData["schemeList"] = schemeList;
            ViewData["schemeSelected"] = schemeSelected;
            ViewData["plantName"] = existingPlant.LicenceNo + " - " + existingPlant.PlantName;
            ViewData["plantId"] = existingPlant.PlantId.ToString();

            IQueryable<Audit> query = db.Audit.Where(x => x.PlantId == plantId);

            string action = schemeSelected switch
            {
                "1" => "BCC",
                "2" => "BLS",
                "3" => "PCG",
                _ => null
            };

            if (action != null)
            {
                query = query.Where(x => x.Action == action);
            }
            else if (!string.IsNullOrEmpty(schemeSelected))
            {
                query = query.Where(x => false);
            }

            Audits = new PagedList<Audit>(query.OrderByDescending(x => x.Date).ToList(), page, pageSize);

            return Page();
        }
    }
}
