using EF.Audit;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace RPA.MTSInspections.Models
{

    public class PlantOption
    {
        [Key]
        [Column(Order = 1)]
        public Guid PlantId { get; set; }
        [Key]
        [Column(Order = 2)]
        public Guid SchemeId { get; set; }
        [Key]
        [Column(Order = 3)]
        public Guid OptionId { get; set; }

        public virtual Plant Plant { get; set; }

        public virtual Scheme Scheme { get; set; }

        public virtual Option Option { get; set; }

      
    }
}