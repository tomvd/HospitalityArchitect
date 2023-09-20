using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace HospitalityArchitect
{
    internal class LordToilData_Patient : LordToilData
    {
        public float radius;


        public override void ExposeData()
        {
            Scribe_Values.Look(ref radius, "radius", 100f);
        }
    }

    internal class LordToil_Patient : LordToil
    {
        public LordToilData_Patient Data => (LordToilData_Patient) data;

        public LordToil_Patient()
        {
            data = new LordToilData_Patient();
        }

        public override void Init()
        {
            base.Init();
            Arrive();
        }

        private void Arrive()
        {
            //Log.Message("Init State_VisitPoint "+brain.ownedPawns.Count + " - "+brain.faction.name);
        }

        public override void Cleanup()
        {
            Leave();

            base.Cleanup();
        }

        private void Leave()
        {
            var hospital = Map.GetComponent<VisitorMapComponent>();
            foreach (var pawn in lord.ownedPawns.ToArray())
            {
                hospital.PatientLeaves(pawn);
            }
        }
        public override void UpdateAllDuties()
        {
            foreach (Pawn pawn in lord.ownedPawns)
            {
                pawn.mindState.duty = new PawnDuty(DefDatabase<DutyDef>.GetNamed("Relax"), pawn.Position, Data.radius);
            }
        }
    }
}
