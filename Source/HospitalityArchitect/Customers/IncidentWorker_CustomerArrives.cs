using System.Collections.Generic;
using System.Linq;
using Hospitality;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace HospitalityArchitect
{
    // modified IncidentWorker_VisitorGroup
    public class IncidentWorker_CustomerArrives : IncidentWorker
    {
        public override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }

            Map map = (Map)parms.target;
            return CanSpawnCustomer(map);
        }

        public virtual Pawn GeneratePawn(Faction faction)
        {
            return PawnGenerator.GeneratePawn(new PawnGenerationRequest(def.pawnKind, faction,
                PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true, allowDead: false, allowDowned: false,
                canGeneratePawnRelations: true, def.pawnMustBeCapableOfViolence, 1f,
                forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: true, allowFood: true,
                allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false,
                forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f,
                null, null, null, null, null, null, null, null, null, null, null, null));
        }

        /*
         * TODO we need to check if there are facilities that visitors can enjoy
         */
        public virtual bool CanSpawnCustomer(Map map)
        {
            var hospital = map.GetComponent<CustomerService>();
            //if (!hospital.IsOpen()) return false;
            //if (hospital.IsRecreationAvailable()) return false;
            //Log.Message((int)HospitalMod.Settings.PatientLimit + " - " + map.GetComponent<HospitalMapComponent>().Patients.Count);
            IntVec3 cell;
            return TryFindEntryCell(map, out cell);
        }



        public override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (!CanSpawnCustomer(map))
            {
                return false;
            }
            Faction faction = Find.FactionManager.AllFactions.Where(f => !f.IsPlayer && !f.defeated && !f.def.hidden && !f.HostileTo(Faction.OfPlayer) && f.def.humanlikeFaction).RandomElement();
            parms.faction = faction;
            Pawn pawn = GeneratePawn(faction);
            CustomerData customer = SpawnCustomers(map, pawn);
            var list = new List<Pawn> { pawn };
            LordMaker.MakeNewLord(parms.faction, CreateLordJob(parms, list), map, list);
            //pawn.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("Patient"), pawn.Position, 1000);
            TaggedString text = def.letterText.Formatted(pawn.Named("PAWN"), customer.Type.ToString()).AdjustedFor(pawn);
            //text += " " + customer.baseCost.ToStringMoney();
            TaggedString title = def.letterLabel.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn);
            Messages.Message(title + ": " + text, MessageTypeDefOf.PositiveEvent);                

            return true;
        }

        // TODO could be more than 1 customer in 1 group?
        protected virtual CustomerData SpawnCustomers(Map map, Pawn pawn)
        {
            pawn.guest.SetGuestStatus(Faction.OfPlayer); // mark as guest otherwise the pawn just wanders off again
            if (pawn.needs.joy == null)
            {
                pawn.needs.AddNeed( DefDatabase<NeedDef>.GetNamed("Joy"));
            }
            pawn.needs.joy.CurLevel = pawn.needs.joy.MaxLevel * 0.1f;

            List<JoyKindDef> joys = new List<JoyKindDef>();
            joys.Add(DefDatabase<JoyKindDef>.GetNamed("Gamble"));
            joys.Add(DefDatabase<JoyKindDef>.GetNamed("Hydrotherapy"));
            
            //joys.Add(DefDatabase<JoyKindDef>.GetNamed("Shopping")); // only works for vending machines - need to check for storefront
            //joys.Add(DefDatabase<JoyKindDef>.GetNamed("Gluttonous")); // actually does not work, it is a gastronomy customer :p 
            // TODO check for gastronomy or storefront as those dont really refer to a joykind this needs a whole other approach
            
            CustomerType type = new List<CustomerType>{CustomerType.Gambling, CustomerType.Wellness}.RandomElement(); // for now, all the rest wont work properly
            joys.RemoveAll(kindDef =>
                !map.listerBuildings.allBuildingsColonist.Any(building => building.def.building.joyKind.Equals(kindDef)));
            //Log.Message(pawn.Label + " -> " +type.ToString());
            CustomerData data = new CustomerData(GenDate.TicksGame, type);
            //TryFindEntryCell(map, out var cell);
            //GenSpawn.Spawn(pawn, cell, map);
            /*var spot = map.listerBuildings.AllBuildingsColonistOfDef(ThingDef.Named("PatientLandingSpot")).RandomElement();
            var loc = DropCellFinder.TryFindSafeLandingSpotCloseToColony(map, IntVec2.Two);
            if (spot != null)
            {
                loc = spot.Position;
            }

            var activeDropPodInfo = new ActiveDropPodInfo();
            activeDropPodInfo.innerContainer.TryAdd(pawn, 1);
            activeDropPodInfo.openDelay = 60;
            activeDropPodInfo.leaveSlag = false;
            activeDropPodInfo.despawnPodBeforeSpawningThing = true;
            activeDropPodInfo.spawnWipeMode = WipeMode.Vanish;
            DropPodUtility.MakeDropPodAt(loc, map, activeDropPodInfo);
            */
            VehiculumService busService = map.GetComponent<VehiculumService>();
            busService.StartBus(new []{pawn});
            
            CustomerService shop = map.GetComponent<CustomerService>();
            //PatientUtility.DamagePawn(pawn, data, hospital);
            shop.CustomerArrived(pawn, data);
            return data;
        }
        
        private bool TryFindEntryCell(Map map, out IntVec3 cell)
        {
            return CellFinder.TryFindRandomEdgeCellWith(
                (IntVec3 c) => map.reachability.CanReachColony(c) && !c.Fogged(map), map,
                CellFinder.EdgeRoadChance_Neutral, out cell);
        }
        
        protected virtual LordJob_VisitColonyAsCustomer CreateLordJob(IncidentParms parms, List<Pawn> pawns)
        {
            //RCellFinder.TryFindRandomSpotJustOutsideColony(pawns[0], out var result);
            return new LordJob_VisitColonyAsCustomer(parms.faction);
        }
    }
}