using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Storefront.Utilities;
using Verse;
using Verse.AI;

namespace HospitalityArchitect.Reception
{
    public class WorkGiver_Host : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn) => pawn.GetAllReceptionsEmployed().SelectMany(r=>r.SpawnedShoppingPawns).Distinct().ToList();

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Pawn customer)) return false;

            if (pawn == t) return false;
            
            // am I standby?
            //var pawnDriver = pawn.jobs?.curDriver as JobDriver_StandBy;
            //if (pawnDriver == null) return false;

            // is there a customer waiting in the queue?
            if (customer.jobs?.curDriver is not JobDriver_CheckIn || !customer.IsWaitingInQueue()) return false;
            
            if (!customer.Spawned || customer.Dead)
            {
                Log.Message($"Sales canceled. dead? {customer.Dead} unspawned? {!customer.Spawned}");
                return false;
            }
            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Pawn customer))
            {
                Log.Message("WorkGiver_Sell customer is null");
                return null;
            }
            var driver = customer.jobs?.curDriver as JobDriver_CheckIn;
            if (driver == null)            
            {
                Log.Message("JobDriver_BuyItem customer is null");
                return null;
            }
            return JobMaker.MakeJob(HostDefOf.Reception_Host, customer, driver.job.targetA);
        }
    }
}
