using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using RT.UItils;

namespace RT
{
    public class Dialog_Hire : Window
    {
        private          float                                      availableSilver;
        private readonly Hireable                                   hireable;
        private readonly Dictionary<PawnKindDef, Pair<int, string>> hireData;
        private readonly Map                                        targetMap;
        private          Vector2               pawnsScrollPos = new Vector2(0, 0);

        public Dialog_Hire(Thing negotiator, Hireable hireable)
        {
            targetMap     = negotiator.Map;
            this.hireable = hireable;
            hireData      = hireable.SelectMany(def => def.pawnKinds).ToDictionary(def => def, _ => new Pair<int, string>(0, ""));
            closeOnCancel = true;
            forcePause    = true;
            closeOnAccept = true;
            availableSilver = targetMap.listerThings.ThingsOfDef(ThingDefOf.Silver)
                                    .Where(x => !x.Position.Fogged(x.Map) && (targetMap.areaManager.Home[x.Position] || x.IsInAnyStorage())).Sum(t => t.stackCount);
        }

        public override    Vector2 InitialSize => new Vector2(750f, 650f);
        protected override float   Margin      => 15f;

        private float CostPawns(ICollection<PawnKindDef> except = null) =>
            hireData.Select(kv => new Pair<PawnKindDef, int>(kv.Key, kv.Value.First)).Where(pair => pair.Second > 0 && (except == null || !except.Contains(pair.First)))
                 .Sum(pair => pair.Second * pair.First.combatPower); // calculate $ per hour

        private void OnHireKeyPressed(HireableFactionDef def, PawnKindDef pawnKindDef, int contractType)
        {
            //base.OnAcceptKeyPressed();
            SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();

            //if (hoursAmount > 0 && hireData.Any(kvp => kvp.Value.First > 0))
            //{
                //var pawns = new List<Pawn>();

                var hiringCost = Mathf.RoundToInt(pawnKindDef.combatPower); // 1 hour charged
                if (contractType == 1) hiringCost *= 8; // 1 day charged

                Find.World.GetComponent<FinanceTracker>().bookHiringExpenses(targetMap, hiringCost);
                this.availableSilver -= hiringCost;

                if (!RCellFinder.TryFindRandomPawnEntryCell(out var cell, targetMap, 1f))
                    cell = CellFinder.RandomEdgeCell(targetMap);

//                foreach (var kvp in hireData)
                    //for (var i = 0; i < kvp.Value.First; i++)
                    //{
                        var flag = pawnKindDef.ignoreFactionApparelStuffRequirements;
                        pawnKindDef.ignoreFactionApparelStuffRequirements = true;
                        var pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKindDef, faction: Faction.OfPlayer));
                        pawnKindDef.ignoreFactionApparelStuffRequirements = flag;
                        //pawn.playerSettings.hostilityResponse         = HostilityResponseMode.Attack;
                        // pawns.Add(pawn);
                        // TODO - wander in from the side of the map?
                        var loc = DropCellFinder.TryFindSafeLandingSpotCloseToColony(targetMap, IntVec2.Two);
                        var activeDropPodInfo = new ActiveDropPodInfo();
                        activeDropPodInfo.innerContainer.TryAdd(pawn, 1);
                        activeDropPodInfo.openDelay                     = 60;
                        activeDropPodInfo.leaveSlag                     = false;
                        activeDropPodInfo.despawnPodBeforeSpawningThing = true;
                        activeDropPodInfo.spawnWipeMode                 = WipeMode.Vanish;
                        DropPodUtility.MakeDropPodAt(loc, targetMap, activeDropPodInfo);
                    //}

                Find.World.GetComponent<HiringContractTracker>().SetNewContract(contractType, pawn, hireable, def, targetMap);
            //}
        }

        public override void DoWindowContents(Rect inRect)
        {
            var rect   = new Rect(inRect);
            var anchor = Text.Anchor;
            var font   = Text.Font;
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font   = GameFont.Medium;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 40f), hireable.GetCallLabel());
            Text.Font =  GameFont.Small;
            rect.yMin += 40f;
            Widgets.Label(new Rect(rect.x, rect.y, rect.width, 20f), "VEF.AvailableSilver".Translate(availableSilver.ToStringMoney()));
            rect.yMin += 30f;
            
            foreach (var def in hireable) DoHireableFaction(ref rect, def);

            DoCurrentContracts(ref rect);
            
            if (Widgets.ButtonText(rect.TakeRightPart(120f).BottomPartPixels(40f), "Close".Translate())) OnCancelKeyPressed();
            //rect.yMin += 40f;
            /*
            Text.Font = GameFont.Tiny;
            Widgets.Label(rect.ContractedBy(30f, 0f), "VEF.HiringDesc".Translate(hireable.Key).Colorize(ColoredText.SubtleGrayColor));*/
            Text.Anchor = anchor;
            Text.Font   = font;
        }

        private void DoHireableFaction(ref Rect inRect, HireableFactionDef def)
        {
            var rect = inRect.TopPartPixels(Mathf.Max(20f + def.pawnKinds.Count * 30f, 120f));
            inRect.yMin += rect.height;
            var titleRect = rect.TakeTopPart(20f);
            var iconRect  = rect.LeftPartPixels(105f).ContractedBy(5f);
            titleRect.x += 115f;
            Text.Anchor =  TextAnchor.MiddleLeft;
            Text.Font   =  GameFont.Tiny;
            var nameRect = new Rect(titleRect);
            Widgets.Label(titleRect, "VEF.Hire".Translate(def.LabelCap));
            titleRect.x     += 200f;
            titleRect.width =  120f;
            Text.Anchor     =  TextAnchor.MiddleCenter;
            var valueRect = new Rect(titleRect);
            Widgets.Label(titleRect, "hourly/daily".Translate());
            titleRect.x     += 100f;
            titleRect.width =  300f;
            var numRect = new Rect(titleRect);
            Text.Font = GameFont.Tiny;
            Widgets.Label(titleRect, "VEF.ChooseNumberOfUnits".Translate().Colorize(ColoredText.SubtleGrayColor));
            Text.Font = GameFont.Small;
            Widgets.DrawLightHighlight(iconRect);
            GUI.color = def.color;
            Widgets.DrawTextureFitted(iconRect, def.Texture, 1f);
            GUI.color = Color.white;
            var highlight = true;
            foreach (var kind in def.pawnKinds)
            {
                nameRect.y  += 20f;
                valueRect.y += 20f;
                numRect.y   += 20f;
                var fullRect = new Rect(nameRect.x - 4f, nameRect.y, nameRect.width + valueRect.width + numRect.width, 20f);
                if (highlight) Widgets.DrawHighlight(fullRect);
                highlight   = !highlight;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(nameRect, kind.LabelCap);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(valueRect, kind.combatPower.ToStringMoney() +"/" + (kind.combatPower*8).ToStringMoney());
                /*var data   = hireData[kind];
                var amount = data.First;
                var buffer = data.Second;
                UIUtility.DrawCountAdjuster(ref amount, numRect, ref buffer, 0, 99, curFaction != null && curFaction != def, null, Mathf.Max(Mathf.FloorToInt(Mathf.Pow(
                                             (availableSilver / CostDays - CostPawns(new HashSet<PawnKindDef> { kind })) /
                                             kind.combatPower, 1f / 1.2f)), 0));
                if (amount != data.First || buffer != data.Second)
                {
                    hireData[kind] = new Pair<int, string>(amount, buffer);
                    if (amount > 0  && curFaction == null) curFaction                                                    = def;
                    if (amount == 0 && curFaction == def && def.pawnKinds.All(pk => hireData[pk].First == 0)) curFaction = null;
                }*/
                if (Widgets.ButtonText(numRect.LeftHalf(), "Short".Translate()))
                {
                    if (kind.combatPower > availableSilver)
                        Messages.Message("NotEnoughSilver".Translate(), MessageTypeDefOf.RejectInput);
                    else
                        OnHireKeyPressed(def, kind, 0);
                }
                if (Widgets.ButtonText(numRect.RightHalf(), "Long".Translate()))
                {
                    if ((kind.combatPower*8) > availableSilver)
                        Messages.Message("NotEnoughSilver".Translate(), MessageTypeDefOf.RejectInput);
                    else
                        OnHireKeyPressed(def, kind, 1);
                }
            }
        }

        private void DoCurrentContracts(ref Rect inRect)
        {
            List<Contract> contracts = Find.World.GetComponent<HiringContractTracker>().contracts;

            var rect = inRect.TopPartPixels(Mathf.Max(20f + contracts.Count * 30f, 120f));
            inRect.yMin += rect.height;
            var titleRect = rect.TakeTopPart(20f);
            titleRect.x += 115f;
            Text.Anchor =  TextAnchor.MiddleLeft;
            Text.Font   =  GameFont.Tiny;
            var nameRect = new Rect(titleRect);
            Widgets.Label(titleRect, "Name".Translate());
            titleRect.x     += 200f;
            titleRect.width =  120f;
            Text.Anchor     =  TextAnchor.MiddleCenter;
            var valueRect = new Rect(titleRect);
            Widgets.Label(titleRect, "VEF.TimeLeft".Translate());
            titleRect.x     += 100f;
            titleRect.width =  300f;
            var numRect = new Rect(titleRect);
            Text.Font = GameFont.Tiny;
            Widgets.Label(titleRect, "Cancel".Translate().Colorize(ColoredText.SubtleGrayColor));
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            var highlight = true;
            foreach (var contract in Find.World.GetComponent<HiringContractTracker>().contracts)
            {
                nameRect.y  += 20f;
                valueRect.y += 20f;
                numRect.y   += 20f;
                var fullRect = new Rect(nameRect.x - 4f, nameRect.y, nameRect.width + valueRect.width + numRect.width, 20f);
                if (highlight) Widgets.DrawHighlight(fullRect);
                highlight   = !highlight;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(nameRect, contract.pawn.NameShortColored + ", " +contract.pawn.story.Adulthood.TitleFor(contract.pawn.gender));
                Text.Anchor = TextAnchor.MiddleCenter;
                
                int remainingTicks = (contract.endTicks - Find.TickManager.TicksAbs);
                Widgets.Label(valueRect, (remainingTicks < 0 ? 0 : remainingTicks).ToStringTicksToPeriodVerbose().Colorize(ColoredText.DateTimeColor));
                if (Widgets.ButtonText(numRect, "VEF.CancelContract".Translate()))
                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("Are you sure?".Translate(), () =>
                                                                                                        {
                                                                                                            //Close();
                                                                                                            Find.World.GetComponent<HiringContractTracker>().EndContract(contract);
                                                                                                        }, true, "VEF.CancelContract".Translate()));
            }
        }
    }
}