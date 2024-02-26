using System.Linq;
using Hospitality;
using HospitalityArchitect.Facilities;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace HospitalityArchitect
{
    public class Dialog_Facilities : MainTabWindow
    {
        private readonly Map map;
        private readonly FacilitiesService _facilitiesService;

        public Dialog_Facilities()
        {
            map = Find.CurrentMap;
            _facilitiesService = map.GetComponent<FacilitiesService>();
            closeOnCancel = true;
            forcePause = false;
            closeOnAccept = true;
        }

        public override Vector2 InitialSize => new Vector2(600f, 300f);
        public override float Margin => 15f;

        public override void DoWindowContents(Rect inRect)
        {
            var anchor = Text.Anchor;
            var font = Text.Font;
            var marketingData = _facilitiesService.facilitiesData;

            var rect = inRect.TopPartPixels(Mathf.Max(20f + marketingData.Count * 30f, 120f));
            inRect.yMin += rect.height;
            var titleRect = rect.TakeTopPart(20f);
            //Widgets.Label(titleRect, "Employer reputation: " + _hiringContractService.Reputation.ToStringDecimalIfSmall());
            //titleRect.y += 20f;
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Tiny;
            var nameRect = new Rect(titleRect);
            Widgets.Label(nameRect, "Facility");

            titleRect.x += 100f;
            titleRect.width = 100f;
            var IPRect = new Rect(titleRect);
            Widgets.Label(IPRect, "Open?");
            
            titleRect.x += 100f;
            titleRect.width = 100f;
            var valueRect = new Rect(titleRect);
            Widgets.Label(valueRect, "Reserved/Capacity");
            
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            var highlight = true;
            foreach (var data in marketingData)
            {
                nameRect.y += 20f;
                valueRect.y += 20f;
                IPRect.y += 20f;
                var fullRect = new Rect(nameRect.x - 4f, nameRect.y, nameRect.width + valueRect.width + IPRect.width,
                    20f);
                if (highlight) Widgets.DrawHighlight(fullRect);
                highlight = !highlight;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(nameRect,data.Key);
                Widgets.Label(IPRect,data.Value.isOpen?"Open":"Closed");
                Widgets.Label(valueRect,data.Value.reservedCapacity.ToString() + "/" + data.Value.totalCapacity.ToString());
            }
            Text.Anchor = anchor;
            Text.Font = font;
        }
        
        /*
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
        */
    }
}