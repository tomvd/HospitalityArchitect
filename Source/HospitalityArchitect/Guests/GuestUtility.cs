using System;
using System.Collections.Generic;
using System.Linq;
using Hospitality;
using Hospitality.Utilities;
using HospitalityArchitect;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using LordJob_VisitColony = Hospitality.LordJob_VisitColony;
using DubsBadHygiene;

namespace HospitalityArchitect
{
    public static class GuestUtility
    {
        public static int EmptyQualifiedBedsAvailable(Map map, GuestTypeDef guestTypeDef, PawnKindDef pawnKindDef, bool onlyDoubleBeds = false)
        {
            return BedUtility.FindQualifiedBeds(map, guestTypeDef, pawnKindDef).Count(bed => !onlyDoubleBeds || bed.SleepingSlotsCount == 2);
        }
        
        public static int QualifiedBedsCount(Map map, GuestTypeDef guestTypeDef, PawnKindDef pawnKindDef)
        {
            return BedUtility.FindQualifiedBeds(map, guestTypeDef, pawnKindDef, true).Count();
        }        

        public static void SetUpHotelGuest(Pawn pawn)
        {
            CompGuest guest = pawn.GetComp<CompGuest>();
            CompHotelGuest hotelGuest = pawn.GetComp<CompHotelGuest>();
            GuestTypeDef type = pawn.kindDef.GetModExtension<GuestTypeDef>();
            LordJob_VisitColony lordjob = ((LordJob_VisitColony)guest.lord.LordJob);
            hotelGuest.arrived = true;
                
            // determine stayduration
            if (Rand.Chance(type.dayVisitChance))
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
            // setup food
            if (!type.bringsFood)
            {
                var ownedFood =
                    pawn.inventory.innerContainer.FirstOrFallback(thing => thing.def.IsNutritionGivingIngestible, null);
                if (ownedFood != null)
                    pawn.inventory.innerContainer.Remove(ownedFood);
            }
            // setup apparel
            
            // setup needs
            pawn.needs.rest.CurLevel = Mathf.Clamp01(type.initRest+ Rand.Range(-0.1f,0.1f));
            pawn.needs.joy.CurLevel = Mathf.Clamp01(type.initJoy+ Rand.Range(-0.1f,0.1f));
            pawn.needs.food.CurLevel = Mathf.Clamp01(type.initFood+ Rand.Range(-0.1f,0.1f));

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
        }
    }
}