using System.Collections.Generic;
using System.Linq;
using Hospitality;
using HospitalityArchitect.Facilities;
using RimWorld;
using Verse;
using Verse.AI;

namespace HospitalityArchitect;

public class RoomRequirement_Rating : RoomRequirement
{
    public int rating;

    public override string Label(Room r = null)
    {
        return (!labelKey.NullOrEmpty() ? (string)labelKey.Translate() : "Need room rating " + rating);
    }

    public override bool Met(Room r, Pawn p = null)
    {
        Building_GuestBed gb = r.ContainedBeds.FirstOrFallback(null) as Building_GuestBed;
        if (gb == null) return false;
        int value = BedUtility.StaticBedValue(gb,out _, out _, out _, out _, out _, out _);
        return value >= rating;
    }

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (string item in base.ConfigErrors())
        {
            yield return item;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref rating, "rating");
    }    
}