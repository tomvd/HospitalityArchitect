using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace RT
{
	public class LordToil_StealColony : LordToil
	{
		private bool attackDownedIfStarving;

		public override bool ForceHighStoryDanger => true;

		public override bool AllowSatisfyLongNeeds => false;

		public LordToil_StealColony(bool attackDownedIfStarving = false)
		{
			this.attackDownedIfStarving = attackDownedIfStarving;
		}

		public override void Init()
		{
			base.Init();
		}

		public override void UpdateAllDuties()
		{
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
			 //Log.Message("Updating duty: " + lord.ownedPawns[i], true);
				lord.ownedPawns[i].mindState.duty = new PawnDuty(RTDefOf.RT_StealColony);
				lord.ownedPawns[i].mindState.duty.attackDownedIfStarving = attackDownedIfStarving;
			}
		}
	}
}