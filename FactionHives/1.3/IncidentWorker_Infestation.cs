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

		public override bool CanFireNowSub(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			bool result = false;
			if (def.mechClusterBuilding == null)
			{
				Log.Warning("ExtraHives.IncidentWorker_Infestation tried CanFireNowSub with no mechClusterBuilding");
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
						Log.Warning("ExtraHives.IncidentWorker_Infestation tried CanFireNowSub with HivelikeIncidentDef but TryFindCell failed");
					}
				}
			}
			else
			{
				if (!def.mechClusterBuilding.HasModExtension<HiveDefExtension>())
				{
					Log.Warning("ExtraHives.IncidentWorker_Infestation tried CanFireNowSub with a mechClusterBuilding with no HiveDefExtension");
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
						Log.Warning("ExtraHives.IncidentWorker_Infestation tried CanFireNowSub with HiveDefExtension but TryFindCell failed");
					}
				}
			}
			return false;
		}

		public override bool TryExecuteWorker(IncidentParms parms)
		{
			if (def.mechClusterBuilding == null)
			{
				return false;
			}
			FactionDef factionDef;
			if (def is HivelikeIncidentDef incidentDef)
			{
				factionDef = incidentDef.Faction;
			}
            else
			{
				if (!def.mechClusterBuilding.HasModExtension<HiveDefExtension>())
				{
					return false;
				}
				HiveDefExtension ext = def.mechClusterBuilding.GetModExtension<HiveDefExtension>();
				factionDef = ext.Faction;
			}
			Map map = (Map)parms.target;
			if (parms.faction==null)
			{
				try
				{
					IEnumerable<Faction> factions = Find.FactionManager.AllFactions.Where(x => x.def.defName.Contains(factionDef.defName));
                    if (!factions.EnumerableNullOrEmpty())
					{
						parms.faction = factions.RandomElement();
					}
                    else
                    {
						parms.faction = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(factionDef, default, factionDef.hidden));
					}
				//	Log.Message(parms.faction.def.defName);
				}
				catch (System.Exception)
				{
					Faction faction = Find.FactionManager.FirstFactionOfDef(factionDef);
                    if (faction == null)
                    {
						faction = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(factionDef, default, factionDef.hidden));
                    }
					parms.faction = faction;
				}
			}
			CompProperties_SpawnerPawn spawnerPawn = def.mechClusterBuilding.GetCompProperties<CompProperties_SpawnerPawn>();
			float points = spawnerPawn?.initialPawnsPoints ?? 250f;
			int count = Mathf.Max(GenMath.RoundRandom(parms.points / points), 1);

			Thing t = InfestationUtility.SpawnTunnels(def.mechClusterBuilding, count, map, true, true, faction: parms.faction);
			SendStandardLetter(parms, t);
			Find.TickManager.slower.SignalForceNormalSpeedShort();
			return true;
		}
	}

}