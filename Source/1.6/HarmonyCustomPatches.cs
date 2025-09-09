using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using RimWorld.Planet;
using HarmonyLib;

namespace SaveOurShip2
{
	// Here go omplicated things like patches to delegates requiring manual method search rather than simple annotation
	public static class HarmonyCustomPatches
	{
		public static void Apply(Harmony harmony)
		{
			ApplyGravshipTargetingPatch(harmony);
		}
		public static void ApplyGravshipTargetingPatch(Harmony harmony)
		{
			const string innerClassName = "<>c__DisplayClass14_0";
			const string methodName = "<StartChoosingDestination_NewTemp>b__0";
			Type[] types = typeof(CompPilotConsole).GetNestedTypes(AccessTools.all);
			var innerType = types.FirstOrDefault(type => type.Name == innerClassName);
			List<MethodInfo> innerMethods = innerType.GetDeclaredMethods();
			MethodInfo delegateValidateTarget = innerMethods.FirstOrDefault(method => method.Name == methodName);
			harmony.Patch(delegateValidateTarget, prefix: new HarmonyMethod(typeof(HarmonyCustomPatches), nameof(ValidateTargetPrefix)));
		}

		// Delegate validating gravship target will disallow sending it to the pap participating in ship battle
		// to avoid some tricks: for going to enemy, that is cheatingly big boarding shuttle that can have non-ship turrets around
		// and completely bypasses enemy weapons and PDs.
		// For sending to player ship, gravship implements reduced set of ship mechanics, no TWR, so better not be used 
		// as damage sponge that doesn't care about speed.
		public static bool ValidateTargetPrefix(PlanetTile tile)
		{
			MapParent worldObject;
			if (Find.WorldObjects.TryGetWorldObjectAt<MapParent>(tile, out worldObject))
			{
				if (worldObject is WorldObjectOrbitingShip ship &&
					ship.Map.GetComponent<ShipMapComp>().ShipMapState == ShipMapState.inCombat)
				{
					Messages.Message(TranslatorFormattedStringExtensions.Translate("SoS.CantLaunchGravshipToCombatMap"),
						MessageTypeDefOf.RejectInput, historical: false);
					return false;
				}
			}
			return true;
		}
	}
}
