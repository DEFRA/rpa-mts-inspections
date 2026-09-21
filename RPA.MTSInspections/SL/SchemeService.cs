using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using RPA.MTSInspections.ViewModels;

namespace RPA.MTSInspections.SL
{
    public class SchemeService : ISchemeService
    {
        MTSInspectionsContext db = new MTSInspectionsContext();

        public SchemeService()
        {
            this.db = new MTSInspectionsContext();
        }

        public SchemeService(MTSInspectionsContext context)
        {
            this.db = context;
        }

        //Method to get a list of all existing Plant Opptions for this plant/scheme
        public List<PlantOption> GetPlantOptions(Guid plantId, Guid schemeId)
        {

            List<PlantOption> plantOptionList = db.PlantOption.Where(x => x.PlantId == plantId && x.SchemeId == schemeId).ToList();

            return plantOptionList;
        }


        //Method to get the drop down options for each criteria in any scheme and check which existing options are already selected
        public List<SelectListItem> GetDropDownOptions(string criteria, Guid schemeId, PlantOption plantOption)
        {

            List<SelectListItem> list = new List<SelectListItem>();

            list.Add(new SelectListItem() { Text = "", Value = null });

            var CriteriaList = db.Criteria.Where(x => x.Name == criteria && x.SchemeId == schemeId).First().Option.Where(x =>x.Active == true).OrderBy(x => x.Score).ThenBy(x => x.Name).ToList();

            foreach (var item in CriteriaList)
            {
                list.Add(new SelectListItem() { Text = item.Name, Value = item.OptionId.ToString(), Selected = item.OptionId == plantOption?.Option.OptionId });
            }



            return list;
        }

        //Method to check if there are any existing option selected for Nature of Operation Check Boxes (BLS Only)

        public SelectOption GetCheckOption(Guid plantID, Guid schemeId, string optionName)
        {

            PlantOption currentOption = db.PlantOption.Where(x => x.Option.Name == optionName && x.SchemeId == schemeId && x.PlantId == plantID).FirstOrDefault();

            return new SelectOption
            {
                Option = db.Option.Where(x => x.Criteria.SchemeId == schemeId && x.Name == optionName).FirstOrDefault(),
                Selected = currentOption != null
            };


        }

        //Method to find the existing options on POST to update if required

        public PlantOption GetCurrentPlantOption(Guid plantID, Guid schemeID, string criteriaName)
        {

            Plant currentPlant = db.Plant.Where(x => x.PlantId == plantID).FirstOrDefault();

            Guid currentCriteria = db.Criteria.AsNoTracking().Where(x => x.Name == criteriaName && x.SchemeId == schemeID).Select(x => x.CriteriaId).FirstOrDefault();

            PlantOption currentPlantOption = currentPlant.PlantOptions.Where(x => x.PlantId == plantID && x.SchemeId == schemeID && x.Option?.CriteriaId == currentCriteria).FirstOrDefault();


            return currentPlantOption;

        }


        //Method to find the existing options on POST to update if required NOP only

        public PlantOption GetCurrentNoPOption(Guid plantID, Guid schemeID, string optionName)
        {

            return db.PlantOption.Where(x=>x.PlantId == plantID && x.SchemeId == schemeID && x.Option.Name == optionName).FirstOrDefault();
        }


        //Method to find last inspection Date (if there is one)

        public LastInspection GetInspectionDate(Guid plantID, Guid schemeId)
        {
            Plant currentPlant = db.Plant.AsNoTracking().Where(x => x.PlantId == plantID).FirstOrDefault();

            LastInspection Insp = db.LastInspection.AsNoTracking().Where(x => x.PlantId == plantID && x.SchemeId == schemeId).FirstOrDefault();


            if (Insp == null)
            {
                LastInspection newInsp = new LastInspection
                {
                    PlantId = plantID,
                    SchemeId = schemeId
                };
                return newInsp;
            }
            else
            {
                return Insp;
            }
        }


        //Method to create a new plant option if none currently exists for this plant/Criteria
        public PlantOption CreateNewPlantOption(Guid plantID, Guid SchemeId, Guid optionId)
        {
            PlantOption newPlantOption = new PlantOption
            {
                PlantId = plantID,
                Plant = db.Plant.Where(x => x.PlantId == plantID).FirstOrDefault(),
                SchemeId = SchemeId,
                Scheme = db.Scheme.Where(x => x.SchemeId == SchemeId).FirstOrDefault(),
                OptionId = optionId,
                Option = db.Option.Where(x => x.OptionId == optionId).FirstOrDefault()
            };

            return newPlantOption;
        }


    }

}