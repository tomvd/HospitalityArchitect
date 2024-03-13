using Verse;

namespace HospitalityArchitect;

public static class QuickSellUtil
{
    public static float QSPrice(Thing t)
    {
        if (t.MarketValue == 0) // something fishy you are selling - probably rocks or crap
        {
            return -1;
        }
        return (t.MarketValue / 2f) * t.stackCount;
    }
}