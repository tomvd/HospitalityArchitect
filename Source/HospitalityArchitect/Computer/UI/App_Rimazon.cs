using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RimWorld;
using UnityEngine;
using Verse;

namespace HospitalityArchitect
{
    public class App_Rimazon : ComputerApp
    {
        // nice to have: a list of all resources of blueprints that we dont have at hand -> autoconvert to shopping list
        private readonly Dictionary<string, List<Thing>> inventory = new Dictionary<string, List<Thing>>();
        private readonly Map map;
        private string currentCategory;
        private readonly Window_Computer host;
        private readonly RimazonService _rimazonService;
        private readonly FinanceService _financeService;
        private Vector2 scrollPosition = new Vector2(0, 0);
        private Vector2 scrollPosition2 = new Vector2(0, 0);

        public App_Rimazon(Thing user, Window_Computer host)
        {
            map = user.Map;
            this.host = host;
            FillInventory();
            _rimazonService = map.GetComponent<RimazonService>();
            _financeService = map.GetComponent<FinanceService>();
        }

        public override string getLabel()
        {
            return "Rimazon";
        }


        public override void DoWindowContents(Rect inRect)
        {
            var anchor = Text.Anchor;
            var font = Text.Font;
            var topRect = inRect.TopPartPixels(480f);
            
            var categoryRect = topRect.LeftPartPixels(150f);
            var itemRect = topRect.RightPartPixels(topRect.width - categoryRect.width);
            
            var inventoryRect = itemRect.TopPartPixels(300f);
            var cartRect = itemRect.BottomPartPixels(180f);
            
            var bottomRect = inRect.BottomPartPixels(inRect.height - topRect.height);
            
            GUI.color = Color.white;

            //Listing_Standard ls = new Listing_Standard();

            // CategoryRect
            int size2 = inventory.Count * 20;
            var itemListInnerMargin = 0f;
            var viewRect = new Rect(0f, 0f, categoryRect.width - 16f, size2 + itemListInnerMargin * 2f);
            Widgets.BeginScrollView(categoryRect, ref scrollPosition, viewRect);
            var rowRect = viewRect.TopPartPixels(20f);
            foreach (var category in inventory)
            {
                if (category.Key == currentCategory) Widgets.DrawHighlight(rowRect);
                Text.Anchor = TextAnchor.MiddleLeft;
                if (Widgets.ButtonText(rowRect, category.Key, false)) currentCategory = category.Key;
                rowRect.y += 20f;
            }
            Widgets.EndScrollView();

            // ItemRect
            var highlight = true;
            size2 = inventory.GetValueOrDefault(currentCategory).Count * 20;
            viewRect = new Rect(0f, 0f, inventoryRect.width - 16f, size2 + itemListInnerMargin * 2f);
            Widgets.BeginScrollView(inventoryRect, ref scrollPosition2, viewRect);
            rowRect = viewRect.TopPartPixels(20f);
            foreach (var thing in inventory.GetValueOrDefault(currentCategory))
            {
                if (highlight) Widgets.DrawHighlight(rowRect);
                highlight = !highlight;
                DoThingRow(thing, rowRect);
                rowRect.y += 20f;
            }
            Widgets.EndScrollView();

            // CartRect
            rowRect = cartRect.TopPartPixels(20f);
            foreach (var thing in _rimazonService.CurrentOrder.cart.ToList())
            {
                if (highlight) Widgets.DrawHighlight(rowRect);
                highlight = !highlight;
                DoCartRow(thing, rowRect);
                rowRect.y += 20f;
            }
            
            var totalRect = bottomRect.LeftPartPixels(250f);
            rowRect = totalRect.TopPartPixels(20f);
            Widgets.Label(rowRect, "Subtotal: " + _rimazonService.CurrentOrder.subtotal.ToStringMoney());
            rowRect.y += 20f;
            Widgets.Label(rowRect, "Delivery: " + _rimazonService.CurrentOrder.deliverycost.ToStringMoney());
            rowRect.y += 20f;
            Widgets.DrawHighlight(rowRect);
            Widgets.Label(rowRect, "Total   : " + _rimazonService.CurrentOrder.total.ToStringMoney());

            var checkOutRect = bottomRect.RightPartPixels(550f);
            var deliveryRect = checkOutRect.LeftPartPixels(250f);
            var buttonRect = checkOutRect.RightPartPixels(200f);
            
            rowRect = deliveryRect.TopPartPixels(20f);
            rowRect.y += 20f;
            Widgets.Label(rowRect, "Delivery");
            rowRect.y += 20f;
            if (Widgets.RadioButtonLabeled(rowRect, "Normal(10h)",_rimazonService.CurrentOrder.DeliveryType==0)) {
                _rimazonService.CurrentOrder.DeliveryType = 0;
            }
            rowRect.y += 20f;
            if (Widgets.RadioButtonLabeled(rowRect, "Express(1h)",_rimazonService.CurrentOrder.DeliveryType==1)) {
                _rimazonService.CurrentOrder.DeliveryType = 1;
            }
            
            if (Widgets.ButtonText(buttonRect, "Checkout"))
            {
                if (!_financeService.canAfford(_rimazonService.CurrentOrder.total))
                    Messages.Message("NotEnoughSilver".Translate(), MessageTypeDefOf.RejectInput);
                else
                {
                    _rimazonService.DoCheckOut();
                    host.ShutDownComputer();
                }
            }
            Text.Anchor = anchor;
            Text.Font = font;
        }

        private void DoThingRow(Thing thing, Rect rect)
        {
            var columnRect = rect.LeftPartPixels(20f);
            Widgets.ThingIcon(columnRect, thing.def);
            columnRect.x += 20f;
            Widgets.InfoCardButton(columnRect, thing.def);
            columnRect.x += 20f;
            columnRect.width = 250f;
            if (thing is Pawn pawn)
                Widgets.Label(columnRect, thing.LabelCapNoCount + " ("+pawn.gender+")");
            else
                Widgets.Label(columnRect, thing.LabelCapNoCount);
            columnRect.x += 250f;
            Widgets.Label(columnRect, thing.MarketValue.ToStringMoney());
            //columnRect.x += 100f;
            //Widgets.Label(columnRect, thing.def.BaseMass.ToStringMass());
            columnRect.x += 100f;
            columnRect.width = 20f;
            var t = thing is MinifiedThing minifiedThing ? minifiedThing.InnerThing : thing;
            CompQuality compQuality = t.TryGetComp<CompQuality>();
            if (compQuality != null)
            {
                if (Widgets.ButtonText(columnRect, "Q")){compQuality.SetQuality(
                        QualityUtility.AllQualityCategories.SkipWhile(x => x != compQuality.Quality).Skip(1).DefaultIfEmpty( QualityUtility.AllQualityCategories[0] ).FirstOrDefault(),
                        ArtGenerationContext.Outsider);
                }
            }
            columnRect.x += 20f;
            if (t.def.MadeFromStuff)
            {
                if (Widgets.ButtonText(columnRect, "S")) {t.SetStuffDirect(
                        GenStuff.AllowedStuffsFor((BuildableDef) t.def).SkipWhile(x => x != t.Stuff).Skip(1).DefaultIfEmpty( GenStuff.AllowedStuffsFor((BuildableDef) t.def).ToArray()[0] ).FirstOrDefault());
                }
            }
            columnRect.x += 20f;
            
            if (Widgets.ButtonText(columnRect, ">"))
            {
                _rimazonService.CurrentOrder.AddToCart(thing, false, host.actor.Faction);
            }

            columnRect.x += 20f;
            if (Widgets.ButtonText(columnRect, ">>"))
            {
                _rimazonService.CurrentOrder.AddToCart(thing, true, host.actor.Faction);
            }
        }

        private void DoCartRow(Thing thing, Rect rect)
        {
            var columnRect = new Rect(rect.LeftPartPixels(250f));
            if (thing is Pawn pawn)
                Widgets.Label(columnRect, thing.LabelCapNoCount + " ("+pawn.gender+")");
            else
                Widgets.Label(columnRect, thing.LabelCap);
            var t = thing is MinifiedThing minifiedThing ? minifiedThing.InnerThing : thing;
            columnRect.x += 250f;
            Widgets.Label(columnRect, (t.MarketValue * t.stackCount).ToStringMoney());
            //columnRect.x += 100f;
            //Widgets.Label(columnRect, t.def.BaseMass.ToStringMass());
            columnRect.x += 100f;
            columnRect.width = 20f;

            if (Widgets.ButtonText(columnRect, "X")) _rimazonService.CurrentOrder.RemoveFromCart(thing);
        }
        private void FillInventory()
        {
            // Items
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs.Where(def => def.category == ThingCategory.Item))
            {
                if (thingDef.GetStatValueAbstract(StatDefOf.MarketValue) > 0)// && thingDef.tradeability.TraderCanSell())
                {
                    thingDef.thingCategories.ForEach(def =>
                        {
                            String category = def.defName;
                            category = SplitCamelCase(category).First();
                            if (!(category.ToLower().Contains("skilltrainers") || category.ToLower().Contains("artifacts") ||
                                  category.ToLower().Contains("archotech") || category.ToLower().Contains("items")))
                            {
                                if (!inventory.ContainsKey(category))
                                {
                                    inventory.Add(category, new List<Thing>());
                                }

                                inventory.TryGetValue(category).Add(thingDef.MadeFromStuff
                                    ? ThingMaker.MakeThing(thingDef,
                                        GenStuff.DefaultStuffFor(
                                            thingDef)) //GenStuff.RandomStuffByCommonalityFor(thingDef)))
                                    : ThingMaker.MakeThing(thingDef));
                            }
                        }
                    );
                }
            }
            // Buildings
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs.Where(def => def.category == ThingCategory.Building))
            {
                if (thingDef.GetStatValueAbstract(StatDefOf.MarketValue) > 0)
                {
                    String category = (thingDef.FirstThingCategory?.defName ?? thingDef.designationCategory?.defName).ToStringSafe();
                    IEnumerable<string> split = SplitCamelCase(category);
                    if (split.First().Equals("Buildings"))
                    {
                        category = split.Last();
                    }
                    if (!(category.ToLower().Contains("null") || category.ToLower().Contains("special") || category.ToLower().Contains("ship") || category.ToLower().Contains("structure")))
                    {
                        if (!inventory.ContainsKey(category))
                        {
                            inventory.Add(category, new List<Thing>());
                        }

                        inventory.TryGetValue(category).Add(thingDef.MadeFromStuff
                            ? ThingMaker.MakeThing(thingDef, GenStuff.DefaultStuffFor(thingDef)) //GenStuff.RandomStuffByCommonalityFor(thingDef))
                            : ThingMaker.MakeThing(thingDef));
                    }
                }
            }    
            // Animals
            List<PawnKindDef> kinds = DefDatabase<PawnKindDef>.AllDefs
                .Where((PawnKindDef k) => k.race.race.wildness == 0 && k.race.race.Animal && Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(map.Tile, k.race)).ToList();
            List<Thing> animals = new List<Thing>();
            foreach (var kinddef in kinds)
            {
                var animalGen = new PawnGenerationRequest(kinddef, null, PawnGenerationContext.NonPlayer, map.Tile);
                animalGen.FixedGender = Gender.Male;
                animals.Add(PawnGenerator.GeneratePawn(animalGen));
                animalGen.FixedGender = Gender.Female;
                animals.Add(PawnGenerator.GeneratePawn(animalGen));
            }
            inventory.Add("Animals", animals);
            //inventory.Add("debug", new List<Thing>());
            currentCategory = inventory.Keys.First();
        }

        private static IEnumerable<string> SplitCamelCase(string source)
        {
            const string pattern = @"[A-Z][a-z]*|[a-z]+|\d+";
            var matches = Regex.Matches(source, pattern);
            foreach (Match match in matches)
            {
                yield return match.Value;
            }
        }
    }
    

}