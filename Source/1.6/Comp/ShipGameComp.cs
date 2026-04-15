using System.Collections.Generic;
using Verse;
using RimWorld;

namespace SaveOurShip2
{
	public class ShipGameComp : GameComponent
	{
		public HashSet<Thing> shuttleCache = new HashSet<Thing>();
		public List<ShipMapComp> shipHeatMapCompCache = new List<ShipMapComp>();

		public ShipGameComp(Game game)
		{
			AccessExtensions.Utility = this;
		}

		private void ModCompatibilityWarning()
        {
			// Players can try using that mod, but bugreports with it won't be accepted. Mod's design idea is 
			// easy to implement, but waay too expensive to make it work without issues.
			// Not recoomended in favor of new integration features.
			if(ModLister.GetActiveModWithIdentifier("Laurence042.Sos2ShipHullPlatingIsGravshipSubstructure") != null)
			{
				Messages.Message(TranslatorFormattedStringExtensions.Translate("SoS.SoftIncompatibilityWithPlatingIsSubstructure"), null, MessageTypeDefOf.SilentInput);
			}
        }

		public override void StartedNewGame()
		{
			ModCompatibilityWarning();
		}

		public override void LoadedGame()
		{
			ModCompatibilityWarning();
		}

		// Laurence042.Sos2ShipHullPlatingIsGravshipSubstructure 
	}
}