using System;
using System.Collections.Generic;
using System.Linq;
using CashRegister;
using HugsLib.Utils;
using RimWorld;
using Storefront.Selling;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace HospitalityArchitect.Reception
{
    public class ITab_Register_Reception : ITab_Register
    {
        private bool showSettings = true;
        private bool showStats = true;
        private bool showStock = true;
        private ReceptionController reception;
        private readonly ThingFilterUI.UIState menuFilterState = new ThingFilterUI.UIState();

        public ITab_Register_Reception() : base(new Vector2(800, 504))
        {
            labelKey = "TabRegisterReception";
        }

        public override bool IsVisible => true;

        public override void FillTab()
        {
            reception = Register.GetReception();
            var fullRect = new Rect(0, 16, size.x, size.y - 16);
            var rectLeft = fullRect.LeftHalf().ContractedBy(10f);
            var rectRight = fullRect.RightHalf().ContractedBy(10f);

            DrawLeft(rectLeft);
            DrawRight(rectRight);
        }

        private void DrawRight(Rect rect)
        {
            reception ??= Register.GetReception();
            //DrawStock(new Rect(rect), store.Stock);
            // Menu
            {
                var menuRect = new Rect(rect);
                //menuRect.yMax -= 36;

                ThingFilterUI.DoThingFilterConfigWindow(menuRect, menuFilterState, reception.GetStoreFilter());
            }            
        }
        
        private void DrawLeft(Rect rect)
        {
            var topBar = rect.TopPartPixels(24);
            DrawStoreSelection(ref topBar);
            rect.yMin += topBar.height;

            if (showSettings)
            {
                var smallRect = new Rect(rect);

                DrawSettings(ref smallRect);
                rect.yMin += smallRect.height + 10;
            }

            if (showStats)
            {
                var smallRect = new Rect(rect);

                DrawStats(ref smallRect);
                rect.yMin += smallRect.height + 10;
            }
            
            if (showStock)
            {
                var smallRect = new Rect(rect);
                reception ??= Register.GetReception();
                DrawStock(smallRect, reception.Stock);
            }
        }

        private void DrawStoreSelection(ref Rect rect)
        {
            reception ??= Register.GetReception();

            var rectSelection = rect.LeftHalf();

            Widgets.Label(rectSelection, reception.Name.CapitalizeFirst());

            var widgetRow = new WidgetRow(rect.xMax, rect.y, UIDirection.LeftThenDown);

            // Rename
           /* if (widgetRow.ButtonIcon(TexButton.Rename, "StoreTooltipRename".Translate()))
            {
                Find.WindowStack.Add(new Dialog_RenameStore(reception));
            }*/
        }

        private void DrawSettings(ref Rect rect)
        {
            const int ListingItems = 1;
            rect.height = 30 * ListingItems;

            var listing = new Listing_Standard();
            listing.Begin(rect);
            {
                listing.CheckboxLabeled("TabRegisterOpened".Translate(), ref reception.openForBusiness, "TabRegisterOpenedTooltip".Translate());

                // disabled - for now - the price is calculated in a more complex way than a market value multiplier
                //DrawPrice(listing.GetRect(22));

                //bool guestsPay = store.guestPricePercentage > 0;
                //listing.CheckboxLabeled("TabRegisterGuestsHaveToPay".Translate(), ref guestsPay, "TabRegisterGuestHaveToPayTooltip".Translate(MarketPriceFactor.ToStringPercent()));
                //store.guestPricePercentage = guestsPay ? MarketPriceFactor : 0;
            }
            listing.End();
        }

        private void DrawStats(ref Rect rect)
        {
            const int listingItems = 4;
            rect.height = listingItems * 24 + 20;

            Widgets.DrawBoxSolid(rect, new Color(1f, 1f, 1f, 0.08f));
            rect = rect.ContractedBy(10);
            var listing = new Listing_Standard();
            listing.Begin(rect);
            {
                var activeStaff = reception.ActiveStaff;
                var customers = reception.Customers;

                listing.LabelDouble("TabRegisterActiveStaff".Translate(), activeStaff.FirstOrDefault()?.LabelShort);
                listing.LabelDouble("TabRegisterCustomers".Translate(), customers.Count.ToString(), customers.Select(p=>p.LabelShort).ToCommaList());
                
                listing.LabelDouble("TabRegisterEarnedYesterday".Translate(), reception.incomeYesterday.ToStringMoney());
                listing.LabelDouble("TabRegisterEarnedToday".Translate(), reception.incomeToday.ToStringMoney());

            }
            listing.End();
        }

        private static void DrawStock(Rect rect,  IReadOnlyCollection<Thing> stock)
        {
            const int listingItems = 12;
            rect.height = listingItems * 20 + 10;
            rect.y += 10;
            var grouped = stock.GroupBy(s => s.GetInnerIfMinified().def ?? s.def);
            
            var rectImage = new Rect(rect);
            rectImage.height = 20;
            var row = 0;
            // Icons for each type of stock
            foreach (var group in grouped)
            {
                if (group.Key == null) continue;
                // Amount label
                string amountText = $"{stock.Where(s => (s.GetInnerIfMinified().def ?? s.def) == group.Key).Sum(s => s.stackCount)}x {group.Key.LabelCap}, ";
                var amountSize = Text.CalcSize(amountText);
                rectImage.width = amountSize.x;
                // Will it fit?
                if (rectImage.x + rectImage.width > rect.width)
                {
                    row++;
                    rectImage.x = rect.x;
                }
                rectImage.y = rect.y + (row * rectImage.height);
                // Draw label
                Widgets.Label(rectImage, amountText);
                //rectImage.x += rectImage.width;
                // Icon
                //rectImage.width = rectImage.height;
                //DrawDefIcon(rectImage, group.Key, group.Key.LabelCap);
                //rectImage.x += rectImage.width;
                rectImage.x += rectImage.width;                    
            }
        }

        private static void DrawDefIcon(Rect rect, ThingDef def, string tooltip = null, Action onClicked = null)
        {
            if (tooltip != null)
            {
                TooltipHandler.TipRegion(rect, tooltip);
                Widgets.DrawHighlightIfMouseover(rect);
            }

            GUI.DrawTexture(rect, def.uiIcon);
            if (onClicked != null && Widgets.ButtonInvisible(rect)) onClicked.Invoke();
        }

        public override bool CanAssignToShift(Pawn pawn)
        {
            // TODO
            return true; //pawn.workSettings.WorkIsActive(SellingDefOf.Storefront_Selling);
        }
    }
}
