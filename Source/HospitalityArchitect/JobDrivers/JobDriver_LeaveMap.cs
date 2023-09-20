using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace HospitalityArchitect
{
    public class JobDriver_LeaveMap : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.ClosestTouch);
            yield return new Toil
            {
                initAction = () =>
                {
                    FleckMaker.ThrowSmoke(pawn.Position.ToVector3(), Map, 2f);
                    pawn.ExitMap(false, Rot4.Random);
                    //Map.GetComponent<HiringContractService>().EndContract(pawn);
                }
            }.FailOn(() => pawn.Dead);
        }
    }
}