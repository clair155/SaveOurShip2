using HarmonyLib;
using PipeSystem;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace SaveOurShip2
{
    [HarmonyPatch(typeof(WorldComponent_GravshipController), "TakeoffEnded")]
    public static class MakeShipNotTravel
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.MethodReplacer(AccessTools.Method(typeof(GravshipUtility), "TravelTo"), AccessTools.Method(typeof(MakeShipNotTravel), "TravelTo"));
        }

        public static void TravelTo(Gravship gravship, PlanetTile oldTile, PlanetTile newTile)
        {
            gravship.SetFaction(gravship.Engine.Faction);
            gravship.Tile = new PlanetTile(0);
            Find.WorldObjects.Add(gravship);
        }
    }

    //[HarmonyPatch(typeof(WorldObject), "Draw")]
    //public static class DisableDraw
    //{
    //    public static bool Prefix(WorldObject __instance)
    //    {
    //        if (__instance is WorldObjectOrbitingShip && (bool)__instance.Material)
    //        {
    //            Log.Message("DisableDraw");
    //            float averageTileSize = __instance.Tile.Layer.AverageTileSize;
    //            float rawTransitionPct = ExpandableWorldObjectsUtility.RawTransitionPct;
    //            float num = Rand.RangeSeeded(0f, 0.01f, __instance.ID) + __instance.def.drawAltitudeOffset;
    //            if (__instance.def.expandingIcon && rawTransitionPct > 0f && !ExpandableWorldObjectsUtility.HiddenByRules(__instance))
    //            {
    //                Color color = __instance.Material.color;
    //                float num2 = 1f - rawTransitionPct;
    //                WorldObject.propertyBlock.SetColor(ShaderPropertyIDs.Color, new Color(color.r, color.g, color.b, color.a * num2));
    //                WorldRendererUtility.DrawQuadTangentialToPlanet(__instance.DrawPos, 0.7f * averageTileSize, __instance.DrawAltitude + num, __instance.Material, 0f, counterClockwise: false, useSkyboxLayer: false, WorldObject.propertyBlock);
    //            }
    //            else
    //            {
    //                WorldRendererUtility.DrawQuadTangentialToPlanet(__instance.DrawPos, 0.7f * averageTileSize, __instance.DrawAltitude + num, __instance.Material);
    //            }
    //            return false;
    //        }
    //        return true;
    //    }
    //}

    //[HarmonyPatch(typeof(WorldCameraDriver), "ApplyMapPositionToGameObject")]
    //public static class ReplacePosition
    //{
    //    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
    //    {
    //        List<CodeInstruction> codes = instructions.ToList();
    //        MethodInfo methodinfo = AccessTools.PropertyGetter(typeof(MapParent), "WorldCameraPosition");
    //        MethodInfo methodinfo2 = AccessTools.Method(typeof(WorldGrid), "GetTileCenter");
    //        Label label = il.DefineLabel();
    //        bool patched = false;
    //        for (int i = 0; i < codes.Count; i++)
    //        {
    //            if (codes[i].opcode == OpCodes.Callvirt)
    //            {
    //                //if (codes[i].Calls(methodinfo))
    //                //{
    //                //    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(ReplacePosition), "WorldCameraPosition"));
    //                //}
    //                /*else*/ 
    //                //if (codes[i].Calls(methodinfo2))
    //                //{
    //                //    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(ReplacePosition), "GetTileCenter"));
    //                //}
    //            }
    //            else if (!patched && codes[i + 3].Calls(methodinfo2))
    //            {
    //                List<Label> labels = codes[i].ExtractLabels();
    //                yield return new CodeInstruction(OpCodes.Nop).WithLabels(labels);
    //                yield return new CodeInstruction(OpCodes.Ldloc_0);
    //                yield return codes[i + 2].Clone();
    //                yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(ReplacePosition), "GetTileCenter"));
    //                codes[i + 4].WithLabels(label);
    //                yield return new CodeInstruction(OpCodes.Br, label);
    //                patched = true;
    //            }
    //            else
    //            {
    //                yield return codes[i];
    //            }
    //        }
    //    }

    //    private static Vector3 WorldCameraPosition(MapParent mapParent)
    //    {
    //        if (mapParent.Map.IsSOS2Space() && mapParent is WorldObjectOrbitingShip wo)
    //        {
    //            return wo.DrawPos;
    //        }
    //        return mapParent.WorldCameraPosition;
    //    }

    //    private static Vector3 GetTileCenter(PlanetTile tile)
    //    {
    //        Map map = Find.CurrentMap;
    //        var mapComp = map.GetComponent<ShipMapComp>();
    //        if (map.IsSOS2Space() && map.Parent is WorldObjectOrbitingShip wo)
    //        {
    //            return wo.DrawPos;
    //        }
    //        return Find.WorldGrid.GetTileCenter(tile);
    //    }
    //}

    [HarmonyPatch(typeof(WorldCameraDriver), "ApplyMapPositionToGameObject")]
    public static class ReplacePosition
    {
        public static bool Prefix(WorldCameraDriver __instance)
        {
            Map currentMap = Find.CurrentMap;
            if (currentMap == null)
            {
                return false;
            }
            Vector3 vector;
            if (!(currentMap.ParentHolder is MapParent mapParent))
            {
                Map map = Find.CurrentMap;
                if (map.Parent is WorldObjectOrbitingShip wo)
                {
                    vector = wo.DrawPos;
                }
                else
                {
                    vector = Find.WorldGrid.GetTileCenter(currentMap.Tile);
                }
            }
            else
            {
                if (mapParent is WorldObjectOrbitingShip wo)
                {
                    vector = wo.DrawPos;
                }
                else
                {
                    vector = mapParent.WorldCameraPosition;
                }
            }
            if (vector == Vector3.zero)
            {
                return false;
            }
            Vector3 vector2 = -vector.normalized;
            vector += -vector2 * currentMap.Tile.Layer.BackgroundWorldCameraOffset;
            Transform transform = __instance.MyCamera.transform;
            Quaternion rotation = Quaternion.LookRotation(vector2, Vector3.up);
            transform.rotation = rotation;
            float num = currentMap.Tile.Layer.BackgroundWorldCameraParallaxDistancePer100Cells;
            if (num == 0f)
            {
                transform.position = vector;
                return false;
            }
            Vector2 viewSpacePosition = Find.CameraDriver.ViewSpacePosition;
            IntVec3 size = Find.CurrentMap.Size;
            float num2 = 1f;
            float num3 = 1f;
            if (size.x > size.z)
            {
                num3 = (float)size.z / (float)size.x;
                num = num * (float)size.x / 100f;
            }
            else if (size.z > size.x)
            {
                num2 = (float)size.x / (float)size.z;
                num = num * (float)size.z / 100f;
            }
            Vector3 up = transform.up;
            Vector3 right = transform.right;
            Vector3 vector3 = up * (viewSpacePosition.y * num * num3) - up * num / 2f * num3;
            Vector3 vector4 = right * (viewSpacePosition.x * num * num2) - right * num / 2f * num2;
            transform.position = vector + vector3 + vector4 + currentMap.Tile.Layer.Origin;
            return false;
        }
    }

    [HarmonyPatch(typeof(PlanetLayer), "BackgroundWorldCameraOffset", MethodType.Getter)]
    public static class ChangeAltitude
    {
        public static bool Prefix(ref float __result)
        {
            Map map = Find.CurrentMap;
            if (map == null)
            {
                return true;
            }
            var mapComp = map.GetComponent<ShipMapComp>();
            if (map?.Parent is WorldObjectOrbitingShip wo && !WorldComponent_GravshipController.CutsceneInProgress)
            {
                if (mapComp.ShipMapState == ShipMapState.inTransit)
                {
                    if (mapComp.Hovering)
                    {
                        __result = (Mathf.InverseLerp(0f, Vector3.Distance(wo.groundPos, wo.hoverPos), Vector3.Distance(wo.groundPos, wo.drawPos)) * 50f) + 50f;
                        return false;
                    }
                    else
                    {
                        __result = Mathf.InverseLerp(0f, Vector3.Distance(wo.groundPos, wo.spacePos), Vector3.Distance(wo.groundPos, wo.drawPos)) * 900f + 100f;
                        return false;
                    }
                }
                else
                {
                    if (mapComp.Hovering)
                    {
                        __result = 100f;
                        return false;
                    }
                    else
                    {
                        __result = 1000f;
                        return false;
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(WorldComponent_GravshipController), "LandingEnded")]
    public static class PostSetupAfterLanding
    {
        public static void Postfix(WorldComponent_GravshipController __instance)
        {
            if (ShipInteriorMod2.GravshipCutscenes)
            {
                Prefs.GravshipCutscenes = true;
            }
        }
    }

    [HarmonyPatch(typeof(GravshipUtility), "GenerateGravship")]
    public static class PickUpShip
    {
        public static void Prefix(Building_GravEngine engine)
        {
            ShipMapComp mapComp = engine.Map.GetComponent<ShipMapComp>();
            int shipIndex = mapComp.ShipIndexOnVec(engine.Position);
            PostSetupAfterPlacing.Ship = mapComp.ShipsOnMap[shipIndex];
            PostSetupAfterPlacing.GravArea.AddRange(engine.ValidSubstructure);

            List<IntVec3> fireExplosions = new List<IntVec3>();
            List<IntVec3> astroFireExplosions = new List<IntVec3>();
            bool sourceMapIsSpace = engine.Map.IsSOS2Space();
            IEnumerable<CompEngineTrail> engines = PostSetupAfterPlacing.Ship.Engines.Where(e => e.flickComp.SwitchIsOn && !e.Props.energy && !e.Props.reactionless && e.refuelComp.Fuel > 0 && e.Props.takeOff);
            PostSetupAfterPlacing.engines = engines;
            foreach (CompEngineTrail compEngine in engines)
            {
                if (!sourceMapIsSpace)
                {
                    if (compEngine.refuelComp.Props.fuelFilter.AllowedThingDefs.Contains(EP_DefOf.VGE_Astrofuel))
                    {
                        if (compEngine.parent.Rotation.AsByte == 0)
                            astroFireExplosions.Add(compEngine.parent.Position + new IntVec3(0, 0, -3));
                        else if (compEngine.parent.Rotation.AsByte == 1)
                            astroFireExplosions.Add(compEngine.parent.Position + new IntVec3(-3, 0, 0));
                        else if (compEngine.parent.Rotation.AsByte == 2)
                            astroFireExplosions.Add(compEngine.parent.Position + new IntVec3(0, 0, 3));
                        else
                            astroFireExplosions.Add(compEngine.parent.Position + new IntVec3(3, 0, 0));
                    }
                    else
                    {
                        if (compEngine.parent.Rotation.AsByte == 0)
                            fireExplosions.Add(compEngine.parent.Position + new IntVec3(0, 0, -3));
                        else if (compEngine.parent.Rotation.AsByte == 1)
                            fireExplosions.Add(compEngine.parent.Position + new IntVec3(-3, 0, 0));
                        else if (compEngine.parent.Rotation.AsByte == 2)
                            fireExplosions.Add(compEngine.parent.Position + new IntVec3(0, 0, 3));
                        else
                            fireExplosions.Add(compEngine.parent.Position + new IntVec3(3, 0, 0));
                    }
                }
            }
            //takeoff - explosions
            foreach (IntVec3 pos in fireExplosions)
            {
                GenExplosion.DoExplosion(pos, engine.Map, 3.9f, DamageDefOf.Flame, null);
            }
            foreach (IntVec3 pos in astroFireExplosions)
            {
                GenExplosion.DoExplosion(pos, engine.Map, 3.9f, EP_DefOf.VGE_AstrofireDamage, null);
            }
        }
    }

    [HarmonyPatch(typeof(WorldComponent_GravshipController), "PlaceGravship")]
    public static class PostSetupAfterPlacing
    {
        public static SpaceShipCache Ship;

        public static IEnumerable<CompEngineTrail> engines;

        public static HashSet<IntVec3> GravArea = new HashSet<IntVec3>();

        public static void Postfix(Gravship gravship, IntVec3 root, Map map)
        {
            if (Ship == null)
            {
                Log.Warning("Ship is somehow null.");
                return;
            }

            // Transforms vector from initial position to final according to desired movement/rotation.
            Func<IntVec3, IntVec3> Transform;
            Transform = (IntVec3 from) => PrefabUtility.GetAdjustedLocalPosition(from - gravship.originalPosition, gravship.Rotation) + root;

            Map sourceMap = Ship.Map;
            HashSet<IntVec3> shipArea = new HashSet<IntVec3>(Ship.Area);
            HashSet<IntVec3> targetArea = new HashSet<IntVec3>();
            HashSet<int> shipIndexes = new HashSet<int> { Ship.Index };
            bool targetMapIsSpace = map.IsSOS2Space();
            bool sourceMapIsSpace = sourceMap.IsSOS2Space();
            float weBeCrashing = 0;
            ShipMapComp sourceMapComp = sourceMap.GetComponent<ShipMapComp>();
            ShipMapComp targetMapComp = map.GetComponent<ShipMapComp>();
            Dictionary<IntVec3, Tuple<int, int>> tmpCells = new Dictionary<IntVec3, Tuple<int, int>>();

            foreach (IntVec3 pos2 in shipArea)
            {
                if (ShipInteriorMod2.ArriveShipFlag || GravArea.Contains(pos2))
                {
                    IntVec3 adjustedPos = Transform(pos2);
                    tmpCells.Add(adjustedPos, new Tuple<int, int>(sourceMapComp.MapShipCells[pos2].Item1, sourceMapComp.MapShipCells[pos2].Item2));
                    targetArea.Add(adjustedPos);
                }
                sourceMapComp.MapShipCells.Remove(pos2);
            }
            targetMapComp.MapShipCells.AddRange(tmpCells);
            if (map.IsSOS2Space()) //find adjacent ships
            {
                foreach (IntVec3 pos in targetArea)
                {
                    foreach (IntVec3 vec in GenAdj.CellsAdjacentCardinal(pos, Rot4.North, new IntVec2(1, 1)).Where(v => !targetArea.Contains(v) && targetMapComp.MapShipCells.ContainsKey(v)))
                    {
                        var adjShip = targetMapComp.ShipsOnMap[targetMapComp.ShipIndexOnVec(vec)];
                        //if non fac ship near, abort
                        if (adjShip.Faction != Ship.Faction)
                        {
                            Messages.Message(TranslatorFormattedStringExtensions.Translate("SoS.MoveFailFaction"), null, MessageTypeDefOf.NegativeEvent);
                            return;
                        }
                        shipIndexes.Add(targetMapComp.MapShipCells[vec].Item1);
                    }
                }
            }
            //if (devMode)
            //    watch.Record("processSourceArea");

            //adjust cache
            if (map != sourceMap) //ship cache: if moving to different map, move cache
            {
                targetMapComp.ShipsOnMap.Add(Ship.Index, sourceMapComp.ShipsOnMap[Ship.Index]);
                Ship = targetMapComp.ShipsOnMap[Ship.Index];
                Ship.Map = map;
                if (Ship.BuildingsDestroyed.Any()) //cache: adjust destroyed
                {
                    HashSet<Tuple<ThingDef, IntVec3, Rot4>> buildingsDestroyed = new HashSet<Tuple<ThingDef, IntVec3, Rot4>>(Ship.BuildingsDestroyed);
                    Ship.BuildingsDestroyed.Clear();
                    foreach (var sh in buildingsDestroyed)
                    {
                        Ship.BuildingsDestroyed.Add(new Tuple<ThingDef, IntVec3, Rot4>(sh.Item1, Transform(sh.Item2), sh.Item3));
                    }
                    buildingsDestroyed.Clear();
                }
                sourceMapComp.RemoveShipFromCache(Ship.Index);
            }

            Ship.Area.Clear();
            Ship.Area.AddRange(targetArea);

            //draw fuel, exhaust area actions
            if (Ship.Core is Building_ShipBridge)
            {
                float fuelNeeded = Ship.MassActual;
                float fuelStored = 0f;
                List<PipeNet> lastPipeNets = new List<PipeNet>();
                foreach (CompEngineTrail engine in engines)
                {
                    fuelStored += engine.refuelComp.Fuel;
                    if (engine.PodFueled)
                    {
                        fuelStored += engine.refuelComp.Fuel;
                    }
                    CompRefillWithPipes pipeComp = engine.parent.TryGetComp<CompRefillWithPipes>();
                    if (pipeComp != null)
                    {
                        if (lastPipeNets.Contains(pipeComp.PipeNet))
                        {
                            continue;
                        }
                        fuelStored += pipeComp.PipeNet.CurrentStored();
                        lastPipeNets.Add(pipeComp.PipeNet);
                    }
                }
                if (sourceMapIsSpace)
                {
                    if (targetMapIsSpace) //space map
                    {
                        if (map == sourceMap)
                            fuelNeeded *= ShipInteriorMod2.pctFuelLocal;
                        else
                            fuelNeeded *= ShipInteriorMod2.pctFuelMap;
                    }
                    else //to ground
                    {
                        fuelNeeded *= ShipInteriorMod2.pctFuelLand;
                        if (fuelNeeded > fuelStored)
                            weBeCrashing = fuelStored / fuelNeeded;
                        else if (!Ship.CanMove())
                            weBeCrashing = 1f;
                    }
                }
                else //to space
                {
                    fuelNeeded = Ship.MassTakeoff * (ShipInteriorMod2.pctFuelTakeoff - ShipInteriorMod2.pctFuelTakeoffPerOptimizer * Ship.EffectiveFuelOptimizerCount);
                }
                float fuelToConsume = 0f;
                foreach (CompEngineTrail engine in engines)
                {
                    fuelToConsume = fuelNeeded * engine.refuelComp.Fuel / fuelStored;
                    engine.refuelComp.ConsumeFuel(fuelToConsume);
                }
                foreach (PipeNet pipeNet in lastPipeNets)
                {
                    fuelToConsume = fuelNeeded * pipeNet.CurrentStored() / fuelStored;
                    pipeNet.DrawAmongStorage(fuelToConsume);
                }
                //if (devMode)
                //    watch.Record("takeoffEffects");
            }

            if (shipIndexes.Count > 1) //ship cache: adjacent ships found, merge in order: largest ship, ship, wreck
            {
                Log.Message("SOS2: ".Colorize(Color.cyan) + " ship move found adjacent ships in area, merging!");
                targetMapComp.CheckAndMerge(shipIndexes);
            }

            //landing - remove space map if no pawns or cores
            if (!targetMapIsSpace && !sourceMap.spawnedThings.Any((Thing t) => (t is Pawn || (t is Building_ShipBridge b && b.mannableComp == null)) && !t.Destroyed))
            {
                WorldObject oldParent = sourceMap.Parent;
                Current.Game.DeinitAndRemoveMap(sourceMap, false);
                Find.World.worldObjects.Remove(oldParent);
            }

            //crash damage
            if (!targetMapIsSpace && weBeCrashing > 0f)
            {
                Ship.WeBeCrashing = weBeCrashing;
                //Ship.CrashShip();
            }

            //heat
            map.GetComponent<ShipMapComp>().heatGridDirty = true;

            if (ShipInteriorMod2.HoverShipFlag)
            {
                targetMapComp.Hovering = true;
            }
            ShipInteriorMod2.MoveShipFlag = false;
            ShipInteriorMod2.HoverShipFlag = false;
            ShipInteriorMod2.LaunchShipFlag = false;
            ShipInteriorMod2.ArriveShipFlag = false;
            ShipInteriorMod2.AfterPlaceFlag = true;
            targetMapComp.Ship = null;

            if (map.GetComponent<ShipMapComp>().ShipMapState != ShipMapState.inTransit)
            {
                map.GetComponent<ShipMapComp>().MapFullStop();
            }
        }
    }

    [HarmonyPatch(typeof(Gravship), "DetermineLaunchDirection")]
    public static class IfNoCore
    {
        public static bool Prefix(Gravship __instance)
        {
            if (__instance.PilotConsole == null)
            {
                __instance.launchDirection = Rot4.North.AsIntVec3;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Building_GravEngine), "OnCooldownGraphic", MethodType.Getter)]
    public static class OnCooldownGraphic
    {
        public static bool Prefix(Building_GravEngine __instance, ref Graphic __result)
        {
            if (__instance.def == ResourceBank.ThingDefOf.TempGravEngine)
            {
                __result = GraphicDatabase.Get<Graphic_Single>("Things/Filth/Trash/TrashA", ShaderDatabase.Cutout, new Vector2(0, 0), Color.white);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(GravshipUtility), "InsideFootprint")]
    public static class DisableFootprint
    {
        public static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(PlaceWorker_InRangeOfGravEngine), "AllowsPlacing")]
    public static class DisableFootprint2
    {
        public static bool Prefix(ref AcceptanceReport __result)
        {
            __result = AcceptanceReport.WasAccepted;
            return false;
        }
    }

    [HarmonyPatch(typeof(SubstructureGrid), "DrawSubstructureFootprint")]
    public static class DisableFieldedges
    {
        public static bool Prefix()
        {
            return false;
        }
    }

    //[HarmonyPatch(typeof(ShipCountdown), "InitiateCountdown", new Type[] { typeof(Building) })]
    //public static class CancelShipCountdown
    //{
    //    public static bool Prefix(Building launchingShipRoot)
    //    {
    //        //SoundDefOf.ShipTakeoff.PlayOneShotOnCamera();
    //        ShipCountdown.shipRoot = launchingShipRoot;
    //        ShipCountdown.timeLeft = 0.1f;
    //        ShipCountdown.customLaunchString = null;
    //        return false;
    //    }
    //}

    [HarmonyPatch(typeof(RitualOutcomeEffectWorker_GravshipLaunch), "Apply")]
    public static class ReplaceRitualOutcome
    {
        public static bool Prefix(RitualOutcomeEffectWorker_GravshipLaunch __instance, float progress, LordJob_Ritual jobRitual)
        {
            if (progress < 1f)
            {
                Messages.Message("GravshipLaunchInterrupted".Translate(), MessageTypeDefOf.NegativeEvent);
                return false;
            }
            try
            {
                ShipMapComp mapComp = jobRitual.selectedTarget.Thing.Map.GetComponent<ShipMapComp>();
                if (ShipInteriorMod2.ArriveShipFlag)
                {
                    SetQuality(jobRitual, progress, __instance);
                    CleanupJob(jobRitual);
                    mapComp.MapFullStop();
                    mapComp.BurnTimer = 0;
                    ShipInteriorMod2.PlaceShip(((Building_ShipBridge)jobRitual.selectedTarget.Thing).Ship, mapComp.MoveToMap, IntVec3.Zero, true);
                }
                else
                {
                    SetQuality(jobRitual, progress, __instance);
                    ShipInteriorMod2.worldTileOverride = PlanetTile.Invalid;
                    ShipInteriorMod2.launchOrigin = jobRitual.selectedTarget.Thing.Map.Tile;
                    ShipInteriorMod2.LaunchShip((Building)jobRitual.selectedTarget.Thing, ShipInteriorMod2.HoverShipFlag);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error launching gravship: " + ex);
            }
            return false;
        }

        public static void SetQuality(LordJob_Ritual jobRitual, float progress, RitualOutcomeEffectWorker_GravshipLaunch ___instance)
        {
            CompPilotConsole consoleComp = jobRitual.selectedTarget.Thing?.TryGetComp<CompPilotConsole>();
            float quality = ___instance.GetQuality(jobRitual, progress);
            consoleComp.engine.launchInfo = new LaunchInfo
            {
                pilot = jobRitual.PawnWithRole("pilot"),
                copilot = jobRitual.PawnWithRole("copilot"),
                quality = quality,
                doNegativeOutcome = Rand.Chance(GravshipUtility.NegativeLandingOutcomeFromQuality(quality))
            };
        }

        public static void CleanupJob(LordJob_Ritual jobRitual)
        {
            Building_GravEngine building_GravEngine = jobRitual.selectedTarget.Thing?.TryGetComp<CompPilotConsole>()?.engine;
            List<Pawn> tmpPawnToEndJob = new List<Pawn>();
            if (building_GravEngine != null)
            {
                if (building_GravEngine.pawnsToBoard != null)
                {
                    tmpPawnToEndJob.AddRange(building_GravEngine.pawnsToBoard);
                }
                if (building_GravEngine.pawnsToLeave != null)
                {
                    tmpPawnToEndJob.AddRange(building_GravEngine.pawnsToLeave);
                }
                building_GravEngine.pawnsToBoard = null;
                building_GravEngine.pawnsToLeave = null;
            }
            foreach (Pawn item in tmpPawnToEndJob)
            {
                item.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }
        }
    }

    [HarmonyPatch(typeof(Building_GravEngine), "RenamableLabel", MethodType.Setter)]
    public static class SyncName
    {
        public static void Postfix(Building_GravEngine __instance, string value)
        {
            SpaceShipCache ship = ShipInteriorMod2.GetShipFromGrav(__instance);
            if (ship != null)
            {
                ship.Name = value;
            }
            else
            {
                Log.Message("Ship name sync failed.");
            }
        }
    }

    [HarmonyPatch(typeof(CompPilotConsole), "CompInspectStringExtra")]
    public static class RemoveUnwantedStrings
    {
        public static void Postfix(CompPilotConsole __instance, ref string __result)
        {
            string st = __result;
            int count = Mathf.Abs(st.IndexOf("Stored") - st.IndexOf("Gravship range"));
            st = st.Remove(st.IndexOf("Gravship range") - 1, count);
            count = Mathf.Abs(st.IndexOf("Fuel consumption") - st.Count());
            st = st.Remove(st.IndexOf("Fuel consumption"), count);
            st = st.Trim();
            __result = st;
        }
    }

    [HarmonyPatch(typeof(CompAffectedByFacilities), "CanLinkTo")]
    public static class FixMultiEnginesBug
    {
        public static bool Prefix(ref bool __result, CompAffectedByFacilities __instance, Thing facility)
        {
            if (__instance.parent is Building_GravEngine building_GravEngine && !building_GravEngine.AllConnectedSubstructureNoRegen.Contains(facility.Position))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Gravship), "TickInterval")]
    public static class DisableTick
    {
        public static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(Gravship), "DrawPos", MethodType.Getter)]
    public static class DisableDrawPos
    {
        public static bool Prefix()
        {
            return false;
        }
    }
}
    
