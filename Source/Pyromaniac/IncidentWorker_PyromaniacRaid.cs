using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RT
{
    public class IncidentWorker_PyromaniacRaid : IncidentWorker_RaidEnemy
    {
        /*protected override bool TryResolveRaidFaction(IncidentParms parms)
        {
            parms.faction = Find.FactionManager.FirstFactionOfDef(FactionDefOf.Pirate);
            return true;
        }

        protected override void ResolveRaidPoints(IncidentParms parms)
        {
            parms.points = StorytellerUtility.DefaultThreatPointsNow(parms.target) * 2f;
        }*/

        public override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
        }

        public override void ResolveRaidArriveMode(IncidentParms parms)
        {
            parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            ResolveRaidPoints(parms);
            if (!TryResolveRaidFaction(parms))
            {
                return false;
            }
            /*if (!parms.faction.HostileTo(Faction.OfPlayer))
            {
                return false;
            }*/
            PawnGroupKindDef combat = PawnGroupKindDefOf.Combat;
            ResolveRaidStrategy(parms, combat);
            ResolveRaidArriveMode(parms);
            parms.raidStrategy.Worker.TryGenerateThreats(parms);
            if (!parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms))
            {
                return false;
            }
            float points = parms.points;
            parms.points = AdjustedRaidPoints(parms.points, parms.raidArrivalMode, parms.raidStrategy, parms.faction, combat);
            List<Pawn> list = parms.raidStrategy.Worker.SpawnThreats(parms);
            if (list == null)
            {
                list = PawnGroupMakerUtility.GeneratePawns(IncidentParmsUtility.GetDefaultPawnGroupMakerParms(combat, parms)).ToList();
                if (list.Count == 0)
                {
                    Log.Error("Got no pawns spawning raid from parms " + parms);
                    return false;
                }
                parms.raidArrivalMode.Worker.Arrive(list, parms);
            }
            GenerateRaidLoot(parms, points, list);
            // end of TryGenerateRaidInfo
            //Log.Message($"Raid: {parms.faction.Name} ({parms.faction.def.defName}) {parms.raidArrivalMode.defName} {parms.raidStrategy.defName} c={parms.spawnCenter} p={parms.points}");
            
            //
            TaggedString letterLabel = GetLetterLabel(parms);
            TaggedString letterText = GetLetterText(parms, list);
            //PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(list, ref letterLabel, ref letterText, GetRelatedPawnsInfoLetterText(parms), informEvenIfSeenBefore: true);
            List<TargetInfo> list2 = new List<TargetInfo>();
            if (parms.pawnGroups != null)
            {
                List<List<Pawn>> list3 = IncidentParmsUtility.SplitIntoGroups(list, parms.pawnGroups);
                List<Pawn> list4 = list3.MaxBy((List<Pawn> x) => x.Count);
                if (list4.Any())
                {
                    list2.Add(list4[0]);
                }
                for (int i = 0; i < list3.Count; i++)
                {
                    if (list3[i] != list4 && list3[i].Any())
                    {
                        list2.Add(list3[i][0]);
                    }
                }
            }
            else if (list.Any())
            {
                foreach (Pawn item in list)
                {
                    list2.Add(item);
                }
            }
            //SendStandardLetter(letterLabel, letterText, GetLetterDef(), parms, list2);
            List<ThingDef> allFlamingWeapons = new List<ThingDef>();
            Predicate<ThingDef> isWeapon = (ThingDef td) => !td.weaponTags.NullOrEmpty<string>() && td.weaponTags.Contains("GrenadeFlame");
            foreach (ThingDef thingDef in from td in DefDatabase<ThingDef>.AllDefs
                                          where isWeapon(td)
                                          select td)
            {
                allFlamingWeapons.Add(thingDef);
            } //Weapon_GrenadeMolotov

            var ignitors = new List<Pawn>();
            foreach (var pawn in list)
            {
                pawn.equipment.DestroyAllEquipment();
                pawn.equipment.AddEquipment((ThingWithComps)ThingMaker.MakeThing(ThingDef.Named("Weapon_GrenadeMolotov")));
//                if (pawn.apparel != null && Rand.Chance(0.5f))
                //{                   
//                    var throwableTorches = ThingMaker.MakeThing(allFlamingWeapons.FirstOrDefault()) as Apparel;
                    //pawn.inventory.TryAddAndUnforbid(throwableTorches);
                    ignitors.Add(pawn);
                //}
                pawn.mindState.duty = new PawnDuty(DutyDefOf.AssaultColony);
            }

            var lord = new LordJob_BurnColony(parms.faction, false, true, true, true, true);
            LordMaker.MakeNewLord(parms.faction, lord, (Map)parms.target, list);

            LessonAutoActivator.TeachOpportunity(ConceptDefOf.EquippingWeapons, OpportunityType.Critical);
            if (!PlayerKnowledgeDatabase.IsComplete(ConceptDefOf.ShieldBelts))
            {
                for (int j = 0; j < list.Count; j++)
                {
                    Pawn pawn = list[j];
                    if (pawn.apparel != null && pawn.apparel.WornApparel.Any(ap => ap.def == ThingDefOf.Apparel_ShieldBelt))
                    {
                        LessonAutoActivator.TeachOpportunity(ConceptDefOf.ShieldBelts, OpportunityType.Critical);
                        break;
                    }
                }
            }
            return true;
        }



        protected override string GetLetterLabel(IncidentParms parms)
        {
            return base.def.letterLabel;
        }

        protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
        {
            return "VFEV.PillageRaidDesc".Translate(parms.faction);
        }

        protected override LetterDef GetLetterDef()
        {
            return LetterDefOf.ThreatBig;
        }

        protected override string GetRelatedPawnsInfoLetterText(IncidentParms parms)
        {
            return TranslatorFormattedStringExtensions.Translate("LetterRelatedPawnsRaidEnemy", Faction.OfPlayer.def.pawnsPlural, parms.faction.def.pawnsPlural);
        }
    }
}