using System.Collections.Generic;
using System.Linq;
using Hospitality;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace HospitalityArchitect
{
    public class Dialog_Guests : MainTabWindow
    {
        private readonly Map map;
        private Vector2 pawnsScrollPos = new Vector2(0, 0);
        private HiringContractService _hiringContractService;
        private FinanceService _financeService;
        private MarketingService _marketingService;
        private int currentTab = 0;
        private float lastTimeCached;

        public Dialog_Guests()
        {
            map = Find.CurrentMap;
            _hiringContractService = map.GetComponent<HiringContractService>();
            _financeService = map.GetComponent<FinanceService>();
            _marketingService = map.GetComponent<MarketingService>();
        }

        public override Vector2 InitialSize => new Vector2(1000f, 600f);
        public override float Margin => 15f;

        public override void PostOpen()
        {
            base.PostOpen();
            Find.World.renderer.wantedMode = WorldRenderMode.None;
        }

        public override void DoWindowContents(Rect inRect)
        {
            var rect = new Rect(inRect);
            var anchor = Text.Anchor;
            var font = Text.Font;
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Small;

            switch (currentTab)
            {
                case 0:
                    DoGuestList(ref rect);
                    break;
                case 1:
                    DoOverview(ref rect);
                    break;
                case 2:
                    DoToughts(ref rect);
                    break;                     
                case 3:
                    DoReviews(ref rect);
                    break;                
            }
            Text.Anchor = anchor;
            Text.Font = font;

            var buttonBarRect = rect.TakeBottomPart(40f);
            buttonBarRect.x = 150;
            buttonBarRect.width = 100;
            if (Widgets.ButtonText(buttonBarRect, "Guests")) currentTab = 0;
            buttonBarRect.x += 100;
            if (Widgets.ButtonText(buttonBarRect, "Guest types")) currentTab = 1;
            buttonBarRect.x += 100;
            if (Widgets.ButtonText(buttonBarRect, "Guest thoughts")) currentTab = 2;
            buttonBarRect.x += 100;            
            if (Widgets.ButtonText(buttonBarRect, "Reviews")) currentTab = 3;

        }
        
        private void DoOverview(ref Rect inRect)
        {
            var marketingData = _marketingService.MarketingData.OrderBy(data => data.Key.GetModExtension<GuestTypeDef>().bedBudget).ToList();

            var rect = inRect.TopPartPixels(Mathf.Max(20f + marketingData.Count * 30f, 120f));
            inRect.yMin += rect.height;
            var titleRect = rect.TakeTopPart(20f);
            //Widgets.Label(titleRect, "Employer reputation: " + _hiringContractService.Reputation.ToStringDecimalIfSmall());
            //titleRect.y += 20f;
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Tiny;
            var nameRect = new Rect(titleRect);
            Widgets.Label(nameRect, "Guest type");

            titleRect.x += 100f;
            titleRect.width = 500f;
            var RRect = new Rect(titleRect);
            Widgets.Label(RRect, "Requirements");
            
            titleRect.x += 500f;
            titleRect.width = 100f;
            var IPRect = new Rect(titleRect);
            Widgets.Label(IPRect, "Influence Points");
            
            titleRect.x += 100f;
            titleRect.width = 100f;
            var valueRect = new Rect(titleRect);
            Widgets.Label(valueRect, "Bookings/Capacity");

            titleRect.x += 100f;
            titleRect.width = 100f;
            var numRect = new Rect(titleRect);
            Widgets.Label(numRect, "Advertisement".Translate().Colorize(ColoredText.SubtleGrayColor));
            
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            var highlight = true;
            foreach (var data in marketingData)
            {
                GuestTypeDef type = data.Key.GetModExtension<GuestTypeDef>();
                nameRect.y += 20f;
                valueRect.y += 20f;
                numRect.y += 20f;
                IPRect.y += 20f;
                RRect.y += 20f;
                var fullRect = new Rect(nameRect.x - 4f, nameRect.y, nameRect.width + valueRect.width + numRect.width + IPRect.width + RRect.width,
                    20f);
                if (highlight) Widgets.DrawHighlight(fullRect);
                highlight = !highlight;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(nameRect,data.Key.label);
                if (type.bedroomRequirements != null)
                    Widgets.Label(RRect,string.Join(",",type.bedroomRequirements.Select(br => br.Label() ?? "")));
                Widgets.Label(IPRect,data.Value.influencePoints.ToString());
                if (Time.unscaledTime > lastTimeCached + 2)
                {
                    data.Value.QualifiedBedsCached = GuestUtility.QualifiedBedsCount(map, type, data.Key);
                    lastTimeCached = Time.unscaledTime;
                }                
                Widgets.Label(valueRect,data.Value.bookings.ToString() + "/" + data.Value.QualifiedBedsCached);
                if (data.Key == _marketingService.campaignRunning)
                {
                    if (Widgets.ButtonText(numRect, "Cancel ad"))
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("Are you sure?".Translate(),
                            () => { _marketingService.setCampaign(null); }, true, "Cancel ad"));                    
                }
                else
                {
                    if (Widgets.ButtonText(numRect, "Run ad (" + (type.budget.Average * 2) + "/day)"))
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("Are you sure?".Translate(),
                            () => { _marketingService.setCampaign(data.Key); }, true, "Run ad"));
                }
            }
        }

        private void DoGuestList(ref Rect inRect)
        {
            List<Lord> visitorGroups = map.GetComponent<Hospitality_MapComponent>().PresentLords.ToList();
            var rect = inRect.TopPartPixels(Mathf.Max(20f + visitorGroups.Count * 30f, 120f));
            inRect.yMin += rect.height;
            var titleRect = rect.TakeTopPart(20f);
            var iconRect = rect.LeftPartPixels(105f).ContractedBy(5f);
            titleRect.x += 15f;
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Tiny;
            var nameRect = new Rect(titleRect);
            Widgets.Label(titleRect, "name");
            titleRect.x += 550f;
            titleRect.width = 120f;
            Text.Anchor = TextAnchor.MiddleCenter;
            var valueRect = new Rect(titleRect);
            Widgets.Label(titleRect, "type");
            titleRect.x += 100f;
            titleRect.width = 100f;
            var numRect = new Rect(titleRect);
            Widgets.Label(titleRect, "refund");
            GUI.color = Color.white;
            var highlight = true;
            foreach (var lord in visitorGroups.ToList())
            {
                nameRect.y += 20f;
                valueRect.y += 20f;
                numRect.y += 20f;
                var fullRect = new Rect(nameRect.x - 4f, nameRect.y, nameRect.width + valueRect.width + numRect.width,
                    20f);
                if (highlight) Widgets.DrawHighlight(fullRect);
                highlight = !highlight;
                Text.Anchor = TextAnchor.MiddleLeft;
                var names = string.Join(",",lord.ownedPawns.Select(pawn => pawn.NameShortColored).ToList());
                Widgets.Label(nameRect, names);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(valueRect, lord.ownedPawns.RandomElement().KindLabel);

                if (Widgets.ButtonText(numRect.RightHalf(), "Refund"))
                {
                    Log.Message("not implemented yet");
                }
            }
        }
        
        private void DoReviews(ref Rect inRect)
        {
            var col = UIUtility.CreateColumns(inRect, 4);
            Widgets.Label(col[0], "Guest");
            Widgets.Label(col[1], "Positives");
            Widgets.Label(col[2], "Negatives");
            Widgets.Label(col[3], "Rating");
        }        
        
        private void DoToughts(ref Rect inRect)
        {
            // TODO group them
            var col = UIUtility.CreateColumns(inRect, 1);
            Widgets.Label(col[0], "Active guest thoughts");
            List<Lord> visitorGroups = map.GetComponent<Hospitality_MapComponent>().PresentLords.ToList();
            foreach (var lord in visitorGroups.ToList())
            {
                List<Thought> thoughts = new List<Thought>();
                lord.ownedPawns.RandomElement().needs.mood.thoughts.GetAllMoodThoughts(thoughts);
                foreach (var thought in thoughts)
                {
                    GuestThoughtRating gtr = thought.def.GetModExtension<GuestThoughtRating>();
                    if (gtr != null)
                    {
                        UIUtility.NextRow(col);
                        Widgets.Label(col[0], thought.ToString());
                    }
                }
            }            
        }        
    }
}