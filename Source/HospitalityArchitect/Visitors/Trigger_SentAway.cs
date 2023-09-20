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

        // fire the patient from the hospital when cured
        private static bool SentAway(Pawn pawn)
        {
            //Log.Message($"SentAway? HasHediffsNeedingTend? {pawn.health.HasHediffsNeedingTend()} ShouldSeekMedicalRest? {HealthAIUtility.ShouldSeekMedicalRest(pawn)} pawn.health.surgeryBills.Count? {pawn.health.surgeryBills.Count } pawn.health.healthState? {pawn.health.healthState}");
            if (pawn?.Map == null) return false; // has not arrived yet...
            
            var canbedismissed =  (!pawn.health.HasHediffsNeedingTend() 
                    && !HealthAIUtility.ShouldSeekMedicalRest(pawn)
                    && pawn.health.surgeryBills.Count == 0
                    && pawn.health.healthState == PawnHealthState.Mobile);
            //Log.Message("result=" + canbedismissed);
            // sometimes patients have to be reminded to stay in bed :)
            if (pawn.IsVisitor(out _) && !canbedismissed && (pawn.mindState.duty == null || !pawn.mindState.duty.def.defName.Equals("Patient")))
            {
                Log.Message("mindState duty was " + pawn.mindState.duty?.def.defName.ToStringSafe());
                pawn.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("Patient"), pawn.Position, 100f);
            }
            // we indicate that the patient is just resting to get his anesthetic worked out
            /*if (pawn.IsPatient() && !canbedismissed && pawn.health.surgeryBills.Count == 0 && pawn.health.healthState == PawnHealthState.Down)
            {
                pawn.Map..treatment = "resting";
            }*/

            return canbedismissed || !pawn.IsVisitor(out _);
        }
    }
}