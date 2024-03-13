using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace HospitalityArchitect
{
    public class Dialog_Finance : MainTabWindow
    {
        private readonly FinanceService _financeService;
        private int currentTab = 0;
        private int currentSubTab = 0;
        private HistoryAutoRecorderGroup historyAutoRecorderGroup;
        private static List<CurveMark> marks = new List<CurveMark>();
        private FloatRange graphSection;
        
        public Dialog_Finance()
        {
            _financeService = Find.CurrentMap.GetComponent<FinanceService>();
            closeOnCancel = true;
            forcePause = false;
            closeOnAccept = true;
        }

        public override Vector2 InitialSize => new Vector2(600, 400f);
        public override float Margin => 15f;


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
                    doReports(ref rect);
                    break;
                case 1:
                    DoLoans(ref rect);
                    break;
                case 2:
                    DoGraph(ref rect);
                    break;
            }


            var buttonBarRect = rect.TakeBottomPart(40f);
            buttonBarRect.x = 150;
            buttonBarRect.width = 100;
            if (Widgets.ButtonText(buttonBarRect, "Reports"))
            {
                currentTab = 0;
                SoundDefOf.Click.PlayOneShotOnCamera();
            }
            buttonBarRect.x += 100;
            if (Widgets.ButtonText(buttonBarRect, "Loans"))
            {
                currentTab = 1;
                SoundDefOf.Click.PlayOneShotOnCamera();
            }
            buttonBarRect.x += 100;
            if (Widgets.ButtonText(buttonBarRect, "Graph"))
            {
                currentTab = 2;
                SoundDefOf.Click.PlayOneShotOnCamera();
            }            
            Text.Anchor = anchor;
            Text.Font = font;
        }

        private void doReports(ref Rect inRect)
        {
            var buttonBarRect = inRect.TakeTopPart(40f);
            buttonBarRect.x = 150;
            buttonBarRect.width = 100;
            float num = (float)Find.TickManager.TicksGame / 60000f;
            graphSection = new FloatRange(0f, num);
            if (Widgets.ButtonText(buttonBarRect, "Today"))
            {
                currentSubTab = 0;
                SoundDefOf.Click.PlayOneShotOnCamera();                
            }
            buttonBarRect.x += 100;
            if (Widgets.ButtonText(buttonBarRect, "Yesterday"))
            {
                currentSubTab = 1;
                SoundDefOf.Click.PlayOneShotOnCamera();                
            }
            buttonBarRect.x += 100;
            if (Widgets.ButtonText(buttonBarRect, "All Time"))
            {
                currentSubTab = 2;
                SoundDefOf.Click.PlayOneShotOnCamera();
            }

            var col = UIUtility.CreateColumns(inRect, 5);
            col[0].width = UIUtility.Percentage(inRect, 20f);
            col[1].width = UIUtility.Percentage(inRect, 20f);
            col[2].width = UIUtility.Percentage(inRect, 20f);
            col[3].width = UIUtility.Percentage(inRect, 20f);
            col[4].width = UIUtility.Percentage(inRect, 20f);
            Widgets.Label(col[0], "");
            Widgets.Label(col[1], "Income");
            Widgets.Label(col[2], "Expenses");
            var highlight = true;
            foreach (var (type, value) in _financeService.getReport(currentSubTab).getBooking())
            {
                UIUtility.NextRow(col);
                var fullRect = new Rect(col[0].x - 4f, col[0].y, inRect.width, 20f);                
                if (highlight) Widgets.DrawHighlight(fullRect);
                highlight = !highlight;
                Widgets.Label(col[0], type.ToString());
                if (value < 0)
                    Widgets.Label(col[2], ((float)value).ToStringMoney());
                if (value > 0)
                    Widgets.Label(col[1], ((float)value).ToStringMoney());
            }

            var result = _financeService.getReport(currentSubTab).getNetResult();
            UIUtility.NextRow(col);
            Widgets.Label(col[0], "TOTAL RESULT:");
            if (result < 0)
                Widgets.Label(col[2], ((float)result).ToStringMoney());
            if (result > 0)
                Widgets.Label(col[1], ((float)result).ToStringMoney());
        }
        
        private void DoLoans(ref Rect inRect)
        {
            var categoryRect = inRect.TakeLeftPart(100f);
            categoryRect.height = 20f;
            categoryRect.width = 200f;
            if (Widgets.ButtonText(categoryRect, "Deposit all silver (" + _financeService.getAvailableSilver()+")"))
            {
                _financeService.Deposit();
            }
            categoryRect.width = 300f;
            categoryRect.x = 0f;
            categoryRect.y += 20f;
            Widgets.Label(categoryRect,
                $"Loan at {_financeService.getLoanInterest().ToStringPercent()} daily.");
            categoryRect.x += 150f;
            if (_financeService.moneyInLoan > 0)
            {
                Widgets.Label(categoryRect,
                    "balance: " + _financeService.moneyInLoan.ToStringMoney());
                categoryRect.x += 100f;
                categoryRect.width = 150f;
                if (Widgets.ButtonText(categoryRect, "Repay 500s"))
                {
                    if (!_financeService.canAfford(500f))
                        Messages.Message("Not enough funds", MessageTypeDefOf.RejectInput);
                    else
                        _financeService.Repay(500f);
                }
            }
            categoryRect.x += 150f;
            categoryRect.width = 150f;
            if (Widgets.ButtonText(categoryRect, "Loan 500s"))
            {
                _financeService.TakeLoan(500f);
            }
        }

        private void DoGraph(ref Rect rect)
        {
            rect.yMin += 17f;
            Rect graphRect = new Rect(0f, 0f, rect.width - 10f, 260f);
            Rect legendRect = new Rect(0f, graphRect.yMax, (rect.width - 10f) / 2f, 40f);
            if (historyAutoRecorderGroup != null)
            {
                marks.Clear();
                List<Tale> allTalesListForReading = Find.TaleManager.AllTalesListForReading;
                for (int i = 0; i < allTalesListForReading.Count; i++)
                {
                    Tale tale = allTalesListForReading[i];
                    if (tale.def.type == TaleType.PermanentHistorical)
                    {
                        float x = (float)GenDate.TickAbsToGame(tale.date) / 60000f;
                        marks.Add(new CurveMark(x, tale.ShortSummary, tale.def.historyGraphColor));
                    }
                }

                historyAutoRecorderGroup.DrawGraph(graphRect, legendRect, graphSection, marks);
            }
            else
            {
                historyAutoRecorderGroup = Find.History.Groups().Find(harg => harg.def.defName.Equals("Graphs_Finances"));
            }
        }
    }
}