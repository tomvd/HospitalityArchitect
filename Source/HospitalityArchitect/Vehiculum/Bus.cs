using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RT.DeliverySystem;

/*
* this is a vehicle that transports pawns 
*/
public class Bus : Vehiculum, IThingHolder
{
    public Bus() => this.innerContainer = (ThingOwner)new ThingOwner<Pawn>((IThingHolder)this);
    
    public override void ExposeData()
    {
        base.ExposeData();
    }
    
    public void GetChildHolders(List<IThingHolder> outChildren)
    {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, (IList<Thing>) this.GetDirectlyHeldThings());
    }

    public ThingOwner GetDirectlyHeldThings()
    {
        return innerContainer;
    }
    
}