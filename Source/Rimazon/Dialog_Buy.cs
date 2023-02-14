using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using RT.UItils;

namespace RT
{
    public class Dialog_Caller : Window
    {
        private readonly Map                                        map;
        private readonly List<ThingCategoryDef>                     thingCategories;
        private readonly List<ThingCategoryDef>                     disallowedCategories;
        private ThingCategoryDef currentCategory;

        public Dialog_Caller(Thing negotiator)
        {
            map     = negotiator.Map;
            thingCategories = DefDatabase<ThingCategoryDef>.AllDefs.Where(x => !disallowedCategories.Contains(x)).ToList();
            currentCategory = thingCategories.First();
            closeOnCancel = true;
            forcePause    = true;
            closeOnAccept = true;
        }

        public override    Vector2 InitialSize => new Vector2(600f, 300f);
        protected override float   Margin      => 15f;

        private void OnBuyKeyPressed(ThingDef def)
        {
            SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
        }

        public override void DoWindowContents(Rect inRect)
        {
            // TODO on the left we have a list of categories, where the user can pick one
            var anchor = Text.Anchor;
            var font   = Text.Font;

            var categoryRect = inRect.TakeLeftPart(100f);
            var nameRect = categoryRect.TakeTopPart(20f);
            nameRect.width = 100f;
            GUI.color = Color.white;
            foreach (var category in thingCategories)
            {
                nameRect.y  += 20f;
                var fullRect = new Rect(nameRect.x - 4f, nameRect.y, nameRect.width, 20f);
                if (category == currentCategory) Widgets.DrawHighlight(fullRect);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(nameRect, category.label);
            }

            categoryRect = inRect.TakeRightPart(500f);
            nameRect = categoryRect.TakeTopPart(20f);
            nameRect.width = 100f;
            var buttonRect = new Rect(nameRect);
            buttonRect.x     += 100f;
            GUI.color = Color.white;
            var highlight = true;
            foreach (var thing in currentCategory.SortedChildThingDefs)
            {
                nameRect.y  += 20f;
                buttonRect.y += 20f;
                var fullRect = new Rect(nameRect.x - 4f, nameRect.y, nameRect.width + buttonRect.width, 20f);
                if (highlight) Widgets.DrawHighlight(fullRect);
                highlight   = !highlight;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(nameRect, thing.LabelCap);
                if (Widgets.ButtonText(buttonRect, "Buy".Translate()))
                {
                        OnBuyKeyPressed(thing);
                }
            }

            if (Widgets.ButtonText(inRect.TakeRightPart(120f).BottomPartPixels(40f), "Close".Translate())) OnCancelKeyPressed();
            Text.Anchor = anchor;
            Text.Font   = font;
        }

        
    }
}