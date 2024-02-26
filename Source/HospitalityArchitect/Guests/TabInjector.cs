using System;
using System.Linq;
using Hospitality;
using Verse;

namespace HospitalityArchitect;

public static class InjectHotelGuestTab
{
    public static void Inject()
    {
        var defs = DefDatabase<ThingDef>.AllDefs.Where(def => def.race?.Humanlike == true).ToList();
        defs.RemoveDuplicates();

        var tabBase = InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_HotelGuest));
        var tabBase2 = InspectTabManager.GetSharedInstance(typeof(ITab_Pawn_Guest));

        foreach (var def in defs)
        {
            if (def.inspectorTabs == null || def.inspectorTabsResolved == null) continue;

            def.inspectorTabs.Remove(typeof(ITab_Pawn_Guest));

            //if (!def.inspectorTabs.Contains(typeof(ITab_Pawn_Guest)))
            //{
                def.inspectorTabs.Add(typeof(ITab_Pawn_HotelGuest));
                def.inspectorTabsResolved.Add(tabBase);
                def.inspectorTabsResolved.Remove(tabBase2);
                //Log.Message(def.defName+": "+def.inspectorTabsResolved.Select(d=>d.GetType().Name).Aggregate((a,b)=>a+", "+b));
                //}
        }
    }
}