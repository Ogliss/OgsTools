using RimWorld;
using System;
using System.Reflection;
using Verse;

namespace HunterMarkingSystem
{
    [StaticConstructorOnStartup]
    static class UtilCE
    {
        public static bool CombatExtended = false;
        public static ModContentPack modContent = null;
        static UtilCE()
        {
            foreach (ModContentPack p in LoadedModManager.RunningMods)
            {
                if (p.PackageIdPlayerFacing.Contains("CETeam.CombatExtended"))
                {
                    modContent = p;
                    CombatExtended = true;
                }
            }
        }

    }

    [StaticConstructorOnStartup]
    static class UtilAvPSynths
    {
        private static bool initialized = false;
        public static bool AvP = false;
        static UtilAvPSynths()
        {
            if (!initialized)
            {
                foreach (ModContentPack p in LoadedModManager.RunningMods)
                {
                    if (p.PackageId == "Ogliss.AlienVsPredator" || p.Name == "Alien Vs Predator")
                    {
                        AvP = true;
                    }
                }
                initialized = true;
            }
        }

        public static bool isAvPSynth(PawnKindDef pawn)
        {
            bool Result = pawn.RaceProps.FleshType.defName == "AvP_SynthFlesh";

            return Result;
        }
        public static bool isAvPSynth(Pawn pawn)
        {
            bool Result = pawn.def.race.FleshType.defName == "AvP_SynthFlesh";

            return Result;
        }
        public static bool isAvPSynth(ThingDef td)
        {
            bool Result = td.race.FleshType.defName == "AvP_SynthFlesh";

            return Result;
        }
    }

    [StaticConstructorOnStartup]
    static class UtilChjAndroids
    {
        public static string tag = "ChJees.Androids";
        public static bool ChjAndroid = false;
        public static ModContentPack modContent = null;
        static UtilChjAndroids()
        {
            foreach (ModContentPack p in LoadedModManager.RunningMods)
            {
                if (p.PackageIdPlayerFacing == tag || p.PackageId == tag)
                {
                    modContent = p;
                    ChjAndroid = true;
                }
            }

        }

        public static bool isChjAndroid(PawnKindDef pawn)
        {
            return pawn.race.modContentPack == modContent;
        }
        public static bool isChjAndroid(Pawn pawn)
        {
            return pawn.def.modContentPack == modContent;
        }
        public static bool isChjAndroid(ThingDef td)
        {
            if (td.modContentPack == null)
            {
            //    Log.Warning(string.Format("{0}: modContentPack = NULL", td.LabelCap));
                return false;
            }
            return !td.modContentPack.PackageId.NullOrEmpty() ? td.modContentPack == modContent : false;
        }
    }

    [StaticConstructorOnStartup]
    static class UtilTieredAndroids
    {
        public static string tag = "Atlas.AndroidTiers";
        public static bool TieredAndroid = false;
        public static ModContentPack modContent = null;
        static UtilTieredAndroids()
        {
            foreach (ModContentPack p in LoadedModManager.RunningMods)
            {
                if (p.PackageIdPlayerFacing == tag || p.PackageId == tag)
                {
                    modContent = p;
                    //    Log.Message(string.Format("{0}: PackageId: {1}, PackageIdPlayerFacing: {2}", p.Name, p.PackageId, p.PackageIdPlayerFacing)); Haplo.Miscellaneous.Robots
                    TieredAndroid = true;
                }
            }
        }

        public static bool isAtlasAndroid(PawnKindDef pawn)
        {
            return pawn.race.modContentPack == modContent;
        }
        public static bool isAtlasAndroid(Pawn pawn)
        {
            return pawn.def.modContentPack == modContent;
        }
        public static bool isAtlasAndroid(ThingDef td)
        {
            if (td.modContentPack == null)
            {
            //    Log.Warning(string.Format("{0}: modContentPack = NULL", td.LabelCap));
                return false;
            }
            return !td.modContentPack.PackageId.NullOrEmpty() ? td.modContentPack == modContent : false;
        }
    }

    [StaticConstructorOnStartup]
    static class UtilMiscRobots
    {
        public static string tag = "Haplo.Miscellaneous.Robots";
        public static bool TieredAndroid = false;
        public static ModContentPack modContent = null;
        static UtilMiscRobots()
        {
            foreach (ModContentPack p in LoadedModManager.RunningMods)
            {
                if (p.PackageIdPlayerFacing == tag || p.PackageId == tag)
                {
                    modContent = p;
                    //    Log.Message(string.Format("{0}: PackageId: {1}, PackageIdPlayerFacing: {2}", p.Name, p.PackageId, p.PackageIdPlayerFacing)); Alaestor.MiscRobots.PlusPlus
                    TieredAndroid = true;
                }
            }
        }

        public static bool isMiscRobot(PawnKindDef pawn)
        {
            return pawn.race.modContentPack == modContent;
        }
        public static bool isMiscRobot(Pawn pawn)
        {
            return pawn.def.modContentPack == modContent;
        }
        public static bool isMiscRobot(ThingDef td)
        {
            if (td.modContentPack == null)
            {
            //    Log.Warning(string.Format("{0}: modContentPack = NULL", td.LabelCap));
                return false;
            }
            return !td.modContentPack.PackageId.NullOrEmpty() ? td.modContentPack == modContent : false;
        }
    }
    [StaticConstructorOnStartup]
    static class UtilMiscRobotsPP
    {
        public static string tag = "Alaestor.MiscRobots.PlusPlus";
        public static bool TieredAndroid = false;
        public static ModContentPack modContent = null;
        static UtilMiscRobotsPP()
        {
            foreach (ModContentPack p in LoadedModManager.RunningMods)
            {
                if (p.PackageIdPlayerFacing == tag || p.PackageId == tag)
                {
                    modContent = p;
                    //    Log.Message(string.Format("{0}: PackageId: {1}, PackageIdPlayerFacing: {2}", p.Name, p.PackageId, p.PackageIdPlayerFacing)); Alaestor.MiscRobots.PlusPlus
                    TieredAndroid = true;
                }
            }
        }

        public static bool isMiscRobot(PawnKindDef pawn)
        {
            return pawn.race.modContentPack == modContent;
        }
        public static bool isMiscRobot(Pawn pawn)
        {
            return pawn.def.modContentPack == modContent;
        }
        public static bool isMiscRobot(ThingDef td)
        {
            if (td.modContentPack == null)
            {
            //    Log.Warning(string.Format("{0}: modContentPack = NULL", td.LabelCap));
                return false;
            }
            return !td.modContentPack.PackageId.NullOrEmpty() ? td.modContentPack == modContent : false;
        }
    }

    [StaticConstructorOnStartup]
    static class UtilDinosauria
    {
        public static string tag = "spincrus.dinosauria";
        public static bool Dinosauria = false;
        public static ModContentPack modContent = null;
        static UtilDinosauria()
        {
            foreach (ModContentPack p in LoadedModManager.RunningMods)
            {
                if (p.PackageIdPlayerFacing == tag || p.PackageId == tag)
                {
                    modContent = p;
                    Dinosauria = true;
                }
            }
        }

        public static bool isDinosauria(PawnKindDef pawn)
        {
            return pawn.race.modContentPack == modContent;
        }
        public static bool isDinosauria(Pawn pawn)
        {
            return pawn.def.modContentPack == modContent;
        }
        public static bool isDinosauria(ThingDef td)
        {
            if (td.modContentPack == null)
            {
            //    Log.Warning(string.Format("{0}: modContentPack = NULL", td.LabelCap));
                return false;
            }
            return !td.modContentPack.PackageId.NullOrEmpty() ? td.modContentPack == modContent : false;
        }

    }

    [StaticConstructorOnStartup]
    static class UtilJurassicRimworld
    {
        public static string tag = "Serpy.JurassicRimworld";
        public static bool JurassicRimworld = false;
        public static ModContentPack modContent = null;
        static UtilJurassicRimworld()
        {
            foreach (ModContentPack p in LoadedModManager.RunningMods)
            {
                if (p.PackageIdPlayerFacing == tag || p.PackageId == tag)
                {
                    modContent = p;
                    JurassicRimworld = true;
                }
            }
        }

        public static bool isJurassic(PawnKindDef pawn)
        {
            return pawn.race.modContentPack == modContent;
        }
        public static bool isJurassic(Pawn pawn)
        {
            return pawn.def.modContentPack == modContent;
        }
        public static bool isJurassic(ThingDef td)
        {
            if (td.modContentPack == null)
            {
            //    Log.Warning(string.Format("{0}: modContentPack = NULL", td.LabelCap));
                return false;
            }
            return !td.modContentPack.PackageId.NullOrEmpty() ? td.modContentPack == modContent : false;
        }
    }
}