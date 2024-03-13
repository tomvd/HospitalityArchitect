using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace HospitalityArchitect
{
    public class Building_Computer : Building_ResearchBench, IBillGiver, IBillGiverWithTickAction
    {
    public BillStack billStack;

	private CompPowerTrader powerComp;

	private CompRefuelable refuelableComp;

	private CompBreakdownable breakdownableComp;

	private CompMoteEmitter moteEmitterComp;

	public bool CanWorkWithoutPower
	{
		get
		{
			if (powerComp == null)
			{
				return true;
			}
			if (def.building.unpoweredWorkTableWorkSpeedFactor > 0f)
			{
				return true;
			}
			return false;
		}
	}

	public bool CanWorkWithoutFuel => refuelableComp == null;

	public BillStack BillStack => billStack;

	public IntVec3 BillInteractionCell => InteractionCell;

	public IEnumerable<IntVec3> IngredientStackCells => GenAdj.CellsOccupiedBy(this);

	public Building_Computer()
	{
		billStack = new BillStack(this);
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Deep.Look(ref billStack, "billStack", this);
	}



	public virtual void UsedThisTick()
	{
		if (refuelableComp != null)
		{
			refuelableComp.Notify_UsedThisTick();
		}
		if (moteEmitterComp != null)
		{
			if (!moteEmitterComp.MoteLive)
			{
				moteEmitterComp.Emit();
			}
			moteEmitterComp.Maintain();
		}
	}

	public bool CurrentlyUsableForBills()
	{
		if (!UsableForBillsAfterFueling())
		{
			return false;
		}
		if (!CanWorkWithoutPower && (powerComp == null || !powerComp.PowerOn))
		{
			return false;
		}
		if (!CanWorkWithoutFuel && (refuelableComp == null || !refuelableComp.HasFuel))
		{
			return false;
		}
		return true;
	}

	public bool UsableForBillsAfterFueling()
	{
		if (!CanWorkWithoutPower && (powerComp == null || !powerComp.PowerOn))
		{
			return false;
		}
		if (breakdownableComp != null && breakdownableComp.BrokenDown)
		{
			return false;
		}
		return true;
	}

	public virtual void Notify_BillDeleted(Bill bill)
	{
	}
        public bool CanUseComputerNow
        {
            get
            {
                if (Spawned && Map.gameConditionManager.ElectricityDisabled)
                    return false;
                return powerComp == null || powerComp.PowerOn;
            }
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            powerComp = GetComp<CompPowerTrader>();
            
	        refuelableComp = GetComp<CompRefuelable>();
	        breakdownableComp = GetComp<CompBreakdownable>();
	        moteEmitterComp = GetComp<CompMoteEmitter>();
	        foreach (Bill item in billStack)
	        {
		        item.ValidateSettings();
	        }
        }

        private void UseAct(Pawn myPawn, ICommunicable commTarget)
        {
            var job = JobMaker.MakeJob(HADefOf.HA_WatchYoutube, (LocalTargetInfo)(Thing)this);
            job.commTarget = commTarget;
            myPawn.jobs.TryTakeOrderedJob(job);
        }

        private FloatMenuOption GetFailureReason(Pawn myPawn)
        {
            if (!myPawn.CanReach((LocalTargetInfo)(Thing)this, PathEndMode.InteractionCell, Danger.Some))
                return new FloatMenuOption("CannotUseNoPath".Translate(), null);
            if (Spawned && Map.gameConditionManager.ElectricityDisabled)
                return new FloatMenuOption("CannotUseSolarFlare".Translate(), null);
            if (powerComp != null && !powerComp.PowerOn)
                return new FloatMenuOption("CannotUseNoPower".Translate(), null);
            if (!myPawn.health.capacities.CapableOf(PawnCapacityDefOf.Sight))
                return new FloatMenuOption(
                    "CannotUseReason".Translate(
                        "IncapableOfCapacity".Translate((NamedArgument)PawnCapacityDefOf.Sight.label,
                            myPawn.Named("PAWN"))), null);
            if (CanUseComputerNow)
                return null;
            Log.Error(myPawn + " could not use computer for unknown reason.");
            return new FloatMenuOption("Cannot use now", null);
        }
        
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
        {
            FloatMenuOption failureReason = GetFailureReason(myPawn);
            if (failureReason != null)
            {
                yield return failureReason;
                yield break;
            }
            /*FloatMenuOption floatMenuOption = new FloatMenuOption("Play Farming Simulator 52",
                delegate {
                Job job = JobMaker.MakeJob(HADefOf.HA_PlayGame, this);
                //job.targetB = Game;
                myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.OpeningComms, KnowledgeAmount.Total);                
            });
            if (floatMenuOption != null)
            {
                yield return floatMenuOption;
            }*/
            foreach (FloatMenuOption floatMenuOption2 in base.GetFloatMenuOptions(myPawn))
            {
                yield return floatMenuOption2;
            }
        }        
    }
}