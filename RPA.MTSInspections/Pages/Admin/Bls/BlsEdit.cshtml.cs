using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.Services;

namespace RPA.MTSInspections.Pages.Admin.Bls;

[Authorize(Policy = MtsPolicies.AdminOnly)]
public class BlsEditModel : PageModel
{
    private readonly MTSInspectionsContext db;

    public BlsEditModel(MTSInspectionsContext context)
    {
        db = context;
    }

    [BindProperty]
    public Option Option { get; set; }

    public IActionResult OnGet(Guid id)
    {
        Option = db.Option.AsNoTracking().Include(x => x.Criteria).FirstOrDefault(x => x.OptionId == id);

        if (Option == null)
        {
            return NotFound();
        }

        return Page();
    }

    public IActionResult OnPost()
    {
        db.SetModified(Option);

        try
        {
            if (ModelState.IsValid)
            {
                db.SaveChanges();
            }
        }
        catch (RetryLimitExceededException)
        {
            ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
        }

        return RedirectToPage("/Admin/Bls/BlsOptions");
    }
}
