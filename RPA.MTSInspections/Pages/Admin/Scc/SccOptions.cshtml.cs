using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.Services;
using RPA.MTSInspections.SL.Interfaces;

namespace RPA.MTSInspections.Pages.Admin.Scc;

[Authorize(Policy = MtsPolicies.AdminOnly)]
public class SccOptionsModel : PageModel
{
    private readonly ICriteriaService criteriaService;

    public SccOptionsModel(ICriteriaService criteriaService)
    {
        this.criteriaService = criteriaService;
    }

    public List<Criteria> CriteriaList { get; private set; }

    public void OnGet()
    {
        CriteriaList = criteriaService.GetSccCriteria();
    }
}
