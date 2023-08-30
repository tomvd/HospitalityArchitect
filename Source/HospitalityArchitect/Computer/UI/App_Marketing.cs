using System.Collections.Generic;
using System.Linq;
using Hospitality;
using Hospitality.Utilities;
using RimWorld;
using RT.DeliverySystem;
using RT.UItils;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RT
{
    /*
     * Marketing: "buys" goodwill, similar to what you can do through comms 
     */
    public class App_Marketing : ComputerApp
    {
        private readonly Map map;
        private readonly Window_Computer host;
        private readonly FinanceService _financeService;

        public App_Marketing(Thing user, Window_Computer host)
        {
            map = user.Map;
            this.host = host;
            _financeService = map.GetComponent<FinanceService>();
        }

        public override string getLabel()
        {
            return "Marketing";
        }


        public override void DoWindowContents(Rect inRect)
        {
            var anchor = Text.Anchor;
            var font = Text.Font;
            if (Widgets.ButtonText(inRect, "Launch promotion (100$)"))
            {
                _financeService.bookExpenses(FinanceReport.ReportEntryType.Marketing, 100.0f);
                PlanNewVisit(map, 1,null);
            }

            Text.Anchor = anchor;
            Text.Font = font;
        }
        
        public static void PlanNewVisit(IIncidentTarget theMap, float afterDays, Faction faction = null)
        {
            if (!(theMap is Map realMap)) return;

            var incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.FactionArrival, realMap);

            if (faction == null)
            {
                faction = Find.FactionManager.AllFactions.Where(f => !f.IsPlayer && !f.defeated && !f.def.hidden && !f.HostileTo(Faction.OfPlayer) && f.def.humanlikeFaction).RandomElement();
            }
            incidentParms.faction = faction;
            var incident = new FiringIncident(IncidentDefOf.VisitorGroup, null, incidentParms);
            realMap.GetMapComponent()?.QueueIncident(incident, afterDays);
        }
    }
}