using System.Collections.Generic;
using RimWorld;
using Verse;

namespace HospitalityArchitect;

// this translates a certain thought stage into a rating
public class GuestThoughtRating : DefModExtension
{
    public List<int> ratings; // the stages need to correspond to the list here
    public List<PawnKindDef> importantTo;
    public List<PawnKindDef> notImportantTo;
}