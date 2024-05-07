using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hospitality;
using Hospitality.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using LordJob_VisitColony = Hospitality.LordJob_VisitColony;

namespace HospitalityArchitect
{
    public static class GuestUtility
    {
        public static int EmptyQualifiedBedsAvailable(Map map, GuestTypeDef guestTypeDef, PawnKindDef pawnKindDef, bool onlyDoubleBeds = false)
        {
            if (guestTypeDef.dayVisitor)
            {
                return guestTypeDef.facilityRequirements.MetAndOpen(map)?1:0;
            }
            return BedUtility.FindQualifiedBeds(map, guestTypeDef, pawnKindDef).Count(bed => !onlyDoubleBeds || bed.SleepingSlotsCount == 2);
        }
        
        public static int QualifiedBedsCount(Map map, GuestTypeDef guestTypeDef, PawnKindDef pawnKindDef)
        {
            if (guestTypeDef.dayVisitor) return guestTypeDef.facilityRequirements.Capacity(map);
            return BedUtility.FindQualifiedBeds(map, guestTypeDef, pawnKindDef, true).Count();
        }        

        public static void SetUpHotelGuest(Pawn pawn)
        {
            CompGuest guest = pawn.GetComp<CompGuest>();
            CompHotelGuest hotelGuest = pawn.GetComp<CompHotelGuest>();
            GuestTypeDef type = pawn.kindDef.GetModExtension<GuestTypeDef>();
            LordJob_VisitColony lordjob = ((LordJob_VisitColony)guest.lord.LordJob);
            hotelGuest.arrived = true;
            hotelGuest.lastHourSeen = GenLocalDate.HourOfDay(pawn.Map);
            hotelGuest.hoursSpent = 0;
            hotelGuest.totalSpent = 0;
            hotelGuest.left = false;
            hotelGuest.dayVisit = false;
            // reset memories
            pawn.needs.mood.thoughts.memories.memories.Clear();
            // determine stayduration
            if (type.dayVisitor)
            {
                guest.lord.ownedPawns.ForEach(pawn1 => pawn1.GetComp<CompHotelGuest>().dayVisit = true);
            }
            // setup money
            var ownedMoney =
                pawn.inventory.innerContainer.FirstOrFallback(thing => thing.def.Equals(ThingDefOf.Silver), null);
            if (ownedMoney != null)
                pawn.inventory.innerContainer.Remove(ownedMoney);
            var money = ThingMaker.MakeThing(ThingDefOf.Silver);
            money.stackCount = type.budget.RandomInRange;
            var spaceFor = pawn.GetInventorySpaceFor(money);
            if (spaceFor > 0)
            {
                var success = pawn.inventory.innerContainer.TryAdd(money);
                if (!success) money.Destroy();
            }
            hotelGuest.initialMoney = money.stackCount;
            // setup food
            if (!type.bringsFood)
            {
                pawn.inventory.innerContainer.RemoveAll(thing => thing.def.ingestible is
                {
                    cachedNutrition: > 0.0f
                });
                pawn.carryTracker.DestroyCarriedThing(); // very hungry pawns carry food in their hands ?
            }
            // setup apparel
            
            // setup needs
            pawn.needs.rest.CurLevel = Mathf.Clamp01(type.initRest+ Rand.Range(-0.1f,0.1f));
            pawn.needs.joy.CurLevel = Mathf.Clamp01(type.initJoy+ Rand.Range(-0.1f,0.1f));
            pawn.needs.food.CurLevel = Mathf.Clamp01(type.initFood+ Rand.Range(-0.1f,0.1f));

            // setup forced traits
            // first remove all possible forced traits
            if (pawn.story.traits.HasTrait(DefDatabase<TraitDef>.GetNamed("SpaLover", false)))
            {
                pawn.story.traits.RemoveTrait(pawn.story.traits.GetTrait(DefDatabase<TraitDef>.GetNamed("SpaLover", false)));
            }
            if (pawn.story.traits.HasTrait(DefDatabase<TraitDef>.GetNamed("Gambler", false)))
            {
                pawn.story.traits.RemoveTrait(pawn.story.traits.GetTrait(DefDatabase<TraitDef>.GetNamed("Gambler", false)));
            }
            if (pawn.story.traits.HasTrait(DefDatabase<TraitDef>.GetNamed("Ascetic", false)))
            {
                pawn.story.traits.RemoveTrait(pawn.story.traits.GetTrait(DefDatabase<TraitDef>.GetNamed("Ascetic", false)));
            }
            if (pawn.story.traits.HasTrait(DefDatabase<TraitDef>.GetNamed("Shopper", false)))
            {
                pawn.story.traits.RemoveTrait(pawn.story.traits.GetTrait(DefDatabase<TraitDef>.GetNamed("Shopper", false)));
            }
            
            if (pawn.kindDef.forcedTraits != null)
            {
                foreach (TraitRequirement forcedTrait in pawn.kindDef.forcedTraits)
                {
                    pawn.story.traits.GainTrait(new Trait(forcedTrait.def, forcedTrait.degree ?? 0, forced: true));
                }                
            }
            
            // setup relationship with other guest (for now, we either have 1 or 2 guests, and 2 guests are always lovers
            // always do this last in this method otherwise we might unintentionally skip stuff
            if (guest.lord.ownedPawns.Count == 2)
            {
                if (guest.lord.ownedPawns.Count(pawn1 => !pawn1.GetComp<CompHotelGuest>().arrived) > 0)
                {
                    // wait until both arrived
                    return;
                }
                // some would wish it is just a one-liner like this IRL :)
                guest.lord.ownedPawns[0].relations.AddDirectRelation(Rand.Chance(0.5f)?PawnRelationDefOf.Lover:PawnRelationDefOf.Spouse, guest.lord.ownedPawns[1]);
            }
            
            // first impressions of your company for specific kinds of guests
            if (pawn.story.traits.HasTrait(DefDatabase<TraitDef>.GetNamed("SpaLover", false)))
            {
                pawn.ThoughtSpaLover();
            }
            
            if (pawn.story.traits.HasTrait(DefDatabase<TraitDef>.GetNamed("Gambler", false)))
            {
                pawn.ThoughtGambler();
            }            
            
        }
        
        public static void ThoughtAboutRoomCleanliness(this Pawn pawn, Building_GuestBed bed)
        {
            var thoughtDef = ThoughtDef.Named("HospitalityArchitect_Cleanliness");
            if (pawn == null || bed == null) return;
            int scoreStageIndex = RoomStatDefOf.Cleanliness.GetScoreStageIndex(bed.GetRoom().GetStat(RoomStatDefOf.Cleanliness));
            if (thoughtDef.stages[scoreStageIndex] != null)
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(thoughtDef, scoreStageIndex));
            }

            if (bed.GetComp<CompHotelGuestBed>().needBedding)
            {
                thoughtDef = ThoughtDef.Named("HospitalityArchitect_NoCleanBedding");
                var thoughtMemory = ThoughtMaker.MakeThought(thoughtDef,0);
                pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(thoughtMemory);
            }
        }
        
        public static void ThoughtAboutToiletCleanliness(this Pawn pawn, Thing targetAThing)
        {
            var thoughtDef = ThoughtDef.Named("HospitalityArchitect_Toilets");
            if (pawn == null || targetAThing == null) return;
            int scoreStageIndex = RoomStatDefOf.Cleanliness.GetScoreStageIndex(targetAThing.GetRoom().GetStat(RoomStatDefOf.Cleanliness));
            if (thoughtDef.stages[scoreStageIndex] != null)
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(ThoughtMaker.MakeThought(thoughtDef, scoreStageIndex));
            } 
        }
        
        public static void ThoughtSpaLover(this Pawn pawn)
        {
            var thoughtDef = ThoughtDef.Named("SpaLoverArrival");
            if (pawn == null) return;

            int distinctSpaFacilities = pawn.Map.listerBuildings.allBuildingsColonist.Where(building =>
                building.def.building?.joyKind?.defName.EqualsIgnoreCase("Hydrotherapy") == true ||
                building.def.defName.EqualsIgnoreCase("DBHSaunaSeating")).ToList().Distinct().Count();

            int stage = 0;
            if (distinctSpaFacilities > 1) stage = 1;
            if (distinctSpaFacilities > 2) stage = 2;

            var thoughtMemory = ThoughtMaker.MakeThought(thoughtDef, stage);
            pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(thoughtMemory); // *cough* Extra defensive        
        }    
        
        public static void ThoughtGambler(this Pawn pawn)
        {
            return;
            // TODO
            /*
            var thoughtDef = ThoughtDef.Named("GamblerArrival");
            if (pawn == null) return;

            int distinctSpaFacilities = pawn.Map.listerBuildings.allBuildingsColonist.Where(building =>
                building.def.building?.joyKind?.defName.EqualsIgnoreCase("Hydrotherapy") == true ||
                building.def.defName.EqualsIgnoreCase("DBHSaunaSeating")).ToList().Distinct().Count();

            int stage = 0;
            if (distinctSpaFacilities > 1) stage = 1;
            if (distinctSpaFacilities > 2) stage = 2;

            var thoughtMemory = ThoughtMaker.MakeThought(thoughtDef, stage);
            pawn.needs?.mood?.thoughts?.memories?.TryGainMemory(thoughtMemory); // *cough* Extra defensive
            */
        }            
        
        public static void AddNeedJoy(Pawn pawn)
        {
            if (pawn.needs.joy == null)
            {
                pawn.needs.AddNeed( DefDatabase<NeedDef>.GetNamed("Joy"));
            }

            pawn.needs.joy.CurLevel = Rand.Range(0, 0.5f);
        }

        public static void AddNeedComfort(Pawn pawn)
        {
            if (pawn.needs.comfort == null)
            {
                var addNeed = typeof(Pawn_NeedsTracker).GetMethod("AddNeed", BindingFlags.Instance | BindingFlags.NonPublic);
                addNeed.Invoke(pawn.needs, new object[] {DefDatabase<NeedDef>.GetNamed("Comfort")});
            }

            pawn.needs.comfort.CurLevel = Rand.Range(0, 0.5f);
        }

        public static void FixTimetable(this Pawn pawn)
        {
            pawn.mindState ??= new Pawn_MindState(pawn);
            pawn.timetable = new Pawn_TimetableTracker(pawn) {times = new List<TimeAssignmentDef>(24)};
            for (int i = 0; i < 24; i++)
            {
                var def = TimeAssignmentDefOf.Anything;
                pawn.timetable.times.Add(def);
            }
        }
        
        private static FieldInfo _fieldPriorities = typeof(Pawn_WorkSettings).GetField("priorities", BindingFlags.NonPublic | BindingFlags.Instance);
        public static void EnsureHasWorkSettings(Pawn pawn)
        {
            var priorities = _fieldPriorities.GetValue(pawn.workSettings);
            if (priorities == null)
            {
                pawn.workSettings.EnableAndInitialize();
            }
        }
        
        public static void HotelGuestLeaves(Pawn guest)
        {
            Map map = guest.Map;
            map.GetComponent<MarketingService>().leaveRating(guest);
            if (TentUtility.StayedInTent(guest))
            {
                if (!guest.ownership.OwnedBed.Destroyed)
                    TentUtility.BreakDownTent(guest.ownership.OwnedBed as Building_GuestBed);
            }
            else
            {
                guest.ownership.OwnedBed?.GetComp<CompHotelGuestBed>().MakeBeddingDirty();
            }

            Lord lord = guest.GetLord();
            if (lord == null)
            {
                Log.Warning("lord == null");
                return;
            }

            foreach (var pawn in lord.ownedPawns)
            {
                //pawn.GetComp<CompGuest>().sentAway = true;
                pawn.GetComp<CompHotelGuest>().left = true;
            }
            VehiculumService busService = map.GetComponent<VehiculumService>();
            busService.GetPawnsReadyForDeparture(lord.ownedPawns);            
        }


        // 40s for 20%
        public static void Gift(Pawn lordOwnedPawn)
        {
            var thoughtDef = ThoughtDef.Named("HospitalityArchitect_Gift");
            var thoughtMemory = ThoughtMaker.MakeThought(thoughtDef,0);
            lordOwnedPawn.needs?.mood?.thoughts?.memories?.TryGainMemory(thoughtMemory);
        }
    }
}