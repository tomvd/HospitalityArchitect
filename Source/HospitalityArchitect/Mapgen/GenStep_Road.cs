using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace HospitalityArchitect;

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
				GenSpawn.Spawn(ThingMaker.MakeThing((ThingDef)HADefOf.HA_DecalLineThin), tile, map, tile.x == startX?Rot4.East:Rot4.West);
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
		// bit lower start putting 2 tents and small cabin
		/*
		var cabin = new IntVec3(startX - 10, 0, (map.Size.z / 2) + 3);
		ThingDef stuff = Rand.Element(ThingDefOf.WoodLog, ThingDefOf.Steel);
		foreach (IntVec3 corner in cellRect2.Corners)
		{
			if (corner.InBounds(thing.Map) && corner.Standable(thing.Map) && corner.GetFirstItem(thing.Map) == null && corner.GetFirstBuilding(thing.Map) == null && corner.GetFirstPawn(thing.Map) == null && !GenAdj.CellsAdjacent8Way(new TargetInfo(corner, thing.Map)).Any((IntVec3 x) => !x.InBounds(thing.Map) || !x.Walkable(thing.Map)) && corner.SupportsStructureType(thing.Map, ThingDefOf.Wall.terrainAffordanceNeeded))
			{
				Thing thing2 = ThingMaker.MakeThing(ThingDefOf.Wall, stuff);
				GenSpawn.Spawn(thing2, corner, thing.Map);
				thing2.SetFaction(thing.Faction);
				num++;
			}
		}*/
		
	}

}
