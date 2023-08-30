namespace RT.Mapgen;

using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.BaseGen;
using UnityEngine;
using Verse;
using Verse.AI;

public class GenStep_Road : GenStep
{

	public override int SeedPart => 1187464702;

	public override void Generate(Map map, GenStepParams parms)
	{
		var startX = map.Size.x / 2;
		var endX = startX + 7;
		// build road in the middle of the map
		var cellist = map.AllCells.Where(vec3 => vec3.x >= startX && vec3.x <= endX).ToList();
		foreach (var tile in cellist)
		{
			// bulldoze
			List<Thing> thingList = map.thingGrid.ThingsListAtFast(tile);
			for (int index = 0; index < thingList.Count; ++index)
			{
				if (!thingList[index].def.IsFilth && thingList[index].def.destroyable)
					thingList[index].Destroy();
			}
			// place concrete
			map.terrainGrid.SetTerrain(tile, TerrainDefOf.Concrete);
			// draw lines
			if (tile.x == startX || tile.x == endX)
			{
				GenSpawn.Spawn(ThingMaker.MakeThing((ThingDef)RTDefOf.RT_DecalLineThin), tile, map, tile.x == startX?Rot4.East:Rot4.West);
			}
		}
		// place concrete slab for the delivery area
		CellRect deliveryRect = CellRect.CenteredOn(new IntVec3(startX-2,0,map.Size.z / 2), 1);
		foreach (var deliveryRectCell in deliveryRect.Cells)
		{
			// bulldoze
			List<Thing> thingList = map.thingGrid.ThingsListAtFast(deliveryRectCell);
			for (int index = 0; index < thingList.Count; ++index)
			{
				if (!thingList[index].def.IsFilth && thingList[index].def.destroyable)
					thingList[index].Destroy();
			}
			// place concrete
			map.terrainGrid.SetTerrain(deliveryRectCell, TerrainDefOf.Concrete);
		}
		
	}

}
