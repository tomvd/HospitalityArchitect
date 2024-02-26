using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Hospitality;
using Hospitality.Utilities;
using RimWorld;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;
using Verse.AI.Group;

namespace HospitalityArchitect;

public class Scheduler : MapComponent
{
    private bool dailyDone;
    private MarketingService _marketingService;
    
    public Scheduler(Map map) : base(map)
    {
        this._marketingService = map.GetComponent<MarketingService>();
    }
    
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref dailyDone, "dailyDone");
    }

    public override void MapComponentTick()
    {
        base.MapComponentTick();
        if (GenTicks.TicksGame % 40 == 0) // every ingame minute (+/-)
        {
            DoMinutely();
        }
        if (GenTicks.TicksGame % GenDate.TicksPerHour == 0) // every ingame hour
        {
            DoHourly();
        }             
    }
    
    private void DoMinutely()
    {
        _marketingService.DoMinutely();
        
        // handle leaving guests + recalculate totalSpent
        Hospitality.Utilities.GuestUtility.GetAllGuests(map).ToList().ForEach(
                guest =>
                {
                    GuestTypeDef type = guest.kindDef.GetModExtension<GuestTypeDef>();
                    CompHotelGuest hotelGuest = guest.GetComp<CompHotelGuest>();
                    Thing silver = guest.inventory.innerContainer.FirstOrDefault(i => i.def == ThingDefOf.Silver);
                    if ( silver != null )
                        hotelGuest.totalSpent = hotelGuest.initialMoney - silver.stackCount;
                    else
                    {
                        hotelGuest.totalSpent = hotelGuest.initialMoney;
                    }
                    bool leave = false;
                    if (!hotelGuest.left)
                    {
                        if (!hotelGuest.slept && GenLocalDate.HourOfDay(map) == 0) hotelGuest.slept = true;
                        if (hotelGuest.dayVisit)
                        {
                            if (guest.IsTired() || GenLocalDate.HourOfDay(map) > type.leavesAt.RandomInRange)
                            {
                                leave = true;
                            }
                        }
                        else
                        {
                            if (hotelGuest.slept &&
                                GenLocalDate.HourOfDay(map) > type.leavesAt.RandomInRange)
                            {
                                leave = true;
                            }
                        }

                        if (leave)
                        {
                            GuestUtility.HotelGuestLeaves(guest);
                        }
                    }
                }
                );
    }

    private void DoHourly()
    {
        if (!dailyDone && GenLocalDate.HourOfDay(map) == 1) DoDaily(GenDate.DaysPassed);
        if (GenLocalDate.HourOfDay(map) == 2) dailyDone = false;
    }

    private void DoDaily(int today)
    {
        _marketingService.DoDaily(today);
        dailyDone = true;
    }
}