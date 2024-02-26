using System;
using HarmonyLib;
using RimWorld;
using Tent;
using Verse;

namespace HospitalityArchitect;

// for some reason the postfix of tents did not work, so I added a prefix here to catch tents first
public class PatchTentAgressive
{
    [HarmonyPatch(typeof(Toils_LayDown), "ApplyBedThoughts")]
    public class Toils_LayDown_ApplyBedThoughts
    {
        [HarmonyPrefix]
        public static bool Prefix(Pawn actor, Building_Bed bed)
        {
            if (bed == null) return true;
            var modExt = bed.def.GetModExtension<TentModExtension>();
            if (modExt != null)
            {
                actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBedroom);
                actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInBarracks);
                actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptOutside);
                actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptOnGround);
                actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInCold);
                actor.needs.mood.thoughts.memories.RemoveMemoriesOfDef(ThoughtDefOf.SleptInHeat);
                return false;
            }

            return true;
        }
    }    
}