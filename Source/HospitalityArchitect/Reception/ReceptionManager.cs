using System.Collections.Generic;
using CashRegister;
using CashRegister.TableTops;
using Verse;

namespace HospitalityArchitect.Reception
{
    /*
     * StoresManagers keeps track of a collection of stores on the map
     */
    public class ReceptionManager : MapComponent
    {
        public List<ReceptionController> Receptions = new();

        public ReceptionManager(Map map) : base(map)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref Receptions, "receptions", LookMode.Deep, map);
            Receptions ??= new List<ReceptionController>();
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            foreach (var store in Receptions) store.FinalizeInit();
            //if (stores.Count == 0) AddStore(); // AddStore also calls FinalizeInit

            // Check unclaimed registers (when adding to an existing game with registers on the map)
            foreach (var register in map.listerBuildings.AllBuildingsColonistOfClass<Building_CashRegister>())
            {
                if (Receptions.Any(r => r.Register.Equals(register))) continue;
                AddReception(register);
            }
            
            // if registers are created or removed - create and remove their stores
            TableTop_Events.onAnyBuildingSpawned.AddListener(RegisterSpawned);
            TableTop_Events.onAnyBuildingDespawned.AddListener(RegisterDespawned);
            
        }
        
        private void RegisterSpawned(Building building, Map mapSpawned)
        {
            if (mapSpawned != map) return;
            if (building is Building_CashRegister register)
            {
                AddReception(register);
            }
        }
        
        private void RegisterDespawned(Building building, Map mapSpawned)
        {
            if (mapSpawned != map) return;
            if (building is Building_CashRegister register)
            {
                DeleteStore(Receptions.FirstOrDefault(r => r.Register.Equals(register)));
            }
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            ReceptionUtility.OnTick();
            foreach (var store in Receptions) store.OnTick();
        }

        public ReceptionController GetLinkedReception(Building_CashRegister register)
        {
            return Receptions.FirstOrDefault(controller => controller.Register.Equals(register));
        }

        
        private void AddReception(Building_CashRegister register)
        {
            //Log.Message("AddStore for new register...");
            var store = new ReceptionController(map);
            store.LinkRegister(register);
            //Log.Message("New store is now linked to register at " + store.Register.Position);

            // Find an unused name, numbering upwards
            for (int i = 1; i < 100; i++)
            {
                var name = "ReceptionDefaultName".Translate(Receptions.Count + i);
                if (NameIsInUse(name, store)) continue;

                store.Name = name;
                break;
            }
            //Log.Message("New store got a temporary name " + store.Name);
            store.FinalizeInit();
            Receptions.Add(store);
            //Log.Message("We now have " + Stores.Count + " stores.");
        }

        private void DeleteStore(ReceptionController store)
        {
            store?.CleanUpForRemoval();
            Receptions.Remove(store);
        }

        public bool NameIsInUse(string name, ReceptionController store)
        {
            return Receptions.Any(controller => controller != store && controller.Name.EqualsIgnoreCase(name));
        }
    }
}