using EF.Audit;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RPA.MTSInspections.Models
{

    public class LastInspection
    {
        [Key]
        [Column(Order = 0)]
        public Guid PlantId { get; set; }

        [Key]
        [Column(Order = 1)]
        public Guid SchemeId { get; set; }

        [Display(Name = "Date of Last Inspection")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/mm/yyyy}")]
        public DateTime Date { get; set; }

        public virtual Plant Plant { get; set; }

        public virtual Scheme Scheme { get; set;}

        public LastInspection()
        {
            Date = DateTime.UtcNow;
        }
    }
}