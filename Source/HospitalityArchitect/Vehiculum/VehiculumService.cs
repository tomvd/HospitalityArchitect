using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace HospitalityArchitect
{
    /*
     * this class basically spawns  vehicles and manages them if they are queued, it is basically the driver for all vehicles
     * 
 */
    public class VehiculumService : MapComponent
    {
        private List<Vehiculum> vehicles;
        private IntVec3 deliveryLocation;
        private IntVec3 startLocation;
        private int lastHourChecked;
        private int ticksDelay; // this delay so shortlived (5ticks) that it is not needed to persist
        
        public VehiculumService(Map map) : base(map)
        {
            vehicles = new List<Vehiculum>();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<Vehiculum>(ref vehicles, "vehicles", LookMode.Reference);
            Scribe_Values.Look(ref lastHourChecked, "lastHourChecked");
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

        public void PutPawnsOnTheBusToColony(List<Pawn> passengers)
        {
            var busOnTheWay = vehicles.Find(v => v is Bus b && b.State == 0) as Bus;
            // just add pawns on an already driving bus :)
            if (busOnTheWay != null)
            {
                AddPawnsToBus(passengers, busOnTheWay);
            }
            else
            {
                var newBus = SpawnBus(passengers, startLocation, map);
                newBus.SetDestination(deliveryLocation);
                vehicles.Add(newBus);
            }
        }
        public void GetPawnsReadyForDeparture(List<Pawn> passengers)
        {
            var waitingLocation = deliveryLocation + IntVec3.West * 4;
            foreach (var passenger in passengers)
            {
                passenger.jobs.ClearQueuedJobs();
                passenger.jobs.StopAll();
                passenger.jobs.StartJob(new Job(HADefOf.WaitForBus, waitingLocation));                
            }            
        }        

        public DeliveryVehicle SpawnDeliveryVehicle(IEnumerable<Thing> things, IntVec3 pos, Map map)
        {
            DeliveryVehicle vehicle = (DeliveryVehicle)ThingMaker.MakeThing(HADefOf.HA_DeliveryVehicle);
            vehicle.innerContainer.TryAddRangeOrTransfer(things);
            vehicle.service = this;
            return (DeliveryVehicle)GenSpawn.Spawn(vehicle, pos, map);
        }
        
        public Bus SpawnBus(IEnumerable<Pawn> pawns, IntVec3 pos, Map map)
        {
            Bus bus = (Bus)ThingMaker.MakeThing(HADefOf.HA_Bus);
            if (pawns.Any())
            {
                AddPawnsToBus(pawns, bus);
            }
            bus.service = this;
            return (Bus)GenSpawn.Spawn(bus, pos, map);
        }

        private static void AddPawnsToBus(IEnumerable<Pawn> pawns, Bus bus)
        {
            foreach (var pawn in pawns)
            {
                if (pawn.Spawned) pawn.DeSpawn();
            }

            bus.innerContainer.TryAddRangeOrTransfer(pawns);
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            vehicles.RemoveAll(vehiculum => vehiculum == null); // bit weird but vehicles sometimes get bugged;

            if (GenLocalDate.HourOfDay(map) != lastHourChecked)
            {
                ticksDelay++;
                if (ticksDelay > 5)
                {
                    ticksDelay = 0;
                    lastHourChecked = GenLocalDate.HourOfDay(map);
                    var busOnTheWay = vehicles.Find(v => v is Bus b && b.State == 0) as Bus;

                    if (busOnTheWay != null || map.mapPawns.pawnsSpawned.Any(pa => pa.CurJobDef.Equals(HADefOf.WaitForBus)))
                    {
                        if (busOnTheWay == null)
                        {
                            Bus b = SpawnBus(new List<Pawn>(), startLocation, map);
                            b.SetDestination(deliveryLocation);
                            vehicles.Add(b);
                        }
                    }
                }
            }
            
            
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
                            vehicle.UnloadLoad();
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