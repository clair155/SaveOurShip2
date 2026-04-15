using UnityEngine;
using Verse;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse.AI;

namespace SaveOurShip2
{
	public class WeatherEvent_VacuumDamage : WeatherEvent
	{
		public override bool Expired
		{
			get
			{
				return true;
			}
		}
		public WeatherEvent_VacuumDamage(Map map) : base(map)
		{
		}
		public override void WeatherEventTick()
		{
		}

		public override void FireEvent()
		{
			List<Pawn> allPawns = map.mapPawns.AllPawnsSpawned.Where(p => !p.Dead).ToList();
			foreach (Pawn pawn in allPawns)
			{
				if (!pawn.HarmedByVacuum)
				{
					continue;
				}

				Room room = pawn.Position.GetRoom(map);

				if (ShipInteriorMod2.ExposedToOutside(room))
				{
					if (ActivateSpaceBubble(pawn))
					{
						continue;
					}
				}
				else if (!map.GetComponent<ShipMapComp>().VecHasLS(pawn.Position)) // in ship, no air
				{
					if (ActivateSpaceBubble(pawn))
					{
						continue;
					}
				}
			}
		}

		public bool ActivateSpaceBubble(Pawn pawn)
		{
			Verb verb = pawn?.apparel?.AllApparelVerbs?.FirstOrDefault(apparel => apparel is Verb_SpaceBubblePop);
			if (verb?.Available() ?? false)
			{
				verb.TryStartCastOn(pawn);
				return true;
			}
			return false;
		}
	}
}
