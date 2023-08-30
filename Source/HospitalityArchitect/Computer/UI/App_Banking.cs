using RimWorld;
using RT.UItils;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RT
{
    public class App_Banking : ComputerApp
    {
        private readonly Map map;
        private readonly FinanceService _financeService;

        public App_Banking(Thing user, Window_Computer host)
        {
            map = user.Map;
            _financeService = map.GetComponent<FinanceService>();
        }

        public override string getLabel()
        {
            return "Banking";
        }

        private void OnBuyKeyPressed(ThingDef def)
        {
            SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
        }

        public override void DoWindowContents(Rect inRect)
        {
            var anchor = Text.Anchor;
            var font = Text.Font;

            var categoryRect = inRect.TakeLeftPart(100f);
            categoryRect.height = 20f;
            categoryRect.width = 200f;
            if (Widgets.ButtonText(categoryRect, "Deposit all silver (" + _financeService.getAvailableSilver()+")"))
            {
                _financeService.Deposit();
            }
            categoryRect.width = 100f;
            foreach (var loanType in _financeService.LoanTypes)
            {
                categoryRect.x = 0f;
                categoryRect.y += 20f;
                Widgets.Label(categoryRect,
                    $"loan {loanType.Amount.ToStringMoney()} from {loanType.Company} at {loanType.Interest.ToStringPercent()} daily");
                categoryRect.x += 100f;
                if (_financeService.Loans.Exists(loan => loan.LoanType == loanType.GetUniqueLoadID()))
                {
                    Loan l = _financeService.Loans.Find(loan => loan.LoanType == loanType.GetUniqueLoadID());
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

            Text.Anchor = anchor;
            Text.Font = font;
        }

    }
}