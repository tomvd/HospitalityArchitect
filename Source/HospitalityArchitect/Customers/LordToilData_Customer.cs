using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace HospitalityArchitect
{
    internal class LordToilData_Customer : LordToilData
    {
        public float radius;


        public override void ExposeData()
        {
            Scribe_Values.Look(ref radius, "radius", 100f);
        }
    }

    internal class LordToil_Customer : LordToil
    {
        public LordToilData_Customer Data => (LordToilData_Customer) data;

        public LordToil_Customer()
        {
            data = new LordToilData_Customer();
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
            var hospital = Map.GetComponent<CustomerService>();
            foreach (var pawn in lord.ownedPawns.ToArray())
            {
                hospital.CustomerLeaves(pawn);
            }
        }
        public override void UpdateAllDuties()
        {
            foreach (Pawn pawn in lord.ownedPawns)
            {
                CustomerUtility.SetCustomerDuty(pawn);
            }
        }
    }
}
