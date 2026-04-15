using RimWorld;
using System;
using System.Collections.Generic;
using Verse;

namespace SaveOurShip2
{
    public class GenStep_FindPlayerStartSpotSpace : GenStep
    {
        private const int MinRoomCellCount = 10;

        public override int SeedPart => 1187186631;

        public override void Generate(Map map, GenStepParams parms)
        {
            HashSet<IntVec3> largestOpenArea;
            List<CellRect> usedRects;
            if (!map.wasSpawnedViaGravShipLanding)
            {
                DeepProfiler.Start("RebuildAllRegions");
                map.regionAndRoomUpdater.RebuildAllRegionsAndRooms();
                DeepProfiler.End();
                if (!MapGenerator.PlayerStartSpotValid)
                {
                    largestOpenArea = FindLargestContiguousOpenArea(map);
                    usedRects = MapGenerator.GetOrGenerateVar<List<CellRect>>("UsedRects");
                    MapGenerator.PlayerStartSpot = TryFindCentralCell(map, 7, 10, Validator);
                }
            }
            bool Validator(IntVec3 cell)
            {
                if (!largestOpenArea.Contains(cell))
                {
                    return false;
                }
                foreach (LayoutStructureSketch layoutStructureSketch in map.layoutStructureSketches)
                {
                    if (layoutStructureSketch.structureLayout != null && layoutStructureSketch.structureLayout.container.Contains(cell))
                    {
                        return false;
                    }
                }
                foreach (CellRect item in usedRects)
                {
                    if (item.Contains(cell))
                    {
                        return false;
                    }
                }
                if (!cell.GetAffordances(map).Contains(TerrainAffordanceDefOf.Heavy))
                {
                    return false;
                }
                return !cell.Roofed(map);
            }
        }

        private HashSet<IntVec3> FindLargestContiguousOpenArea(Map map)
        {
            HashSet<IntVec3> checkedCells = new HashSet<IntVec3>();
            HashSet<IntVec3> hashSet = new HashSet<IntVec3>();
            HashSet<IntVec3> current = new HashSet<IntVec3>();
            _ = MapGenerator.Elevation;
            foreach (IntVec3 cell in map.AllCells)
            {
                if (cell.GetEdifice(map) == null && !checkedCells.Contains(cell))
                {
                    current.Clear();
                    map.floodFiller.FloodFill(cell, (IntVec3 x) => cell.GetEdifice(map) == null, delegate (IntVec3 x)
                    {
                        current.Add(x);
                        checkedCells.Add(x);
                    });
                    if (current.Count > hashSet.Count)
                    {
                        hashSet.Clear();
                        hashSet.AddRange(current);
                    }
                }
            }
            return hashSet;
        }

        public static IntVec3 TryFindCentralCell(Map map, int tightness, int minCellCount, Predicate<IntVec3> extraValidator = null, bool returnInvalidOnFail = false)
        {
            int debug_numDistrict = 0;
            int debug_numTouch = 0;
            int debug_numDistrictCellCount = 0;
            int debug_numExtraValidator = 0;
            Predicate<IntVec3> validator = delegate (IntVec3 c)
            {
                Region validRegionAt = map.regionGrid.GetValidRegionAt(c);
                if (validRegionAt != null)
                {
                    validRegionAt.type = RegionType.Normal;
                }

                District district = c.GetDistrict(map);
                if (district == null)
                {
                    debug_numDistrict++;
                    return false;
                }

                if (!district.TouchesMapEdge)
                {
                    debug_numTouch++;
                    return false;
                }

                if (district.CellCount < minCellCount)
                {
                    debug_numDistrictCellCount++;
                    return false;
                }

                validRegionAt.type = RegionType.ImpassableFreeAirExchange;

                if (extraValidator != null && !extraValidator(c))
                {
                    debug_numExtraValidator++;
                    return false;
                }

                return true;
            };
            for (int num = tightness; num >= 1; num--)
            {
                int num2 = map.Size.x / num;
                if (CellFinderLoose.TryFindRandomNotEdgeCellWith((map.Size.x - num2) / 2, validator, map, out var result))
                {
                    return result;
                }
            }

            if (returnInvalidOnFail)
            {
                return IntVec3.Invalid;
            }

            Log.Error("Found no good central spot. Choosing randomly. numDistrict=" + debug_numDistrict + ", numTouch=" + debug_numTouch + ", numDistrictCellCount=" + debug_numDistrictCellCount + ", numExtraValidator=" + debug_numExtraValidator);
            return CellFinderLoose.RandomCellWith((IntVec3 x) => x.Standable(map), map);
        }
    }
}
