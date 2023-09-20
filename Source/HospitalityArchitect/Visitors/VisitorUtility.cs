using System;
using System.Collections.Generic;
using System.Linq;
using HospitalityArchitect;
using RimWorld;
using UnityEngine;
using Verse;

namespace HospitalityArchitect
{
    public static class VisitorUtility
    {
        public static bool IsVisitor(this Pawn pawn,out VisitorMapComponent hospital)
        {
            hospital = null;
            if (pawn?.Map == null) return false;

            hospital = pawn.Map.GetComponent<VisitorMapComponent>(); // TODO cache this?
            return hospital?.Patients.ContainsKey(pawn) == true;
        }
    }
}