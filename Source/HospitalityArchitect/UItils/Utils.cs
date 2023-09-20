using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace HospitalityArchitect
{
    public static class Utils
    {
        public static float Wage(this Pawn pawn)
        {
            return pawn.MarketValue / 100;
        }

        public static float GetLandValue(Map cachedMap, IntVec3 intVec3)
        {
            var terrain = cachedMap.terrainGrid.TerrainAt(intVec3);
            if (terrain.IsWater) return 1f;
            float cost = 1f + terrain.fertility;
            List<Thing> thingList = intVec3.GetThingList(cachedMap);
            foreach (var thing in thingList)
            {
                if (thing.def == ThingDefOf.SteamGeyser) cost += 100f;
                if (thing.def.mineable)
                {
                    cost += Mathf.CeilToInt(thing.def.building.mineableThing.BaseMarketValue * (thing.def.building.mineableYield/2f));
                }

                if (thing.def.plant is { IsTree: true })
                {
                    cost += 10f;
                }
            }
            return cost;
        }
    }
}