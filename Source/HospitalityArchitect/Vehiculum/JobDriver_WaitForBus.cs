using System.Collections.Generic;
using RimWorld;
using Storefront.Utilities;
using Verse;
using Verse.AI;
using Hospitality;
using Hospitality.Utilities;

namespace HospitalityArchitect
{
   
    public class JobDriver_WaitForBus : JobDriver
    {
        
        private TargetIndex WaitCellInd = TargetIndex.A;
      
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
                if (pawn.IsGuest())
                {
                    pawn.WearHeadgear();
                }
                yield return Toils_Goto.GotoCell(WaitCellInd, PathEndMode.ClosestTouch);
                yield return Toils_General.Wait(GenDate.TicksPerHour * 2);
        }
    }
}
