using Hospitality;
using Hospitality.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace HospitalityArchitect;
public class ITab_Pawn_HotelGuest : ITab_Pawn_Visitor
{
    private static readonly Listing_Standard listingStandard = new Listing_Standard();

    public ITab_Pawn_HotelGuest()
    {
        labelKey = "TabGuest";
        tutorTag = "HotelGuest";
        size = new Vector2(500f, 450f);
    }

    public override bool IsVisible => SelPawn.IsGuest();

    public override void FillTab()
    {
        Text.Font = GameFont.Small;
        Rect rect = new Rect(0f, 20f, size.x, size.y - 20).ContractedBy(10f);
        listingStandard.Begin(rect);
        {
            FillTabGuest(rect);
        }
        listingStandard.End();
    }

    private void FillTabGuest(Rect rect)
    {
        listingStandard.Label("TODO"); // TODO
    }
}
