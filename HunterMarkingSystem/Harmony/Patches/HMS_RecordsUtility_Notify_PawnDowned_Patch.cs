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
    /*
    [HarmonyPatch(typeof(RecordsUtility), "Notify_PawnDowned")]
    public static class HMS_RecordsUtility_Notify_PawnDowned_Patch
    {
        [HarmonyPostfix]
        public static void IncrementPostfix(Pawn downed, Pawn instigator)
        {
        //    Log.Message(string.Format("{0} downed {1}", instigator.LabelShortCap, downed.LabelShortCap));
            if (instigator != null && instigator.IsColonist && instigator.isBloodable() && instigator.Markable() is Comp_Markable Markable)
            {
                if (!Markable.MarkerRace)
                {
                //    Log.Message("not a race that can mark themself");
                    if (!instigator.Map.mapPawns.FreeColonists.Any(x => Markable.markerRaces.Contains(x.def)))
                    {
                    //    Log.Message("No race that can mark in colony");
                        return;
                    }
                    else
                    {
                    //    Log.Message("Race that can mark in colony");
                    }
                }
                else
                {
                //    Log.Message("Race that can mark themself");
                }
                if (Markable.BloodStatus == BloodStatusMode.None)
                {
                    instigator.health.AddHediff(HMSDefOf.HMS_Hediff_Unblooded, Markable.partRecord);
                }
                if (downed.isWorthyKillFor(instigator))
                {
                    MarkData markData = new MarkData(downed);
                    bool use = true;
                    if (markData.MarkScore < Markable.MarkScore)
                    {
                        use = false;
                    }
                    if (use)
                    {
                        if (instigator.health.hediffSet.HasHediff(HMSDefOf.HMS_Hediff_Unblooded))
                        {
                            instigator.health.RemoveHediff(instigator.health.hediffSet.GetFirstHediffOfDef(HMSDefOf.HMS_Hediff_Unblooded));
                        }
                        if (instigator.health.hediffSet.HasHediff(HMSDefOf.HMS_Hediff_Unblooded))
                        {
                            instigator.health.RemoveHediff(instigator.health.hediffSet.GetFirstHediffOfDef(HMSDefOf.HMS_Hediff_BloodedUM));
                        }
                        instigator.health.AddHediff(HMSDefOf.HMS_Hediff_BloodedUM, Markable.partRecord);
                        Markable.Mark = downed;
                        Markable.markDataKill = markData;
                    }
                }
            }
            //
            if (instigator.isYautja())
            {
                List<Thought_Memory> _Memories = instigator.needs.mood.thoughts.memories.Memories.FindAll(x => x.def == YautjaDefOf.AvP_Thought_ThrillOfTheHunt);
                if (_Memories.Count < HMSDefOf.AvP_Thought_ThrillOfTheHunt.stackLimit)
                {
                    instigator.needs.mood.thoughts.memories.Memories.Add(new Thought_Memory()
                    {
                        def = HMSDefOf.AvP_Thought_ThrillOfTheHunt
                    });
                }
            }
            //
        }
    }
    */
}