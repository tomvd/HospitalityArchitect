using System.Collections.Generic;
using RimWorld;
using Verse;

namespace HospitalityArchitect;

public class GuestTypeDef : DefModExtension
{
    public IntRange budget = new IntRange(10, 15);
    public int bedBudget = 0;
    public bool isCamper = false;
    public bool bringsFood = false;
    public float travelWithPartnerChance = 0.0f;
    public bool dayVisitor = false;
    public IntRange arrivesAt = new IntRange(14, 16);
    public IntRange leavesAt = new IntRange(10, 12);
    public int maxVisitors = 15; // limit to visitors a day - ever - totals to about a limit of 100 over all types
    public int baseRating = 50; // 0-100 starting point of the guest rating
    public float seasonalVariance = 0f; // how much the season affects the bookings
    public float initRest = 0.5f;
    public float initJoy = 0.5f;
    public float initFood = 0.5f;
    public PawnKindDef influenceSpillover;
    
    public List<JoyKindDef> needsJoyKind; // roughly means which joykinds the guest needs to make a booking, special cases are
    // Gluttonous - which means the guest needs a restaurant, which is open during visit time and staffed
    // Shopping - needs a storefront, which is open during visit time and staffed.
    // Gamble - needs at least two(!) of the casino building types: slots, poker, roulette
    // Hydrotherapy - at least two(!) of the hygiene building types: pool, hottub, sauna
    
    // VIP/luxury guest requirements
    public RoyalTitleFoodRequirement foodRequirement;
    public List<RoomRequirement> bedroomRequirements;
    public RoomRequirement_FacilitiesAvailable facilityRequirements;
    
}