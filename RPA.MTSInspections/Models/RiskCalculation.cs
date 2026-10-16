using System;
using System.Collections.Generic;
using System.Linq;

namespace RPA.MTSInspections.Models
{
    public abstract class RiskCalculation
    {
        public abstract int Calculation(List<PlantOption> plantOptions);

        protected abstract bool IsPlantOptionValid(List<PlantOption> plantOptions);
    }
}