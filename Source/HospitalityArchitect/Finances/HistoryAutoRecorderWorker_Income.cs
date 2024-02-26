using System.Collections.Generic;
using RimWorld;
using Verse;

namespace HospitalityArchitect;

public class HistoryAutoRecorderWorker_Income: HistoryAutoRecorderWorker
{
    public override float PullRecord()
    {
        float num = 0f;
        List<Map> maps = Find.Maps;
        for (int i = 0; i < maps.Count; i++)
        {
            if (maps[i].IsPlayerHome)
            {
                num += maps[i].GetComponent<FinanceService>().getReport(0).getIncome();
            }
        }
        return num;
    }
}