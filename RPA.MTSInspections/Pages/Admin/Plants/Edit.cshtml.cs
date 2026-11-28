using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.Services;
using RPA.MTSInspections.SL;

namespace RPA.MTSInspections.Pages.Admin.Plants
{
    [Authorize(Policy = MtsPolicies.SupportOrAdmin)]
    public class EditModel : PageModel
    {
        private readonly MTSInspectionsContext db;
        private readonly IAuditService auditService;
        private readonly IUser user;

        public EditModel(MTSInspectionsContext db, IAuditService auditService, IUser user)
        {
            this.db = db;
            this.auditService = auditService;
            this.user = user;
        }

        [BindProperty]
        public Plant Plant { get; set; }

        public IActionResult OnGet(Guid guid)
        {
            Plant = db.Plant.Find(guid);

            if (Plant == null)
            {
                return NotFound();
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var plant = Plant;
                    Plant existingPlant = db.Plant.AsNoTracking().Where(x => x.PlantId == plant.PlantId).FirstOrDefault();

                    if (existingPlant == null)
                    {
                        return NotFound();
                    }

                    if (existingPlant.LicenceNo != plant.LicenceNo)
                    {
                        auditService.Log(plant.PlantId, user.Name, "Plant Update", "Licence Number", existingPlant.LicenceNo, plant.LicenceNo);
                    }
                    if (existingPlant.PlantName != plant.PlantName)
                    {
                        auditService.Log(plant.PlantId, user.Name, "Plant Update", "Plant Name", existingPlant.PlantName, plant.PlantName);
                    }
                    if (existingPlant.AddressOne != plant.AddressOne)
                    {
                        auditService.Log(plant.PlantId, user.Name, "Plant Update", "Address Line One", existingPlant.AddressOne, plant.AddressOne);
                    }
                    if (existingPlant.AddressTwo != plant.AddressTwo)
                    {
                        auditService.Log(plant.PlantId, user.Name, "Plant Update", "Address Line Two", existingPlant.AddressTwo, plant.AddressTwo);
                    }
                    if (existingPlant.TownCity != plant.TownCity)
                    {
                        auditService.Log(plant.PlantId, user.Name, "Plant Update", "Town/City", existingPlant.TownCity, plant.TownCity);
                    }
                    if (existingPlant.County != plant.County)
                    {
                        auditService.Log(plant.PlantId, user.Name, "Plant Update", "Address Line Two", existingPlant.AddressTwo, plant.County);
                    }
                    if (existingPlant.PostCode != plant.PostCode)
                    {
                        auditService.Log(plant.PlantId, user.Name, "Plant Update", "Address Line Two", existingPlant.PostCode, plant.PostCode);
                    }
                    if (existingPlant.Active != plant.Active)
                    {
                        auditService.Log(plant.PlantId, user.Name, "Plant Update", "Plant Active", existingPlant.Active.ToString(), plant.Active.ToString());
                    }

                    db.SetModified(plant);
                    db.SaveChanges();
                }
            }
            catch
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            return RedirectToPage("/Admin/Plants/Index");
        }
    }
}
