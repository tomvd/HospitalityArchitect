using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using RT.UItils;

namespace RT
{
    public class Dialog_Finance : Window
    {
        private readonly Map                                        map;
        private readonly List<TraderKindDef>                        traders;

        public Dialog_Finance(Thing negotiator)
        {
            map     = negotiator.Map;
            traders = DefDatabase<TraderKindDef>.AllDefs.Where(x => x.orbital).ToList();
            closeOnCancel = true;
            forcePause    = true;
            closeOnAccept = true;
        }

        public override    Vector2 InitialSize => new Vector2(600f, 300f);
        protected override float   Margin      => 15f;


        private void OnCallKeyPressed(TraderKindDef def)
        {
            base.OnAcceptKeyPressed();
            //SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
            TradeShip tradeShip = new TradeShip(def);
            if (map.listerBuildings.allBuildingsColonist.Any((Building b) => b.def.IsCommsConsole && b.GetComp<CompPowerTrader>().PowerOn))
            {
                Find.LetterStack.ReceiveLetter(tradeShip.def.LabelCap, "TraderArrival".Translate(
                    tradeShip.name,
                    tradeShip.def.label,
                    "TraderArrivalNoFaction".Translate()
                ), LetterDefOf.PositiveEvent, null);
            }
            map.passingShipManager.AddShip(tradeShip);
            tradeShip.GenerateThings();            
        }

        public override void DoWindowContents(Rect inRect)
        {
            var anchor = Text.Anchor;
            var font   = Text.Font;
            var nameRect = inRect.TakeTopPart(20f);
            nameRect.width = 300f;
            var buttonRect = new Rect(nameRect);
            buttonRect.x     += 100f;
            GUI.color = Color.white;
            var highlight = true;
            foreach (var trader in traders)
            {
                nameRect.y  += 20f;
                buttonRect.y += 20f;
                var fullRect = new Rect(nameRect.x - 4f, nameRect.y, nameRect.width + buttonRect.width, 20f);
                if (highlight) Widgets.DrawHighlight(fullRect);
                highlight   = !highlight;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(nameRect, trader.LabelCap);
                if (Widgets.ButtonText(buttonRect, "Call".Translate()))
                {
                        OnCallKeyPressed(trader);
                }
            }

            if (Widgets.ButtonText(inRect.TakeRightPart(120f).BottomPartPixels(40f), "Close".Translate())) OnCancelKeyPressed();
            Text.Anchor = anchor;
            Text.Font   = font;
        }
    }
}