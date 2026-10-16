using System;
using System.Collections.Generic;
using RPA.MTSInspections.Models;
using X.PagedList;
using Microsoft.AspNetCore.Mvc;

namespace RPA.MTSInspections.SL
{
    public interface IPlantService
    {
        List<Plant> GetPlant();

        PagedList<Plant> GetPlant(string searchString, int page, int pageSize, bool riskNeeded);

        Plant GetPlantById(Guid plantId);
    }
}