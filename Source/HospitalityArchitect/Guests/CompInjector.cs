using System;
using System.Linq;
using Hospitality;
using Verse;

namespace HospitalityArchitect;

[StaticConstructorOnStartup]
public static class InjectHotelGuestComp
{
    static InjectHotelGuestComp()
    {
        var defs = DefDatabase<ThingDef>.AllDefsListForReading.Where(def => def.race?.Humanlike == true).ToList();
        defs.RemoveDuplicates();

        foreach (var def in defs)
        {
            if (def.comps == null) continue;

            if (!def.comps.Any(c => c.GetType() == typeof(CompProperties_HotelGuest)))
            {
                def.comps.Add((CompProperties)Activator.CreateInstance(typeof(CompProperties_HotelGuest)));
                //Log.Message(def.defName+": "+def.inspectorTabsResolved.Select(d=>d.GetType().Name).Aggregate((a,b)=>a+", "+b));
            }
        }
    }
}