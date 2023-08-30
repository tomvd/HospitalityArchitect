using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace RT.DeliverySystem
{
    /*
* this is a vehicle that can hold stuff and drop it when it does the delivery 
*/
    public class DeliveryVehicle : Vehiculum, IThingHolder
    {
        // public static readonly IntVec3 North = new IntVec3(0, 0, 1);
        // public static readonly IntVec3 East = new IntVec3(1, 0, 0);
        private Vector3[] itemPos = new[]
        {
            new Vector3(-0.5f, 0f, 2.5f), new Vector3(0.5f, 0, 2.5f),
            new Vector3(-0.5f, 0f, 1.5f), new Vector3(0.5f, 0, 1.5f),
            new Vector3(-0.5f, 0f, 0.5f), new Vector3(0.5f, 0, 0.5f)
        };

        public DeliveryVehicle() => this.innerContainer = (ThingOwner)new ThingOwner<Thing>((IThingHolder)this);

        public override void ExposeData()
        {
            base.ExposeData();
        }

        public override void Draw()
        {
            base.Draw();
            int pos = 0;
            foreach (var thing in innerContainer)
            {
                if (thing.def.Minifiable)
                    thing.MakeMinified().DrawAt(DrawPos + itemPos[pos]); // opposite rotation, a vehicle is drawn upside down
                else
                    thing.DrawAt(DrawPos + itemPos[pos]); // opposite rotation, a vehicle is drawn upside down
                //thing.Graphic.Draw(DrawPos + itemPos[pos], Rotation.Opposite, thing);
                if (++pos == 6) break;
            }
        }

        /*protected override void Impact()
        {
            for (int i = 0; i < 6; i++)
            {
                FleckMaker.ThrowDustPuff(base.Position.ToVector3Shifted() + Gen.RandomHorizontalVector(1f), base.Map, 1.2f);
            }
            FleckMaker.ThrowLightningGlow(base.Position.ToVector3Shifted(), base.Map, 2f);
            GenClamor.DoClamor(this, 15f, ClamorDefOf.Impact);
            base.Impact();
        }*/
        
        //private Thing GetThingForGraphic() => this.innerContainer.Any ? this.innerContainer[0] : null;
        
        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, (IList<Thing>) this.GetDirectlyHeldThings());
        }

        public ThingOwner GetDirectlyHeldThings()
        {
            return innerContainer;
        }
        
    }
}