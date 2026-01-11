using RimWorld;
using UnityEngine;
using Verse;

namespace SaveOurShip2
{
	public class ModIntegration
	{
		public static bool HasActiveModWithIdentifierAndOptionalSuffix(string modID)
		{
			string suffixedModID = modID + "_steam";
			return (ModLister.GetActiveModWithIdentifier(modID) ??
				    ModLister.GetActiveModWithIdentifier(suffixedModID)) != null;
		}

		// Todo: it is to be verified with mod maintainers that mod identification can be switched from name to mod ID
		public const string CEModName = "Combat Extended";

		public const string UnlockModID = "Boris.SOS2uas";
		public static bool IsCEEnabled()
		{
			return ModLister.HasActiveModWithName(CEModName);
		}
	}
}

