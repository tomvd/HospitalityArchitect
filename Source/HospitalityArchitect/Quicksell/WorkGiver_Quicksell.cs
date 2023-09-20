using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace HospitalityArchitect;

public class WorkGiver_Quicksell : WorkGiver_Scanner
{
    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public override Danger MaxPathDanger(Pawn pawn)
    {
        return Danger.Deadly;
    }

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
    {
        foreach (Designation item in pawn.Map.designationManager.designationsByDef[HADefOf.Quicksell])
        {
            yield return item.target.Thing;
        }
    }

    public override bool ShouldSkip(Pawn pawn, bool forced = false)
    {
        return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(HADefOf.Quicksell);
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        if (t.def.category != ThingCategory.Item)
        {
            return null;
        }
        if (!t.def.alwaysHaulable)
        {
            return null;
        }
        /*if (!pawn.CanReserve(t, 1, -1, null, forced))
        {
            return null;
        }
        if (t.IsForbidden(pawn))
        {
            return null;
        }*/
        if (t.IsBurning())
        {
            return null;
        }
        foreach (Designation item in pawn.Map.designationManager.AllDesignationsOn(t))
        {
            if (item.def == HADefOf.Quicksell)
            {
                return JobMaker.MakeJob(HADefOf.QuicksellDesignated, t);
            }
        }
        return null;
    }
/*
    public override string PostProcessedGerund(Job job)
    {
        if (job.def == InternalDefOf.ClipPlantDesignated)
        {
            return "ClipGerund".Translate();
        }
        return def.gerund;
    }*/
}