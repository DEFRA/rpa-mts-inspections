using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RPA.MTSInspections.Models
{
    public class BccRiskCalculation : RiskCalculation
    {
        PlantOption throughput;
        PlantOption opHours;
        PlantOption preSignIn;
        PlantOption recKeep;
        PlantOption lineClear;
        PlantOption carcassHang;
        PlantOption trimming;
        PlantOption hidePuller;
        PlantOption carcassDress;
        PlantOption noNearMiss;
        PlantOption noFailedThree;
        PlantOption failedLastVisit;
        PlantOption noOfWeeks;

        protected override bool IsPlantOptionValid(List<PlantOption> plantOption)
        {

            throughput = plantOption.Where(x => x.Option.Criteria.Name == "Throughput").FirstOrDefault();
            opHours = plantOption.Where(x => x.Option.Criteria.Name == "Operating Hours").FirstOrDefault();
            preSignIn = plantOption.Where(x => x.Option.Criteria.Name == "Pre Inspection Sign In").FirstOrDefault();
            recKeep = plantOption.Where(x => x.Option.Criteria.Name == "Record Keeping").FirstOrDefault();
            lineClear = plantOption.Where(x => x.Option.Criteria.Name == "Line Clearing").FirstOrDefault();
            carcassHang = plantOption.Where(x => x.Option.Criteria.Name == "Carcass Hanging Method").FirstOrDefault();
            trimming = plantOption.Where(x => x.Option.Criteria.Name == "Trimming").FirstOrDefault();
            hidePuller = plantOption.Where(x => x.Option.Criteria.Name == "Hide Puller").FirstOrDefault();
            carcassDress = plantOption.Where(x => x.Option.Criteria.Name == "Carcass Dressing").FirstOrDefault();
            noNearMiss = plantOption.Where(x => x.Option.Criteria.Name == "Number of Near Misses in Previous 3 Years").FirstOrDefault();
            noFailedThree = plantOption.Where(x => x.Option.Criteria.Name == "Number of Failed/Unsatisfactory in Previous 3 Years").FirstOrDefault();
            failedLastVisit = plantOption.Where(x => x.Option.Criteria.Name == "Failed/Unsatisfactory on Last Visit").FirstOrDefault();
            noOfWeeks = plantOption.Where(x => x.Option.Criteria.Name == "Number of Weeks Since Last Inspection").FirstOrDefault();

            //check none are null

            if (throughput != null && opHours != null && preSignIn != null && recKeep != null && lineClear != null && carcassHang != null && trimming != null && hidePuller != null && carcassDress != null
                && noNearMiss != null && noFailedThree != null && failedLastVisit != null && noOfWeeks != null)

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

            //check that all fields have values(see list above)

            if (!IsPlantOptionValid(plantOption))
            {
                return 0;
            }

            //set default risk score

            int RiskScore = 0;

            //Multiply Rating by Weighting for each Criteria

            int ThroughputScore = throughput.Option.Score * throughput.Option.Criteria.Weighting;

            int OpHoursScore = opHours.Option.Score * opHours.Option.Criteria.Weighting;

            int preSignInScore = preSignIn.Option.Score * preSignIn.Option.Criteria.Weighting;

            int recKeepScore = recKeep.Option.Score * recKeep.Option.Criteria.Weighting;

            int lineClearScore = lineClear.Option.Score * lineClear.Option.Criteria.Weighting;

            int carcassHangScore = carcassHang.Option.Score * carcassHang.Option.Criteria.Weighting;

            int trimmingScore = trimming.Option.Score * trimming.Option.Criteria.Weighting;

            int hidePullerScore = hidePuller.Option.Score * hidePuller.Option.Criteria.Weighting;

            int carcassDressScore = carcassDress.Option.Score * carcassDress.Option.Criteria.Weighting;

            //Calculate Performance Score

            int PerformanceScore = (noNearMiss.Option.Score + noFailedThree.Option.Score + failedLastVisit.Option.Score) * noNearMiss.Option.Criteria.Weighting;


            //Calculate Final BCC Risk Score

            int FinalPlantScore = ThroughputScore + OpHoursScore + preSignInScore + recKeepScore + lineClearScore + carcassHangScore + trimmingScore + hidePullerScore + carcassDressScore;

            int FinalPerformanceScore = PerformanceScore + noOfWeeks.Option.Score;

            RiskScore = FinalPlantScore + FinalPerformanceScore;

            return RiskScore;
        }
    }
}