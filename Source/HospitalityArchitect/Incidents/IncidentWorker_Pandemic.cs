using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace HospitalityArchitect;

public class IncidentWorker_Pandemic : IncidentWorker
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
        DoConditionAndLetter(parms, map, Mathf.RoundToInt(def.durationDays.RandomInRange * 60000f));
        return true;
    }

    protected void DoConditionAndLetter(IncidentParms parms, Map map, int duration)
    {
        GameCondition_Pandemic gc = (GameCondition_Pandemic)GameConditionMaker.MakeCondition(HADefOf.Pandemic, duration);
        map.gameConditionManager.RegisterCondition(gc);
        SendStandardLetter(gc.LabelCap, gc.LetterText, LetterDefOf.ThreatSmall, parms, LookTargets.Invalid);
    }
}