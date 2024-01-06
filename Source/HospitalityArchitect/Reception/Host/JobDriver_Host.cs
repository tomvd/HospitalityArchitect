using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace HospitalityArchitect.Reception
{
    public class JobDriver_Host : JobDriver
    {
        private TargetIndex CustomerInd = TargetIndex.A;
        private TargetIndex RegisterInd = TargetIndex.B;
        private Pawn Customer => job.GetTarget(CustomerInd).Pawn;
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if ((Customer.jobs.curDriver as JobDriver_CheckIn) == null)
            {
                Log.Message($"{Customer.NameShortColored} is not buying anything anymore.");
                return false;
            }
            return true;
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            //this.FailOnDestroyedOrNull(ItemInd);
            //this.FailOnForbidden(ItemInd);
            this.FailOnDowned(CustomerInd);
            yield return Toils_Goto.GotoThing(RegisterInd, PathEndMode.InteractionCell);
            yield return Toils_Hosting.AnnounceSelling(CustomerInd);
            yield return Toils_General.Wait(100, CustomerInd).FailOnDowned(CustomerInd);
            yield return Toils_Hosting.serveCustomer(CustomerInd, RegisterInd).PlaySoundAtStart(HostDefOf.CashRegister_Register_Kaching);
        }
    }
}
