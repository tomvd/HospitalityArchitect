using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace HospitalityArchitect
{
    public class Dialog_Staff : MainTabWindow
    {
        private readonly Map map;
        private Vector2 pawnsScrollPos = new Vector2(0, 0);
        private HiringContractService _hiringContractService;
        private FinanceService _financeService;
        private int currentTab = 0;

        public Dialog_Staff()
        {
            map = Find.CurrentMap;
            _hiringContractService = map.GetComponent<HiringContractService>();
            _financeService = map.GetComponent<FinanceService>();
        }

        public override Vector2 InitialSize => new Vector2(1000f, 600f);
        public override float Margin => 15f;

        private void OnHireKeyPressed(Pawn pawn, int type)
        {
            SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
            _hiringContractService.hire(pawn, type);
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
                    DoCurrentContracts(ref rect);
                    break;
                case 1:
                    DoHireableFaction(ref rect);
                    break;
            }


            var buttonBarRect = rect.TakeBottomPart(40f);
            buttonBarRect.x = 150;
            buttonBarRect.width = 100;
            if (Widgets.ButtonText(buttonBarRect, "Staff"))
            {
                currentTab = 0;
                SoundDefOf.Click.PlayOneShotOnCamera();
            }
            buttonBarRect.x += 100;
            if (Widgets.ButtonText(buttonBarRect, "Hiring"))
            {
                currentTab = 1;
                SoundDefOf.Click.PlayOneShotOnCamera();
            }
            Text.Anchor = anchor;
            Text.Font = font;
        }
        
        private void DoCurrentContracts(ref Rect inRect)
        {
            var contracts = _hiringContractService.contracts;

            var rect = inRect.TopPartPixels(Mathf.Max(20f + contracts.Count * 30f, 120f));
            inRect.yMin += rect.height;
            var titleRect = rect.TakeTopPart(20f);
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Tiny;
            var nameRect = new Rect(titleRect);
            Widgets.Label(titleRect, "Name".Translate());
            titleRect.x += 200f;
            titleRect.width = 120f;
            Text.Anchor = TextAnchor.MiddleCenter;
            var valueRect = new Rect(titleRect);
            Widgets.Label(titleRect, "Days in service");
            titleRect.x += 100f;
            titleRect.width = 300f;
            var numRect = new Rect(titleRect);
            Text.Font = GameFont.Tiny;
            Widgets.Label(titleRect, "Cancel".Translate().Colorize(ColoredText.SubtleGrayColor));
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            var highlight = true;
            foreach (var contract in _hiringContractService.contracts)
            {
                nameRect.y += 20f;
                valueRect.y += 20f;
                numRect.y += 20f;
                var fullRect = new Rect(nameRect.x - 4f, nameRect.y, nameRect.width + valueRect.width + numRect.width,
                    20f);
                if (highlight) Widgets.DrawHighlight(fullRect);
                highlight = !highlight;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(nameRect,
                    contract.pawn.NameShortColored + ", " +
                    contract.pawn.story.Adulthood.TitleFor(contract.pawn.gender) + ", " + contract.pawn.Wage().ToStringMoney() + ", " + contract.arrivesAt+"-"+contract.leavesAt);
                Text.Anchor = TextAnchor.MiddleCenter;

                var days = contract.daysHired;
                
                if (contract.pawn.mindState.mentalBreaker.CurMood < contract.pawn.mindState.mentalBreaker.BreakThresholdMinor)
                    Widgets.Label(valueRect,"low mood!"); // todo make mood a separate column
                else
                    Widgets.Label(valueRect,days.ToString());
                
                
                if (Widgets.ButtonText(numRect, "Cancel contract"))
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("Are you sure?".Translate(), () =>
                    {
                        //Close();
                        _hiringContractService.EndContract(contract);
                    }, true, "VEF.CancelContract".Translate()));
            }
        }

        private void DoHireableFaction(ref Rect inRect)
        {
            var colTop0 = UIUtility.CreateColumns(inRect, 3);
            if (Widgets.ButtonText(colTop0[0], "Look around (20s)")) _hiringContractService.addPawn(0);
            if (Widgets.ButtonText(colTop0[1], "Advertisement (40s)")) _hiringContractService.addPawn(1);
            if (Widgets.ButtonText(colTop0[2], "Recruitment agency (80s)")) _hiringContractService.addPawn(2);

            var colTop = UIUtility.CreateColumns(inRect, 9);
            UIUtility.NextRow(colTop);
            Widgets.Label(colTop[0], "Spec(100s):");
            // either some passion or level 7 or higher 
            if (Widgets.ButtonText(colTop[1], "Construction")) _hiringContractService.addPawn(3,SkillDefOf.Construction);
            if (Widgets.ButtonText(colTop[2], "Cook")) _hiringContractService.addPawn(3,SkillDefOf.Cooking);
            if (Widgets.ButtonText(colTop[3], "Gardener")) _hiringContractService.addPawn(3,SkillDefOf.Plants);
            if (Widgets.ButtonText(colTop[4], "Hospitality")) _hiringContractService.addPawn(3,SkillDefOf.Social);
            if (Widgets.ButtonText(colTop[5], "Researcher")) _hiringContractService.addPawn(3,SkillDefOf.Intellectual);
            if (Widgets.ButtonText(colTop[6], "Doctor")) _hiringContractService.addPawn(3,SkillDefOf.Medicine);
            if (Widgets.ButtonText(colTop[7], "Craftsman")) _hiringContractService.addPawn(3,SkillDefOf.Crafting);
            if (Widgets.ButtonText(colTop[8], "Artist")) _hiringContractService.addPawn(3,SkillDefOf.Artistic);

            var col = UIUtility.CreateColumns(inRect, 7);
            UIUtility.NextRow(col);
            UIUtility.NextRow(col);
            UIUtility.NextRow(col);
            Widgets.Label(col[0], "Name");
            Widgets.Label(col[1], "Background(child)");
            Widgets.Label(col[2], "Background(adult)");
            Widgets.Label(col[3], "Passion");
            Widgets.Label(col[4], "Wage");
            Widgets.Label(col[5], "Hire (day)");
            Widgets.Label(col[6], "Hire (late)");

            GUI.color = Color.white;
            var highlight = true;
            foreach (var candidate in _hiringContractService.candidates.ToList())
            {
                UIUtility.NextRow(col);
                var fullRect = new Rect(col[0].x - 4f, col[0].y, inRect.width, 20f);
                if (highlight) Widgets.DrawHighlight(fullRect);
                highlight = !highlight;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(col[0], candidate.NameFullColored);
                Widgets.Label(col[1], candidate.story.Childhood.TitleFor(candidate.gender));
                Widgets.Label(col[2], candidate.story.Adulthood.TitleFor(candidate.gender));
                Widgets.Label(col[3], candidate.skills.skills.OrderByDescending(record => record.passion).First().def.skillLabel);

                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(col[4],
                    (candidate.Wage()).ToStringMoney()+"/day");


                if (Widgets.ButtonText(col[5], "7h-16h"))
                {
                    if (!_financeService.canAfford(candidate.Wage()))
                        Messages.Message("NotEnoughSilver".Translate(), MessageTypeDefOf.RejectInput);
                    else
                        OnHireKeyPressed(candidate,0);
                }
                if (Widgets.ButtonText(col[6], "16h-1h"))
                {
                    if (!_financeService.canAfford(candidate.Wage()))
                        Messages.Message("NotEnoughSilver".Translate(), MessageTypeDefOf.RejectInput);
                    else
                        OnHireKeyPressed(candidate,1);
                }
            }
        }
    }
}