using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RPA.MTSInspections.Models
{
    public class BlsRiskCalculation : RiskCalculation
    {
        PlantOption premisesType;
        PlantOption opHours;
        PlantOption throughput;
        PlantOption origin;
        PlantOption typeofOp;
        PlantOption noOfInsp;
        PlantOption noOfNonComp;
        PlantOption sevOfNonComp;
        PlantOption noOfYears;
        PlantOption nop;
       



        protected override bool IsPlantOptionValid(List<PlantOption> plantOption)
        {


            premisesType = plantOption.Where(x => x.Option.Criteria.Name == "Premises Type").FirstOrDefault();
            opHours = plantOption.Where(x => x.Option.Criteria.Name == "Operating Hours").FirstOrDefault();
            throughput = plantOption.Where(x => x.Option.Criteria.Name == "Throughput").FirstOrDefault();
            origin = plantOption.Where(x => x.Option.Criteria.Name == "Origin of Products Handled").FirstOrDefault();
            typeofOp = plantOption.Where(x => x.Option.Criteria.Name == "Type of Operation").FirstOrDefault();
            noOfInsp = plantOption.Where(x => x.Option.Criteria.Name == "Number of Inspections Resulting in Failure").FirstOrDefault();
            noOfNonComp = plantOption.Where(x => x.Option.Criteria.Name == "Number of Non-Compliance").FirstOrDefault();
            sevOfNonComp = plantOption.Where(x => x.Option.Criteria.Name == "Severity of Non-Compliance").FirstOrDefault();
            noOfYears = plantOption.Where(x => x.Option.Criteria.Name == "Number of Months Since Last Inspection").FirstOrDefault();
            

            if (premisesType != null && opHours != null && throughput != null && origin != null && typeofOp != null && noOfInsp != null && noOfNonComp != null && sevOfNonComp != null && noOfYears != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int Calculation(List<PlantOption> plantOption)
        {

            //check that Premises Type, Op Hours, Throughput, Origin, Type of Op, No of Insp, No of Non Comp, sev of non Comp, No of Yrs Since insp all have values

            if (!IsPlantOptionValid(plantOption))
            {
                return 0;
            }

            //set default risk score

            int RiskScore = 0;


            //Multiply Rating by Weighting for each Criteria


            int PremisesScore = premisesType.Option.Score * premisesType.Option.Criteria.Weighting;

            int OpHoursScore = opHours.Option.Score * opHours.Option.Criteria.Weighting;

            int ThroughputScore = throughput.Option.Score * throughput.Option.Criteria.Weighting;
            
            int TypeOfOpScore = typeofOp.Option.Score * typeofOp.Option.Criteria.Weighting;

            int OriginScore = origin.Option.Score * origin.Option.Criteria.Weighting;

            //Nature of Op Calculation

            int NopScore = 0;

            nop = plantOption.Where(x => x.Option.Criteria.Name == "Nature of Operation").FirstOrDefault();

            int NopWeighting = 0;

            if (nop != null)
            {
                NopWeighting = nop.Option.Criteria.Weighting;
            }

            foreach (PlantOption p in plantOption.Where(x => x.Option.Criteria.Name == "Nature of Operation"))
            {
                if (p.Option.Active == true)
                {
                    NopScore = NopScore + p.Option.Score;
                }
            }


            NopScore = NopScore * NopWeighting;
            

            //Performance Calculation 

            int PerformanceScore = (noOfInsp.Option.Score + noOfNonComp.Option.Score + sevOfNonComp.Option.Score) * noOfInsp.Option.Criteria.Weighting;


            //Calculate Final Risk Score

            int FinalPlantScore = PremisesScore + OpHoursScore + ThroughputScore + TypeOfOpScore + OriginScore + NopScore;

            int FinalPerformanceScore = PerformanceScore + noOfYears.Option.Score;

            RiskScore = FinalPlantScore + FinalPerformanceScore;

            return RiskScore;

        }



    }
}