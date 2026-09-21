using System;
using System.Collections.Generic;
using RPA.MTSInspections.Models;
using PagedList;
using System.Web.Mvc;

namespace RPA.MTSInspections.SL
{
    public interface IPlantService
    {
        List<Plant> GetPlant();

        PagedList<Plant> GetPlant(string searchString, int page, int pageSize, bool riskNeeded);

        Plant GetPlantById(Guid plantId);
    }
}