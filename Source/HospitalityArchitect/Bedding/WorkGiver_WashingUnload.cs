using RimWorld;
using Verse;
using Verse.AI;

namespace HospitalityArchitect;

public class WorkGiver_WashingUnload : WorkGiver_Scanner
{
    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(HADefOf.WashingMachine);

    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (t.IsForbidden(pawn) || !pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger()))
        {
            return false;
        }
        Building_WashingMachine building_WashingMachine = t as Building_WashingMachine;
        if (!t.IsBurning() && building_WashingMachine != null)
        {
            return building_WashingMachine.MustUnload;
        }
        return false;
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        return JobMaker.MakeJob(HADefOf.UnloadWashing, t, null);
    }
}