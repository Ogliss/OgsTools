using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using Verse.Sound;
using UnityEngine;
using System.Reflection;
using AdvancedGraphics;

namespace AdvancedGraphics.HarmonyInstance
{
    [HarmonyPatch(typeof(VerbTracker), "CreateVerbTargetCommand")]
    public static class VerbTracker_CreateVerbTargetCommand_AdvancedGraphics_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(VerbTracker __instance, Thing ownerThing, Verb verb, ref Command_VerbTarget __result)
        {
            if (__instance != null)
            {
                if (ownerThing.Graphic is Graphic_SingleQuality)
                {

                    Log.Message("CreateVerbTargetCommand_AdvancedGraphics start: " + __result.icon.name);
                    __result.icon = ownerThing.Graphic.MatSingleFor(ownerThing).mainTexture as Texture2D;
                    __result.defaultIconColor = ownerThing.Graphic.color;
                }
                /*
                if (true)
                {
                    __result.icon = ownerThing.Graphic.MatSingleFor(ownerThing).mainTexture as Texture2D;
                }
                */
            }
        }
    }
}