using RimWorld;
using RimWorld.Planet;
using Verse;

namespace SaveOurShip2
{
	public static class AccessExtensions
	{
		public static ShipGameComp Utility;

		public static bool IsSOS2Space(this Map map)
		{
            if (map != null)
            {
                if (map.info?.parent != null && (map.info.parent is WorldObjectOrbitingShip || map.info.parent is SpaceSite || map.info.parent is MoonBase || map.Parent.AllComps.Any((WorldObjectComp comp) => comp is MoonPillarSiteComp)))
                {
                    return true;
                }
            }
			return false;
		}
	}
}

