using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.Services;
using RPA.MTSInspections.SL.Interfaces;

namespace RPA.MTSInspections.Pages.Admin.Bcc;

[Authorize(Policy = MtsPolicies.AdminOnly)]
public class BccOptionsModel : PageModel
{
    private readonly ICriteriaService criteriaService;

    public BccOptionsModel(ICriteriaService criteriaService)
    {
        this.criteriaService = criteriaService;
    }

    public List<Criteria> CriteriaList { get; private set; }

    public void OnGet()
    {
        CriteriaList = criteriaService.GetBccCriteria();
    }
}
