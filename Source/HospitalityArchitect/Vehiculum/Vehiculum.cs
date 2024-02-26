using System;
using System.Collections.Generic;
using System.Linq;
using Hospitality;
using Hospitality.Utilities;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace HospitalityArchitect
{
    /*
* a vehiculum is a transport that moves along the road (z-axis), it can be given a destination and will be moving towards it
     * and then stop, or completly despawns if its off the map. 
*/
    public class Vehiculum : ThingWithComps
    {
        public int ageTicks;
        private Sustainer anticipationSoundPlaying;
        public float maxSpeed = 0.1f;
        public float currentSpeed = 0.1f;
        public float deltaSpeed = 0.001f;
        public Vector3 currentPos;
        public Vector3 destPos;
        public VehiculumService service;
        private const float Margin = 0.1f;
        private const float RampMargin = 5f;
        public int UnloadTick;
        public ThingOwner innerContainer;
        public int State; // 0 arriving, 1 waiting, 2 unloading, 3 leaving, 4 loading
        protected IntVec3 destLocation;
        public int LOAD_UNLOAD_DELAY = 100;
        public bool load;

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Values.Look<int>(ref this.ticksToImpact, "ticksToImpact");
            //Scribe_Values.Look<int>(ref this.ticksToDiscard, "ticksToDiscard");
            Scribe_Values.Look<int>(ref this.ageTicks, "ageTicks");
            Scribe_Values.Look<int>(ref this.UnloadTick, "UnloadTick");
            Scribe_Values.Look(ref currentPos, "currentPos");
            Scribe_Values.Look(ref destPos, "destPos");
            Scribe_Values.Look(ref State, "State");
            Scribe_Values.Look(ref destLocation, "destLocation");
            Scribe_Values.Look(ref load, "load");
            //Scribe_Values.Look<int>(ref this.ticksToImpactMax, "ticksToImpactMax", this.LeaveMapAfterTicks);
            //Scribe_Values.Look<float>(ref this.angle, "angle");
            Scribe_Deep.Look<ThingOwner>(ref this.innerContainer, "innerContainer", (object)this);
        }
        
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            this.innerContainer.ClearAndDestroyContents();
        }

        public override void Draw()
        {
            base.Draw();
        }

        public override Vector3 DrawPos => currentPos;

        
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            currentPos = this.TrueCenter();
            Log.Message("spawnpos:" + currentPos);
        }
/*
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            base.Destroy(mode);
            if (this.anticipationSoundPlaying == null)
                return;
            this.anticipationSoundPlaying.End();
            this.anticipationSoundPlaying = (Sustainer)null;
        }
*/
        public override void Tick()
        {
            base.Tick();
            currentPos.z -= currentSpeed;
            SetPositionDirect(currentPos.ToIntVec3());
            //SetPositionDirect();
            ++ageTicks;
            if (ageTicks > 10000 || currentPos.z <= RampMargin)
            {
                Log.Message("leavepos:" + currentPos + "ageTicks:"+ageTicks);
                LeaveMap();
            }
            if ((currentPos.z - destPos.z) > RampMargin && currentSpeed < maxSpeed)
            {
                // still far away, if not at maxSpeed, accelerate!
                currentSpeed += deltaSpeed;
            }
            if ((currentPos.z - destPos.z) < RampMargin && currentSpeed > deltaSpeed)
            {
                // getting closer, stop!
                currentSpeed -= deltaSpeed;
            }
            if (AtDestination() && currentSpeed > 0)
            {
                // we arrived, notify service and stand still
                currentSpeed = 0;
                Log.Message("AtDestination:" + currentPos);
                State = 2;
                UnloadTick = LOAD_UNLOAD_DELAY;
            }
        }

        protected virtual void LeaveMap()
        {
            foreach (Thing item in (IEnumerable<Thing>)innerContainer)
            {
                if (item is Pawn pawn)
                {
                    pawn.ExitMap(allowedToJoinOrCreateCaravan: false, Rot4.Invalid);
                }
            }
            innerContainer.ClearAndDestroyContentsOrPassToWorld(DestroyMode.QuestLogic);
            this.Destroy(DestroyMode.Vanish);
        } 
        
        public void SetDestination(IntVec3 location)
        {
            destLocation = location;
            destPos = location.ToVector3Shifted();
            currentSpeed = 0.01f;
        }
        public bool AtDestination()
        {
            return Math.Abs(currentPos.z - destPos.z) < Margin;
        }
        public bool AtDestination(IntVec3 loc)
        {
            return Math.Abs(currentPos.z - loc.ToVector3Shifted().z) < Margin;
        }

        public void UnloadLoad()
        {
            if (load)
            {
                Load();
            }
            else
            {
                Unload();
            }
        }
        
        // Currently only works for pawns!
        private void Load()
        {
            CellRect deliveryRect = CellRect.CenteredOn(destLocation + IntVec3.West * 4, 1);
            List<IntVec3> deliveryCells = deliveryRect.Cells.InRandomOrder().ToList();
            Pawn p = null;
            for (int triedCell = 0; triedCell < deliveryCells.Count;triedCell++)
            {
                p = deliveryCells[triedCell].GetFirstPawn(Map); //GetItemCount(Map)
                if (p != null && p.CurJobDef.Equals(HADefOf.WaitForBus)) 
                {
                    //p.jobs.StopAll(false, false);
                    //p.ClearMind();
                    // before the vehicle leaves, we drop off the guests again and let them leave the map on their own
                    // weird stuff happens if we do not finish this lordjob properly
                    if (p.IsGuest() && p.GetLord() != null && p.GetLord().LordJob is Hospitality.LordJob_VisitColony lordJob)
                    {
                        p.guest?.SetGuestStatus(null);
                        p.GetComp<CompGuest>()?.Leave(false);
                        if (p.GetLord().ownedPawns.Where(p => p.Spawned).EnumerableCount() < 2)
                            p.GetLord().Destroy(); // last pawn destroys the lord
                    }
                    p.DeSpawn();

                    innerContainer.TryAddOrTransfer(p); // in the bus we go
                    break;
                }
            }
            if (p is null) // no more pawns waiting - lets go!
            {
                State = 3;
                return;
            }            
            UnloadTick = LOAD_UNLOAD_DELAY;            
        }

        private void Unload() {            
            if (innerContainer.Count == 0)
            {
                State = 3;
                return;
            }
            CellRect deliveryRect = CellRect.CenteredOn(destLocation + IntVec3.West * 4, 1);
            List<IntVec3> deliveryCells = deliveryRect.Cells.InRandomOrder().ToList();
            for (int triedCell = 0; triedCell < deliveryCells.Count;triedCell++)
            {
                bool isStaff = false;
                if (deliveryCells[triedCell].GetItemCount(Map) > 0)
                {
                    continue;
                }
                if (innerContainer.GetAt(innerContainer.Count - 1) is Pawn p && p.IsWorldPawn())
                {
                    Find.WorldPawns.RemovePawn(p);
                    if (Find.CurrentMap.GetComponent<HiringContractService>().IsHired(p))
                    {
                        p.SetFaction(Faction.OfPlayer);
                        isStaff = true;
                    }
                }
                Thing spawn = GenSpawn.Spawn(innerContainer.GetAt(innerContainer.Count - 1), deliveryCells[triedCell], Map);
                if (spawn is Pawn pawn && pawn.IsGuest())
                {
                    GuestUtility.SetUpHotelGuest(pawn);
                }

                if (isStaff && spawn is Pawn staff)
                {
                    StaffUtility.SetUpArrivingStaff(staff);
                }

                break;
            }
            UnloadTick = LOAD_UNLOAD_DELAY;
        }
        
        // picks up all pawns in the delivery area
        public void Load(Map map)
        {
            CellRect deliveryRect = CellRect.CenteredOn(destLocation + IntVec3.West * 4, 1);
            List<IntVec3> deliveryCells = deliveryRect.Cells.InRandomOrder().ToList();
            for (int triedCell = 0; triedCell < deliveryCells.Count;triedCell++)
            {
                Pawn p = deliveryCells[triedCell].GetFirstPawn(map);
                if (p == null)
                {
                    continue;
                }
                innerContainer.TryAddOrTransfer(p);
                break;
            }
            State = 3;
        }        
    }
}