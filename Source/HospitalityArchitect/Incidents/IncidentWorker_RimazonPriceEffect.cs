using RimWorld;
using UnityEngine;
using Verse;

namespace HospitalityArchitect;

public abstract class IncidentWorker_RimazonPriceEffect : IncidentWorker
{
    public override bool CanFireNowSub(IncidentParms parms)
    {
        Map map = (Map)parms.target;
        if (map.mapPawns.FreeColonistsCount == 0)
        {
            return false;
        }
        return true;
    }

    public override bool TryExecuteWorker(IncidentParms parms)
    {
        Map map = (Map)parms.target;
        DoConditionAndLetter(parms, map, Mathf.RoundToInt(def.durationDays.RandomInRange * 60000f), map.mapPawns.FreeColonists.RandomElement().gender, parms.points);
        return true;
    }

    protected abstract void DoConditionAndLetter(IncidentParms parms, Map map, int duration, Gender gender, float points);
}