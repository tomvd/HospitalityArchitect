using UnityEngine;
using Verse;

namespace HospitalityArchitect;

public static class StaffUtility
{
    public static void SetUpArrivingStaff(Pawn pawn)
    {
        // reset memories
        pawn.needs.mood.thoughts.memories.memories.Clear();
        pawn.needs.rest.CurLevel = 1f;
        pawn.needs.joy.CurLevel = 1f;
        pawn.needs.food.CurLevel = 1f;
        // TODO make sure 
        /*HiringContract contract = Find.CurrentMap.GetComponent<HiringContractService>().contracts
            .FirstOrFallback(contract => contract.pawn != null && contract.pawn.Equals(pawn), null);*/
    }
}