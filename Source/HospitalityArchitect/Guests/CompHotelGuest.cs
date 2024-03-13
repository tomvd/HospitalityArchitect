using System.Collections.Generic;
using System.Linq;
using Hospitality.Utilities;
using JetBrains.Annotations;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace HospitalityArchitect
{
    public class CompHotelGuest : ThingComp
    {
        public float totalSpent;
        public float initialMoney;
        public bool dayVisit;
        public bool arrived;
        public int lastHourSeen;
        public int hoursSpent;
        public bool left;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref totalSpent, "totalSpent");
            Scribe_Values.Look(ref initialMoney, "initialMoney");
            Scribe_Values.Look(ref dayVisit, "dayVisit");
            Scribe_Values.Look(ref arrived, "arrived");
            Scribe_Values.Look(ref lastHourSeen, "lastHourSeen");
            Scribe_Values.Look(ref hoursSpent, "hoursSpent");
            Scribe_Values.Look(ref left, "left");
        }

        public override string CompInspectStringExtra()
        {
            if (!dayVisit) return "";
            return "Daytime visitor";
        }
    }
    
    
}