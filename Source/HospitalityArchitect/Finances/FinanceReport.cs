using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace HospitalityArchitect
{
    public class FinanceReport : IExposable
    {
        private Dictionary<string,float> booking = new Dictionary<string,float>();
        private int day = GenDate.DaysPassed;

        public Dictionary<string,float> getBooking() => booking;
        
        public enum ReportEntryType
        {
            Wages,
            Rimazon,
            Interest,
            Taxes,
            Sales,
            Beds,
            Marketing,
            GroundRent
        }

        public void recordBooking(ReportEntryType type, float value)
        {
            booking.SetOrAdd(type.ToString(), booking.GetValueOrDefault(type.ToString(), 0) + value);
        }
        
        public float getNetResult()
        {
            return booking.Values.Sum();
        }
        
        public float getIncome()
        {
            return booking.Values.Where(v => v > 0).Sum();
        }
        
        public float getExpenses()
        {
            return booking.Values.Where(v => v < 0).Sum();
        }        
        

        public void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                reset();
            }
            Scribe_Collections.Look(ref this.booking, "booking", LookMode.Value, LookMode.Value);
            Scribe_Values.Look(ref day, "day");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
                initNull();
        }

        private void initNull()
        {
            if (booking == null)
                booking = new Dictionary<string, float>();
        }

        private void reset()
        {
            booking.Clear();
        }
    }
}