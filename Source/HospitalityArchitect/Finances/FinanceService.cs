using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace RT
{
    /*
     * 
     */
    public class FinanceService : MapComponent
    {
        private float availableSilverCache;

        public int currentDay;
        private Map mapCache;

        public float moneyInBank;

        private List<FinanceReport> _reports = new List<FinanceReport>();
        private List<LoanType> _loanTypes = new List<LoanType>();
        private List<Loan> _loans = new List<Loan>();

        public List<Loan> Loans => _loans;
        
        public List<LoanType> LoanTypes => _loanTypes;

        public FinanceService(Map map) : base(map)
        {
            currentDay = GenDate.DaysPassed;
            initLists();
        }

        public FinanceReport getCurrentReport()
        {
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
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref _reports, "reports", LookMode.Deep);
            Scribe_Collections.Look(ref _loans, "loans", LookMode.Deep);
            Scribe_Values.Look(ref currentDay, "currentDay");
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
            _loans ??= new List<Loan>();
            _loanTypes ??= new List<LoanType>();
            _loanTypes.Add(new LoanType(1000,0.01f,"Aunt Leeman", "loan1"));
            _loanTypes.Add(new LoanType(2500,0.02f,"Wireshark Inc", "loan2"));
            _loanTypes.Add(new LoanType(5000,0.03f,"Caxigo Oplo", "loan3"));
            _loanTypes.Add(new LoanType(10000,0.04f,"Rimbank", "loan4"));
        }



        public void bookExpenses(FinanceReport.ReportEntryType type, float value)
        {
            removeMoney(value);
            //Log.Message($"expenses[{type.ToString()}]: {value} result: {moneyInBank}");
            Messages.Message($"{type.ToString()} -{value.ToStringMoney()}", null, MessageTypeDefOf.NegativeEvent);
            _reports[currentDay].recordExpense(type, value);
        }
        
        public void bookIncome(FinanceReport.ReportEntryType type, float value)
        {
            //Log.Message($"income[{type.ToString()}]: {value} result: {moneyInBank}");
            Messages.Message($"{type.ToString()} +{value.ToStringMoney()}", null, MessageTypeDefOf.PositiveEvent);
            _reports[currentDay].recordIncome(type, value);
        }

        public void Deposit()
        {
            float amount = getAvailableSilver();
            //bookIncome(FinanceReport.ReportEntryType.Income, amount);
            removeMoney(amount, false);
            moneyInBank += amount;
        }
        
        public void TakeLoan(LoanType type)
        {
            moneyInBank += type.Amount;
            _loans.Add(new Loan(type));
        }

        public void Repay(float amount, Loan loan)
        {
            removeMoney(loan.Repay(amount));
            if (loan.Balance == 0f)
            {
                _loans.Remove(loan);
            }
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
                while (value > 0)
                {
                    var silver = silverList.First(t => t.stackCount > 0);
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
            if (GenDate.DaysPassed > currentDay && GenLocalDate.HourInteger(map) == 0) OnNextDay(GenDate.DaysPassed);
        }
        
        private void OnNextDay(int today)
        {
            Log.Message("OnNextDay "+today);
            // calculate building taxes of this map = Daily property taxes per cell build on. 0.1s per 25 tiles of home (or tile of roof??).
            /*if (Math.Floor(map.areaManager.Home.ActiveCells.Count() / 250f) > 0)
                bookExpenses(FinanceReport.ReportEntryType.Taxes, map.areaManager.Home.ActiveCells.Count() / 250f);*/
            
            // pay taxes on the money on the bank, 1s per 1000s on the bank - this is the price player pays for the advantage of having "hidden wealth"/smaller raids 
            if (Math.Floor(moneyInBank / 1000f) > 0)
                bookExpenses(FinanceReport.ReportEntryType.Taxes, (float)Math.Floor(moneyInBank / 1000f));

            foreach (var loan  in _loans)
            {
                // pay interest on the balance of the loan
                if (loan.Balance > 0)
                {
                    bookExpenses(FinanceReport.ReportEntryType.Interest, loan.Balance * loan.Interest);
                }
            }
            Messages.Message("Cashflow today: " + _reports[currentDay].getNetResult().ToStringMoney(), MessageTypeDefOf.NeutralEvent);
            
            currentDay = today;
            _reports.Add(new FinanceReport());
        }


        public float GetCashFlow()
        {
            return _reports[currentDay].getNetResult();
        }
    }
}