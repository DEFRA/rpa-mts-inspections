using System.Collections.Generic;
using System.Linq;

namespace RPA.MTSInspections.Models
{
    public class SccRiskCalculation : RiskCalculation
    {
        PlantOption throughput;
        PlantOption opHours;
        PlantOption preSignIn;
        PlantOption trimming;
        PlantOption skinRemoval;
        PlantOption carcassDress;
        PlantOption headRemoval;
        PlantOption feetRemoval;

        PlantOption failedUnsatisfactory;
        PlantOption failsInPrevious12Months;
        PlantOption nearMissesInPrevious12Months;
        PlantOption numberOfEnforcementsInLast12Months;
        PlantOption numberOfPenaltyNoticesInLast12Months;

        protected override bool IsPlantOptionValid(List<PlantOption> plantOption)
        {

            throughput = plantOption.Where(x => x.Option.Criteria.Name == "Throughput").FirstOrDefault();
            opHours = plantOption.Where(x => x.Option.Criteria.Name == "Operating hours").FirstOrDefault();
            preSignIn = plantOption.Where(x => x.Option.Criteria.Name == "Sign in prior to inspection").FirstOrDefault();
            trimming = plantOption.Where(x => x.Option.Criteria.Name == "Trimming").FirstOrDefault();
            skinRemoval = plantOption.Where(x => x.Option.Criteria.Name == "Skin Removal").FirstOrDefault();
            carcassDress = plantOption.Where(x => x.Option.Criteria.Name == "Carcass dressing").FirstOrDefault();
            headRemoval = plantOption.Where(x => x.Option.Criteria.Name == "Head Removal").FirstOrDefault();
            feetRemoval = plantOption.Where(x => x.Option.Criteria.Name == "Feet Removal").FirstOrDefault();
            failedUnsatisfactory = plantOption.Where(x => x.Option.Criteria.Name == "Failed / Unsatisfactory").FirstOrDefault();
            failsInPrevious12Months = plantOption.Where(x => x.Option.Criteria.Name == "How many fails / unsatisfactory in previous 12 Months").FirstOrDefault();
            nearMissesInPrevious12Months = plantOption.Where(x => x.Option.Criteria.Name == "How many near misses in previous 12 Months").FirstOrDefault();
            numberOfEnforcementsInLast12Months = plantOption.Where(x => x.Option.Criteria.Name == "Number of Enforcement Notices Issued in Last 12 Months").FirstOrDefault();
            numberOfPenaltyNoticesInLast12Months = plantOption.Where(x => x.Option.Criteria.Name == "Number of Penalty Notices Issued in Last 12 Months").FirstOrDefault();

            //check none are null
            if (throughput != null && opHours != null && preSignIn != null && trimming != null && 
                skinRemoval != null && carcassDress != null && headRemoval != null && feetRemoval != null &&
                failedUnsatisfactory != null && failsInPrevious12Months != null && nearMissesInPrevious12Months != null &&
                numberOfEnforcementsInLast12Months != null && numberOfPenaltyNoticesInLast12Months != null)
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

            int trimmingScore = trimming.Option.Score * trimming.Option.Criteria.Weighting;

            int skinRemovalScore = skinRemoval.Option.Score * skinRemoval.Option.Criteria.Weighting;

            int carcassDressScore = carcassDress.Option.Score * carcassDress.Option.Criteria.Weighting;

            int headRemovalScore = headRemoval.Option.Score * headRemoval.Option.Criteria.Weighting;

            int feetRemovalScore = feetRemoval.Option.Score * feetRemoval.Option.Criteria.Weighting;

            int failedUnsatisfactoryScore = failedUnsatisfactory.Option.Score * failedUnsatisfactory.Option.Criteria.Weighting;

            int failsInPrevious12MonthsScore = failsInPrevious12Months.Option.Score * failsInPrevious12Months.Option.Criteria.Weighting;

            int nearMissesInPrevious12MonthsScore = nearMissesInPrevious12Months.Option.Score * nearMissesInPrevious12Months.Option.Criteria.Weighting;

            int numberOfEnforcementsInLast12MonthsScore = numberOfEnforcementsInLast12Months.Option.Score * failsInPrevious12Months.Option.Criteria.Weighting;

            int numberOfPenaltyNoticesInLast12MonthsScore = numberOfPenaltyNoticesInLast12Months.Option.Score * failsInPrevious12Months.Option.Criteria.Weighting;

            return ThroughputScore + OpHoursScore + preSignInScore + trimmingScore + skinRemovalScore + carcassDressScore + headRemovalScore + feetRemovalScore +
                failedUnsatisfactoryScore + failsInPrevious12MonthsScore + nearMissesInPrevious12MonthsScore + numberOfEnforcementsInLast12MonthsScore + numberOfPenaltyNoticesInLast12MonthsScore;
        }
    }
}