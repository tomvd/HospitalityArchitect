using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace HospitalityArchitect
{
    public class Window_Computer : Window
    {
        private readonly List<ComputerApp> apps = new List<ComputerApp>();
        public readonly Pawn actor;

        private readonly Map map;
        private readonly FinanceService _financeService;
        private readonly HiringContractService _hiringContractService;
        
        private ComputerApp currentApp;

        public Window_Computer(Pawn actor)
        {
            map = actor.Map;
            this.actor = actor;
            _financeService = map.GetComponent<FinanceService>();
            _hiringContractService = map.GetComponent<HiringContractService>();

            // TODO move apps to defs, to allow for other mods to add apps?
            //apps.Add(new Dialog_Staff(actor, this));
            //apps.Add(new App_Staff(actor, this));
            //apps.Add(new Dialog_Rimazon(actor, this));
            //apps.Add(new App_Banking(actor, this));

            currentApp = apps[0];

            closeOnCancel = true;
            forcePause = true;
            closeOnAccept = true;
        }

        public override Vector2 InitialSize => new Vector2(800, Mathf.Min(740, UI.screenHeight));
        public override float Margin => 5f;

        public override void DoWindowContents(Rect inRect)
        {
            var anchor = Text.Anchor;
            var font = Text.Font;
            var rect = new Rect(inRect);
            var buttonBarRect = rect.TakeBottomPart(40f);
            Widgets.DrawHighlight(buttonBarRect);
            var topBarRect = rect.TakeTopPart(40f);
            Widgets.DrawHighlight(topBarRect);
            DoTopBarContents(topBarRect);

            currentApp.DoWindowContents(rect);

            buttonBarRect.x = 150;
            buttonBarRect.width = 100;
            foreach (var app in apps)
            {
                if (Widgets.ButtonText(buttonBarRect, app.getLabel())) currentApp = app;
                buttonBarRect.x += 100;
            }

            //if (Widgets.ButtonText(buttonBarRect.TakeRightPart(120f), "Close".Translate())) OnCancelKeyPressed();
            Text.Anchor = anchor;
            Text.Font = font;
        }

        public void ShutDownComputer()
        {
            base.Close();
        }

        /*
         * Bank balance: xxx Daily cashflow: xxx
         */
        private void DoTopBarContents(Rect inRect)
        {
            inRect.width = 150f;
            Widgets.Label(inRect,
                "Bank: " + _financeService.getMoneyInBank().ToStringMoney());
            inRect.x += 150f;
            Widgets.Label(inRect,
                "Cashflow: " + _financeService.GetCashFlow().ToStringMoney());

        }
    }
}