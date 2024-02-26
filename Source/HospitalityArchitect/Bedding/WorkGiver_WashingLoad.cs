using RimWorld;
using Verse;
using Verse.AI;

namespace HospitalityArchitect;


public class WorkGiver_WashingLoad : WorkGiver_Scanner
{
    public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(HADefOf.WashingMachine);

    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (!(t is Building_WashingMachine building_WashingMachine))
        {
            return false;
        }
        if (t.IsForbidden(pawn) || !pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger()))
        {
            return false;
        }
        if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
        {
            return false;
        }
        if (!building_WashingMachine.HasSpace)
        {
            JobFailReason.Is("WashingNoSpace".Translate());
            return false;
        }
        if (FindClothes(pawn, (IStoreSettingsParent)t) == null)
        {
            JobFailReason.Is("No dirty bedding found");
            return false;
        }
        return !t.IsBurning();
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        Thing thing = FindClothes(pawn, t as IStoreSettingsParent);
        if (thing == null)
        {
            JobFailReason.Is("No dirty bedding found");
            return null;
        }
        Job job = JobMaker.MakeJob(HADefOf.LoadWashing, t, thing);
        job.count = 1;
        return job;
    }

    private bool Predicate(Thing x, Pawn pawn, IStoreSettingsParent dude)
    {
        return !x.IsForbidden(pawn) && pawn.CanReserve(x);
    }

    private Thing FindClothes(Pawn pawn, IStoreSettingsParent dude)
    {
        return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(HADefOf.DirtyBedding), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, (Thing x) => Predicate(x, pawn, dude));
    }
}