using RimWorld;
using Verse;

namespace HospitalityArchitect
{
    public class HiringContract : IExposable
    {
        public int daysHired;
        public int lastBilledTicks;
        public Map map;
        public Pawn pawn;
        public int startTicks;

        public HiringContract()
        {
        }

        public HiringContract(int startTicks, Pawn pawn, Map map)
        {
            this.startTicks = startTicks;
            lastBilledTicks = startTicks;
            this.pawn = pawn;
            this.map = map;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref startTicks, "startTicks");
            Scribe_Values.Look(ref daysHired, "daysHired");
            Scribe_Values.Look(ref lastBilledTicks, "lastBilledTicks");
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_References.Look(ref map, "map");
        }
    }
}