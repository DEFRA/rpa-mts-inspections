using EF.Audit;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RPA.MTSInspections.Models
{

    public class RiskScore
    {
        [Key]
        [Column(Order =1)]
        public Guid PlantId { get; set; }
        [Key]
        [Column(Order =2)]
        public Guid SchemeId { get; set; }

        [Display(Name = "Risk Score")]
        public int Score { get; set; }

        public DateTime DateCalculated { get; set; }

        public virtual Plant Plant { get; set; }

        public virtual Scheme Scheme { get; set; }
    }
}