using EF.Audit;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RPA.MTSInspections.Models
{


    public class Plant
    {

        [Key]
        public Guid PlantId { get; set; }

        [Required]
        [Display(Name = "Plant Name")]
        public string PlantName { get; set; }

        [Display(Name = "Address Line 1")]
        public string AddressOne { get; set; }

        [Display(Name = "Address Line 2")]
        public string AddressTwo { get; set; }

        [Display(Name = "Town/City")]
        public string TownCity { get; set; }

        [Display(Name = "County")]
        public string County { get; set; }

        [Display(Name = "Post Code")]
        public string PostCode { get; set; }

        [Display(Name = "Licence Number")]
        [Required]
        public string LicenceNo { get; set; }

        [Display(Name = "Plant Active?")]
        public bool Active { get; set; }

        public bool BCC { get; set; }
        public bool BLS { get; set; }
        public bool PCG { get; set; }
        public bool SCC { get; set; }


        public virtual List<PlantOption> PlantOptions { get; set; }

        public virtual List<LastInspection> LastInspection { get; set; }

        public virtual List<RiskScore> RiskScore { get; set; }
        public Plant()
        {
            PlantId = Guid.NewGuid();
            Active = true;
        }
    }
}