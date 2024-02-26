using Hospitality;
using RimWorld;
using Verse;

namespace HospitalityArchitect;

public static class TentUtility
{
    
    // we basically replace the camping tent spot by a tent
    public static Building_GuestBed SetUpTent(Building_GuestBed spot)
    {
        foreach (var pawn in spot.CurOccupants)
        {
            RestUtility.KickOutOfBed(pawn, spot);
        }
        spot.RemoveAllOwners();

        Building_Bed tent;
        var newDef = DefDatabase<ThingDef>.GetNamed("ModernTentGuest");
        tent = (Building_Bed)ThingMaker.MakeThing(newDef, ThingDefOf.Cloth);
        tent.SetFactionDirect(spot.Faction);
        Building_GuestBed spawnedTent = (Building_GuestBed) GenSpawn.Spawn(tent, spot.Position, spot.Map, spot.Rotation);
        spawnedTent.ForPrisoners = false;
        spawnedTent.Medical = false;
        spawnedTent.SetRentalFee(spot.GetRentalFee());
        spawnedTent.GetComp<CompHotelGuestBed>().guestType = DefDatabase<PawnKindDef>.GetNamed("CamperGuest", false);
        return spawnedTent;
    }    
    
    public static Building_GuestBed BreakDownTent(Building_GuestBed tent)
    {
        foreach (var pawn in tent.CurOccupants)
        {
            RestUtility.KickOutOfBed(pawn, tent);
        }
        tent.RemoveAllOwners();

        Building_GuestBed spot;
        var newDef = DefDatabase<ThingDef>.GetNamed("CampingTentSpotGuest");
        spot = (Building_GuestBed)ThingMaker.MakeThing(newDef);
        spot.SetFactionDirect(tent.Faction);
        Building_GuestBed spawnedSpot = (Building_GuestBed) GenSpawn.Spawn(spot, tent.Position, tent.Map, tent.Rotation);
        spawnedSpot.ForPrisoners = false;
        spawnedSpot.Medical = false;
        spawnedSpot.SetRentalFee(tent.GetRentalFee());
        spawnedSpot.GetComp<CompHotelGuestBed>().guestType = DefDatabase<PawnKindDef>.GetNamed("CamperGuest", false);
        Find.Selector.Select(spawnedSpot, false);
        if (!tent.Destroyed) tent.Destroy();
        return spawnedSpot;
    }

    public static bool StayedInTent(Pawn guest)
    {
        if (guest.ownership.OwnedBed == null) return false;
        return guest.ownership.OwnedBed.def.defName.Equals("ModernTentGuest");
    }
}