using System;
using System.Collections.Generic;
using System.Linq;
using Hospitality;
using HospitalityArchitect;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace HospitalityArchitect
{
    public static class CustomerUtility
    {
        public static bool IsCustomer(this Pawn pawn,out CustomerService shop)
        {
            shop = null;
            if (pawn?.Map == null) return false;

            shop = pawn.Map.GetComponent<CustomerService>(); // TODO cache this?
            return shop?.Customers.ContainsKey(pawn) == true;
        }

        public static void SetUpNewCustomer(Pawn pawn)
        {
            /*
             * give customers money - TODO
             */
            var amountS = 100;
            if (amountS > 0)
            {
                var money = ThingMaker.MakeThing(ThingDefOf.Silver);
                money.stackCount = amountS;

                var spaceFor = pawn.GetInventorySpaceFor(money);
                if (spaceFor > 0)
                {
                    money.stackCount = Mathf.Min(spaceFor, amountS);
                    var success = pawn.inventory.innerContainer.TryAdd(money);
                }
            }
        }

        public static void SetCustomerDuty(Pawn pawn)
        {
            CustomerService shop;
            if (pawn.IsCustomer(out shop))
            {
                if (shop.Customers[pawn].Type.Equals(CustomerType.Gambling) || shop.Customers[pawn].Type.Equals(CustomerType.Wellness))
                    pawn.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("HaveJoy"), pawn.Position, 100);
                if (shop.Customers[pawn].Type.Equals(CustomerType.Shopping))
                    pawn.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("DoShopping"), pawn.Position, 100);
                if (shop.Customers[pawn].Type.Equals(CustomerType.Dining))
                    pawn.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("HaveDinner"), pawn.Position, 100);
            }
        }
    }
}