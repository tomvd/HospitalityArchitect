using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using UnityEngine;

namespace RT
{
    public class Contract : IExposable {
        public Contract() {}
        public Contract(int startTicks, HireableFactionDef factionDef, Hireable hireable, Pawn pawn, int contractType, Map map) 
        {
            this.startTicks = startTicks;
            if (contractType == 0) this.endTicks = startTicks + 8 * GenDate.TicksPerHour;
            if (contractType == 1) this.endTicks = startTicks + GenDate.TicksPerQuadrum;
            this.lastBilledTicks = startTicks;
            this.factionDef = factionDef;
            this.hireable = hireable;
            this.pawn = pawn;
            this.contractType = contractType;
            this.map = map;
        }
        public int startTicks;
        public int endTicks;
        public int lastBilledTicks;
        public HireableFactionDef factionDef;
        public Hireable hireable;
        public Pawn pawn;
        public int contractType;
        public Map map;

        public void ExposeData()
        {
            Scribe_Values.Look(ref startTicks, "startTicks");
            Scribe_Values.Look(ref endTicks, "endTicks");
            Scribe_Values.Look(ref lastBilledTicks, "lastBilledTicks");
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_References.Look(ref hireable, "hireable");
            Scribe_Values.Look(ref contractType, "contractType");
            Scribe_Defs.Look(ref factionDef, "faction");            
            Scribe_References.Look(ref map, "map");
        }
    }
    public class HiringContractTracker : WorldComponent
    {
        public List<Contract> contracts = new List<Contract>();

        public HiringContractTracker(World world) : base(world)
        {
            contracts = new List<Contract>();
        }

        public string GetInfoText() => "";

        public Faction GetFaction() => null;

        public bool IsHired(Pawn pawn) {
            return contracts.Any(contract => contract.pawn.Equals(pawn));
        }

        public void SetNewContract(int contractType, Pawn pawn, Hireable hireable, HireableFactionDef faction, Map map)
        {
            contracts.Add(new Contract(Find.TickManager.TicksAbs,faction,hireable,pawn,contractType, map));
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            if (Find.TickManager.TicksAbs % 500 == 0) {
                foreach (var contract in contracts.ToList())
                {
                    if (Find.TickManager.TicksAbs >= contract.endTicks) {
                        this.EndContract(contract);
                        return;
                    }
                    var totalBilling = 0;
                    var targetMap = contract.map;
                    var availableSilver = targetMap.listerThings.ThingsOfDef(ThingDefOf.Silver)
                                    .Where(x => !x.Position.Fogged(x.Map) && (targetMap.areaManager.Home[x.Position] || x.IsInAnyStorage())).Sum(t => t.stackCount);

                    if (contract.contractType == 0) {
                        // hourly billing
                        if ((Find.TickManager.TicksAbs - contract.lastBilledTicks) > GenDate.TicksPerHour) {
                            totalBilling += Mathf.RoundToInt(contract.pawn.kindDef.combatPower);
                            if (availableSilver < totalBilling) {
                                Messages.Message("Failed to pay staff!", null, MessageTypeDefOf.NegativeEvent);
                                this.EndContract(contract);
                                return;
                            }
                            contract.lastBilledTicks = Find.TickManager.TicksAbs;
                        }
                    }
                    if (contract.contractType == 1) {
                        // daily billing
                        if ((Find.TickManager.TicksAbs - contract.lastBilledTicks) > GenDate.TicksPerDay) {
                            totalBilling += Mathf.RoundToInt(contract.pawn.kindDef.combatPower * 8);
                            if (availableSilver < totalBilling) {
                                Messages.Message("Failed to pay staff!", null, MessageTypeDefOf.NegativeEvent);
                                this.EndContract(contract);
                                return;
                            }
                            contract.lastBilledTicks = Find.TickManager.TicksAbs;
                        }
                    }
                    Find.World.GetComponent<FinanceTracker>().bookHiringExpenses(targetMap, totalBilling);
                }
            }
        }

        public void EndContract(Pawn pawn)
        {
            foreach (var item in contracts)
            {
                if (item.pawn.Equals(pawn)) EndContract(item);
            }

        }

        public void EndContract(Contract contract)
        {
            Pawn pawn = contract.pawn;
            contracts.Remove(contract);
            if (pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving))
            {
                if (pawn.Map != null && pawn.CurJobDef != RTDefOf.RT_LeaveMap)
                {
                    pawn.jobs.StopAll();
                    if (!CellFinder.TryFindRandomPawnExitCell(pawn, out IntVec3 exit))
                        if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => !pawn.Map.roofGrid.Roofed(c) && c.WalkableBy(pawn.Map, pawn) &&
                                                                                    pawn.CanReach(c, PathEndMode.OnCell, Danger.Deadly, canBashDoors: true, canBashFences: true,
                                                                                                TraverseMode.PassDoors), pawn.Map, 0f, out exit))
                        {
                            // if pawn can not leave for some reason - they just vanish ;)
                            pawn.Destroy();
                            return;
                        }
                    Messages.Message(pawn.NameFullColored + " contract ended. Pawn is leaving.", null, MessageTypeDefOf.NeutralEvent);
                    pawn.jobs.TryTakeOrderedJob(new Job(RTDefOf.RT_LeaveMap, exit));
                }
                else if (pawn.GetCaravan() != null)
                {
                    // if pawn was in a caravan while contract ends - just vanish ;)
                    pawn.GetCaravan().RemovePawn(pawn);
                    pawn.Destroy();
                }
            } else {
                // dead? Still in drop pod?
                pawn.Destroy();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref contracts, "contracts", LookMode.Deep);
            if (contracts is null) contracts = new List<Contract>();
        }
    }
}