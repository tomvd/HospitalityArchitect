using System.Collections.Generic;
using System.Linq;
using HospitalityArchitect.Facilities;
using RimWorld;
using Verse;
using Verse.AI;

namespace HospitalityArchitect;

public class RoomRequirement_FacilitiesAvailable : RoomRequirement
{
    public List<string> facilities;

    public override string Label(Room r = null)
    {
        return (!labelKey.NullOrEmpty() ? (string)labelKey.Translate() : "Need " + string.Join(",",facilities));
    }

    public override bool Met(Room r, Pawn p = null)
    {
        FacilitiesService fs = r.Map.GetComponent<FacilitiesService>();
        foreach (string f in facilities)
        {
            if (!fs.FacilityAvailable(f))
            {
                return false;
            }
        }
        return true;
    }
    
    public bool Met(Map map)
    {
        FacilitiesService fs = map.GetComponent<FacilitiesService>();
        foreach (string f in facilities)
        {
            if (!fs.FacilityAvailable(f))
            {
                return false;
            }
        }
        return true;
    }    
    
    public bool MetAndOpen(Map map)
    {
        FacilitiesService fs = map.GetComponent<FacilitiesService>();
        foreach (string f in facilities)
        {
            if (!fs.FacilityIsOpen(f))
            {
                return false;
            }
        }
        return true;
    }
    
    public int Capacity(Map map)
    {
        FacilitiesService fs = map.GetComponent<FacilitiesService>();
        foreach (string f in facilities)
        {
            return fs.FacilityCapacity(f); // just return first facility, since this is used for daytime guests with only 1 facility requirement
        }
        return 0;
    }     

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (string item in base.ConfigErrors())
        {
            yield return item;
        }
        if (facilities.NullOrEmpty())
        {
            yield return "facilities are null or empty";
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref facilities, "facilities");
    }    
}