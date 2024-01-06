using RimWorld;
using Verse;
using Verse.AI;

namespace HospitalityArchitect
{
    public class Building_Computer : Building
    {
        private CompPowerTrader powerComp;

        public bool CanUseComputerNow
        {
            get
            {
                if (Spawned && Map.gameConditionManager.ElectricityDisabled)
                    return false;
                return powerComp == null || powerComp.PowerOn;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
        }

        private void UseAct(Pawn myPawn, ICommunicable commTarget)
        {
            var job = JobMaker.MakeJob(HADefOf.HA_UseComputer, (LocalTargetInfo)(Thing)this);
            job.commTarget = commTarget;
            myPawn.jobs.TryTakeOrderedJob(job);
        }

        private FloatMenuOption GetFailureReason(Pawn myPawn)
        {
            if (!myPawn.CanReach((LocalTargetInfo)(Thing)this, PathEndMode.InteractionCell, Danger.Some))
                return new FloatMenuOption("CannotUseNoPath".Translate(), null);
            if (Spawned && Map.gameConditionManager.ElectricityDisabled)
                return new FloatMenuOption("CannotUseSolarFlare".Translate(), null);
            if (powerComp != null && !powerComp.PowerOn)
                return new FloatMenuOption("CannotUseNoPower".Translate(), null);
            if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
                return new FloatMenuOption(
                    "CannotUseReason".Translate(
                        "IncapableOfCapacity".Translate((NamedArgument)PawnCapacityDefOf.Sight.label,
                            myPawn.Named("PAWN"))), null);
            if (CanUseComputerNow)
                return null;
            Log.Error(myPawn + " could not use comm console for unknown reason.");
            return new FloatMenuOption("Cannot use now", null);
        }
    }
}