using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace HospitalityArchitect;

public abstract class JobDriver_Quicksell : JobDriver
{
	private float workLeft;

	private float totalNeededWork;

	protected Thing Target => job.targetA.Thing;

	protected abstract DesignationDef Designation { get; }

	protected abstract float TotalNeededWork { get; }

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Values.Look(ref workLeft, "workLeft", 0f);
		Scribe_Values.Look(ref totalNeededWork, "totalNeededWork", 0f);
	}

	public override bool TryMakePreToilReservations(bool errorOnFailed)
	{
		return pawn.Reserve(Target, job, 1, -1, null, errorOnFailed);
	}

	public override IEnumerable<Toil> MakeNewToils()
	{
		this.FailOnThingMissingDesignation(TargetIndex.A, Designation);
		this.FailOnForbidden(TargetIndex.A);
		yield return Toils_Goto.GotoThing(TargetIndex.A, (Target is Building_Trap) ? PathEndMode.OnCell : PathEndMode.Touch);
		Toil doWork = ToilMaker.MakeToil("MakeNewToils").FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
		doWork.initAction = delegate
		{
			totalNeededWork = TotalNeededWork;
			workLeft = totalNeededWork;
		};
		doWork.tickAction = delegate
		{
			TickAction();
			if (workLeft <= 0f)
			{
				doWork.actor.jobs.curDriver.ReadyForNextToil();
			}
		};
		doWork.defaultCompleteMode = ToilCompleteMode.Never;
		doWork.WithProgressBar(TargetIndex.A, () => 1f - workLeft / totalNeededWork);
		yield return doWork;
		Toil toil = ToilMaker.MakeToil("MakeNewToils");
		toil.initAction = delegate
		{
			FinishedRemoving();
			base.Map.designationManager.RemoveAllDesignationsOn(Target);
		};
		toil.defaultCompleteMode = ToilCompleteMode.Instant;
		yield return toil;
	}

	protected virtual void FinishedRemoving()
	{
	}

	protected virtual void TickAction()
	{
	}
}