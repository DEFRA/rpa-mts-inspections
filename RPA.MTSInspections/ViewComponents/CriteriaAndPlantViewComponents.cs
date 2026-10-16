using Microsoft.AspNetCore.Mvc;
using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Models;

namespace RPA.MTSInspections.ViewComponents
{
    // Replace the child actions (Html.RenderAction / Html.Action) used on the criteria and plant screens.

    public class OptionPartialViewComponent : ViewComponent
    {
        private readonly MTSInspectionsContext db;

        public OptionPartialViewComponent(MTSInspectionsContext db)
        {
            this.db = db;
        }

        public IViewComponentResult Invoke(Guid criteriaId)
        {
            List<Option> options = db.Option.Where(x => x.CriteriaId == criteriaId && x.Active == true).ToList();

            ViewBag.criteriaId = criteriaId;

            return View(options);
        }
    }

    public class WeightingScoreViewComponent : ViewComponent
    {
        private readonly MTSInspectionsContext db;

        public WeightingScoreViewComponent(MTSInspectionsContext db)
        {
            this.db = db;
        }

        public IViewComponentResult Invoke(Guid criteriaId)
        {
            Criteria weightingCriteria = db.Criteria.Where(x => x.CriteriaId == criteriaId).FirstOrDefault();

            return View(weightingCriteria);
        }
    }

    public class RiskScoreViewComponent : ViewComponent
    {
        private readonly MTSInspectionsContext db;

        public RiskScoreViewComponent(MTSInspectionsContext db)
        {
            this.db = db;
        }

        public IViewComponentResult Invoke(Guid id, string scheme)
        {
            Guid schemeId = db.Scheme.Where(x => x.Name == scheme).Select(x => x.SchemeId).FirstOrDefault();

            RiskScore riskScore = db.RiskScore.Where(x => x.PlantId == id && x.SchemeId == schemeId).FirstOrDefault();

            return View(riskScore);
        }
    }
}
