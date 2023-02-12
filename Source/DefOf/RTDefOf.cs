using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.AI;

namespace RT
{
    [DefOf]
    public static class RTDefOf
    {
        public static JobDef RT_LeaveMap;
        public static DutyDef RT_StealColony;
        public static DutyDef RT_BurnColony;
        public static JobDef RT_IgniteWithTorches;
    }
}