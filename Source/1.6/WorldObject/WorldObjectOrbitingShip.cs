using RimWorld.Planet;
using RimWorld;
using System;
using UnityEngine;
using Verse;
using System.Collections.Generic;
using Verse.Sound;
using System.Diagnostics;
using System.Text;
using System.Linq;
using HarmonyLib;
using Vehicles;

namespace SaveOurShip2
{
	public class WorldObjectOrbitingShip : MapParent
	{
		private string nameInt;
		public string Name
		{
			get
			{
				return nameInt;
			}
			set
			{
				nameInt = value;
			}
		}
		public override string Label
		{
			get
			{
				if (nameInt == null)
				{
					return base.Label;
				}
				return nameInt;
			}
		}

		//used for orbit transition only
		public override Vector3 DrawPos
		{
			get
			{
				return drawPos;
			}
		}
		public Vector3 drawPos;
		public Vector3 prevDrawPos;
		public Vector3 originDrawPos = Vector3.zero;
		public Vector3 targetDrawPos = Vector3.zero;
        public Vector3 groundPos = Vector3.zero;
        public Vector3 hoverPos = Vector3.zero;
        public Vector3 spacePos = Vector3.zero;
		public Vector2 direction = Vector2.zero;

        //used in orbit
        public static Vector3 vecPolar = new Vector3(0, 1, 0);
        public static readonly Vector2 North = new Vector2(0, 1);
        public static readonly Vector2 South = new Vector2(0, -1);
        public static readonly Vector2 West = new Vector2(1, 0);
        public static readonly Vector2 East = new Vector2(-1, 0);
        public int outputLevel = 1;
        public bool movingDrawPos = false;
        private float radius = ShipInteriorMod2.spaceRadius; //altitude ~95-150
        private float phi = 0; //up/down on radius //td change to N/S orbital
        private float theta = -3; //E/W orbital on radius

        public Vector3 NominalPos => Vector3.SlerpUnclamped(WorldObjectMath.vecEquator * ShipInteriorMod2.spaceRadius, WorldObjectMath.vecEquator * -ShipInteriorMod2.spaceRadius, 3);
        ShipMapComp mapComp => Map?.GetComponent<ShipMapComp>() ?? null;

        public int OutputLevel
        {
            get { return outputLevel; }
            set
            {
				outputLevel += value;
                if (outputLevel > 2)
				{
					outputLevel = 0;
				}
            }
        }

        public float Radius
		{
			get { return radius; }
			set
			{
				radius = value;
				OrbitSet();
			}
		}

		public float Phi
		{
			get { return phi; }
			set
			{
				phi = value;
				OrbitSet();
			}
		}

		public float Theta
		{
			get { return theta; }
			set
			{
				theta = value;
				OrbitSet();
			}
		}

        public bool InSlowDown
        {
            get { return mapComp?.MoveToMap != null && Vector3.Distance(targetDrawPos, DrawPos) <= 3f; }
        }

        public void SetNominalPos(bool hover)
        {
            if (hover)
            {
                Radius = ShipInteriorMod2.hoverRadius;
            }
            else
            {
                Radius = ShipInteriorMod2.spaceRadius;
            }
        }

        public void StartMoving(Vector2 dir)
        {
            prevDrawPos = Vector3.zero;
            direction = dir;
            mapComp.EnginesOn = true;
            movingDrawPos = true;
        }

        public void StopMoving()
        {
			direction = Vector2.zero;
            mapComp.MapFullStop();
            movingDrawPos = false;
        }

        public void OrbitSet() //recalc on change only
		{
			drawPos = WorldObjectMath.GetPos(phi, theta, radius);
		}

        public void GetGroundSpacePos(Vector3 setVec) //get ground/space pos
        {
			Radius = ShipInteriorMod2.groundRadius;
            groundPos = drawPos;
            Radius = ShipInteriorMod2.hoverRadius;
            hoverPos = drawPos;
            Radius = ShipInteriorMod2.spaceRadius;
            spacePos = drawPos;
            drawPos = setVec;
        }

        public override void SpawnSetup()
		{
			drawPos = Vector3.zero;
            OrbitSet();

            base.SpawnSetup();
        }

        protected override void Tick()
		{
			base.Tick();

   //         if (mapComp.EnginesOn && prevDrawPos != drawPos && mapComp.AnyShipCanMove())
   //         {
   //             movingDrawPos = true;
   //             prevDrawPos = drawPos;
   //         }
   //         else
			//{
   //             movingDrawPos = false;
			//	return;
   //         }

			if (!movingDrawPos || mapComp.ShipMapState == ShipMapState.inTransit)
			{
                return;
			}

			Theta = Theta + ShipInteriorMod2.ShipMoveSpeed(mapComp, OutputLevel) * direction.x;
			Phi = Phi + ShipInteriorMod2.ShipMoveSpeed(mapComp, OutputLevel) * direction.y;

			if (this.IsHashIntervalTick(60))
			{
                if (mapComp.ShipMapState != ShipMapState.nominal || !mapComp.EnginesOn || !mapComp.AnyShipCanMove())
                {
                    StopMoving();
                    return;
                }
                foreach (TravellingTransporters obj in Find.WorldObjects.TravellingTransporters)
                {
                    int initialTile = obj.initialTile;
                    if (initialTile == Tile || obj.destinationTile == Tile)
                    {
                        StopMoving();
                        return;
                    }
                }
            }
        }

        public override void ExposeData()
		{
			base.ExposeData();
			WorldObjectMath.SerializeTheta(ref theta, false);
			Scribe_Values.Look<float>(ref phi, "phi", 0f, false);
			Scribe_Values.Look<float>(ref radius, "radius", ShipInteriorMod2.spaceRadius, false);
            Scribe_Values.Look<int>(ref outputLevel, "outputLevel", 1, false);
            Scribe_Values.Look<bool>(ref movingDrawPos, "movingDrawPos", false, false);
            Scribe_Values.Look<string>(ref nameInt, "nameInt", null, false);
			Scribe_Values.Look<Vector3>(ref drawPos, "drawPos", Vector3.zero, false);
            Scribe_Values.Look<Vector3>(ref prevDrawPos, "prevDrawPos", Vector3.zero, false);
            Scribe_Values.Look<Vector3>(ref originDrawPos, "originDrawPos", Vector3.zero, false);
			Scribe_Values.Look<Vector3>(ref targetDrawPos, "targetDrawPos", Vector3.zero, false);
            Scribe_Values.Look<Vector3>(ref groundPos, "groundPos", Vector3.zero, false);
            Scribe_Values.Look<Vector3>(ref hoverPos, "hoverPos", Vector3.zero, false);
            Scribe_Values.Look<Vector3>(ref spacePos, "spacePos", Vector3.zero, false);
            Scribe_Values.Look<Vector2>(ref direction, "direction", Vector2.zero, false);
        }

        public override void Print(LayerSubMesh subMesh)
		{
			float averageTileSize = Find.WorldGrid.AverageTileSize;
			WorldRendererUtility.PrintQuadTangentialToPlanet(DrawPos, 1.7f * averageTileSize, 0.015f, subMesh, false, 0f, true);
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo g in base.GetGizmos())
			{
				yield return g;
			}
			if (HasMap)
			{
				yield return new Command_Action
				{
					defaultLabel = TranslatorFormattedStringExtensions.Translate("CommandShowMap"), // Core\GameplayCommands.xml
					defaultDesc = TranslatorFormattedStringExtensions.Translate("CommandShowMapDesc"), // Core\GameplayCommands.xml
					icon = ShowMapCommand,
					hotKey = KeyBindingDefOf.Misc1,
					action = delegate
					{
						Current.Game.CurrentMap = Map;
						if (!CameraJumper.TryHideWorld())
						{
							SoundDefOf.TabClose.PlayOneShotOnCamera(null);
						}
					}
				};
				if (def.canBePlayerHome)
				{
					yield return new Command_Action
					{
						defaultLabel = TranslatorFormattedStringExtensions.Translate("SoS.AbandonHome"),
						defaultDesc = TranslatorFormattedStringExtensions.Translate("SoS.AbandonHomeDesc"),
						icon = ContentFinder<Texture2D>.Get("UI/ShipAbandon_Icon", true),
						action = delegate
						{
							Map map = this.Map;
							if (map == null)
							{
								Destroy();
								SoundDefOf.Tick_High.PlayOneShotOnCamera();
								return;
							}

							foreach (TravellingTransporters obj in Find.WorldObjects.TravellingTransporters)
							{
								int initialTile = (int)Traverse.Create(obj).Field("initialTile").GetValue();
								if (initialTile == this.Tile || obj.destinationTile == this.Tile)
								{
									Messages.Message(TranslatorFormattedStringExtensions.Translate("SoS.ScuttleShipPods"), this, MessageTypeDefOf.NeutralEvent);
									return;
								}
							}
							StringBuilder stringBuilder = new StringBuilder();
							IEnumerable<Pawn> source = map.mapPawns.PawnsInFaction(Faction.OfPlayer).Where(pawn => !pawn.InContainerEnclosed || (pawn.ParentHolder is Thing && ((Thing)pawn.ParentHolder).def != ResourceBank.ThingDefOf.Ship_CryptosleepCasket));
							if (source.Any())
							{
								StringBuilder stringBuilder2 = new StringBuilder();
								foreach (Pawn item in source.OrderByDescending((Pawn x) => x.IsColonist))
								{
									if (stringBuilder2.Length > 0)
									{
										stringBuilder2.AppendLine();
									}
									stringBuilder2.Append("	" + item.LabelCap);
								}
								stringBuilder.Append(TranslatorFormattedStringExtensions.Translate("ConfirmAbandonHomeWithColonyPawns", stringBuilder2)); // Core\Dialogs_Various.xml
							}
							PawnDiedOrDownedThoughtsUtility.BuildMoodThoughtsListString(
								source, PawnDiedOrDownedThoughtsKind.Died, stringBuilder, null,
								"\n\n" + TranslatorFormattedStringExtensions.Translate("ConfirmAbandonHomeNegativeThoughts_Everyone"), // Core\Dialogs_Various.xml
								"ConfirmAbandonHomeNegativeThoughts");
							if (stringBuilder.Length == 0)
							{
								Destroy();
								SoundDefOf.Tick_High.PlayOneShotOnCamera();
							}
							else
							{
								Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(stringBuilder.ToString(), delegate
								{
									Destroy();
								}));
							}
						}
					};
                    if (Prefs.DevMode)
					{
						yield return new Command_Action
						{
							action = delegate ()
							{
                                StopMoving();
                                Vector3 tileCenterPos = this.Map.Tile.Layer.Origin + Find.WorldGrid.GetTileCenter(this.Map.Tile);
								WorldObjectMath.GetSphericalFromCartesian(tileCenterPos, out phi, out theta, out radius);
								radius = WorldObjectMath.defaultRadius;
							},
							defaultLabel = TranslatorFormattedStringExtensions.Translate("SoS.Dev.ShipPositionReset"),
							defaultDesc = TranslatorFormattedStringExtensions.Translate("SoS.Dev.ShipPositionResetDesc"),
						};
					}
				}
				if (mapComp.ShipMapState == ShipMapState.isGraveyard && !mapComp.IsGraveOriginInCombat && mapComp.ShipMapState != ShipMapState.burnUpSet)
				{
					yield return new Command_Action
					{
						action = delegate
						{
							StringBuilder sb = new StringBuilder();
							sb.Append(TranslatorFormattedStringExtensions.Translate("SoS.LeaveGraveyardConfirmation1"));
							sb.Append(" ");
							// #bb8fo4 color commonly used in XML text highlihts
							sb.Append(Label.Colorize(new Color(0.733f, 0.561f, 0.016f)));
							sb.Append(" ");
							int colonistCount = mapComp.map.mapPawns.ColonistCount;
							int buildingCount = mapComp.map.listerBuildings.allBuildingsColonist.Count + mapComp.map.listerBuildings.allBuildingsNonColonist.Count;
							sb.Append(TranslatorFormattedStringExtensions.Translate("SoS.LeaveGraveyardConfirmation2", buildingCount));
							if (colonistCount > 0)
							{
								sb.Append("\n\n");
								sb.Append(TranslatorFormattedStringExtensions.Translate("SoS.LeaveGraveyardConfirmationColonistsParagraph", colonistCount));
							}
							Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(sb.ToString(), delegate
							{
								mapComp.ShipMapState = ShipMapState.burnUpSet;
							}));
						},
						defaultLabel = TranslatorFormattedStringExtensions.Translate("SoS.LeaveGraveyard"),
						defaultDesc = TranslatorFormattedStringExtensions.Translate("SoS.LeaveGraveyardDesc"),
						hotKey = KeyBindingDefOf.Misc5,
						icon = ContentFinder<Texture2D>.Get("UI/ShipAbandon_Icon", true)
					};
				}
				if (Prefs.DevMode && mapComp.ShipMapState != ShipMapState.burnUpSet)
				{
					yield return new Command_Action
					{
						defaultLabel = TranslatorFormattedStringExtensions.Translate("SoS.Dev.RemoveShip"),
						defaultDesc = TranslatorFormattedStringExtensions.Translate("SoS.Dev.RemoveShipDesc"),
						action = delegate
						{
							mapComp.ShipMapState = ShipMapState.burnUpSet;
						}
					};
				}
			}
		}
		public override string GetInspectString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			string inspectString = base.GetInspectString();
			if (!inspectString.NullOrEmpty())
			{
				stringBuilder.AppendLine(inspectString);
			}
			if (Prefs.DevMode)
			{
				stringBuilder.AppendLine(TranslatorFormattedStringExtensions.Translate("SoS.Dev.OrbitingShipInfo", mapComp.ShipMapState.ToString(),
					"Null", radius, theta.ToString("F2"), phi.ToString("F2"), DrawPos.ToString(), originDrawPos.ToString(), targetDrawPos.ToString()));
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}
		public override void Destroy()
		{
			if (mapComp != null && mapComp.ShipMapState == ShipMapState.inCombat)
				mapComp.EndBattle(Map, false);
			if (Map != null && Map.mapPawns.AnyColonistSpawned)
			{
				Find.GameEnder.CheckOrUpdateGameOver();
			}
			base.Destroy();
			//base.Abandon();
		}

		public override MapGeneratorDef MapGeneratorDef
		{
			get
			{
				return def.mapGenerator ?? DefDatabase<MapGeneratorDef>.GetNamed("EmptySpaceMap");
            }
		}

		public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
		{
			return new List<FloatMenuOption>();
		}

		[DebuggerHidden]
		public override IEnumerable<FloatMenuOption> GetTransportersFloatMenuOptions(IEnumerable<IThingHolder> pods, Action<PlanetTile, TransportersArrivalAction> launchAction)
		{
			return new List<FloatMenuOption>();
		}

		public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject) //on tick check to remove
		{
			if (mapComp.ShipMapState == ShipMapState.burnUpSet)
			{
				//td recheck all of this after VF, generally pods need origin to exist till they land
				foreach (TravellingTransporters obj in Find.WorldObjects.TravellingTransporters)
				{
					int initialTile = obj.initialTile;
					if (initialTile == Tile) //dont remove if pods in flight from this WO
					{
						alsoRemoveWorldObject = false;
						return false;
					}
					else if (obj.destinationTile == Tile) //divert from this WO to initial //td might not work
					{
						obj.destinationTile = initialTile;
						alsoRemoveWorldObject = false;
						return false;
					}
				}

				// Kill vehicles first, in order to avoid reservation issues when killing both normal pawns and vehicles using one pass
				List<VehiclePawn> vehiclesToKill = new List<VehiclePawn>();
				foreach (Thing t in Map.spawnedThings)
				{
					if (t is VehiclePawn v)
					{
						vehiclesToKill.Add(v);
					}
				}
				foreach (VehiclePawn v in vehiclesToKill)
				{
					v.Kill(new DamageInfo(DamageDefOf.Bomb, 99999));
				}

				//kill off pawns to prevent reappearance, tell player
				List<Pawn> toKill = new List<Pawn>();
				foreach (Thing t in Map.spawnedThings)
				{
					if (t is Pawn p)
						toKill.Add(p);
				}
				foreach (Pawn p in toKill)
				{
					p.Kill(new DamageInfo(DamageDefOf.Bomb, 99999));
				}
				if (toKill.Any(p => p.Faction == Faction.OfPlayer))
				{
					string letterString = TranslatorFormattedStringExtensions.Translate("SoS.PawnsLostReEntry") + "\n\n";
					foreach (Pawn deadPawn in toKill.Where(p => p.Faction == Faction.OfPlayer))
						letterString += deadPawn.LabelShort + "\n";
					Find.LetterStack.ReceiveLetter(TranslatorFormattedStringExtensions.Translate("SoS.PawnsLostReEntryDesc"), letterString,
						LetterDefOf.NegativeEvent);
				}

				alsoRemoveWorldObject = true;
				return true;
			}
			alsoRemoveWorldObject = false;
			return false;
		}
	}
}

