using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RimWorld;
using Verse;

namespace RT.DeliverySystem
{
    /*
 * this class basically spawns  vehicles and manages them if they are queued, it is basically the driver for all vehicles
     * TODO: handle multiple vehicles
 */
    public class VehiculumService : MapComponent
    {
        private List<Vehiculum> vehicles;
        private IntVec3 deliveryLocation;
        private IntVec3 startLocation;
        
        public VehiculumService(Map map) : base(map)
        {
            vehicles = new List<Vehiculum>();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<Vehiculum>(ref vehicles, "vehicles", LookMode.Reference);
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            deliveryLocation = new IntVec3(map.Size.x / 2, 0, map.Size.z / 2) + IntVec3.East * 2;
            startLocation = deliveryLocation;
            startLocation.z = map.Size.z - 1;
        }

        public void StartDelivery(IEnumerable<Thing> things)
        {
            var v = SpawnDeliveryVehicle(things, startLocation, map);
            v.SetDestination(deliveryLocation);
            vehicles.Add(v);
        }
        
        public void StartBus(IEnumerable<Pawn> passengers)
        {
            // TODO check if we have a bus that did not leave yet, then add it to that one
            var v = SpawnBus(passengers, startLocation, map);
            v.SetDestination(deliveryLocation);
            vehicles.Add(v);
        }

        public DeliveryVehicle SpawnDeliveryVehicle(IEnumerable<Thing> things, IntVec3 pos, Map map)
        {
            DeliveryVehicle vehicle = (DeliveryVehicle)ThingMaker.MakeThing(RTDefOf.RT_DeliveryVehicle);
            vehicle.innerContainer.TryAddRangeOrTransfer(things);
            vehicle.service = this;
            return (DeliveryVehicle)GenSpawn.Spawn(vehicle, pos, map);
        }
        
        public Bus SpawnBus(IEnumerable<Pawn> pawns, IntVec3 pos, Map map)
        {
            Bus vehicle = (Bus)ThingMaker.MakeThing(RTDefOf.RT_Bus);
            foreach (var pawn in pawns)
            {
                if (pawn.Spawned) pawn.DeSpawn();
            }
            vehicle.innerContainer.TryAddRangeOrTransfer(pawns);
            vehicle.service = this;
            return (Bus)GenSpawn.Spawn(vehicle, pos, map);
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            foreach (var vehicle in vehicles.ToList())
            {
                if (vehicle.State == 0)
                {
                    // TODO check if vehicle driving in front of us
                }
                if (vehicle.State == 1)
                {
                    // TODO check if vehicle in front of us is moving again
                }
                if (vehicle.State == 2)
                {
                    if (vehicle.UnloadTick > 0)
                    {
                        vehicle.UnloadTick--;
                        if (vehicle.UnloadTick == 0)
                        {
                            vehicle.Unload();
                            vehicle.UnloadTick = 1000;                                
                        }
                    }
                }

                if (vehicle.State == 3)
                {
                    vehicle.ageTicks = 0;
                    IntVec3 finalLocation = deliveryLocation;
                    finalLocation.z = 0;
                    vehicle.SetDestination(finalLocation);
                    vehicles.Remove(vehicle); // BYE!                    
                }
            }
        }
    }

}