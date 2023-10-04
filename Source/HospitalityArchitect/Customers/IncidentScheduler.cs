using RimWorld;
using UnityEngine;
using Verse;

namespace HospitalityArchitect;

/*
 * tries to trigger an incident every hour
 */
public class IncidentScheduler : MapComponent
{
    
    public IncidentScheduler(Map map) : base(map)
    {
    }
    
    public override void ExposeData()
    {
        base.ExposeData();
        //Scribe_Values.Look(ref hour, "hour");
    }

    public override void MapComponentTick()
    {
        base.MapComponentTick();
        if (GenTicks.TicksGame % 42 == 0) // every ingame minute
        {
            float baseChance = 0.001f; // 1% every minute, gives about a customer per 100 minutes  
            if (Rand.Chance(baseChance))
            {
                IncidentParms parms = new IncidentParms();
                parms.target = map;
                DefDatabase<IncidentDef>.GetNamed("CustomerArrives").Worker.TryExecuteWorker(parms);
            }
        }        
    }
}