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
        public bool dayVisit;
        public bool arrived;
        public bool slept;
        public bool left;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref totalSpent, "totalSpent");
            Scribe_Values.Look(ref dayVisit, "dayVisit");
            Scribe_Values.Look(ref arrived, "arrived");
            Scribe_Values.Look(ref slept, "slept");
            Scribe_Values.Look(ref left, "left");
        }

        public override string CompInspectStringExtra()
        {
            if (!dayVisit) return "";
            return "Daytime visitor";
        }
    }
    
    
}