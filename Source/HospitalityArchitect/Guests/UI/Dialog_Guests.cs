using System;
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
            if (Widgets.ButtonText(buttonBarRect, "Guests"))
            {
                currentTab = 0;
                SoundDefOf.Click.PlayOneShotOnCamera();
            }
            buttonBarRect.x += 100;
            if (Widgets.ButtonText(buttonBarRect, "Guest types"))
            {
                currentTab = 1;
                SoundDefOf.Click.PlayOneShotOnCamera();
            }
            buttonBarRect.x += 100;
            if (Widgets.ButtonText(buttonBarRect, "Guest thoughts"))
            {
                currentTab = 2;
                SoundDefOf.Click.PlayOneShotOnCamera();
            }
            buttonBarRect.x += 100;
            if (Widgets.ButtonText(buttonBarRect, "Reviews"))
            {
                currentTab = 3;
                SoundDefOf.Click.PlayOneShotOnCamera();
            }

        }
        
        private void DoOverview(ref Rect inRect)
        {
            var marketingData = _marketingService.MarketingData.OrderBy(data => data.Key.GetModExtension<GuestTypeDef>().bedBudget).ToList();
            var col = UIUtility.CreateColumns(inRect, 6);
            Widgets.Label(col[0], "Guest type");
            Widgets.Label(col[1], "Arrives at");
            Widgets.Label(col[2], "Influence Points");
            Widgets.Label(col[3], "Bookings/Capacity");
            Widgets.Label(col[4], "Requirements");
            Widgets.Label(col[5], "Advertisement");
            var highlight = true;
            foreach (var data in marketingData)
            {
                UIUtility.NextRow(col);                
                GuestTypeDef type = data.Key.GetModExtension<GuestTypeDef>();
                var fullRect = new Rect(col[0].x - 4f, col[0].y, inRect.width, 20f);                
                if (highlight) Widgets.DrawHighlight(fullRect);
                highlight = !highlight;
                Text.Anchor = TextAnchor.MiddleLeft;
                // Guest type
                Widgets.Label(col[0],data.Key.label);
                
                // Arrives at
                Widgets.Label(col[1],type.arrivesAt.ToString());
                
                //Influence Points
                Widgets.Label(col[2],data.Value.influencePoints.ToStringDecimalIfSmall());
                
                // Bookings/Capacity
                if (Time.unscaledTime > lastTimeCached + 2)
                {
                    data.Value.QualifiedBedsCached = GuestUtility.QualifiedBedsCount(map, type, data.Key);
                }                
                Widgets.Label(col[3],data.Value.bookings.ToString() + "/" + data.Value.QualifiedBedsCached);

                // Requirements
                if (Widgets.ButtonText(col[4], "Show Info"))
                    Find.WindowStack.Add(new Window_GuestRequirements(data.Key));

                // Advertisement
                if (data.Key == _marketingService.campaignRunning)
                {
                    if (Widgets.ButtonText(col[5], "Cancel ad"))
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("Are you sure?".Translate(),
                            () => { _marketingService.setCampaign(null); }, true, "Cancel ad"));                    
                }
                else
                {
                    if (Widgets.ButtonText(col[5], "Run ad (" + MarketingUtility.GetMarketingCost(type) + "/day)"))
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("Are you sure?".Translate(),
                            () => { _marketingService.setCampaign(data.Key); }, true, "Run ad"));
                }
            }
            if (Time.unscaledTime > lastTimeCached + 2)
                lastTimeCached = Time.unscaledTime;
        }

        private void DoGuestList(ref Rect inRect)
        {
            List<Lord> visitorGroups = map.GetComponent<Hospitality_MapComponent>().PresentLords.ToList();
            var col = UIUtility.CreateColumns(inRect, 7);
            Widgets.Label(col[0], "Name");
            Widgets.Label(col[1], "Type");
            Widgets.Label(col[2], "Rating");
            Widgets.Label(col[3], "Money Spent");
            Widgets.Label(col[4], "Current activity");
            Widgets.Label(col[5], "Gift");
            Widgets.Label(col[6], "Refund");
            var highlight = true;
            foreach (var lord in visitorGroups.ToList())
            {
                UIUtility.NextRow(col);
                var fullRect = new Rect(col[0].x - 4f, col[0].y, inRect.width, 20f);
                if (highlight) Widgets.DrawHighlight(fullRect);
                highlight = !highlight;
                Text.Anchor = TextAnchor.MiddleLeft;
                var names = string.Join(",",lord.ownedPawns.Select(pawn => pawn.NameShortColored).ToList());
                Widgets.Label(col[0], names);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(col[1], lord.ownedPawns[0].KindLabel);
                Widgets.Label(col[2], (_marketingService.GetRating(lord.ownedPawns[0], false)/100f).ToStringPercent());
                Widgets.Label(col[3], lord.ownedPawns.Sum(pawn => pawn.GetComp<CompHotelGuest>().totalSpent).ToStringMoney());
                if (!lord.ownedPawns[0].GetComp<CompHotelGuest>().left)
                {
                    Widgets.Label(col[4],
                        string.Join(",", lord.ownedPawns.Select(pawn => pawn.jobs.curDriver?.GetReport()).ToList()));
                    if (Widgets.ButtonText(col[6], "Refund"))
                    {
                        foreach (var pawn in lord.ownedPawns)
                        {
                            _financeService.doAndBookExpenses(FinanceReport.ReportEntryType.Beds,
                                pawn.GetComp<CompHotelGuest>().totalSpent);
                        }

                        GuestUtility.HotelGuestLeaves(lord.ownedPawns[0]);
                    }

                    if (Widgets.ButtonText(col[5], "Gift"))
                    {
                        Log.Message("not implemented yet");
                    }
                }
                else
                {
                    Widgets.Label(col[4],"Leaving...");                    
                }
            }
        }
        
        private void DoReviews(ref Rect inRect)
        {
            var col = UIUtility.CreateColumns(inRect, 4);
            Widgets.Label(col[0], "Guest");
            Widgets.Label(col[1], "Good");
            Widgets.Label(col[2], "Bad");
            Widgets.Label(col[3], "Rating");
            if (_marketingService.guestRatings.Count == 0) return;
            for (int i =_marketingService.guestRatings.Count-1; i >= 0; i--) // in reverse so we see newest first
            {
                GuestRating rating = _marketingService.guestRatings[i];
                UIUtility.NextRow(col);
                if (col[0].y > inRect.height) break;
                Widgets.Label(col[0], rating.name + ", " + rating.kind);
                Widgets.Label(col[1], rating.good);
                Widgets.Label(col[2], rating.bad);
                Widgets.Label(col[3], (rating.rating/100f).ToStringPercent());
            }
        }        
        
        private void DoToughts(ref Rect inRect)
        {
            // TODO group them
            var col = UIUtility.CreateColumns(inRect, 1);
            Widgets.Label(col[0], "Active guest thoughts:");
            var fullRect = new Rect(col[0].x - 4f, col[0].y, inRect.width, 20f);
            Widgets.DrawHighlight(fullRect);
            
            List<Lord> visitorGroups = map.GetComponent<Hospitality_MapComponent>().PresentLords.ToList();
            Dictionary<String, int> groupedThoughts = new Dictionary<string, int>();
            foreach (var lord in visitorGroups.ToList())
            {
                List<Thought> thoughts = new List<Thought>();
                lord.ownedPawns[0].needs.mood.thoughts.GetAllMoodThoughts(thoughts);
                foreach (var thought in thoughts)
                {
                    GuestThoughtRating gtr = thought.def.GetModExtension<GuestThoughtRating>();
                    if (gtr != null)
                    {
                        groupedThoughts.SetOrAdd(thought.CurStage.LabelCap, groupedThoughts.GetValueOrDefault(thought.CurStage.LabelCap, 0) + 1);
                    }
                }
            }

            foreach (var groupedThought in groupedThoughts)
            {
                UIUtility.NextRow(col);
                if (col[0].y > inRect.height) break;
                Widgets.Label(col[0], groupedThought.Key + " (" + groupedThought.Value + ")");
            }
        }        
    }
}