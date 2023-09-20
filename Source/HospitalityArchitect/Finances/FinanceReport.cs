using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace HospitalityArchitect
{
    public class FinanceReport : IExposable
    {
        private Dictionary<string,float> income = new Dictionary<string,float>();
        private Dictionary<string,float> expenses = new Dictionary<string,float>();
        private int day = GenDate.DaysPassed;

        public Dictionary<string,float> getIncome() => income;
        public Dictionary<string,float> getExpenses() => expenses;
        
        public enum ReportEntryType
        {
            Wages,
            Rimazon,
            Interest,
            Taxes,
            Sales,
            Beds,
            Marketing,
            Land
        }

        public void recordIncome(ReportEntryType type, float value)
        {
            income.SetOrAdd(type.ToString(), income.GetValueOrDefault(type.ToString(), 0) + value);
        }
        
        public void recordExpense(ReportEntryType type, float value)
        {
            expenses.SetOrAdd(type.ToString(), expenses.GetValueOrDefault(type.ToString(), 0) + value);
        }

        public float getNetResult()
        {
            return income.Values.Sum() - expenses.Values.Sum();
        }

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                reset();
            }
            Scribe_Collections.Look(ref this.income, "income", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref this.expenses, "expenses", LookMode.Value, LookMode.Value);
            Scribe_Values.Look(ref day, "day");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
                initNull();
        }

        private void initNull()
        {
            if (income == null)
                income = new Dictionary<string, float>();
            if (expenses == null)
                expenses = new Dictionary<string, float>();
        }

        private void reset()
        {
            expenses.Clear();
            income.Clear();
        }
    }
}