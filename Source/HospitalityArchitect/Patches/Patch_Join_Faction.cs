using HarmonyLib;
using RimWorld;
using Verse;

namespace HospitalityArchitect;

public class Patch_Join_Faction
{
    // - ownership: when pawn joins colony check relationship, if not a partner, ask if you want to swap owner.
    /*
     * pawn public override void SetFaction(Faction newFaction, Pawn recruiter = null)
     * 
     */
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.SetFaction))]
    public class Patch2
    {
        [HarmonyPrefix]
        public static bool Prefix(Faction newFaction, Pawn __instance)
        {
            if (!__instance.IsColonist && newFaction.IsPlayer)
            {
                Log.Warning("SetFaction is disabled ");
                return false;
            }

            return true;
        }
    }
}
