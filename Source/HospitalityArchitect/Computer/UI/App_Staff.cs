using UnityEngine;
using Verse;

namespace HospitalityArchitect
{
    public class App_Staff : ComputerApp
    {
        private readonly Map map;
        private HiringContractService _hiringContractService;

        public App_Staff(Thing user, Window_Computer host)
        {
            map = user.Map;
            _hiringContractService = map.GetComponent<HiringContractService>();
        }

        public override string getLabel()
        {
            return "Staff";
        }

        public override void DoWindowContents(Rect inRect)
        {
            var rect = new Rect(inRect);
            var anchor = Text.Anchor;
            var font = Text.Font;
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Small;
            rect.yMin += 40f;
            DoCurrentContracts(ref rect);

            //rect.yMin += 40f;
            /*
            Text.Font = GameFont.Tiny;
            Widgets.Label(rect.ContractedBy(30f, 0f), "VEF.HiringDesc".Translate(hireable.Key).Colorize(ColoredText.SubtleGrayColor));*/
            Text.Anchor = anchor;
            Text.Font = font;
        }

        private void DoCurrentContracts(ref Rect inRect)
        {
            var contracts = _hiringContractService.contracts;

            var rect = inRect.TopPartPixels(Mathf.Max(20f + contracts.Count * 30f, 120f));
            inRect.yMin += rect.height;
            var titleRect = rect.TakeTopPart(20f);
            Widgets.Label(titleRect, "Employer reputation: " + _hiringContractService.Reputation.ToStringDecimalIfSmall());
            titleRect.y += 20f;
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
                    contract.pawn.story.Adulthood.TitleFor(contract.pawn.gender) + ", " + contract.pawn.Wage().ToStringMoney());
                Text.Anchor = TextAnchor.MiddleCenter;

                var days = contract.daysHired;
                Widgets.Label(valueRect,days.ToString());
                if (Widgets.ButtonText(numRect, "Cancel contract"))
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("Are you sure?".Translate(), () =>
                    {
                        //Close();
                        _hiringContractService.EndContract(contract);
                    }, true, "VEF.CancelContract".Translate()));
            }
        }
    }
}