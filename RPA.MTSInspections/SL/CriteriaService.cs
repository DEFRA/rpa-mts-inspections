using RPA.MTSInspections.DAL;
using RPA.MTSInspections.Models;
using RPA.MTSInspections.SL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RPA.MTSInspections.SL
{
    public class CriteriaService : ICriteriaService
    {

        MTSInspectionsContext db;

        public CriteriaService(MTSInspectionsContext context)
        {
            this.db = context;
        }


        //Get BLS Criteria Select List

        public List<SelectListItem> GetBlsSelectCriteria()
        {
            List<SelectListItem> blsCriteria = new List<SelectListItem>();

            List<Criteria> existingblsCriteria = db.Criteria.Where(x => x.Scheme.Name == "BLS").OrderBy(x => x.Order).ToList();

            foreach(Criteria item in existingblsCriteria)
            {
                SelectListItem newOne = new SelectListItem { Text = item.Name, Value = item.CriteriaId.ToString() };

                blsCriteria.Add(newOne);

            }
            return blsCriteria;
        }

        //Get BLS Criteria List

        public List<Criteria> GetBlsCriteria()
        {
            
            List<Criteria> blsCriteria = db.Criteria.Where(x => x.Scheme.Name == "BLS").OrderBy(x => x.Order).ToList();

            return blsCriteria;
        }

        
        //Get BCC Criteria Select List

        public List<SelectListItem> GetBccSelectCriteria()
        {
            List<SelectListItem> bccCriteria = new List<SelectListItem>();

            List<Criteria> existingbccCriteria = db.Criteria.Where(x => x.Scheme.Name == "BCC").OrderBy(x => x.Order).ToList();

            foreach (Criteria item in existingbccCriteria)
            {
                SelectListItem newOne = new SelectListItem { Text = item.Name, Value = item.CriteriaId.ToString() };

                bccCriteria.Add(newOne);

            }
            return bccCriteria;
        }

        //Get BCC Criteria List

        public List<Criteria> GetBccCriteria()
        {

            List<Criteria> bccCriteria = db.Criteria.Where(x => x.Scheme.Name == "BCC").OrderBy(x => x.Order).ToList();

            return bccCriteria;
        }

        //Get PCG Criteria Select List

        public List<SelectListItem> GetPcgSelectCriteria()
        {
            List<SelectListItem> pcgCriteria = new List<SelectListItem>();

            List<Criteria> existingpcgCriteria = db.Criteria.Where(x => x.Scheme.Name == "PCG").OrderBy(x => x.Order).ToList();

            foreach (Criteria item in existingpcgCriteria)
            {
                SelectListItem newOne = new SelectListItem { Text = item.Name, Value = item.CriteriaId.ToString() };

                pcgCriteria.Add(newOne);

            }

            return pcgCriteria;
        }

        //Get SCC Criteria Select List

        public List<SelectListItem> GetSccSelectCriteria()
        {
            List<SelectListItem> sccCriteria = new List<SelectListItem>();

            List<Criteria> existingsccCriteria = db.Criteria.Where(x => x.Scheme.Name == "SCC").OrderBy(x => x.Order).ToList();

            foreach (Criteria item in existingsccCriteria)
            {
                SelectListItem newOne = new SelectListItem { Text = item.Name, Value = item.CriteriaId.ToString() };

                sccCriteria.Add(newOne);

            }

            return sccCriteria;
        }

        //Get PCG Criteria List

        public List<Criteria> GetPcgCriteria()
        {

            List<Criteria> pcgCriteria = db.Criteria.Where(x => x.Scheme.Name == "PCG").OrderBy(x => x.Order).ToList();

            return pcgCriteria;
        }

        //Get SCC Criteria List

        public List<Criteria> GetSccCriteria()
        {

            List<Criteria> sccCriteria = db.Criteria.Where(x => x.Scheme.Name == "SCC").OrderBy(x => x.Order).ToList();

            return sccCriteria;
        }
    }
}