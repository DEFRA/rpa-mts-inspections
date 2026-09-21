using System;
using System.Collections.Generic;
using System.Web.Mvc;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.ViewModels;

namespace RPA.MTSInspections.SL
{
    public interface ISchemeService
    {
        List<PlantOption> GetPlantOptions(Guid plantId, Guid schemeId);

        List<SelectListItem> GetDropDownOptions(string crtieria, Guid schemeId, PlantOption plantOption);

        PlantOption GetCurrentPlantOption(Guid plantID, Guid schemeID, string criteriaName);

        PlantOption CreateNewPlantOption(Guid plantID, Guid SchemeId, Guid optionId);

        SelectOption GetCheckOption(Guid plantID, Guid schemeId, string optionName);

        PlantOption GetCurrentNoPOption(Guid plantID, Guid schemeID, string optionName);

        LastInspection GetInspectionDate(Guid plantID, Guid schemeId);
    }
}