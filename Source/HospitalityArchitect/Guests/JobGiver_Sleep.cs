using Hospitality;
using Hospitality.Utilities;
using HospitalityArchitect.Reception;
using RimWorld;
using Verse;
using Verse.AI;

namespace HospitalityArchitect;

    public class JobGiver_Sleep : ThinkNode
    {
        public override float GetPriority(Pawn pawn)
        {
            if (pawn.health.hediffSet.HasNaturallyHealingInjury() || HealthAIUtility.ShouldSeekMedicalRest(pawn)) return 6;

            if (pawn.needs?.rest == null) return 0f;
            
            if (pawn.GetComp<CompHotelGuest>().dayVisit)
            {
                return 0f; // dayvisitors dont sleep                
            }

            float curLevel = pawn.needs.rest.CurLevel;

            int hourOfDay = GenLocalDate.HourOfDay(pawn);
            if (hourOfDay < 7 || hourOfDay > 21)
            {
                curLevel -= 0.2f;
            }

            if (curLevel < 0.35f)
            {
                return 6f;
            }

            if (curLevel > 0.4f)
            {
                return 0f;
            }
            return 1-curLevel;
        }

        public override ThinkResult TryIssueJobPackage(Pawn pawn, JobIssueParams jobParams)
        {
            if (pawn.GetComp<CompHotelGuest>().dayVisit)
            {
                return ThinkResult.NoJob; // dayvisitors dont sleep -- other processes are making sure they leave when tired                
            }
            if (pawn.CurJob != null)
            {
                //Log.Message(pawn.NameStringShort + " already has a job: " + pawn.CurJob);
                return new ThinkResult(pawn.CurJob, this);
            }
            if (pawn.needs?.rest == null)
            {
                // Some races have this. It's fine.
                return ThinkResult.NoJob;
            }
            if (pawn.mindState == null)
            {
                Log.ErrorOnce(pawn.Name.ToStringShort + " has no mindstate", 23892 + pawn.thingIDNumber);
                pawn.mindState = new Pawn_MindState(pawn);
            }

            if (Find.TickManager.TicksGame - pawn.mindState.lastDisturbanceTick < 400)
            {
                Log.Message(pawn.Name.ToStringShort + " can't sleep - got disturbed");
                return ThinkResult.NoJob;
            }

            var compGuest = pawn.GetComp<CompGuest>();
            if (compGuest != null && compGuest.HasBed)
            {
                return new ThinkResult(new Job(JobDefOf.LayDown, compGuest.bed), this);
            }
            else
            {
                ReceptionController reception = pawn.GetAllOpenReceptions().RandomElement(); // TODO closestby
                return new ThinkResult(new Job(HADefOf.Reception_CheckIn, reception.Register), this);
            }
        }
    }