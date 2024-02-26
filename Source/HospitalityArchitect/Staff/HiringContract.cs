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
        public int type; // 0 -day (7h-16h) 1 -late (16h-1h)
        public int arrivesAt = 7;
        public int leavesAt = 16;
        public bool atWork = false;

        public HiringContract()
        {
        }

        public HiringContract(int startTicks, Pawn pawn, Map map, int type)
        {
            this.startTicks = startTicks;
            lastBilledTicks = startTicks;
            this.pawn = pawn;
            this.map = map;
            this.type = type;
            ChangeType(type);
        }

        public void ChangeType(int i)
        {
            this.type = i;
            switch (type)
            {
                case 0:
                    arrivesAt = 7;
                    leavesAt = 16;
                    break;
                case 1:
                    arrivesAt = 16;
                    leavesAt = 1;
                    break;
            }
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref startTicks, "startTicks");
            Scribe_Values.Look(ref daysHired, "daysHired");
            Scribe_Values.Look(ref lastBilledTicks, "lastBilledTicks");
            Scribe_Values.Look(ref type, "type");
            Scribe_Values.Look(ref arrivesAt, "arrivesAt");
            Scribe_Values.Look(ref leavesAt, "leavesAt");
            Scribe_Values.Look(ref atWork, "atWork");
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_References.Look(ref map, "map");
        }
    }
}