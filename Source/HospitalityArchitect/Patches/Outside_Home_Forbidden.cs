using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace HospitalityArchitect;

public class Outside_Home_Forbidden
{
    
    /*
     * // RimWorld.ForbidUtility
using Verse;

public static bool InAllowedArea(this IntVec3 c, Pawn forPawn)
{
	if (forPawn.playerSettings != null)
	{
		Area effectiveAreaRestrictionInPawnCurrentMap = forPawn.playerSettings.EffectiveAreaRestrictionInPawnCurrentMap;
		if (effectiveAreaRestrictionInPawnCurrentMap != null && effectiveAreaRestrictionInPawnCurrentMap.TrueCount > 0 && !effectiveAreaRestrictionInPawnCurrentMap[c])
		{
			return false;
		}
	}
	return true;
}
     */
    [HarmonyPatch(typeof(ForbidUtility), nameof(ForbidUtility.InAllowedArea))]
    public class Patch1
    {
	    [HarmonyPostfix]
	    public static void Postfix(ref bool __result, Pawn forPawn, IntVec3 c)
	    {
		    if (forPawn.playerSettings != null && __result == true)
		    {
			    Area homeArea = forPawn.Map.areaManager.Home;
			    if (forPawn.playerSettings.EffectiveAreaRestriction == null)
			    {
				    forPawn.playerSettings.AreaRestriction = homeArea;
			    }
			    if (homeArea != null && homeArea.TrueCount > 0 && !homeArea[c])
			    {
				    __result = false;
			    }
		    }
	    }
    }
  
    // do not ever enable auto expand home area!!! player needs to buy it
    
    // RimWorld.AutoHomeAreaMaker
    /*using Verse;
    public static void MarkHomeAroundThing(Thing t)*/
    [HarmonyPatch(typeof(AutoHomeAreaMaker), nameof(AutoHomeAreaMaker.MarkHomeAroundThing))]
    public class Patch3
    {
	    [HarmonyPrefix]
	    public static bool Prefix()
	    {
		    if (Find.CurrentMap.areaManager.Home.TrueCount > 0)
		    {
			    Find.PlaySettings.autoHomeArea = false;
			    return false;
		    }
		    else
		    {
			    return true;
		    }
	    }
    }
}