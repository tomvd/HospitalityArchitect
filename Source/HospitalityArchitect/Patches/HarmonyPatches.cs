using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using DubsBadHygiene;
using HarmonyLib;
using Hospitality;
using Hospitality.Utilities;
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
                var methodTryDrop = AccessTools.Method(typeof(ThingOwner), "TryDrop",
                    new Type[] { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(ThingPlaceMode), typeof(int), typeof(Thing).MakeByRefType() , typeof(Action<Thing, int>), typeof(Predicate<IntVec3>) });
                RimworldTycoon.harmonyInstance.Patch(methodTryDrop,
                    postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(RegisterIncome)));
                
                // disable default hospitality leave message
                var m_LeaveMessage = AccessTools.Method("Hospitality.LordToil_VisitPoint:DisplayLeaveMessage");
                RimworldTycoon.harmonyInstance.Patch(m_LeaveMessage,
                    prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(DoNothing)));
                
                
                var method = AccessTools.Method("DubsBadHygiene.RoomRoleWorker_PrivateBathroom:GetScore");
                RimworldTycoon.harmonyInstance.Patch(method,
                    prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(RoomRoleWorker_PrivateBathroom)));
                
                method = AccessTools.Method("DubsBadHygiene.RoomRoleWorker_PublicBathroom:GetScore");
                RimworldTycoon.harmonyInstance.Patch(method,
                    prefix: new HarmonyMethod(typeof(HarmonyPatches), nameof(RoomRoleWorker_PublicBathroom)));    
                
                method = AccessTools.Method("DubsBadHygiene.SanitationUtil:ApplyBathroomThought");
                RimworldTycoon.harmonyInstance.Patch(method,
                    postfix: new HarmonyMethod(typeof(HarmonyPatches), nameof(ApplyBathroomCleanlinessThought)));   
                
        }
        public static bool DoNothing()
        {
            return false;
        }

        public static void ApplyBathroomCleanlinessThought(Pawn actor, Thing fixture)
        {
            if (actor.IsGuest())
                actor.ThoughtAboutToiletCleanliness(fixture);            
        }        
        
        // fixes the fact that in the original roomworker the bed had to have pawns assigned to it,
        // but that is not the case when a private bathroom has no guests yet, but it still is a private bathroom
        public static bool RoomRoleWorker_PrivateBathroom(Room room, ref float __result)
        {
            int num = 0;
            List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
            for (int i = 0; i < containedAndAdjacentThings.Count; i++)
            {
                if (containedAndAdjacentThings[i] is Building_AssignableFixture building_AssignableFixture && building_AssignableFixture.AllSpawnedBeds().Any())
                {
                    num++;
                }
            }
            if (num > 0 && !room.isPrisonCell)
            {
                __result = 4000f;
                return false;
            }
            __result = 0f;
            return false;
        }
        
        // fixes the fact that basins and latrines did not count as public bathroom items
        public static bool RoomRoleWorker_PublicBathroom(Room room, ref float __result)
        {
            float num = 0;
            List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
            for (int i = 0; i < containedAndAdjacentThings.Count; i++)
            {
                if (containedAndAdjacentThings[i] is Building_AssignableFixture building_AssignableFixture && !building_AssignableFixture.AllSpawnedBeds().Any())
                {
                    num += 4000;
                } else
                if (containedAndAdjacentThings[i] is Building_Bed || containedAndAdjacentThings[i] is Building_GuestBed)
                {
                    num -= 4000; // if some weird mind is creating some kind of barracks with toilets, this is no public bathroom :)
                }
            }
            if (num > 0 && !room.isPrisonCell)
            {
                __result = Mathf.Clamp(num,0,4000);
                return false;
            }
            __result = 0f;
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
    }
}