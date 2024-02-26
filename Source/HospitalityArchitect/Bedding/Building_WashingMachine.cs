using System.Collections.Generic;
using System.Linq;
using System.Text;
using DubsBadHygiene;
using RimWorld;
using Verse;
using Verse.Sound;

namespace HospitalityArchitect;

[StaticConstructorOnStartup]
public class Building_WashingMachine : Building, IStoreSettingsParent, IThingHolder
{
	public ThingOwner innerContainer;

	private int lastTickUsed;

	public CompPipe PipeComp;

	public CompPowerTrader powerComp;

	public int SpinCycleTicksLeft;

	public int storageCap = 10;

	private StorageSettings storageSettings;

	private Sustainer wickSustainer;

	public bool MustUnload
	{
		get
		{
			if (SpinCycleTicksLeft <= 0)
			{
				return innerContainer.Count > 0;
			}
			return false;
		}
	}

	public bool HasSpace
	{
		get
		{
			Thing dirtyBedding = innerContainer.FirstOrDefault();
			if (dirtyBedding == null) return true;
			if (dirtyBedding.stackCount < storageCap)
			{
				return !MustUnload;
			}
			return false;
		}
	}

	public bool StorageTabVisible => true;

	public Building_WashingMachine()
	{
		innerContainer = new ThingOwner<Thing>(this, oneStackOnly: false);
	}

	public void Notify_SettingsChanged()
	{
	}

	public StorageSettings GetStoreSettings()
	{
		return storageSettings;
	}

	public StorageSettings GetParentStoreSettings()
	{
		return def.building.fixedStorageSettings;
	}

	public ThingOwner GetDirectlyHeldThings()
	{
		return innerContainer;
	}

	public void GetChildHolders(List<IThingHolder> outChildren)
	{
		ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
	}

	public void StartWickSustainer()
	{
		SoundInfo info = SoundInfo.InMap(this, MaintenanceType.PerTick);
		wickSustainer = DubDef.washingmachine.TrySpawnSustainer(info);
	}

	public override void PostMake()
	{
		base.PostMake();
		storageSettings = new StorageSettings(this);
		if (def.building.defaultStorageSettings != null)
		{
			storageSettings.CopyFrom(def.building.defaultStorageSettings);
		}
	}

	public virtual AcceptanceReport Working()
	{
		if (PipeComp.pipeNet == null)
		{
			return "Nowater".Translate();
		}
		if (!(PipeComp.pipeNet.WaterTowers.Sum((CompWaterStorage x) => x.WaterStorage) > 10f))
		{
			return "Nowater".Translate();
		}
		if (!PipeComp.pipeNet.Sewers.Any((CompSewageHandler x) => !x.Blocked))
		{
			return "Nosewage".Translate();
		}
		if (this.IsBrokenDown())
		{
			return false;
		}
		if (powerComp != null && !powerComp.PowerOn)
		{
			return "NoPower".Translate();
		}
		return true;
	}

	public override void SpawnSetup(Map map, bool respawningAfterLoad)
	{
		base.SpawnSetup(map, respawningAfterLoad);
		PipeComp = GetComps<CompPipe>().FirstOrDefault();
		powerComp = GetComp<CompPowerTrader>();
	}

	public override void ExposeData()
	{
		base.ExposeData();
		Scribe_Deep.Look(ref storageSettings, "storageSettings", this);
		Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
	}

	public void LoadWashing(Thing moarDirtyBedding)
	{
		Thing dirtyBedding = innerContainer.FirstOrDefault();
		if (dirtyBedding is null)
		{
			if (moarDirtyBedding.stackCount > storageCap)
			{
				Thing enoughToFit = moarDirtyBedding.SplitOff(storageCap);
				GenPlace.TryPlaceThing(moarDirtyBedding, InteractionCell, Map, ThingPlaceMode.Near, null, NearPlaceValidator);
				innerContainer.TryAddOrTransfer(enoughToFit, storageCap, canMergeWithExistingStacks: true);
			}
			else 
				innerContainer.TryAddOrTransfer(moarDirtyBedding, canMergeWithExistingStacks: true);
		}
		else
		{
			int alreadyIn = dirtyBedding.stackCount;
			if (alreadyIn >= storageCap) return; // full!
			if (moarDirtyBedding.stackCount > storageCap - alreadyIn)
			{
				// bringing more than fits - split of
				Thing enoughToFit = moarDirtyBedding.SplitOff(storageCap - alreadyIn);
				GenPlace.TryPlaceThing(moarDirtyBedding, InteractionCell, Map, ThingPlaceMode.Near, null,
					NearPlaceValidator); // place leftover next to the washing machine
				innerContainer.TryAddOrTransfer(enoughToFit, canMergeWithExistingStacks: true);
			}
			else
			{
				innerContainer.TryAddOrTransfer(moarDirtyBedding, canMergeWithExistingStacks: true);
			}
		}
		SpinCycleTicksLeft += 1500;
	}

	public Thing UnloadWashing()
	{
		SpinCycleTicksLeft = 0;
		Thing dirtyBedding = innerContainer.FirstOrDefault();
		if (dirtyBedding is null) return null;
		
		Thing thing = ThingMaker.MakeThing(HADefOf.CleanBedding, null);
		thing.stackCount = dirtyBedding.stackCount;
		GenPlace.TryPlaceThing(thing, InteractionCell, Map, ThingPlaceMode.Near, null, NearPlaceValidator);
		dirtyBedding.Destroy();
		return thing;
		/*
		foreach (Apparel item in innerContainer.OfType<Apparel>())
		{
			item.wornByCorpseInt = false;
		}
		Apparel thing = innerContainer.FirstOrDefault() as Apparel;
		innerContainer.TryDrop(thing, ThingPlaceMode.Near, out var lastResultingThing);
		innerContainer.TryDropAll(InteractionCell, base.Map, ThingPlaceMode.Near, null, NearPlaceValidator);
		return lastResultingThing;*/
	}

	private bool NearPlaceValidator(IntVec3 c)
	{
		return c.GetRoom(base.Map) == this.GetRoom();
	}

	public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
	{
		if (mode == DestroyMode.Deconstruct || mode == DestroyMode.Refund)
		{
			innerContainer.TryDropAll(base.Position, base.Map, ThingPlaceMode.Near);
		}
		base.DeSpawn(mode);
	}
/*
	public override IEnumerable<Gizmo> GetGizmos()
	{
		foreach (Gizmo gizmo in base.GetGizmos())
		{
			yield return gizmo;
		}
		if (!StorageTabVisible)
		{
			yield break;
		}
		foreach (Gizmo item in StorageSettingsClipboard.CopyPasteGizmosFor(storageSettings))
		{
			yield return item;
		}
	}
*/
	public override void Tick()
	{
		base.Tick();
		powerComp.PowerOutput = -10f;
		if (SpinCycleTicksLeft > 0 && this.IsHashIntervalTick(30) && Working().Accepted && PipeComp.pipeNet.PullWater(1f, out var _))
		{
			PipeComp.pipeNet.PushSewage(0.5f);
			lastTickUsed = Find.TickManager.TicksGame;
			SpinCycleTicksLeft -= 30;
		}
		if (Find.TickManager.TicksGame < lastTickUsed + 100)
		{
			powerComp.PowerOutput = 0f - powerComp.Props.basePowerConsumption;
			if (wickSustainer == null)
			{
				StartWickSustainer();
			}
			else if (wickSustainer.Ended)
			{
				StartWickSustainer();
			}
			else
			{
				wickSustainer.Maintain();
			}
		}
	}

	public override string GetInspectString()
	{
		if (base.ParentHolder is MinifiedThing)
		{
			return base.GetInspectString();
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(base.GetInspectString());
		if (SpinCycleTicksLeft > 0)
		{
			stringBuilder.AppendLine("WashingMachineSpinning".Translate());
		}
		else if (MustUnload)
		{
			stringBuilder.AppendLine("WashingMachineUnload".Translate());
		}
		Thing dirtyBedding = innerContainer.FirstOrDefault();
		int cnt = 0;
		if (dirtyBedding is not null)
		{
			cnt = dirtyBedding.stackCount;
		}
		stringBuilder.AppendLine("WashingMachineLoad".Translate(cnt, storageCap));
		if (!Working().Accepted)
		{
			stringBuilder.AppendLine(Working().Reason);
		}
		return stringBuilder.ToString().TrimEndNewlines();
	}
}