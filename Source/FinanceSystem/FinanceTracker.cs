using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using UnityEngine;

namespace RT
{    
    
    public class FinanceTracker : WorldComponent
    {
        public class QuadrumReport : IExposable 
        {
            public QuadrumReport() {}
            public int hiringExpenses = 0;

            public void ExposeData()
            {
                Scribe_Values.Look(ref hiringExpenses, "hiringExpenses");
            }
        }

        public int currentQuadrum;

        List<QuadrumReport> reports;

        public QuadrumReport getCurrentQuadrumReport() => reports[currentQuadrum];

        public FinanceTracker(World world) : base(world)
        {
            
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref reports, "reports", LookMode.Deep);
            Scribe_Values.Look(ref currentQuadrum, "currentQuadrum");
            if (Scribe.mode == LoadSaveMode.PostLoadInit) {
                if (reports is null){
                    reports = new List<QuadrumReport>();
                    reports.Add(new QuadrumReport());
                    currentQuadrum = 0;
                }
            }
        }

        public void bookHiringExpenses(Map map, int value) {
            removeSilver(map, value);
            reports[0].hiringExpenses += value;
        }

        private void removeSilver(Map map, int value) {
            var silverList = map.listerThings.ThingsOfDef(ThingDefOf.Silver)
                                    .Where(x => !x.Position.Fogged(x.Map) && (map.areaManager.Home[x.Position] || x.IsInAnyStorage())).ToList();
            while (value > 0)
            {
                var silver = silverList.First(t => t.stackCount > 0);
                var num    = Mathf.Min(value, silver.stackCount);
                silver.SplitOff(num).Destroy();
                value -= num;
            }            
        }
    }
}