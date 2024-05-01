using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace ExtraHives
{
	// ExtraHives.IncidentWorker_Infestation
	public class IncidentWorker_Infestation : IncidentWorker
	{
		public const float HivePoints = 220f;
		private Faction Faction = null; 
		public override bool CanFireNowSub(IncidentParms parms)
		{
		//	Faction = null;
			Map map = (Map)parms.target;
			if (def.mechClusterBuilding == null)
			{
				Log.Warning($"ExtraHives Infestation tried CanFireNowSub {this.def.defName} with no mechClusterBuilding");
				return false;
			}
			if (Faction == null && this.GetFactionFromParms(parms) == null)
			{
				Log.Warning($"ExtraHives Infestation tried GetFactionFromParms {this.def.defName} but found not matching faction");
				return false;
			}
			InfestationCellFinder.InfestationParms infestationParms;
			IntVec3 cell;
			FactionDef factionDef = null;
			if (def is HivelikeIncidentDef incidentDef)
			{
				infestationParms = new InfestationCellFinder.InfestationParms(incidentDef);
				factionDef = incidentDef.Faction;
			}
			else
			{
				if (!def.mechClusterBuilding.HasModExtension<HiveDefExtension>())
				{
					Log.Warning($"ExtraHives.IncidentWorker_Infestation tried CanFireNowSub {this.def.defName} with a mechClusterBuilding with no HiveDefExtension");
					return false;
				}
				HiveDefExtension HiveExt = def.mechClusterBuilding.GetModExtension<HiveDefExtension>();
				factionDef = HiveExt.Faction;
				infestationParms = new InfestationCellFinder.InfestationParms(HiveExt);
			}
			if (factionDef == null) return false;
            if (factionDef.earliestRaidDays > GenDate.DaysPassedSinceSettle) return false;
            Faction faction = Find.FactionManager.FirstFactionOfDef(factionDef);
			if (faction == null) return false;
            if (HiveUtility.TotalSpawnedHivesCount(map, def.mechClusterBuilding) < 30)
			{

				if (InfestationCellFinder.TryFindCell(out cell, map, infestationParms))
				{
					return base.CanFireNowSub(parms);
				}
				else
				{
				//	Log.Warning($"ExtraHives.IncidentWorker_Infestation tried CanFireNowSub {this.def.defName} with HivelikeIncidentDef but TryFindCell failed");
				}
			}
			return false;
		}

		public override bool TryExecuteWorker(IncidentParms parms)
		{
			if (def.mechClusterBuilding == null && this.GetFactionFromParms(parms, false) == null)
			{
				return false;
			}
			Map map = (Map)parms.target;
			CompProperties_SpawnerPawn spawnerPawn = def.mechClusterBuilding.GetCompProperties<CompProperties_SpawnerPawn>();
			float points = spawnerPawn?.initialPawnsPoints ?? 250f;
			int count = Mathf.Max(GenMath.RoundRandom(parms.points / points), 1);
            if (Prefs.DevMode)
            {
				Log.Message($"ExtraHives trying {this.def.LabelCap} with {parms.points} Points");
            }

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
					//	Log.Warning($"ExtraHives Infestation HivelikeIncidentDef: {this.def.defName}" + factionDef.defName);
					}
					else
					{
					//	Log.Warning($"ExtraHives Infestation IncidentDef: {this.def.defName}");
						if (def.mechClusterBuilding.HasModExtension<HiveDefExtension>())
						{
							HiveDefExtension ext = def.mechClusterBuilding.GetModExtension<HiveDefExtension>();
							factionDef = ext.Faction;
						//	Log.Warning($"ExtraHives Infestation HiveDefExtension: {this.def.defName}" + factionDef.defName);
						}
					}
                    if (factionDef == null)
					{
						Log.Error($"ExtraHives Infestation factionDef Not found for: {this.def.defName}");
						return null;
					}
					if (factionDef != null)
					{
						Faction = Find.FactionManager.FirstFactionOfDef(factionDef);
						if (Faction == null)
						{
						//	Log.Warning($"ExtraHives Infestation cant find a Faction of def: {this.def.defName} {factionDef.defName}");
                            if (factionDef.canMakeRandomly)
							{
							//	Log.Warning($"Attempting to generate new Faction of def: {this.def.defName} {factionDef.defName}");
								Faction = FactionGenerator.NewGeneratedFaction(new FactionGeneratorParms(factionDef, default, factionDef.hidden));
                                if (Faction == null)
								{
									Log.Error("Failed to generate new Faction of def: " + factionDef.defName);
									return null;
								}
							}
						}
					}
				}
				else
				{
					if (Prefs.DevMode)
                    {
                       	Log.Warning($"ExtraHives Infestation IncidentDef: {this.def.defName} using cached faction");
                    }
                }
				return Faction;
            }
        }
		public Faction GetFactionFromParms(IncidentParms parms, bool CanFireNow = true)
		{
			if (Faction == null)
			{
                if (parms.faction != null)
				{
					Faction = parms.faction;
                    if (Prefs.DevMode) Log.Warning($"{this.def.LabelCap} using parms.faction {parms.faction.Name} CanFireNow: {CanFireNow}");
                }
                else
				{
					parms.faction = this.GetFaction;
					Faction = parms.faction;
                    if (Prefs.DevMode) Log.Warning($"{this.def.LabelCap} using this.GetFaction {this.GetFaction} CanFireNow: {CanFireNow}");
                }
			}
            if (Faction == null)
            {
				Log.Warning($"{this.def.LabelCap} Failed to find viable faction");
            }
			return Faction;
		}
	}

}