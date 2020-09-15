using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System;
using Verse.AI;
using System.Text;
using System.Linq;
using Verse.AI.Group;
using RimWorld.Planet;
using UnityEngine;
using HunterMarkingSystem.Settings;
using HunterMarkingSystem.ExtensionMethods;
using static HunterMarkingSystem.HMSUtility;

namespace HunterMarkingSystem
{
    [HarmonyPatch(typeof(RecordsUtility), "Notify_PawnKilled")]
    public static class HMS_RecordsUtility_Notify_PawnKilled_Patch
    {
        [HarmonyPostfix]
        public static void IncrementPostfix(Pawn killed, Pawn killer)
        {
            if (killer != null && killer.IsColonist && killer.isBloodable() && killer.Markable(out Comp_Markable Markable))
            {
                // if (Prefs.DevMode) Log.Message(string.Format("{0} killed {1}", killer.LabelShortCap, killed.LabelShortCap));
                if (!Markable.MarkerRace)
                {
                    // if (Prefs.DevMode) Log.Message("not a race that can mark themself");
                    if (!killer.Map.mapPawns.FreeColonists.Any(x=> Markable.markerRaces.Contains(x.def)))
                    {
                        // if (Prefs.DevMode) Log.Message("No race that can mark in colony");
                        return;
                    }
                    else
                    {
                        // if (Prefs.DevMode) Log.Message("Race that can mark in colony");
                    }
                }
                else
                {
                    // if (Prefs.DevMode) Log.Message("Race that can mark themself");
                }
                if (Markable.BloodStatus == BloodStatusMode.None )
                {
                    killer.health.AddHediff(HunterMarkingSystem.UnbloodedHediffList.First(), Markable.partRecord);
                    Markable.BloodStatus = BloodStatusMode.Unblooded;
                }
                if (killed.isWorthyKillFor(killer, out MarkData markData))
                {
                    bool use = true;
                    if (markData.MarkScore < Markable.MarkScore)
                    {
                        use = false;
                    }
                    if (use && (Markable.inductable || Markable.Inducted))
                    {
                        // if (Prefs.DevMode) Log.Message(string.Format("use: {0}, inductable || Inducted: {1}", use, Markable.inductable || Markable.Inducted));
                        if (killer.health.hediffSet.hediffs.Any(x => HunterMarkingSystem.UnbloodedHediffList.Contains(x.def)))
                        {
                            killer.health.RemoveHediff(killer.health.hediffSet.hediffs.First(x=> HunterMarkingSystem.UnbloodedHediffList.Contains(x.def)));
                        }
                        if (killer.health.hediffSet.hediffs.Any(x=> HunterMarkingSystem.BloodedUMHediffList.Contains(x.def)))
                        {
                            killer.health.RemoveHediff(killer.health.hediffSet.hediffs.First(x => HunterMarkingSystem.BloodedUMHediffList.Contains(x.def)));
                        }

                        killer.health.AddHediff(HMSDefOf.HMS_Hediff_BloodedUM, Markable.partRecord);
                        Markable.Mark = killed;
                        if (Markable.Markcorpse != null)
                        {
                            // if (Prefs.DevMode) Log.Message(string.Format("Markable.Markcorpse = {0}", Markable.Markcorpse.LabelShortCap));
                        }
                        Markable.markDataKillNew = markData;
                        killed.Corpse.SetForbidden(true, false);
                    }
                    else
                    {
                        // if (Prefs.DevMode) Log.Warning(string.Format("Failed!!! use: {0}, inductable: {1}", use, Markable.inductable));
                    }
                }
                else
                {
                    // if (Prefs.DevMode) Log.Warning(string.Format("Unworthy Kill: {0}",killed.LabelShortCap));
                }
            }
            else
            {
                // if (Prefs.DevMode) Log.Warning(string.Format("Killer: {0}, IsColonist: {1}, isBloodable: {2}", killer != null, killer.IsColonist, killer.isBloodable()));
            }
            /*
            if (killer.isYautja())
            {
                List<Thought_Memory> _Memories = killer.needs.mood.thoughts.memories.Memories.FindAll(x => x.def == HMSDefOf.AvP_Thought_ThrillOfTheHunt);
                if (_Memories.Count < HMSDefOf.AvP_Thought_ThrillOfTheHunt.stackLimit)
                {
                    killer.needs.mood.thoughts.memories.Memories.Add(new Thought_Memory()
                    {
                        def = HMSDefOf.AvP_Thought_ThrillOfTheHunt
                    });
                }
            }
            */
        }
    }
}