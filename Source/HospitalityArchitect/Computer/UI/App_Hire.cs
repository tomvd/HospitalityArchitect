using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RT.DeliverySystem;
using RT.UItils;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RT
{
    public class App_Hire : ComputerApp
    {
        private readonly Map map;
        private Window_Computer host;
        private Vector2 pawnsScrollPos = new Vector2(0, 0);
        private HiringContractService _hiringContractService;
        private FinanceService _financeService;

        public App_Hire(Thing negotiator, Window_Computer host)
        {
            map = negotiator.Map;
            this.host = host;
            _hiringContractService = map.GetComponent<HiringContractService>();
            _financeService = map.GetComponent<FinanceService>();
        }

        public override string getLabel()
        {
            return "Hire";
        }

        private void OnHireKeyPressed(Pawn pawn)
        {
            SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
            _hiringContractService.hire(pawn);
        }

        public override void DoWindowContents(Rect inRect)
        {
            var rect = new Rect(inRect);
            var anchor = Text.Anchor;
            var font = Text.Font;
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Small;

            DoHireableFaction(ref rect);


            Text.Anchor = anchor;
            Text.Font = font;
        }

        private void DoHireableFaction(ref Rect inRect)
        {
            var rect = inRect.TopPartPixels(Mathf.Max(20f + _hiringContractService.candidates.Count * 30f, 120f));
            inRect.yMin += rect.height;
            var titleRect = rect.TakeTopPart(20f);
            var iconRect = rect.LeftPartPixels(105f).ContractedBy(5f);
            titleRect.x += 15f;
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Tiny;
            var nameRect = new Rect(titleRect);
            Widgets.Label(titleRect, "name, background");
            titleRect.x += 400f;
            titleRect.width = 120f;
            Text.Anchor = TextAnchor.MiddleCenter;
            var valueRect = new Rect(titleRect);
            Widgets.Label(titleRect, "wage");
            titleRect.x += 100f;
            titleRect.width = 100f;
            var numRect = new Rect(titleRect);
            Widgets.Label(titleRect, "hire");
            GUI.color = Color.white;
            var highlight = true;
            foreach (var candidate in _hiringContractService.candidates.ToList())
            {
                nameRect.y += 20f;
                valueRect.y += 20f;
                numRect.y += 20f;
                var fullRect = new Rect(nameRect.x - 4f, nameRect.y, nameRect.width + valueRect.width + numRect.width,
                    20f);
                if (highlight) Widgets.DrawHighlight(fullRect);
                highlight = !highlight;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(nameRect, candidate.NameFullColored + ", " + candidate.story.Childhood.TitleFor(candidate.gender)
                                        + ", " + candidate.story.Adulthood.TitleFor(candidate.gender)
                                        + ", passion: " + candidate.skills.skills.OrderByDescending(record => record.passion).First().ToString());
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(valueRect,
                    (candidate.Wage()).ToStringMoney()+"/day");


                if (Widgets.ButtonText(numRect.RightHalf(), "Hire"))
                {
                    if (!_financeService.canAfford(candidate.Wage()))
                        Messages.Message("NotEnoughSilver".Translate(), MessageTypeDefOf.RejectInput);
                    else
                        OnHireKeyPressed(candidate);
                }
            }
        }
    }
}