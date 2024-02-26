using System;
using System.Collections.Generic;
using System.Linq;
using CashRegister;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace HospitalityArchitect.Reception
{
    public static class ReceptionUtility
    {
        private static readonly Dictionary<Pair<Pawn, Region>, bool> dangerousRegionsCache =
            new Dictionary<Pair<Pawn, Region>, bool>();

        private static int lastTick;

        public static void OnTick()
        {
            if (GenTicks.TicksGame > lastTick)
                if (GenTicks.TicksGame % GenTicks.TickRareInterval == 0)
                {
                    // RARE TICK
                    dangerousRegionsCache.Clear();
                    lastTick = GenTicks.TicksGame;
                }
        }

        public static ReceptionManager GetReceptionManager(this Thing thing)
        {
            return GetReceptionManager(thing.Map);
        }

        public static ReceptionManager GetReceptionManager(this Map map)
        {
            return map?.GetComponent<ReceptionManager>();
        }

        public static List<ReceptionController> GetAllOpenReceptions(this Thing thing)
        {
            return GetReceptionManager(thing.Map).Receptions.Where(r => r.openForBusiness).ToList();
        }

        public static IEnumerable<ReceptionController> GetAllReceptionsEmployed(this Pawn pawn)
        {
            return GetReceptionManager(pawn.Map).Receptions.Where(r => r.HasToWork(pawn));
        }

        public static ReceptionManager GetReceptionManager(this ReceptionController store)
        {
            return GetReceptionManager(store.Map);
        }

        public static ReceptionController GetReception(this Building_CashRegister register)
        {
            return register.GetReceptionManager().GetLinkedReception(register);
        }

        public static bool IsRegionDangerous(Pawn pawn, Danger maxDanger, Region region = null)
        {
            region ??= pawn.GetRegion();
            var key = new Pair<Pawn, Region>(pawn, region);
            if (dangerousRegionsCache.TryGetValue(key, out bool result)) return result;

            var isRegionDangerous = region.DangerFor(pawn) > maxDanger;
            dangerousRegionsCache.Add(key, isRegionDangerous);

            return isRegionDangerous;
        }

        // Copied from ToilEffects, had to remove Faction check
        public static Toil WithProgressBar(
            this Toil toil,
            TargetIndex ind,
            Func<float> progressGetter,
            bool interpolateBetweenActorAndTarget = false,
            float offsetZ = -0.5f)
        {
            Effecter effecter = null;
            toil.AddPreTickAction(() =>
            {
                //if (toil.actor.Faction != Faction.OfPlayer)
                //    return;
                if (effecter == null)
                {
                    effecter = EffecterDefOf.ProgressBar.Spawn();
                }
                else
                {
                    LocalTargetInfo target = toil.actor.CurJob.GetTarget(ind);
                    if (!target.IsValid || target.HasThing && !target.Thing.Spawned)
                        effecter.EffectTick((TargetInfo)toil.actor, TargetInfo.Invalid);
                    else if (interpolateBetweenActorAndTarget)
                        effecter.EffectTick(toil.actor.CurJob.GetTarget(ind).ToTargetInfo(toil.actor.Map),
                            (TargetInfo)toil.actor);
                    else
                        effecter.EffectTick(toil.actor.CurJob.GetTarget(ind).ToTargetInfo(toil.actor.Map),
                            TargetInfo.Invalid);
                    MoteProgressBar mote = ((SubEffecter_ProgressBar)effecter.children[0]).mote;
                    if (mote == null)
                        return;
                    mote.progress = Mathf.Clamp01(progressGetter());
                    mote.offsetZ = offsetZ;
                }
            });
            toil.AddFinishAction(() =>
            {
                if (effecter == null)
                    return;
                effecter.Cleanup();
                effecter = null;
            });
            return toil;
        }

        public static bool IsWaitingInQueue(this Pawn pawn)
        {
            if (pawn?.jobs?.curDriver is JobDriver_CheckIn buyJob)
                return buyJob.WaitingInQueue;
            else
                return false;
        }

        public static bool IsWaitingToBeServed(this Pawn pawn)
        {
            if (pawn?.jobs?.curDriver is JobDriver_CheckIn buyJob)
                return buyJob.WaitingToBeServed;
            else
                return false;
        }

        public static void GiveWaitThought(Pawn patron)
        {
            patron.needs.mood?.thoughts.memories.TryGainMemory(ShoppingDefOf.HospitalityArchitect_HadToWait);
        }
    }
}
