using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using RimWorld;
using Verse;
using LudeonTK;

namespace SaveOurShip2
{
	public static class DebugToolsSOS2
	{
		private const string sos2Category = "SOS 2";
		private static void DoEnemyPawnsAction(Action<Pawn> action)
		{
			List<Pawn> pawnsToProcess = new List<Pawn>();
			foreach (Pawn p in Find.CurrentMap.mapPawns.pawnsSpawned)
			{
				if (p.Faction.HostileTo(Faction.OfPlayer))
				{
					pawnsToProcess.Add(p);
				}
			}
			foreach (Pawn p in pawnsToProcess)
			{
				action(p);
			}
		}

		[DebugAction("Map", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		// Using mod name to prevent conflict with this command expected to be added to base game
		private static void KillAllEnemiesOnMapSos2()
		{
			DoEnemyPawnsAction((Pawn p) => p.Kill(null));
		}

		[DebugAction("Map", null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void VanishAllEnemiesOnMapSos2()
		{
			DoEnemyPawnsAction((Pawn p) => p.Destroy(DestroyMode.Vanish));
		}

		[DebugAction(sos2Category, null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void WinShipBattle()
		{
			Map playerShipMap = ShipInteriorMod2.FindPlayerShipMap();
			if (playerShipMap == null)
			{
				return;
			}
			ShipMapComp playerMapComp = playerShipMap.GetComponent<ShipMapComp>();
			if (playerMapComp.ShipMapState != ShipMapState.inCombat)
			{
				return;
			}
			ShipMapComp enemyMapComp = playerMapComp.TargetMapComp;
			if (enemyMapComp == null)
			{
				return;
			}
			List<SpaceShipCache> ships = enemyMapComp.ShipsOnMap.Values.Where(s => !s.IsWreck).ToList();
			foreach (SpaceShipCache ship in ships)
			{
				List<Building_ShipBridge> bridges = ship.Bridges.ListFullCopy();
				foreach (Building_ShipBridge bridge in bridges)
				{
					bridge.Destroy();
				}
			}
		}

		[DebugAction(sos2Category, null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void PngToShipBlueprint()
		{
			ImportShipDesign(false);
		}
		[DebugAction(sos2Category, null, false, false, false, false, false, 0, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void PngToShipBuildings()
		{
			ImportShipDesign(true);
		}

		private static bool IsRed(Color color)
		{
			return color.r > 0.98 && color.g < 0.02 && color.b < 0.02;
		}
		private static bool IsGreen(Color color)
		{
			return color.r < 0.02 && color.g > 0.98 && color.b < 0.02;
		}
		private static bool IsCorner2Here(IntVec3 pos, List<IntVec3> walls, List<IntVec3> floors)
		{
			if (walls.Contains(pos) || floors.Contains(pos))
            {
				return false;
            }
			IntVec3 check1 = new IntVec3(pos.x, pos.y, pos.z);
			IntVec3 check2 = new IntVec3(pos.x, pos.y, pos.z);
			check1.x += 1;

			check2.x += 1;
			check2.z += 1;
			if (walls.Contains(check1) && walls.Contains(check2))
            {
				return true;
            }
			return false;
		}

		private static bool IsCornerHere(IntVec3 pos, List<IntVec3> walls, List<IntVec3> floors)
		{
			if (walls.Contains(pos) || floors.Contains(pos))
			{
				return false;
			}
			IntVec3 check1 = new IntVec3(pos.x, pos.y, pos.z);
			check1.x += 1;

			if (walls.Contains(check1))
			{
				return true;
			}
			return false;
		}
		private static void ImportShipDesign(bool PlaceActualBuildings)
		{
			Map map = Find.CurrentMap;
			if (map == null)
			{
				Messages.Message("To import ship blueprint, switch from world view to loacl map view", null, MessageTypeDefOf.NeutralEvent);
				return;
			}
			string path = Path.Combine(GenFilePaths.SaveDataFolderPath, "SoS2\\Blueprint.png");
			if (!File.Exists(path))
            {
				Messages.Message("Blueprint file doesn't exist", null, MessageTypeDefOf.NeutralEvent);
				return;
			}
			byte[] fileData = File.ReadAllBytes(path);
			Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
			if (!tex.LoadImage(fileData))
            {
				tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
				if (!tex.LoadImage(fileData))
				{
					Messages.Message("Error loading blueprint image", null, MessageTypeDefOf.NeutralEvent);
				}
				return;
			}
			tex.filterMode = FilterMode.Bilinear;
			tex.wrapMode = TextureWrapMode.Clamp;
			List<IntVec3> cellsToDesignate = new List<IntVec3>();
			List<IntVec3> floorCells = new List<IntVec3>();

			for (int x = 0; x < Mathf.Min(map.Size.x, tex.width); x++)
			{
				for (int z = 0; z < Mathf.Min(map.Size.z, tex.height); z++)
				{
					Color color = tex.GetPixel(x, z);
					if (IsRed(color))
					{
						cellsToDesignate.Add(new IntVec3(x, 0, z));
					}
					if (IsGreen(color))
					{
						floorCells.Add(new IntVec3(x, 0, z));
					}
				}
			}
			if (PlaceActualBuildings)
            {
				foreach (IntVec3 item in cellsToDesignate)
				{
					try
					{
						if (map.thingGrid.ThingAt(item, ThingCategory.Building) == null)
						{
							GenSpawn.Spawn(ThingDefOf.Ship_Beam, item, map);
						}
						// TODO: make this corner code decomposed, and working with all 4 rotations and also with 3x1 corners.
						/*IntVec3 next = item;
						next.z += 1;
						if (IsCorner2Here(next, cellsToDesignate, floorCells))
						{
							GenSpawn.Spawn(ThingDef.Named("Ship_Corner_OneTwo"), next, map);
						}
						else if (IsCornerHere(next, cellsToDesignate, floorCells))
						{
							GenSpawn.Spawn(ThingDef.Named("Ship_Corner_OneOne"), next, map);
						}*/
					}
					catch (Exception e)
					{
						// if parts of the blueprint from external file failed to spawn, that isn't  considered major issue
						Log.Warning(e.Message);
					}
				}
				foreach (IntVec3 item in floorCells)
				{
					try
					{
						if (map.thingGrid.ThingAt(item, ThingCategory.Building) == null)
						{
							GenSpawn.Spawn(ResourceBank.ThingDefOf.ShipHullTile, item, map);
						}
					}
					catch (Exception e)
					{
						Log.Warning(e.Message);
					}
				}
			}
            else
            {
				Designator_Plan_Add adder = new Designator_Plan_Add();
				adder.PlanCells(cellsToDesignate);
			}
		}


	}
}
