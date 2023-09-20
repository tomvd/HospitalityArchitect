using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
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
        public int State; // 0 arriving, 1 waiting, 2 unloading, 3 leaving 
        protected IntVec3 destLocation;

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
                UnloadTick = 100;
            }
        }

        protected virtual void LeaveMap() => this.Destroy(DestroyMode.Vanish);
        
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

        public void Unload()
        {
            if (innerContainer.Count == 0)
            {
                State = 3;
                return;
            }
            CellRect deliveryRect = CellRect.CenteredOn(destLocation + IntVec3.West * 4, 1);
            List<IntVec3> deliveryCells = deliveryRect.Cells.InRandomOrder().ToList();
            for (int triedCell = 0; triedCell < deliveryCells.Count;triedCell++)
            {
                if (deliveryCells[triedCell].GetItemCount(Map) > 0)
                {
                    continue;
                }
                GenSpawn.Spawn(innerContainer.GetAt(innerContainer.Count - 1), deliveryCells[triedCell], Map);
                break;
            }
        }
    }
}