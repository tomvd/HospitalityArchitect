using System.Collections.Generic;
using System.Linq;
using Hospitality.Utilities;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace HospitalityArchitect
{
    public class JobGiver_DoShopping : ThinkNode_JobGiver
    {
        DefMap<JoyGiverDef, float> joyGiverChances;

        public override float GetPriority(Pawn pawn)
        {
            if (pawn == null)
            {
                Log.ErrorOnce("pawn == null", 745743);
                return 0f;
            }

            if (pawn.needs?.joy == null)
            {
                // Apparently there are guests without joy...
                return 0f;
            }
            float curLevel = pawn.needs.joy.CurLevel;

            if (curLevel < 0.35f)
            {
                return 6f;
            }
            if(curLevel < 0.9f)
                return 1-curLevel;
            return 0f;
        }

        public override void ResolveReferences ()
        {
            joyGiverChances = new DefMap<JoyGiverDef, float>();
        }

        public override Job TryGiveJob(Pawn pawn)
        {
            if (pawn == null)
            {
                Log.ErrorOnce("pawn == null", 987272);
                return null;
            }
            if (pawn.CurJob != null)
            {
                //Log.ErrorOnce(pawn.NameStringShort+ " already has a job: "+pawn.CurJob, 4325+pawn.thingIDNumber);
                return pawn.CurJob;
            }
            if (pawn.needs == null) Log.ErrorOnce(pawn.LabelShort + " has no needs", 3463 + pawn.thingIDNumber);
            if (pawn.needs.joy == null) Log.ErrorOnce(pawn.LabelShort + " has no joy need", 8585 + pawn.thingIDNumber);
            if (pawn.skills == null) Log.ErrorOnce(pawn.LabelShort + " has no skills", 22352 + pawn.thingIDNumber);
            CustomerService customerService;
            if (pawn.IsCustomer(out customerService)) Log.ErrorOnce(pawn.LabelShort + " is no customer", 74564 + pawn.thingIDNumber);
            JoyKindDef joyKindDef = customerService.Customers[pawn].Type.Equals(CustomerType.Gambling)
                ? DefDatabase<JoyKindDef>.GetNamed("Gamble")
                : DefDatabase<JoyKindDef>.GetNamed("Hydrotherapy");
            if (pawn.needs.joy.tolerances.BoredOf(joyKindDef)) Log.ErrorOnce(pawn.LabelShort + " is bored of his joykinddef???", 74564 + pawn.thingIDNumber);

            //var allDefsListForReading = PopulateChances(pawn); // Moved to own function
            List<JoyGiverDef> allDefsListForReading = DefDatabase<JoyGiverDef>.AllDefsListForReading
                .Where(def => def.joyKind.Equals(joyKindDef) && def.Worker.CanBeGivenTo(pawn)).ToList();
            if (GetJob(pawn, allDefsListForReading, out var job)) return job;
            Log.ErrorOnce(pawn.LabelShort + " did not get a "+joyKindDef.label+" job.", 45745 + pawn.thingIDNumber);
            //CheckArea(pawn);
            return null;
        }

        private bool GetJob(Pawn pawn, List<JoyGiverDef> allDefsListForReading, out Job job)
        {
            for (int j = 0; j < joyGiverChances.Count; j++)
            {
                if (!allDefsListForReading.TryRandomElement(out var giverDef))
                {
                    //Log.ErrorOnce($"{pawn.LabelShort} could not find a joygiver. DefsCount = {allDefsListForReading.Count}", 45747 + pawn.thingIDNumber);
                    break;
                }

                job = giverDef?.Worker?.TryGiveJob(pawn);
                if (job != null)
                {
                    return true;
                }
                joyGiverChances[giverDef] = 0f;
            }

            job = null;
            return false;
        }

        private static void CheckArea(Pawn pawn)
        {
            var area = pawn.GetGuestArea();
            //if (area == null)
            //{
            //    Log.ErrorOnce(pawn.LabelShort + " has a null area!", 932463 + pawn.thingIDNumber);
            //    return;
            //}

            if(area is {TrueCount: 0})
            {
                Log.ErrorOnce(pawn.LabelShort + " has an area that is empty!", 43737 + pawn.thingIDNumber);
            }
        }
    }
}