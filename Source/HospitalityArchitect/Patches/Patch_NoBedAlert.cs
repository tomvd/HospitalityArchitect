using HarmonyLib;
using RimWorld;
using Verse;

namespace HospitalityArchitect;

public class Patch_NoBedAlert
{
/*
 * public class Alert_NeedColonistBeds : Alert
{
	public override AlertReport GetReport()
	{

			return false;
	}

 */
/*
  [HarmonyPatch(typeof(Alert_NeedColonistBeds), nameof(Alert_NeedColonistBeds.GetReport))]
    public class Patch2
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }*/
}
