using System.Collections.Generic;
using System.Linq;
using DubsBadHygiene;
using RimWorld;
using Verse;
using Verse.AI;

namespace HospitalityArchitect;

public class WorkGiver_RoomClean : WorkGiver_Scanner {
    public override PathEndMode PathEndMode {
        get => PathEndMode.Touch;
    }

    public override ThingRequest PotentialWorkThingRequest {
        get => ThingRequest.ForGroup(ThingRequestGroup.Filth);
    }

    public override int MaxRegionsToScanBeforeGlobalSearch {
        get => 4;
    }

    public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn) {
        return pawn.Map.listerFilthInHomeArea.FilthInHomeArea.FindAll(filth => IsPriorityFilth((Filth)filth));
    }

    private static bool IsPriorityFilth(Filth f) {
        bool isPriority = false;
        if (f.GetRoom() == null || f.GetRoom().Role == null) return false; // Index was outside the bounds of the array.

        if (!isPriority) {
            switch (f.GetRoom()?.Role?.defName) {
                case "GuestRoom":
                    isPriority = f.GetRoom().ContainedBeds.Any(bed => bed.OwnersForReading.Count > 0); break; // only clean rooms when not occupied
                case "PublicBathroom":
                    isPriority = true; break;
                case "PrivateBathroom":
                    isPriority = true; break;
            }
        }

        return isPriority;
    }
    
    
    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false) {
        return pawn.IsColonistPlayerControlled &&
            t is Filth filth &&
            IsPriorityFilth(filth) &&
            filth.Map.areaManager.Home[filth.Position] &&
            pawn.CanReserve(filth, 1, -1, null, forced);
     }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        Job job = new Job(JobDefOf.Clean);
        Map map = t.Map;
        Room room = t.GetRoom(RegionType.Set_Passable);
        int jobQueueMax = 15;
        int jobScanRadius = 30;
        
        if (t is Filth f && IsPriorityFilth(f)) {
            job.AddQueuedTarget(TargetIndex.A, t);
            foreach (IntVec3 intVec in GenRadial.RadialPatternInRadius(jobScanRadius)) {
                IntVec3 pos = intVec + t.Position;
                if (pos.InBounds(map) && pos.GetRoom(map) == room) {
                    pos.GetThingList(map).ForEach(thing => {
                        if (thing != t && HasJobOnThing(pawn, thing, forced)) {
                            job.AddQueuedTarget(TargetIndex.A, thing);
                        }
                    });

                    if (job.GetTargetQueue(TargetIndex.A).Count >= jobQueueMax) { 
                        break;
                    }
                }
            }
        }
        if (job.targetQueueA?.Count >= 5) {
            job.targetQueueA.SortBy(targ => targ.Cell.DistanceToSquared(pawn.Position));
        }
        return job;
    }
}