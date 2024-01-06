using RimWorld;
using UnityEngine;
using Verse;

namespace HospitalityArchitect
{
    public class Dialog_Finance : MainTabWindow
    {
        private readonly FinanceService _financeService;
        private int currentTab = 0;

        public Dialog_Finance()
        {
            _financeService = Find.CurrentMap.GetComponent<FinanceService>();
            closeOnCancel = true;
            forcePause = false;
            closeOnAccept = true;
        }

        public override Vector2 InitialSize => new Vector2(1000, 600f);
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
            }


            var buttonBarRect = rect.TakeBottomPart(40f);
            buttonBarRect.x = 150;
            buttonBarRect.width = 100;
            if (Widgets.ButtonText(buttonBarRect, "Reports")) currentTab = 0;
            buttonBarRect.x += 100;
            if (Widgets.ButtonText(buttonBarRect, "Loans")) currentTab = 1;
            Text.Anchor = anchor;
            Text.Font = font;
        }

        private void doReports(ref Rect inRect)
        {
            var col = UIUtility.CreateColumns(inRect, 3);
            col[0].width = UIUtility.Percentage(inRect, 50f);
            col[1].width = UIUtility.Percentage(inRect, 25f);
            col[2].width = UIUtility.Percentage(inRect, 25f);
            Widgets.Label(col[0], "");
            Widgets.Label(col[1], "Income");
            Widgets.Label(col[2], "Expenses");
            foreach (var (type, value) in _financeService.getCurrentReport().getBooking())
            {
                UIUtility.NextRow(col);
                Widgets.Label(col[0], type.ToString());
                if (value < 0)
                    Widgets.Label(col[2], ((float)value).ToStringMoney());
                if (value > 0)
                    Widgets.Label(col[1], ((float)value).ToStringMoney());
            }
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
            foreach (var loanType in _financeService.LoanTypes)
            {
                categoryRect.x = 0f;
                categoryRect.y += 20f;
                Widgets.Label(categoryRect,
                    $"loan {loanType.Amount.ToStringMoney()} from {loanType.Company} at {loanType.Interest.ToStringPercent()} daily");
                categoryRect.x += 300f;
                if (_financeService.Loans.Exists(loan => loan.LoanType.Equals(loanType.GetUniqueLoadID())))
                {
                    Loan l = _financeService.Loans.Find(loan => loan.LoanType.Equals(loanType.GetUniqueLoadID()));
                    Widgets.Label(categoryRect,
                        "balance: " + l.Balance.ToStringMoney());
                    categoryRect.x += 100f;
                    if (Widgets.ButtonText(categoryRect, "Repay 1K"))
                    {
                        if (!_financeService.canAfford(1000))
                            Messages.Message("Not enough funds", MessageTypeDefOf.RejectInput);
                        else
                            _financeService.Repay(1000, l);
                    }
                }
                else
                {
                    if (Widgets.ButtonText(categoryRect, "Take loan"))
                    {
                        _financeService.TakeLoan(loanType);
                    }
                }
            }
        }        
    }
}