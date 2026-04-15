using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace SaveOurShip2
{
	public class PlaceWorker_MoveShip : PlaceWorker
	{
		public override void DrawGhost(ThingDef def, IntVec3 loc, Rot4 rot, Color ghostCol, Thing thing = null)
		{
			if (thing is ShipMoveBlueprint ship)
			{
				ship.DrawGhost(loc);
			}
		}

		public override AcceptanceReport AllowsPlacing(BuildableDef def, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			if (thing is ShipMoveBlueprint ship)
			{
				bool targetMapLarger = false; //if target map is larger, allow only up to origin map size
				Map originMap = ShipInteriorMod2.shipOriginMap;
				if (originMap != null && (originMap.Size.x < map.Size.x || originMap.Size.z < map.Size.z))
				{
					targetMapLarger = true;
				}
				AcceptanceReport result = true;
				IEnumerable<SketchEntity> entities;
				if (ship.extenderSketch != null)
					entities = ship.shipSketch.Entities.Concat(ship.extenderSketch?.Entities);
				else
					entities = ship.shipSketch.Entities;
				foreach (SketchEntity current in entities)
				{
					IntVec3 vec = loc + current.pos;
					if (!Designator_MoveGravship.IsValidCell(vec, map))
					{
                        current.DrawGhost(vec, new Color(0.8f, 0.2f, 0.2f, 0.3f));
                        result = false;
						continue;
					}
				}
				return result;
			}
			return true;
		}

        //private static AcceptanceReport IsValidCell(IntVec3 cell, Map map)
        //{
        //    if (!cell.InBounds(map))
        //    {
        //        return "GravshipOutOfBounds".Translate();
        //    }
        //    if (!cell.InBounds(map, 1) || cell.InNoBuildEdgeArea(map))
        //    {
        //        return "GravshipInNoBuildArea".Translate();
        //    }
        //    if (map.landingBlockers != null)
        //    {
        //        foreach (CellRect landingBlocker in map.landingBlockers)
        //        {
        //            if (landingBlocker.Contains(cell))
        //            {
        //                return "GravshipInBlockedArea".Translate();
        //            }
        //        }
        //    }
        //    if (cell.Roofed(map))
        //    {
        //        return "GravshipBlockedByRoof".Translate();
        //    }
        //    if (cell.Fogged(map))
        //    {
        //        return "GravshipBlockedByFog".Translate();
        //    }
        //    foreach (Thing thing in cell.GetThingList(map))
        //    {
        //        if (!thing.def.preventGravshipLandingOn)
        //        {
        //            BuildingProperties building = thing.def.building;
        //            if (building == null || building.canLandGravshipOn)
        //            {
        //                if (thing is Pawn pawn && (pawn.RaceProps.Humanlike || pawn.HostileTo(Faction.OfPlayer)))
        //                {
        //                    return "GravshipBlockedBy".Translate(pawn);
        //                }
        //                continue;
        //            }
        //        }
        //        return "GravshipBlockedBy".Translate(thing);
        //    }
        //    if (!GenConstruct.CanBuildOnTerrain(TerrainDefOf.Substructure, cell, map, Rot4.North))
        //    {
        //        return "GravshipBlockedByTerrain".Translate(cell.GetTerrain(map));
        //    }
        //    return true;
        //}

    }
}
