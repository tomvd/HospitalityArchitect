using Hospitality;
using Hospitality.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace HospitalityArchitect;
public class ITab_Pawn_Guest : ITab_Pawn_Visitor
{
    private static readonly string txtRecruitmentPenalty = "RecruitmentPenalty";
    private static readonly string txtFactionGoodwill = "FactionGoodwill".Translate();
    private static readonly string txtHospitality = "Hospitality".Translate();
    private static readonly string txtMakeDefault = "MakeDefault".Translate();
    private static readonly string txtSendAway = "SendAway".Translate();
    private static readonly string txtSendAwayQuestion = "SendAwayQuestion";
    private static readonly string txtMakeDefaultTooltip = "MakeDefaultTooltip".Translate();
    private static readonly string txtSendAwayTooltip = "SendAwayTooltip".Translate();
    internal static readonly string txtImproveTooltip = "ImproveTooltip".Translate();
    internal static readonly string txtMakeFriendsTooltip = "TryRecruitTooltip".Translate();

    protected static readonly Vector2 buttonSize = new Vector2(120f, 30f);
    private static readonly Listing_Standard listingStandard = new Listing_Standard();

    public ITab_Pawn_Guest()
    {
        labelKey = "TabGuest";
        tutorTag = "Guest";
        size = new Vector2(500f, 450f);
    }

    public override bool IsVisible => SelPawn.IsGuest() || SelPawn.IsTrader();

    public override void FillTab()
    {
        Text.Font = GameFont.Small;
        Rect rect = new Rect(0f, 20f, size.x, size.y - 20).ContractedBy(10f);
        listingStandard.Begin(rect);
        {
            if (SelPawn.IsTrader())
            {
                FillTabTrader();
            }
            else
            {
                FillTabGuest(rect);
            }
        }
        listingStandard.End();
    }

    private void FillTabTrader()
    {
        listingStandard.Label("IsATrader".Translate().AdjustedFor(SelPawn));
    }

    private void FillTabGuest(Rect rect)
    {
        //ConceptDatabase.KnowledgeDemonstrated(ConceptDefOf.PrisonerTab, KnowledgeAmount.GuiFrame);
        var pawn = SelPawn;

        listingStandard.ColumnWidth = size.x - 20;

        var comp = pawn.GetComp<CompGuest>();

        // If the lord is not on the map it's invalid!
        if (comp?.lord != null && comp.lord.ownedPawns.Contains(pawn) && pawn.Map.lordManager.lords.Contains(comp.lord))
        {
            listingStandard.Gap();
            string labelStay = "AreaToStay".Translate();
            string labelBuy = "AreaToBuy".Translate();
            var rectStayLabel = listingStandard.GetRect(Text.CalcHeight(labelStay, listingStandard.ColumnWidth));
            var rectStay = listingStandard.GetRect(24);
            var rectBuyLabel = listingStandard.GetRect(Text.CalcHeight(labelBuy, listingStandard.ColumnWidth));
            var rectBuy = listingStandard.GetRect(24);

            //DialogUtility.LabelWithTooltip(labelStay, "AreaToStayTooltip".Translate(), rectStayLabel);
            //GenericUtility.DoAreaRestriction(rectStay, comp.GuestArea, SetAreaRestriction, AreaUtility.AreaAllowedLabel_Area, pawn.Map);
            //DialogUtility.LabelWithTooltip(labelBuy, "AreaToBuyTooltip".Translate(), rectBuyLabel);
            //GenericUtility.DoAreaRestriction(rectBuy, comp.ShoppingArea, SetAreaShopping, GenericUtility.GetShoppingLabel, pawn.Map);

            listingStandard.Gap(50);

            var rectSetDefault = new Rect(rect.xMax - buttonSize.x - 10, 160, buttonSize.x, buttonSize.y);
            var rectSendHome = new Rect(rect.xMin - 10, 160, buttonSize.x, buttonSize.y);
            //DialogUtility.DrawButton(() => SetAllDefaults(pawn), txtMakeDefault, rectSetDefault, txtMakeDefaultTooltip);
            //DialogUtility.DrawButton(() => SendHomeDialog(pawn.GetLord()), txtSendAway, rectSendHome, txtSendAwayTooltip);

            // Highlight defaults
            if (Mouse.IsOver(rectSetDefault))
            {
                Widgets.DrawHighlight(rectStay);
                Widgets.DrawHighlight(rectStayLabel);
                Widgets.DrawHighlight(rectBuy);
                Widgets.DrawHighlight(rectBuyLabel);
            }
        }
        listingStandard.Gap();

        // Will only have score while "checked in", becomes 0 again when guest leaves
        if (pawn.GetVisitScore(out var score))
        {
            DrawProgressBar(txtHospitality+":", listingStandard.GetRect(32), Mathf.Clamp01(score));
        }
    }

    private static void DrawProgressBar(string label, Rect rect, float score)
    {
        Text.Anchor = TextAnchor.MiddleLeft;
        Rect rectLabel = new Rect(rect.x, rect.y, rect.width / 2f, rect.height);
        Widgets.Label(rectLabel, label);
        Text.Anchor = TextAnchor.UpperLeft;
        Rect rectBar = new Rect(rectLabel.xMax, rect.y, rect.width/2f, rect.height);
        Widgets.FillableBar(rectBar.ContractedBy(4f), score, SocialCardUtility.BarFullTexHor);
    }

    private void SetAreaRestriction(Area area)
    {
        foreach (var pawn in SelPawn.GetLord().ownedPawns)
        {
            pawn.GetComp<CompGuest>().GuestArea = area;
        }
    }

    private void SetAreaShopping(Area area)
    {
        foreach (var pawn in SelPawn.GetLord().ownedPawns)
        {
            pawn.GetComp<CompGuest>().ShoppingArea = area;
        }
    }
    private static void SendHomeDialog(Lord lord)
    {
        var text = txtSendAwayQuestion.Translate(new NamedArgument(lord.faction, "FACTION"));
        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, () => SendHome(lord)));
    }

    internal static void SendHome(Lord lord)
    {
        if (lord == null)
        {
            Log.Warning("lord == null");
            return;
        }

        foreach (var pawn in lord.ownedPawns)
        {
            var compGuest = pawn.GetComp<CompGuest>();
            if (compGuest == null)
            {
                Log.Warning(pawn.LabelShortCap + " has no compGuest.");
                continue;
            }

            compGuest.sentAway = true;
        }
    }

    internal static void SetAllDefaults(Pawn pawn)
    {
        Map map = pawn.MapHeld;
        if (map == null) return;

        var mapComp = map.GetMapComponent();

        mapComp.defaultAreaRestriction = pawn.GetGuestArea();
        mapComp.defaultAreaShopping = pawn.GetShoppingArea();
/*
        var guests = GuestUtility.GetAllGuests(map);
        foreach (var guest in guests)
        {
            var comp = guest.CompGuest();
            if (comp != null)
            {
                comp.entertain = mapComp.defaultEntertain;
                comp.makeFriends = mapComp.defaultMakeFriends;
                comp.GuestArea = mapComp.defaultAreaRestriction;
                comp.ShoppingArea = mapComp.defaultAreaShopping;
            }
        }*/
    }
}
