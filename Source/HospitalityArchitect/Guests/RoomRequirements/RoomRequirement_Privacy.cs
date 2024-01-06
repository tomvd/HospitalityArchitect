using System.Collections.Generic;
using RimWorld;
using Verse;

namespace HospitalityArchitect;

public class RoomRequirement_Privacy : RoomRequirement
{
    public bool privateBedRoom;
    public bool privateBathRoom;

    public override string Label(Room r = null)
    {
        return (privateBedRoom && (r==null || !r.OnlyOneBed()))?"Need private bedroom":"" + ((privateBathRoom && r == null || !HasLinkedBathroom(r))?"Need private bathroom":"");
    }

    public override bool Met(Room r, Pawn p = null)
    {
        if (privateBedRoom)
        {
            if (!r.OnlyOneBed()) return false;
            if (privateBathRoom)
            {
                if (!HasLinkedBathroom(r)) return false;
            }
        }
        return true;
    }

    private bool HasLinkedBathroom(Room r)
    {
        Building_Bed bed = r.ContainedBeds.FirstOrFallback(null);
        if (bed == null) return false; // somethings really wrong
        return BedUtility.HasLinkedBathroom(bed);        
    }

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (string item in base.ConfigErrors())
        {
            yield return item;
        }
        if (privateBathRoom && !privateBedRoom)
        {
            yield return "cant request private bathroom without private bedroom";
        }        
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref privateBedRoom, "privateBedRoom");
        Scribe_Values.Look(ref privateBathRoom, "privateBathRoom");
    }    
}