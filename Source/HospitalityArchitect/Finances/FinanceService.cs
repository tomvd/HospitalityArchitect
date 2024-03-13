using System;
using System.Collections.Generic;
using System.Linq;
using Gastronomy.Restaurant;
using RimWorld;
using RimWorld.Planet;
using Storefront.Store;
using UnityEngine;
using Verse;

namespace HospitalityArchitect
{
    /*
     * 
     */
    public class FinanceService : MapComponent
    {
        private float availableSilverCache;

        public int currentDay; // reports index
        private Map mapCache;

        public float moneyInBank;
        public float moneyInLoan;

        private List<FinanceReport> _reports = new List<FinanceReport>();

        public FinanceService(Map map) : base(map)
        {
            currentDay = GenDate.DaysPassed;
            _reports ??= new List<FinanceReport>();
            if (_reports.Count == 0)
            {
                _reports.Add(new FinanceReport());
                moneyInBank = 2000;
            }
        }

        public FinanceReport getReport(int currentSubTab)
        {
            switch (currentSubTab)
            {
                case 0: // today
                    return _reports[currentDay];
                case 1: // yesterday
                    return _reports.Count > 1? _reports[currentDay-1] : _reports[currentDay];
                case 2: // alltime
                    return FinanceUtil.Sum(_reports);
            }
            return _reports[currentDay];
        }

        public float getAvailableSilver()
        {
            if (map != mapCache)
            {
                availableSilverCache = map.listerThings.ThingsOfDef(ThingDefOf.Silver)
                    .Where(x => !x.Position.Fogged(x.Map) && (map.areaManager.Home[x.Position] || x.IsInAnyStorage()))
                    .Sum(t => t.stackCount);
                mapCache = map;
            }

            return availableSilverCache;
        }

        public bool canAfford(float amount)
        {
            return amount <= availableSilverCache + moneyInBank;
        }

        public float getMoneyInBank()
        {
            return moneyInBank;
        }
        
        public float getFunds()
        {
            return getMoneyInBank() + getAvailableSilver();
        }
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref _reports, "reports", LookMode.Deep);
            Scribe_Values.Look(ref currentDay, "currentDay");
            Scribe_Values.Look(ref moneyInLoan, "moneyInLoan"); // game starts with 2000 in bank
            Scribe_Values.Look(ref moneyInBank, "moneyInBank", 2000); // game starts with 2000 in bank
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                initLists();
            }
        }

        private void initLists()
        {
            _reports ??= new List<FinanceReport>();
            if (_reports.Count == 0)
            {
                _reports.Add(new FinanceReport());
                currentDay = GenDate.DaysPassed;
                moneyInBank = 2000;
            }
        }



        public bool doAndBookExpenses(FinanceReport.ReportEntryType type, float value)
        {
            if (!canAfford(value))
            {
                Messages.Message("Not enough money!", null, MessageTypeDefOf.NegativeEvent);
                return false;
            }
            removeMoney(value);
            //Log.Message($"expenses[{type.ToString()}]: {value} result: {moneyInBank}");
            Messages.Message($"{type.ToString()} -{value.ToStringMoney()}", null, MessageTypeDefOf.NegativeEvent);
            _reports[currentDay].recordBooking(type, -value);
            return true;
        }
        
        public void doAndBookIncome(FinanceReport.ReportEntryType type, float value)
        {
            moneyInBank += value;
            bookIncome(type, value);
        }
        
        public void bookIncome(FinanceReport.ReportEntryType type, float value)
        {
            //Log.Message($"income[{type.ToString()}]: {value} result: {moneyInBank}");
            Messages.Message($"{type.ToString()} +{value.ToStringMoney()}", null, MessageTypeDefOf.PositiveEvent);
            _reports[currentDay].recordBooking(type, value);
        }

        public void Deposit()
        {
            float amount = getAvailableSilver();
            //bookIncome(FinanceReport.ReportEntryType.Income, amount);
            removeMoney(amount, false);
            moneyInBank += amount;
        }
        
        public void TakeLoan(float amount)
        {
            moneyInBank += amount;
            moneyInLoan += amount;
        }

        public void Repay(float amount)
        {
            removeMoney(amount);
            moneyInLoan -= amount;
        }

        private void removeMoney(float value, bool firstFromBank = true)
        {
            if (firstFromBank)
            {
                var payedFromBank = Math.Min(moneyInBank, value);
                moneyInBank -= payedFromBank;
                value -= payedFromBank;
            }

            if (value > 0.9f)
            {
                var silverList = map.listerThings.ThingsOfDef(ThingDefOf.Silver)
                    .Where(x => !x.Position.Fogged(x.Map) && (map.areaManager.Home[x.Position] || x.IsInAnyStorage()))
                    .ToList();
                if (silverList.Count == 0)
                {
                    // we are out of money
                    Messages.Message("You are out of silver! Take a loan or sell stuff or bad things will happen!", MessageTypeDefOf.NegativeEvent);
                } else
                    while (value > 0)
                    {
                        var silver = silverList.FirstOrDefault(t => t.stackCount > 0);
                        if (silver == null)
                        {
                            // we are out of money
                            Messages.Message("You are out of silver! Take a loan or sell stuff or bad things will happen!", MessageTypeDefOf.NegativeEvent);
                            break;
                        } 
                        var num = Mathf.Min(value, silver.stackCount);
                        silver.SplitOff((int)num).Destroy();
                        value -= num;
                    }

                //invalidates cache
                mapCache = null;
            }
        }
        
        public override void MapComponentTick()
        {
            base.MapComponentTick();
            // Don't tick everything at once
            if (GenTicks.TicksGame % 500 == 300) RareTick();
        }

        private void RareTick()
        {
            mapCache = null;
            if (GenDate.DaysPassed > currentDay && GenLocalDate.HourInteger(map) == 0) OnNextDay(GenDate.DaysPassed);
        }
        
        private void OnNextDay(int today)
        {
            Log.Message("OnNextDay "+today);
            // Add store and restaurant incomes of yesterday
            float income = 0f;
            RestaurantsManager restaurants = Find.CurrentMap.GetComponent<RestaurantsManager>();
            foreach (var restaurantController in restaurants.restaurants)
            {
                income += restaurantController.Debts.incomeYesterday;
            }

            bookIncome(FinanceReport.ReportEntryType.Restaurant, income);
            income = 0;
        
            StoresManager stores = Find.CurrentMap.GetComponent<StoresManager>();
            foreach (var store in stores.Stores)
            {
                income += store.incomeYesterday;
            }
            bookIncome(FinanceReport.ReportEntryType.Store, income);
            
            Messages.Message("Cashflow today: " + _reports[currentDay].getNetResult().ToStringMoney(), MessageTypeDefOf.NeutralEvent);
            var taxes = _reports[currentDay].getNetResult() * (HADefOf.HA_TaxReduction.IsFinished? 0.02f : 0.1f);
            currentDay = today; // avoids being stuck when this for some reason became out of sync
            while (_reports.Count < currentDay+1)
                _reports.Add(new FinanceReport());                

            // start a new report
            
            // calculate building taxes of this map = Daily property taxes per cell build on. 0.1s per 25 tiles of home (or tile of roof??).
            Area homeArea = map.areaManager.Home;
            float groundrent = 0f;
            foreach (var cell in homeArea.ActiveCells)
            {
                groundrent += Utils.GetLandValue(map, cell) / 250;
            }
            groundrent = Mathf.Round(groundrent);
                
            if (groundrent > 0)
                doAndBookExpenses(FinanceReport.ReportEntryType.GroundRent, groundrent);
            if (taxes > 0)
                doAndBookExpenses(FinanceReport.ReportEntryType.Taxes, taxes);
            
            // pay taxes on the money on the bank, 1s per 1000s on the bank - this is the price player pays for the advantage of having "hidden wealth"/smaller raids 
            //if (Math.Floor(moneyInBank / 1000f) > 0)
//                    doAndBookExpenses(FinanceReport.ReportEntryType.Taxes, (float)Math.Floor(moneyInBank / 1000f));

            // pay interest on the balance of the loan
            if (moneyInLoan > 0)
            {
                doAndBookExpenses(FinanceReport.ReportEntryType.Interest, moneyInLoan * getLoanInterest());
            }
        }


        public float GetCashFlow()
        {
            return _reports[currentDay].getNetResult();
        }

        public float getLoanInterest()
        {
            return 0.05f;
        }
    }
}