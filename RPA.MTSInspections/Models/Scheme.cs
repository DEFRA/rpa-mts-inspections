using EF.Audit;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RPA.MTSInspections.Models
{

    public class Scheme
    {
        [Key]
        public Guid SchemeId { get; set; }
        public string Name { get; set; }
        public int Weighting { get; set; }
        public bool Active { get; set; }
        public List<PlantOption> PlantOptions { get; set; }
    }
}