using PipeSystem;
using RimWorld;
using RimWorld.BaseGen;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SaveOurShip2
{
	public class SymbolResolver_Ship_Pregen_New : SymbolResolver
	{
		private struct SpawnDescriptor
		{
			public IntVec3 offset;
			public Rot4 rot;
		}

		public override void Resolve(ResolveParams rp)
		{
			List<Building> cores = new List<Building>();
            List<PipeNet> lastPipeNets = new List<PipeNet>();
            try { ShipInteriorMod2.GenerateShip(DefDatabase<ShipDef>.GetNamed("CharlonWhitestone"), BaseGen.globalSettings.map, null, Faction.OfPlayer, null, out cores, false, true); } catch (Exception e) { Log.Error(e.ToString()); }
			foreach(Thing thing in BaseGen.globalSettings.map.listerThings.ThingsInGroup(ThingRequestGroup.Refuelable))
			{
				((ThingWithComps)thing).TryGetComp<CompRefuelable>().Refuel(9999);
                CompRefillWithPipes pipeComp = thing.TryGetComp<CompRefillWithPipes>();
                if (pipeComp != null)
                {
                    if (lastPipeNets.Contains(pipeComp.PipeNet))
                    {
                        continue;
                    }
					foreach (CompResourceStorage compResource in pipeComp.PipeNet.storages)
					{
                        compResource.AddResource(9999);
                    }
                    lastPipeNets.Add(pipeComp.PipeNet);
                }
            }
			cores.FirstOrFallback().TryGetComp<CompBuildingConsciousness>().AIName = "Charlon Whitestone";
		}
	}
}
