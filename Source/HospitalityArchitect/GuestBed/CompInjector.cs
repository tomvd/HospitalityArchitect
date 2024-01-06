using System;
using System.Linq;
using Hospitality;
using RimWorld;
using Verse;

namespace HospitalityArchitect;

[StaticConstructorOnStartup]
public static class InjectHotelGuestBedComp
{
    static InjectHotelGuestBedComp()
    {
        var defs = DefDatabase<ThingDef>.AllDefsListForReading.Where(d => d.thingClass != null && d.thingClass == typeof(Building_Bed)).ToList();
        defs.RemoveDuplicates();

        foreach (var def in defs)
        {
            if (def.comps == null) continue;

            if (!def.comps.Any(c => c.GetType() == typeof(CompProperties_HotelGuestBed)))
            {
                def.comps.Add((CompProperties)Activator.CreateInstance(typeof(CompProperties_HotelGuestBed)));
                Log.Message(def.defName+": hotel bed");
            }
        }
    }
}