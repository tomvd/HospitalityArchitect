using HarmonyLib;
using UnityEngine;
using Verse;

namespace HospitalityArchitect;

public class Patch_Display_Owner
{
    /// <summary>
    // - ownership: in the name of a pawn clearly display owner (owner)
    // prefix PawnNameColorUtility public static Color PawnNameColorOf(Pawn pawn)
    // pawn :
    /*	public override string LabelShort
    {
        get
        {
            if (Name != null)
            {
                return Name.ToStringShort;
            }
            return LabelNoCount;
        }
    }*/
    /// </summary>
    ///
   /* [HarmonyPatch(typeof(Pawn), nameof(Pawn.LabelShort), MethodType.Getter)]
    public class Patch1
    {
        [HarmonyPostfix]
        public static void Postfix(ref string __result, Pawn __instance)
        {
            if (__instance.IsColonist)
            {
                if (!__instance.Map.GetComponent<HiringContractService>().IsHired(__instance))
                {
                    __result += "(Owner)";
                }
            }
        }
    }
    */
    [HarmonyPatch(typeof(PawnNameColorUtility), nameof(PawnNameColorUtility.PawnNameColorOf))]
    public class Patch2
    {
        [HarmonyPostfix]
        public static void Postfix(ref Color __result, Pawn pawn)
        {
            if (pawn.IsColonist && pawn.Map != null)
            {
                if (!pawn.Map.GetComponent<HiringContractService>().IsHired(pawn))
                {
                    __result = new Color32(222, 0, 250, byte.MaxValue);
                    pawn.story.title = "Manager";
                }
            }
        }
    }
}
