using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;

namespace HospitalityArchitect
{
    public class HiringContractService : MapComponent
    {
        public List<HiringContract> contracts = new List<HiringContract>();
        public List<Pawn> candidates = new List<Pawn>();

        private FinanceService _financeService;
        private VehiculumService _busService;

        public HiringContractService(Map map) : base(map)
        {
            contracts = new List<HiringContract>();
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            _financeService = map.GetComponent<FinanceService>();
            _busService = map.GetComponent<VehiculumService>();
        }

        public bool IsHired(Pawn pawn)
        {
            return contracts.Any(contract => contract.pawn != null && contract.pawn.Equals(pawn));
        }

        public void SetNewContract(Pawn pawn, int type)
        {
            contracts.Add(new HiringContract(Find.TickManager.TicksAbs, pawn, map, type));
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            // 6 times every ingame hour, go over the contracts, see if they are finished, do billing, see if people still like it here
            if (Find.TickManager.TicksAbs % (GenDate.TicksPerHour / 6) == 0)
            {
                float totalBilling = 0f;
                foreach (var contract in contracts.ToList())
                {
                    //Log.Message($"Checking contract of {contract.pawn.NameShortColored}");
                    /*if (Find.TickManager.TicksAbs >= contract.endTicks)
                    {
                        Log.Message($"Contract end reached");
                        EndContract(contract);
                        return;
                    }*/

                    // something wgron
                    if (contract.pawn == null)
                    {
                        EndContract(contract);
                    }

                    // if contractor is not atWork, but it is his arrivesAt - spawn him
                    if (!contract.atWork && contract.arrivesAt == GenLocalDate.HourOfDay(map))
                    {
                        _busService.PutPawnsOnTheBusToColony(new List<Pawn> { contract.pawn });
                        contract.atWork = true;
                    }

                    // if contractor is atWork, but it his leavesAt - despawn him and do billing
                    if (contract.atWork && (contract.leavesAt + 1) == GenLocalDate.HourOfDay(map))
                    {
                        _busService.GetPawnsReadyForDeparture(new List<Pawn> { contract.pawn });
                        contract.atWork = false;

                        totalBilling += Mathf.RoundToInt(contract.pawn.Wage());
                        contract.daysHired++;
                    }

                    // while under minor mental breakdown threshold, there is a 5% chance of a pawn leaving
                    //Log.Message($"mood: {contract.pawn.mindState.mentalBreaker.CurMood}");
                    // TODO low mood warning - letter?
                    //if (contract.pawn.mindState.mentalBreaker.CurMood < contract.pawn.mindState.mentalBreaker.BreakThresholdMinor)
                    if (contract.atWork &&
                        contract.pawn.mindState.mentalBreaker.CurMood <
                        contract.pawn.mindState.mentalBreaker.BreakThresholdMinor && Rand.Chance(0.05f))
                    {
                        Messages.Message("Staff left because of low mood!", contract.pawn,
                            MessageTypeDefOf.NegativeEvent);
                        EndContract(contract);
                        return;
                    }
                }

                if (totalBilling > 0)
                    _financeService.doAndBookExpenses(FinanceReport.ReportEntryType.Wages, totalBilling);
            }
        }

        public void EndContract(Pawn pawn)
        {
            foreach (var item in contracts)
                if (item.pawn.Equals(pawn))
                    EndContract(item);
        }

        public void EndContract(HiringContract contract)
        {
            var pawn = contract.pawn;
            contracts.Remove(contract);
            if (pawn == null) return;
            if (contract.atWork && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
            {
                if (pawn.Map != null && pawn.CurJobDef != HADefOf.HA_LeaveMap)
                {
                    pawn.jobs.StopAll();
                    if (!CellFinder.TryFindRandomPawnExitCell(pawn, out var exit))
                        if (!CellFinder.TryFindRandomEdgeCellWith(c =>
                                !pawn.Map.roofGrid.Roofed(c) && c.WalkableBy(pawn.Map, pawn) &&
                                pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly, true, true,
                                    TraverseMode.PassDoors), pawn.Map, 0f, out exit))
                        {
                            // if pawn can not leave for some reason - they just vanish ;)
                            pawn.Destroy();
                            return;
                        }

                    Messages.Message(pawn.NameFullColored + " contract ended. Pawn is leaving.", null,
                        MessageTypeDefOf.NeutralEvent);
                    //pawn.jobs.TryTakeOrderedJob(new Job(HADefOf.HA_LeaveMap, exit));
                    _busService.GetPawnsReadyForDeparture(new List<Pawn> { pawn });
                }
                else if (pawn.GetCaravan() != null)
                {
                    // if pawn was in a caravan while contract ends - just vanish ;)
                    pawn.GetCaravan().RemovePawn(pawn);
                    pawn.Destroy();
                }
            }
            else
            {
                // dead? Still in drop pod?
                pawn.Destroy();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref contracts, "contracts", LookMode.Deep);
            Scribe_Collections.Look(ref candidates, "candidates", LookMode.Deep);
            contracts ??= new List<HiringContract>();
            candidates ??= new List<Pawn>();
        }

        public void hire(Pawn candidate, int type)
        {

            //float wage = candidate.Wage();
            //Log.Message($"hiring {candidate.Name} cost: {wage}");
            //_financeService.doAndBookExpenses(FinanceReport.ReportEntryType.Wages, wage);
            Messages.Message($"{candidate.Name} joins the company", null, MessageTypeDefOf.PositiveEvent);
            candidates.Remove(candidate);
            //_busService.StartBusArrival(new List<Pawn> { candidate });
            SetNewContract(candidate, type);
            //Find.World.worldPawns.PassToWorld(candidate);
        }

        private void addCandidate(int amount, int minTotalSkillLevels, SkillDef skillDef)
        {
            for (int i = 0; i < amount; i++)
            {
                var request = new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer);
                if (skillDef == null)
                    request.ValidatorPreGear = pawn1 => HasCorrectMinTotalSkillLevels(pawn1, minTotalSkillLevels);
                else
                {
                    request.ValidatorPreGear = pawn1 => HasCorrectSkill(pawn1, skillDef);
                }
                request.AllowAddictions = amount == 1 && skillDef == null;
                var pawn = PawnGenerator.GeneratePawn(request);
                candidates.Add(pawn);
            }
        }

        private static bool HasCorrectMinTotalSkillLevels(Pawn pawn, int minTotalSkillLevels)
        {
            int num = 0;
            for (int i = 0; i<pawn.skills.skills.Count; i++)
            {
                num += pawn.skills.skills[i].Level;
                if (num >= minTotalSkillLevels)
                {
                    return true;
                }
            }
            return false;
        }
        
        private static bool HasCorrectSkill(Pawn pawn, SkillDef skillDef)
        {
            for (int i = 0; i<pawn.skills.skills.Count; i++)
            {
                if (pawn.skills.skills[i].def == skillDef)
                {
                    if (pawn.skills.skills[i].Level >= 7)
                    {
                        return true;
                    }
                    if (pawn.skills.skills[i].passion == Passion.Major && pawn.skills.skills[i].Level >= 4)
                    {
                        return true;
                    }
                }
            }
            return false;
        }        

        public void addPawn(int recruitmentType)
        {
            switch (recruitmentType)
            {
                case 0:
                    if (_financeService.doAndBookExpenses(FinanceReport.ReportEntryType.Wages, 20))
                        addCandidate(1, 12, null);    
                    break;
                case 1:
                    if (_financeService.doAndBookExpenses(FinanceReport.ReportEntryType.Wages, 40))
                        addCandidate(2, 24, null); 
                    break;
                case 2:
                    if (_financeService.doAndBookExpenses(FinanceReport.ReportEntryType.Wages, 80))
                        addCandidate(3, 50, null);
                    break;
            }
        }
        
        public void addPawn(int recruitmentType, SkillDef skillDef)
        {
            if (_financeService.doAndBookExpenses(FinanceReport.ReportEntryType.Wages, 100))
                addCandidate(1, 0, skillDef);
        }        
    }
}