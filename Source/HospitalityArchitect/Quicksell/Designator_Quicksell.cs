using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace HospitalityArchitect;

public class Designator_Quicksell : Designator
{
        public Designator_Quicksell()
        {
            this.defaultLabel = "Quicksell";
            this.defaultDesc = "Sell item immediatly at 50% of market value.";
            this.icon = ContentFinder<Texture2D>.Get("UI/Designators/Claim", true);
            this.soundDragSustain = SoundDefOf.Designate_DragStandard;
            this.soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            this.useMouseIcon = true;
            this.soundSucceeded = SoundDefOf.Designate_Claim;
            //this.hotKey = KeyBindingDefOf.Command_ItemForbid;
            this.hasDesignateAllFloatMenuOption = true;
            //this.designateAllLabel = "Forbid all blueprints";
        }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            if (t.def.category != ThingCategory.Item || t.IsForbidden(Faction.OfPlayer))
            {
                return false;
            }
            return true;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(base.Map) || c.Fogged(base.Map))
            {
                return false;
            }
            if (!c.GetThingList(base.Map).Any((Thing t) => CanDesignateThing(t).Accepted))
            {
                return "MessageMustDesignateSellable";
            }
            return true;
        }
        public override void DesignateThing(Thing t)
        {
            Map.GetComponent<FinanceService>().doAndBookIncome(FinanceReport.ReportEntryType.Sales,(t.MarketValue/2f)*t.stackCount);
            t.Destroy(DestroyMode.Refund);
        }
        public override void DesignateSingleCell(IntVec3 c)
        {
            List<Thing> thingList = c.GetThingList(base.Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                if (CanDesignateThing(thingList[i]).Accepted)
                {
                    DesignateThing(thingList[i]);
                }
            }
        }
}