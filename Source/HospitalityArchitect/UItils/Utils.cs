using RimWorld;
using Verse;

namespace RT
{
    public static class Utils
    {
        public static float Wage(this Pawn pawn)
        {
            return pawn.MarketValue / 100;
        }
    }
}