using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace HospitalityArchitect
{
    public class Order : IExposable
    {
        public List<Thing> cart = new List<Thing>();
        private int deliveryType; // 0=normal 1=express
        public int deliveryAtTick = 0;
        public float subtotal;
        public float deliverycost;
        public float total;
        public int DeliveryType
        {
            get => deliveryType;
            set
            {
                deliveryType = value;
                recalculateTotal();
            }
        }
        
        public void recalculateTotal()
        {
            subtotal = 0;
            foreach (var thing in cart.ToList())
            {
                subtotal += thing.MarketValue * thing.stackCount;
            }

            deliverycost = 0;
            switch (deliveryType)
            {
                case 0:
                    deliverycost += 5;
                    break;
                case 1:
                    deliverycost += 50;
                    break;
            }

            total = subtotal + deliverycost;
        }
        
        public void AddToCart(Thing shopThing, bool fullStack)
        {
            Thing cartThing;
            if (shopThing is Pawn pawn)
            {
                var animalGen = new PawnGenerationRequest(pawn.kindDef, Faction.OfPlayer, PawnGenerationContext.NonPlayer, pawn.Tile);
                animalGen.FixedGender = pawn.gender;
                cartThing = PawnGenerator.GeneratePawn(animalGen);
            }
            else
            {
                cartThing = ThingMaker.MakeThing(shopThing.def, shopThing.Stuff);
                CompQuality compQuality = cartThing.TryGetComp<CompQuality>();
                compQuality?.SetQuality(shopThing.TryGetComp<CompQuality>().Quality, ArtGenerationContext.Outsider);
                cartThing = cartThing.TryMakeMinified();
            }

            if (fullStack)
            {
                cartThing.stackCount = cartThing.def.stackLimit;
                cart.Add(cartThing);                
            }
            else
            {
                // see if we can merge it with same nonfullstack item first
                var cartIndex = cart.FindIndex(thing => thing.def.category == ThingCategory.Item && thing.def == cartThing.def && thing.Stuff == cartThing.Stuff && thing.stackCount < thing.def.stackLimit);
                if (cartIndex >= 0)
                {
                    cart[cartIndex].stackCount++;
                }
                else
                {
                    cart.Add(cartThing);
                }
            }
            recalculateTotal();
        }
        
        public void RemoveFromCart(Thing thing)
        {
            cart.Remove(thing);
            recalculateTotal();
        }
            
        public void ExposeData()
        {
            Scribe_Values.Look(ref deliveryType, "deliveryType");
            Scribe_Values.Look(ref deliveryAtTick, "deliveryAtTick");
            Scribe_Values.Look(ref total, "total");
            Scribe_Collections.Look(ref cart, "cart", LookMode.Deep);
            if (cart is null) cart = new List<Thing>();
        }
    }
}