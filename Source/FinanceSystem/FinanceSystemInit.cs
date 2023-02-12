using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RT
{
    [StaticConstructorOnStartup]
    public static class FinanceSystemInit
    {
        private static FinanceTracker cachedTracker      = null;
        private static World                 cachedTrackerWorld = null;

        static FinanceSystemInit()
        {
            // todo hook into expenses and income ... not sure how
            //RimworldTycoon.harmonyInstance.Patch(AccessTools.Method(typeof(EquipmentUtility), nameof(EquipmentUtility.QuestLodgerCanUnequip)),
                                            //postfix: new HarmonyMethod(typeof(HireableSystemStaticInitialization), nameof(QuestLodgerCanUnequip_Postfix)));
        }

        private static FinanceTracker GetFinanceTracker(World world)
        {
            if (cachedTrackerWorld != world)
            {
                cachedTracker      = world.GetComponent<FinanceTracker>();
                cachedTrackerWorld = world;
            }

            return cachedTracker;
        }
    }
}