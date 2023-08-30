using RT.UItils;
using UnityEngine;
using Verse;

namespace RT
{
    public class Dialog_Finance : Window
    {
        private readonly FinanceService _financeService;

        public Dialog_Finance()
        {
            _financeService = Find.CurrentMap.GetComponent<FinanceService>();
            closeOnCancel = true;
            forcePause = false;
            closeOnAccept = true;
        }

        public override Vector2 InitialSize => new Vector2(600f, 300f);
        protected override float Margin => 15f;


        public override void DoWindowContents(Rect inRect)
        {
            var anchor = Text.Anchor;
            var font = Text.Font;
            var nameRect = inRect.TakeTopPart(20f);
            nameRect.width = 300f;
            var buttonRect = new Rect(nameRect);
            buttonRect.x += 100f;
            GUI.color = Color.white;

            Text.Anchor = TextAnchor.MiddleLeft;
            
            foreach (var (type, value) in _financeService.getCurrentReport().getIncome())
            {
                Widgets.Label(nameRect, type.ToString());
                Widgets.Label(buttonRect, ((float)value).ToStringMoney());
                nameRect.y += 20f;
                buttonRect.y += 20f;
            }
            buttonRect.x += 100f;
            foreach (var (type, value) in _financeService.getCurrentReport().getExpenses())
            {
                Widgets.Label(nameRect, type.ToString());
                Widgets.Label(buttonRect, ((float)value).ToStringMoney());
                nameRect.y += 20f;
                buttonRect.y += 20f;
            }
            if (Widgets.ButtonText(inRect.TakeRightPart(120f).BottomPartPixels(40f), "Close".Translate()))
                OnCancelKeyPressed();
            Text.Anchor = anchor;
            Text.Font = font;
        }
    }
}