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
            var cost = type.budget.Average * 2;
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
            float marketing = campaignRunning == guestTypeData.Key ? 1 : 0;
            float bookings = Math.Min(guestTypeData.Value.influencePoints + marketing, GuestUtility.QualifiedBedsCount(map, type, guestTypeData.Key));
            // TODO turned off for now
           /* if (type.seasonalVariance > 0)
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
            */

            guestTypeData.Value.bookings = (int)Math.Round(bookings);
            totalBookings += guestTypeData.Value.bookings;
        }
        Messages.Message("New bookings: " + totalBookings, MessageTypeDefOf.NeutralEvent);
        

    }

    public void DoMinutely()
    {
        // handle guest arrivals
        PawnKindDef pawnKind = DefDatabase<PawnKindDef>.AllDefsListForReading.Where(hasAnyBooking).RandomElement();
        if (pawnKind == null) return; // no more bookings
        GuestTypeDef type = pawnKind.GetModExtension<GuestTypeDef>();
        GuestTypeData data;
        MarketingData.TryGetValue(pawnKind, out data);
        
        //float baseChance = 0.1f * data.bookings;  
        if (GuestUtility.EmptyQualifiedBedsAvailable(map, type, pawnKind) > 0
            && (GenLocalDate.HourOfDay(map) >= type.arrivesAt.min &&
                                  GenLocalDate.HourOfDay(map) <= type.arrivesAt.max))
        {
            data.bookings--;
            IncidentParms parms = new IncidentParms();
            parms.target = map;
            // TODO depending on awareness and reputation more of some type will occur
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
            DefDatabase<IncidentDef>.GetNamed("VisitorGroup").Worker.TryExecuteWorker(parms);
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
        GuestTypeDef hotelGuest = pawn.kindDef.GetModExtension<GuestTypeDef>();
        int rating = hotelGuest.baseRating;
        List<Thought> thoughts = new List<Thought>();
        pawn.needs.mood.thoughts.GetAllMoodThoughts(thoughts);
        foreach (var thought in thoughts)
        {
            GuestThoughtRating gtr = thought.def.GetModExtension<GuestThoughtRating>();
            if (gtr != null)
            {
                rating += gtr.ratings[thought.CurStageIndex];
                Log.Message("added thought to rating: " + thought);
            }
        }
        // TODO if a pawn with Gambler could not gamble, leave complaint 
        // TODO cap rating to 100
        // TODO disable random visits from hospitality
        Messages.Message("Guest left a rating: " + (rating/100.0f).ToStringPercent(), MessageTypeDefOf.NeutralEvent);
        GuestTypeData data;
        MarketingData.TryGetValue(pawn.kindDef, out data);
        data.influencePoints += (rating / 100.0f)-0.5f; // influence goes up with a >50% rating and goes down with a <50% rating
    }

    public void setCampaign(PawnKindDef type)
    {
        //if (campaignChangedToday) return;
        campaignChangedToday = true;
        campaignRunning = type;
    }
}