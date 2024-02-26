using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace HospitalityArchitect;

/*
 * handles marketing but also guest type data such as the guest bookings/arrivals
 */
public class MarketingService : MapComponent
{
    public Dictionary<PawnKindDef, GuestTypeData> MarketingData;
    
    private List<PawnKindDef> defs;
    private List<GuestTypeData> values;
    public List<GuestRating> guestRatings;

    public bool campaignChangedToday;
    public PawnKindDef campaignRunning;
    private FinanceService _financeService;
    
    public MarketingService(Map map) : base(map)
    {
        InitMarketingData();
        _financeService = map.GetComponent<FinanceService>();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref MarketingData, "MarketingData", LookMode.Def, LookMode.Deep, ref defs, ref values);
        Scribe_Defs.Look(ref campaignRunning, "campaignRunning");
        Scribe_Values.Look(ref campaignChangedToday, "campaignChangedToday");
        Scribe_Collections.Look(ref guestRatings, "guestRatings", LookMode.Deep);
        guestRatings ??= new List<GuestRating>();
        InitMarketingData();
    }

    private void InitMarketingData()
    {
        MarketingData ??= new Dictionary<PawnKindDef, GuestTypeData>();
        foreach (PawnKindDef guestTypeDef in DefDatabase<PawnKindDef>.AllDefsListForReading.Where(def => def.GetModExtension<GuestTypeDef>() != null))
        {
            if (MarketingData.GetValueOrDefault(guestTypeDef, null) == null)
            {
                MarketingData.Add(guestTypeDef, new GuestTypeData());
            }
        }
    }

    public void DoDaily(int today)
    {
        // handle running marketing campaigns before bookings - as a campaign might fail (funds)
        if (campaignRunning != null)
        {
            GuestTypeDef type = campaignRunning.GetModExtension<GuestTypeDef>();
            var cost = MarketingUtility.GetMarketingCost(type);
            if (_financeService.canAfford(cost))
            {
                _financeService.doAndBookExpenses(FinanceReport.ReportEntryType.Marketing, cost);
            }
            else
            {
                Messages.Message("Marketing campaign stopped due to insufficient funds!", MessageTypeDefOf.NegativeEvent);
                campaignRunning = null;
            }

            campaignChangedToday = false;
        }
        
        var totalBookings = 0;
        foreach (var guestTypeData in MarketingData)
        {
            GuestTypeDef type = guestTypeData.Key.GetModExtension<GuestTypeDef>();
            if (type.dayVisitor && !type.facilityRequirements.Met(map))
            {
                break;
            }
            float marketing = campaignRunning == guestTypeData.Key ? 1 : 0;
            float bookings = Math.Min(guestTypeData.Value.influencePoints + marketing,
                GuestUtility.QualifiedBedsCount(map, type, guestTypeData.Key));
            if (bookings > type.maxVisitors)
            {
                // soft cap : it really gets hard to get over this number of visitors a day
                 // (all influence points over the softcap only count 10%)
                 bookings = type.maxVisitors + (guestTypeData.Value.influencePoints - type.maxVisitors) * 0.1f;
            }
            if (type.seasonalVariance > 0)
            {
                if (GenLocalDate.Season(map).Equals(Season.Fall) || GenLocalDate.Season(map).Equals(Season.Spring))
                {
                    bookings *= 1.0f - (0.5f * type.seasonalVariance);
                }    
                if (GenLocalDate.Season(map).Equals(Season.Winter))
                {
                    bookings *= 1.0f - (0.80f * type.seasonalVariance);
                }    
            }

            guestTypeData.Value.bookings = (int)Math.Round(bookings);
            
            // spread bookings hourly
            // TODO.. take into account facility capacity
            List<int> bookingHours = new List<int>();
            for (int i = 0; i < guestTypeData.Value.bookings; i++)
            {
                bookingHours.Add(type.arrivesAt.RandomInRange);
            }
            var sorted = bookingHours.OrderBy(b => b);
            guestTypeData.Value.bookingHours = new Stack<int>(sorted);
            Log.Message($"New booking hours for {guestTypeData.Key.defName} : {String.Join(",", guestTypeData.Value.bookingHours)}");
            totalBookings += guestTypeData.Value.bookings;
        }
        Messages.Message("New bookings: " + totalBookings, MessageTypeDefOf.NeutralEvent);
        
        // natural decline of 5% daily
        foreach (var guestTypeData in MarketingData)
        {
            guestTypeData.Value.influencePoints *= 0.95f;
        }

    }

    public void DoMinutely()
    {
        // handle guest arrivals
        if (!DefDatabase<PawnKindDef>.AllDefsListForReading.Any(hasAnyBooking)) return;
        PawnKindDef pawnKind = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(hasAnyBooking).RandomElement();
        GuestTypeDef type = pawnKind.GetModExtension<GuestTypeDef>();
        GuestTypeData data;
        MarketingData.TryGetValue(pawnKind, out data);
        //float baseChance = 0.1f * data.bookings;  
        if (GuestUtility.EmptyQualifiedBedsAvailable(map, type, pawnKind) > 0
            && GenLocalDate.HourOfDay(map) == data.bookingHours.Peek())
        {
            data.bookingHours.Pop();
            data.bookings--;
            IncidentParms parms = new IncidentParms();
            parms.target = map;
            parms.faction = Find.FactionManager.AllFactions.Where(f =>
                !f.IsPlayer && !f.defeated && !f.def.hidden && !f.HostileTo(Faction.OfPlayer) &&
                f.def.humanlikeFaction).RandomElement();
            parms.pawnCount = 1;
            if (Rand.Chance(type.travelWithPartnerChance))
            {
                if (GuestUtility.EmptyQualifiedBedsAvailable(map, type, pawnKind, true) > 0)
                {
                    parms.pawnCount = 2;
                }
            }
            parms.pawnKind = pawnKind;
            DefDatabase<IncidentDef>.GetNamed("HotelGuestGroup").Worker.TryExecuteWorker(parms);
        }
        
    }

    private bool hasAnyBooking(PawnKindDef def)
    {
        GuestTypeDef type = def.GetModExtension<GuestTypeDef>();
        GuestTypeData data;
        MarketingData.TryGetValue(def, out data);
        if (data == null) return false;
        return (data.bookings > 0);
    }

    // convert pawns' thoughts in a rating
    public void leaveRating(Pawn pawn)
    {
        var rating = GetRating(pawn, true);
        if (rating is > 50 and < 75 && Rand.Chance(0.5f)) return; // only half of ppl would leave a rating if it was only mediocre
        if (rating > 99 && Rand.Chance(0.3f)) // 30% give a tip when the stay was perfect
        {
            float tip = 0;
            var silver = pawn.inventory.innerContainer.FirstOrDefault(i => i.def == ThingDefOf.Silver);
            var money = silver?.stackCount ?? 0;
            tip = money / 2;
            if (tip > 0)
            {
                pawn.inventory.innerContainer.TryDrop(silver, pawn.Position, pawn.Map, ThingPlaceMode.Near, (int)tip,
                    out silver);
                pawn.Map.GetComponent<FinanceService>().bookIncome(FinanceReport.ReportEntryType.Beds, tip);

                Messages.Message($"Guest left a {tip.ToStringMoney()} tip because of good service.",
                    MessageTypeDefOf.PositiveEvent);
            }
        }
        
        Messages.Message("Guest left a rating: " + (rating/100.0f).ToStringPercent(), MessageTypeDefOf.NeutralEvent);
        GuestTypeData data;
        MarketingData.TryGetValue(pawn.kindDef, out data);
        data.influencePoints += (rating / 100.0f)-0.5f; // influence goes up with a >50% rating and goes down with a <50% rating
        // handle influence spillover (meaning that a good rating does not only affect this guest type, but maybe a related one as well)
        GuestTypeDef hotelGuest = pawn.kindDef.GetModExtension<GuestTypeDef>();
        if (hotelGuest.influenceSpillover != null)
        {
            float spillOverFactor = 0.1f;
            MarketingData.TryGetValue(hotelGuest.influenceSpillover, out data);
            data.influencePoints += spillOverFactor * ((rating / 100.0f)-0.5f); // influence goes up with a >50% rating and goes down with a <50% rating
        }
    }

    public int GetRating(Pawn pawn, bool save)
    {
        GuestTypeDef hotelGuest = pawn.kindDef.GetModExtension<GuestTypeDef>();
        int rating = hotelGuest.baseRating;
        List<Thought> thoughts = new List<Thought>();
        pawn.needs.mood.thoughts.GetAllMoodThoughts(thoughts);
        string bestThought = "";
        int bestScore = 0;
        string worstThought = "";
        int worstScore = 0;
        foreach (var thought in thoughts.Distinct())
        {
            GuestThoughtRating gtr = thought.def.GetModExtension<GuestThoughtRating>();
            if (gtr != null)
            {
                rating += gtr.ratings[thought.CurStageIndex];
                if (gtr.ratings[thought.CurStageIndex] > bestScore) bestThought = thought.CurStage.LabelCap + " (+" + gtr.ratings[thought.CurStageIndex] + ")";
                if (gtr.ratings[thought.CurStageIndex] < worstScore) worstThought = thought.CurStage.LabelCap + " (" + gtr.ratings[thought.CurStageIndex] + ")";
            }
        }
        // TODO if a pawn with Gambler could not gamble, leave complaint 
        if (rating > 100)
            Log.Message($"rating exceeded {rating} > 100 for type {pawn.kindDef} - lower base");

        rating = Mathf.Clamp(rating, 0, 100);
        if (save)
        {
            guestRatings.Add(new GuestRating(pawn.LabelShortCap, pawn.kindDef.LabelCap,rating, bestThought, worstThought));
        }
        return rating;
    }

    public void setCampaign(PawnKindDef type)
    {
        //if (campaignChangedToday) return;
        campaignChangedToday = true;
        campaignRunning = type;
    }
}