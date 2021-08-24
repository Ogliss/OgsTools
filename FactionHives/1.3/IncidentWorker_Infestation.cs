// RimWorld.IncidentWorker_Infestation
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ExtraHives //ExtraHives.IncidentWorker_Infestation
{
	public class IncidentWorker_Infestation : IncidentWorker
	{
		public const float HivePoints = 220f;
		private Faction Faction = null; 
		public override bool CanFireNowSub(IncidentParms parms)
		{
			Faction = null;
			Map map = (Map)parms.target;
			bool result = false;
			if (def.mechClusterBuilding == null)
			{
				Log.Warning("ExtraHives Infestation tried CanFireNowSub "+this.def.defName+" with no mechClusterBuilding");
				return false;
			}
			if (this.GetFactionFromParms(parms) == null)
			{
				Log.Warning("ExtraHives Infestation tried GetFactionFromParms " + this.def.defName+" but found not matching faction");
				return false;
			}
            if (def is HivelikeIncidentDef incidentDef)
			{
				IntVec3 cell;
				if (/*base.CanFireNowSub(parms) && */HiveUtility.TotalSpawnedHivesCount(map, def.mechClusterBuilding) < 30)
                {
                    if (InfestationCellFinder.TryFindCell(out cell, map, incidentDef))
					{
						return true;
					}
					else
					{
						Log.Warning("ExtraHives.IncidentWorker_Infestation tried CanFireNowSub " + this.def.defName + " with HivelikeIncidentDef but TryFindCell failed");
					}
				}
			}
			else
			{
				if (!def.mechClusterBuilding.HasModExtension<HiveDefExtension>())
				{
					Log.Warning("ExtraHives.IncidentWorker_Infestation tried CanFireNowSub " + this.def.defName + " with a mechClusterBuilding with no HiveDefExtension");
					return false;
				}
				HiveDefExtension HiveExt = def.mechClusterBuilding.GetModExtension<HiveDefExtension>();
				IntVec3 cell;
				if (/*base.CanFireNowSub(parms) && */HiveUtility.TotalSpawnedHivesCount(map, def.mechClusterBuilding) < 30)
				{
					if (InfestationCellFinder.TryFindCell(out cell, map, HiveExt))
					{
						return true;
					}
                    else
					{
						Log.Warning("ExtraHives.IncidentWorker_Infestation tried CanFireNowSub " + this.def.defName + " with HiveDefExtension but TryFindCell failed");
					}
				}
			}
			return false;
		}

		public override bool TryExecuteWorker(IncidentParms parms)
		{
			if (def.mechClusterBuilding == null || this.GetFactionFromParms(parms) == null)
			{
				return false;
			}
			Map map = (Map)parms.target;
			CompProperties_SpawnerPawn spawnerPawn = def.mechClusterBuilding.GetCompProperties<CompProperties_SpawnerPawn>();
			float points = spawnerPawn?.initialPawnsPoints ?? 250f;
			int count = Mathf.Max(GenMath.RoundRandom(parms.points / points), 1);

			Thing t = InfestationUtility.SpawnTunnels(def.mechClusterBuilding, count, map, true, true, faction: parms.faction);
			SendStandardLetter(parms, t);
			Find.TickManager.slower.SignalForceNormalSpeedShort();
			return true;
		}

		public Faction GetFaction
        {
            get
			{
                if (Faction == null)
				{
					FactionDef factionDef = null;

					if (def is HivelikeIncidentDef incidentDef)
					{
						factionDef = incidentDef.Faction;
					}
					else
					{
						if (!def.mechClusterBuilding.HasModExtension<HiveDefExtension>())
						{
							HiveDefExtension ext = def.mechClusterBuilding.GetModExtension<HiveDefExtension>();
							factionDef = ext.Faction;
						}
					}
					if (factionDef != null)
					{
						Faction = Find.FactionManager.FirstFactionOfDef(factionDef);
						if (Faction == null)
						{
							Log.Warning("ExtraHives Infestation cant find a Faction of def: "+ factionDef.defName);
                            if (factionDef.canMakeRandomly)
							{
								Log.Warning("Attempting to generate new Faction of def: " + factionDef.defName);
								Faction = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(factionDef, default, factionDef.hidden));
                                if (Faction == null)
								{
									Log.Error("Failed to generate new Faction of def: " + factionDef.defName);
								}
							}
						}
					}
				}
				return Faction;
            }
        }
		public Faction GetFactionFromParms(IncidentParms parms)
		{
			if (Faction == null)
			{
                if (parms.faction != null)
                {
					Faction = parms.faction;
				}
                else
                {
					parms.faction = this.GetFaction;
				}
			}
			return Faction;
		}
	}

}