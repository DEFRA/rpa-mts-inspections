using EF.Audit;
using PagedList;
using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.SL;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace RPA.MTSInspections.SL
{
    public class PlantService : IPlantService
    {
        MTSInspectionsContext db = new MTSInspectionsContext();

        public PlantService()
        {
            this.db = new MTSInspectionsContext();
        }

        public PlantService(MTSInspectionsContext context)
        {
            this.db = context;
        }

        public Plant GetPlantById(Guid plantId)
        {
            Plant plant = db.Plant.Where(x => x.PlantId == plantId).FirstOrDefault();

            return plant;
        }

        public List<Plant> GetPlant()
        {
            List<Plant> plantList = db.Plant.ToList();

            return plantList;
        }

        public PagedList<Plant> GetPlant(string searchString, int page, int pageSize, bool riskNeeded)
        {
            List<Plant> plantList;

            if (string.IsNullOrEmpty(searchString))
            {
                if (riskNeeded == true)
                {
                    plantList = db.Plant.Include(x => x.RiskScore).OrderBy(x => x.PlantName).ThenBy(x => x.PostCode).ToList();
                }
                else
                {
                    plantList = db.Plant.OrderBy(x => x.PlantName).ThenBy(x => x.PostCode).ToList();
                }
            }
            else
            {
                if (riskNeeded == true)
                {
                    plantList = db.Plant.Include(x => x.RiskScore).Where(x => x.PlantName.ToUpper().Contains(searchString.ToUpper().Trim()) || x.LicenceNo.ToUpper().Contains(searchString.ToUpper().Trim())).ToList();
                }
                else
                {
                    plantList = db.Plant.Where(x => x.PlantName.ToUpper().Contains(searchString.ToUpper().Trim()) || x.LicenceNo.ToUpper().Contains(searchString.ToUpper().Trim())).ToList();
                }
            }

            PagedList<Plant> pagedPlantList = new PagedList<Plant>(plantList, page, pageSize);

            return pagedPlantList;
        }
    }
}