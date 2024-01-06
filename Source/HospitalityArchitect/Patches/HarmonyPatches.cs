using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using GuestUtility = Hospitality.Utilities.GuestUtility;

namespace HospitalityArchitect
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        private static HiringContractService _cachedService;
        private static Map cachedMap;
        
        /*
         * TODO remove stuff from hospitality: recruiting, gift, visitor group, ...
         * hook in reputation tracker
         *
         * 
         */

        static HarmonyPatches()
        {
            //RimworldTycoon.harmonyInstance.Patch(AccessTools.Method(typeof(LoadedObjectDirectory), "Clear"),
            //postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(AddHireablesToLoadedObjectDirectory)));
            //RimworldTycoon.harmonyInstance.Patch(
//                AccessTools.Method(typeof(QuestUtility), nameof(QuestUtility.IsQuestLodger)),
//                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(IsQuestLodger_Postfix)));
            RimworldTycoon.harmonyInstance.Patch(
                AccessTools.Method(typeof(EquipmentUtility), nameof(EquipmentUtility.QuestLodgerCanUnequip)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(QuestLodgerCanUnequip_Postfix)));
            RimworldTycoon.harmonyInstance.Patch(
                AccessTools.Method(typeof(Hospitality.Utilities.GuestUtility), nameof(Hospitality.Utilities.GuestUtility.OnLordSpawned)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(VisitorGroupWithBus_Postfix)));

            RimworldTycoon.harmonyInstance.Patch(
                AccessTools.Method(typeof(CaravanFormingUtility), nameof(CaravanFormingUtility.AllSendablePawns)),
                transpiler: new HarmonyMethod(typeof(HarmonyPatches), nameof(CaravanAllSendablePawns_Transpiler)));
            RimworldTycoon.harmonyInstance.Patch(AccessTools.Method(typeof(Pawn), nameof(Pawn.CheckAcceptArrest)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(CheckAcceptArrestPostfix)));
            RimworldTycoon.harmonyInstance.Patch(
                AccessTools.Method(typeof(BillUtility), nameof(BillUtility.IsSurgeryViolationOnExtraFactionMember)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(IsSurgeryViolation_Postfix)));
            RimworldTycoon.harmonyInstance.Patch(
                AccessTools.Method(typeof(ForbidUtility), nameof(ForbidUtility.CaresAboutForbidden)),
                postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(CaresAboutForbidden_Postfix)));

            /*
            RimworldTycoon.harmonyInstance.Patch(
                AccessTools.Method(typeof(Area_Home), "Set"),
                prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(SetHome_Prefix)));
*/
            
            // quicksell button on everything haulable(hauls to delivery area to be picked up)
            /*
             * 
             */
            

            


            
        /*    
            [HarmonyPatch(typeof(ThingOwner))]
            [HarmonyPatch("TryDrop")]
            [HarmonyPatch(new Type[]
                { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(ThingPlaceMode), typeof(int), typeof(Thing).MakeByRefType() , typeof(Action), typeof(Predicate<IntVec3>) })]
        public static class RegisterIncom
        {
            static void Postfix(Thing thing, IntVec3 dropLoc, Map map, ThingPlaceMode mode, int count)
            {
                FinanceService financeService = thing.Map.GetComponent<FinanceService>();
                if (thing.def == ThingDefOf.Silver)
                {
                    financeService.bookIncome(FinanceReport.ReportEntryType.Income, thing.stackCount);
                }
            }
        }
        */
        
                // empty vending machine
                var m_GetAfterArmorDamage = AccessTools.Method(typeof(ThingOwner), "TryDrop",
                    new Type[] { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(ThingPlaceMode), typeof(int), typeof(Thing).MakeByRefType() , typeof(Action<Thing, int>), typeof(Predicate<IntVec3>) });
                RimworldTycoon.harmonyInstance.Patch(m_GetAfterArmorDamage,
                    postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(RegisterIncome)));
                // disable default hospitality leave message
                var m_LeaveMessage = AccessTools.Method("Hospitality.LordToil_VisitPoint:DisplayLeaveMessage");
                RimworldTycoon.harmonyInstance.Patch(m_LeaveMessage,
                    prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(DoNothing)));
                // TODO improve visit message
        }
        public static bool DoNothing()
        {
            return false;
        }
        
        public static void RegisterIncome(ThingOwner __instance, Thing thing, IntVec3 dropLoc, Map map, ThingPlaceMode mode, int count)
        {
             // TODO further breakdown ?
            if (thing.def == ThingDefOf.Silver)
            {
                FinanceService financeService = map.GetComponent<FinanceService>();
                if (__instance.Owner is Pawn_InventoryTracker)
                    financeService.bookIncome(FinanceReport.ReportEntryType.Beds, count);
                else
                    financeService.bookIncome(FinanceReport.ReportEntryType.Sales, count);
            }
        }

        // patch to show funds and cashflow
        //
        [HarmonyPatch(typeof(GlobalControlsUtility))]
        [HarmonyPatch("DoDate")]
        class GlobalControlsUtility_DoDate_Patch
        {
            static void Postfix(float leftX, float width, ref float curBaseY)
            {
                var map = Find.CurrentMap;
                if (map == null) return;

                FinanceService financeService = map.GetComponent<FinanceService>();
                var zombieCountString = "Funds: " + financeService.getFunds().ToStringMoney();
                var rightMargin = 7f;

                var zlRect = new Rect(leftX, curBaseY - 24f, width, 24f);
                Text.Font = GameFont.Small;
                var len = Text.CalcSize(zombieCountString);
                zlRect.xMin = zlRect.xMax - Math.Min(leftX, len.x + rightMargin);

                if (Mouse.IsOver(zlRect))
                {
                    Widgets.DrawHighlight(zlRect);
                }

                GUI.BeginGroup(zlRect);
                Text.Anchor = TextAnchor.UpperRight;
                var rect = zlRect.AtZero();
                rect.xMax -= rightMargin;
                Widgets.Label(rect, zombieCountString);
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.EndGroup();

                TooltipHandler.TipRegion(zlRect, new TipSignal(delegate
                {
                    //var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
                    //var versionString = currentVersion.Major + "." + currentVersion.Minor + "." + currentVersion.Build;
                    return "Cashflow: " + financeService.GetCashFlow().ToStringMoney() + "/day";
                }, 99899));

                curBaseY -= zlRect.height;
            }
        }

        private static HiringContractService GetContractTracker(Map map)
        {
            if (cachedMap != map)
            {
                _cachedService = map.GetComponent<HiringContractService>();
                cachedMap = map;
            }

            return _cachedService;
        }

        /*
         * hired pawns do not count as wealth 
         */
        public static void IsQuestLodger_Postfix(Pawn p, ref bool __result)
        {
            __result = __result || GetContractTracker(p.Map).IsHired(p);
        }

        /*
         * you cant unequip your staff
         */
        public static void QuestLodgerCanUnequip_Postfix(Pawn pawn, ref bool __result)
        {
            __result = __result && pawn.RaceProps.Humanlike && !GetContractTracker(pawn.Map).IsHired(pawn);
        }

        public static IEnumerable<CodeInstruction> CaravanAllSendablePawns_Transpiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var questLodger = AccessTools.Method(typeof(QuestUtility), nameof(QuestUtility.IsQuestLodger));

            foreach (var instruction in instructions)
                if (instruction.Calls(questLodger))
                {
                    yield return new CodeInstruction(OpCodes.Dup);
                    yield return instruction;
                    yield return CodeInstruction.Call(typeof(HarmonyPatches), nameof(CaravanAllSendablePawns_Helper));
                }
                else
                {
                    yield return instruction;
                }
        }

        public static bool CaravanAllSendablePawns_Helper(Pawn pawn, bool questLodger)
        {
            return questLodger && !GetContractTracker(pawn.Map).IsHired(pawn);
        }

        public static void CheckAcceptArrestPostfix(Pawn __instance, ref bool __result)
        {
            var tracker = GetContractTracker(__instance.Map);
            if (tracker.IsHired(__instance))
            {
                tracker.EndContract(__instance);
                __result = false;
            }
        }

        public static void VisitorGroupWithBus_Postfix(Lord lord)
        {
            VehiculumService busService = lord.Map.GetComponent<VehiculumService>();
            busService.StartBus(lord.ownedPawns);
        }
        public static void IsSurgeryViolation_Postfix(Bill_Medical bill, ref bool __result)
        {
            __result = __result || (GetContractTracker(bill.Map).IsHired(bill.GiverPawn) &&
                                    bill.recipe.Worker.IsViolationOnPawn(bill.GiverPawn, bill.Part, Faction.OfPlayer));
        }

        public static void CaresAboutForbidden_Postfix(Pawn pawn, ref bool __result)
        {
            __result = __result &&
                       (!GetContractTracker(pawn.Map).IsHired(pawn) || pawn.CurJobDef != HADefOf.HA_LeaveMap);
        }
        
        // increase/decrease of home area is billed/refunded
        /*
         * Area_Home.
         * 	protected override void Set(IntVec3 c, bool val)
            {
                if (base[c] != val)
                {
                    base.Set(c, val);
                    base.Map.listerFilthInHomeArea.Notify_HomeAreaChanged(c);
                }
            }
         */
        
        // TODO overwrite Designator_AreaHome
        public static bool SetHome_Prefix(IntVec3 c, bool val, Area_Home __instance)
        {
            if (__instance[c] == val) return true;
            float landValue = Utils.GetLandValue(cachedMap, c);
            if (val)
            {
                if (cachedMap.GetComponent<FinanceService>().canAfford(landValue))
                    cachedMap.GetComponent<FinanceService>().doAndBookExpenses(FinanceReport.ReportEntryType.Land, landValue);
                else
                {
                    return false;
                }
            }
            else
            {
                cachedMap.GetComponent<FinanceService>().doAndBookIncome(FinanceReport.ReportEntryType.Land, landValue);
            }
            return true;
        }
    }
}