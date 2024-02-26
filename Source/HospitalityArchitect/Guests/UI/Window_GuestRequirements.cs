using UnityEngine;
using Verse;

namespace HospitalityArchitect
{
    public class Window_GuestRequirements : Window
    {
        public readonly PawnKindDef type;

        public Window_GuestRequirements(PawnKindDef type)
        {
            this.type = type;
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

            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Small;

            DoList(ref rect);
            Text.Anchor = anchor;
            Text.Font = font;

            if (Widgets.ButtonText(buttonBarRect.TakeRightPart(120f), "Close".Translate())) OnCancelKeyPressed();
            Text.Anchor = anchor;
            Text.Font = font;
        }

        private void DoTopBarContents(Rect inRect)
        {
            inRect.width = 150f;
            Widgets.Label(inRect, type.LabelCap);

        }
        
        private void DoList(ref Rect inRect)
        {
            GuestTypeDef gt = type.GetModExtension<GuestTypeDef>();
            var col = UIUtility.CreateColumns(inRect, 2);
            Widgets.Label(col[0], "Arrives at");
            Widgets.Label(col[1], gt.arrivesAt.ToString());
            UIUtility.NextRow(col);
            Widgets.Label(col[0], "Bed budget");
            Widgets.Label(col[1], ((float)gt.bedBudget).ToStringMoney());
            UIUtility.NextRow(col);
            Widgets.Label(col[0], "Affected by weather");
            Widgets.Label(col[1], gt.seasonalVariance.ToStringPercent());
            UIUtility.NextRow(col);
            Widgets.Label(col[0], "Travels with partner");
            Widgets.Label(col[1], gt.travelWithPartnerChance.ToStringPercent());
            if (gt.isCamper)
            {
                UIUtility.NextRow(col);
                Widgets.Label(col[0], "Needs a camping spot");
            }            
            if (gt.bedroomRequirements != null)
            {
                UIUtility.NextRow(col);
                Widgets.Label(col[0], "Bedroom requirements:");
                foreach (var br in gt.bedroomRequirements)
                {
                    Widgets.Label(col[1], br.Label());
                    UIUtility.NextRow(col);
                }
            }
            if (gt.facilityRequirements != null)
            {
                UIUtility.NextRow(col);
                Widgets.Label(col[0], "Facility requirements:");
                foreach (var br in gt.facilityRequirements.facilities)
                {
                    Widgets.Label(col[1], br);
                    UIUtility.NextRow(col);
                }
            }            
        }        
    }
}