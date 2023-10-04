using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.Sound;

namespace HospitalityArchitect
{
    // gathers all orders and send delivery vehicles at appropriate times
    public class RimazonService : MapComponent
    {
        private VehiculumService _deliveryService;
        private FinanceService _financeService;
        private List<Order> orders = new List<Order>();
        public string categoryOnSale;
        public int salesEndsOn;

        public RimazonService(Map map) : base(map)
        {
        }

        public Order CurrentOrder
        {
            get
            {
                if (!orders.Any(order => order.deliveryAtTick == 0))
                {
                    orders.Add(new Order());
                }
                return orders[orders.Count - 1];
            }
        }

        public void DoCheckOut()
        {
            // TODO buying should place a delivery per deliverytruck (= 6 stacks/items)
            // TODO check if we can afford
            SoundDefOf.ExecuteTrade.PlayOneShotOnCamera();
            _financeService.doAndBookExpenses(FinanceReport.ReportEntryType.Rimazon, CurrentOrder.total);
            switch (CurrentOrder.DeliveryType)
            {
                case 0:
                    CurrentOrder.deliveryAtTick = Find.TickManager.TicksGame + GenDate.TicksPerHour * 10;
                    break;
                case 1:
                    CurrentOrder.deliveryAtTick = Find.TickManager.TicksGame + GenDate.TicksPerHour;
                    break;
            }

            Log.Message($"order set to arrive in {CurrentOrder.deliveryAtTick - Find.TickManager.TicksGame} ticks.");
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            for (int i = orders.Count - 1; i >= 0; i--)
            {
                if (orders[i].deliveryAtTick > 0 && Find.TickManager.TicksGame > orders[i].deliveryAtTick)
                {
                    _deliveryService.StartDelivery(orders[i].cart);
                    orders.RemoveAt(i);
                }
            }

            if (categoryOnSale is { Length: > 0 } && GenDate.TicksGame > salesEndsOn)
            {
                categoryOnSale = "";
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref orders, "orders", LookMode.Deep);
            Scribe_Values.Look(ref categoryOnSale, "categoryOnSale");
            Scribe_Values.Look(ref salesEndsOn, "salesEndsOn");
            
            if (orders is null) orders = new List<Order>();
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            _deliveryService = map.GetComponent<VehiculumService>();
            _financeService = map.GetComponent<FinanceService>();
        }
    }
}