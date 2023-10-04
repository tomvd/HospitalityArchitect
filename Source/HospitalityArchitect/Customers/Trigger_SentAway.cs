using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace HospitalityArchitect
{
    public class Trigger_SentAway : Trigger
    {
        public override bool ActivateOn(Lord lord, TriggerSignal signal)
        {
            if (signal.type == TriggerSignalType.Tick && Find.TickManager.TicksAbs % 250 == 0)
            {
                return lord?.ownedPawns.Any(SentAway) == true;
            }
            return false;
        }

        // a vistior leaves when:
        // recreation need is full (positive outcome)
        // timeout is reached (negative outcome)
        private static bool SentAway(Pawn pawn)
        {
            //Log.Message($"SentAway? HasHediffsNeedingTend? {pawn.health.HasHediffsNeedingTend()} ShouldSeekMedicalRest? {HealthAIUtility.ShouldSeekMedicalRest(pawn)} pawn.health.surgeryBills.Count? {pawn.health.surgeryBills.Count } pawn.health.healthState? {pawn.health.healthState}");
            if (pawn?.Map == null) return false; // has not arrived yet...
            
            var canbedismissed =  pawn.needs.joy.CurLevel >= pawn.needs.joy.MaxLevel*0.99;
            //Log.Message("result=" + canbedismissed);
            // sometimes customers have to be reminded to SpendMoney :)
            if (pawn.IsCustomer(out _) && !canbedismissed && (pawn.mindState.duty == null))
            {
                Log.Message("mindState duty was " + pawn.mindState.duty?.def.defName.ToStringSafe());
                CustomerUtility.SetCustomerDuty(pawn);
            }
            return canbedismissed || !pawn.IsCustomer(out _);
        }
    }
}