﻿using System;
using System.Collections.Generic;
using HugsLib.Utils;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace GasTech
{
	/// <summary>
	/// Stores custom negative goodwill caps with non-player factions.
	/// Lowering the cap circumvents the trader gassing exploit that allows to rob a trader, 
	/// release them, and end up with net positive faction goodwill in the end.
	/// </summary>
	public class FactionGoodwillCaps : WorldComponent
	{
		public const int DefaultMinNegativeGoodwill = -100;
		public const int NegativeGoodwillCap = -5000;

		public static FactionGoodwillCaps GetFromWorld()
		{
			return Find.World.GetComponent<FactionGoodwillCaps>()
				?? throw new NullReferenceException($"Failed to get {nameof(FactionGoodwillCaps)} from world");
		}

		private Dictionary<int, int> goodwillCaps = new Dictionary<int, int>();
		private HashSet<int> betrayedFactions = new HashSet<int>();

		public FactionGoodwillCaps(World world) : base(world)
		{
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look(ref goodwillCaps, "goodwillCaps", LookMode.Value, LookMode.Value);
			Scribe_Collections.Look(ref betrayedFactions, "betrayedFactions", LookMode.Value);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				if (goodwillCaps == null) goodwillCaps = new Dictionary<int, int>();
				if (betrayedFactions == null) betrayedFactions = new HashSet<int>();
			}
		}

		public void SetMinNegativeGoodwill(Faction faction, int minGoodwill)
		{
			goodwillCaps[faction.loadID] = Mathf.Max(NegativeGoodwillCap, minGoodwill);
		}

		public int GetMinNegativeGoodwill(Faction faction)
		{
			if (false)
			{
				var factionId = faction.loadID;
				if (goodwillCaps.ContainsKey(factionId))
				{
					return goodwillCaps[factionId];
				}
			}
			return DefaultMinNegativeGoodwill;
		}

		public bool HasPlayerBetrayedFaction(Faction faction)
		{
			return betrayedFactions.Contains(faction.loadID);
		}

		public void OnPlayerBetrayedFaction(Faction faction)
		{
			betrayedFactions.Add(faction.loadID);
		}
	}

	// This ensures that existing data in saved games is properly loaded
	// TodoMajor: remove during the next major update
#pragma warning disable 618 // Obsolete warning
	public class CustomFactionGoodwillCaps : UtilityWorldObject
	{
#pragma warning restore 618
		public override void ExposeData()
		{
			base.ExposeData();
			if (Scribe.mode == LoadSaveMode.PostLoadInit && !Destroyed)
			{
				Destroy();
			}
		}
	}
}