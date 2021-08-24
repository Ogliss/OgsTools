using RimWorld;
using Verse;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Recruiters
{
    [StaticConstructorOnStartup]
    class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.ogliss.rimworld.mod.RecruitShips");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
    // Token: 0x02000020 RID: 32
    public class RecruiterExt : DefModExtension
    {

    }

}