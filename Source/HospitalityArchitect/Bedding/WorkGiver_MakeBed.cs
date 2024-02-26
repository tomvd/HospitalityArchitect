using System;

namespace HospitalityArchitect;

using System.Collections.Generic;
using DubsBadHygiene;
using RimWorld;
using Verse;
using Verse.AI;

public class WorkGiver_MakeBed : WorkGiver_Scanner
{
	[NullOnMapLoad]
	public static List<ThingDef> cachedGuestBedDefs;

	public override PathEndMode PathEndMode => PathEndMode.Touch;

	public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
	{
		if (cachedGuestBedDefs.NullOrEmpty())
		{
			cachedGuestBedDefs = new List<ThingDef>();
			foreach (ThingDef item in DefDatabase<ThingDef>.AllDefsListForReading)
			{
				if (item.HasComp(typeof(CompHotelGuestBed)) && !item.defName.Equals("ModernTentGuest"))
				{
					cachedGuestBedDefs.Add(item);
				}
				/*if (typeof(Building_washbucket).IsAssignableFrom(item.thingClass))
				{
					cachedGuestBedDefs.Add(item);
				}*/
			}
		}
		foreach (ThingDef def in cachedGuestBedDefs)
		{
			foreach (Thing item2 in pawn.Map.listerThings.ThingsOfDef(def))
			{
				yield return item2;
			}
		}
	}

	public override Job JobOnThing(Pawn pawn, Thing t, bool f)
	{
		if (def.workType == WorkTypeDefOf.Warden && !t.GetRoom().IsPrisonCell)
		{
			return null;
		}
		if (t.Faction != pawn.Faction)
		{
			return null;
		}
		CompHotelGuestBed hotelGuestBed = t.TryGetComp<CompHotelGuestBed>();
		if (!hotelGuestBed.needBedding)
		{
			return null;
		}
		if (!pawn.CanReserveAndReach(t, PathEndMode.ClosestTouch, pawn.NormalMaxDanger(), 1, -1, null, f))
		{
			return null;
		}
		// find clean bedding
		Predicate<Thing> validator = (Thing x) => (!x.IsForbidden(pawn) && pawn.CanReserve(x)) ? true : false;
		LocalTargetInfo localTargetInfo = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForDef(HADefOf.CleanBedding), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, validator);
		// start job makebed
		if (localTargetInfo != null)
		{
			return JobMaker.MakeJob(HADefOf.MakeBed, t, localTargetInfo.Thing);
		}
		JobFailReason.Is("No clean bedding available");
		return null;
	}
}
