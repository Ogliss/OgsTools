using HunterMarkingSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace RimWorld
{
    // Token: 0x020009F9 RID: 2553
    public class StatWorker_MarkScore : StatWorker
    {
        // Token: 0x06003965 RID: 14693 RVA: 0x001B418C File Offset: 0x001B258C
        public override bool ShouldShowFor(StatRequest req)
        {
            ThingDef thingDef = req.Def as ThingDef;
            if (thingDef == null)
            {
                return false;
            }
            if (thingDef.race == null)
            {
                return false;
            }
            return true;
        }

        Pawn p;
        Intelligence intelligence = Intelligence.Animal;
        float meleepower = 0;
        float meleecooldown = 0;
        float MeleeDPS = 0;
        float meleeDamageFactor = 0;
        float baseHealthScale = 0;
        float healthScaleFactor = 0;
        float baseBodySize = 0;
        float bodySizeFactor = 0;
        float ArmorRating_Blunt = 0;
        float ArmorRating_Sharp = 0;
        float ArmorRating_Heat = 0;
        float MeleeHitChance = 0;
        float MeleeDodgeChance = 0;
        float basescore = 0;
        float meleescore = 0;
        float healthscore = 0;
        float sizescore = 0;
        float armourscore = 0;
        float num = 0;
        bool Humanlike = false;
        bool Predator = false;
        bool factionLeader = false;
        // Token: 0x06003966 RID: 14694 RVA: 0x001B4210 File Offset: 0x001B2610
        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            ThingDef thingDef = req.Def as ThingDef;
            if (thingDef == null)
            {
                return 0f;
            }
            if (thingDef.race == null)
            {
                return 0f;
            }
            if (req.Thing!=null)
            {
                p = (Pawn)req.Thing;
                intelligence = p.RaceProps.intelligence;
                meleepower = 0;
                meleecooldown = 0;

                p.def.tools.ForEach(x => meleepower += x.power);
                p.def.tools.ForEach(x => meleecooldown += x.cooldownTime);
                MeleeDPS = meleepower / meleecooldown;
                meleeDamageFactor = p.ageTracker.CurLifeStage.meleeDamageFactor;
                baseHealthScale = p.def.race.baseHealthScale;
                healthScaleFactor = p.ageTracker.CurLifeStage.healthScaleFactor;
                baseBodySize = p.def.race.baseBodySize;
                bodySizeFactor = p.ageTracker.CurLifeStage.bodySizeFactor;
                ArmorRating_Blunt = TryDrawOverallArmor(p, StatDefOf.ArmorRating_Blunt);
                ArmorRating_Sharp = TryDrawOverallArmor(p, StatDefOf.ArmorRating_Sharp);
                ArmorRating_Heat = TryDrawOverallArmor(p, StatDefOf.ArmorRating_Heat);
                MeleeHitChance = p.def.GetStatValueAbstract(StatDefOf.MeleeHitChance);
                MeleeDodgeChance = p.def.GetStatValueAbstract(StatDefOf.MeleeDodgeChance);
                Humanlike = p.RaceProps.Humanlike;
                Predator = p.RaceProps.predator;
                factionLeader = p.kindDef.factionLeader;
            }
            else
            {
                intelligence = thingDef.race.intelligence;
                if (!thingDef.tools.NullOrEmpty())
                {
                    thingDef.tools.ForEach(x => meleepower += x.power);
                    thingDef.tools.ForEach(x => meleecooldown += x.cooldownTime);
                    MeleeDPS = meleepower / meleecooldown;
                    meleeDamageFactor = 1f;
                }
                else
                {
                    MeleeDPS = 0;
                    meleeDamageFactor = 0;
                }
                try
                {
                    baseHealthScale = thingDef.race.baseHealthScale;
                    healthScaleFactor = 1f;
                }
                catch (Exception)
                {
                //    Log.Message(string.Format("health"));
                }
                try
                {
                    baseBodySize = thingDef.race.baseBodySize;
                    bodySizeFactor = 1f;
                }
                catch (Exception)
                {
                //    Log.Message(string.Format("size"));
                }
                try
                {

                    ArmorRating_Blunt = thingDef.GetStatValueAbstract(StatDefOf.ArmorRating_Blunt);
                    ArmorRating_Sharp = thingDef.GetStatValueAbstract(StatDefOf.ArmorRating_Sharp);
                    ArmorRating_Heat = thingDef.GetStatValueAbstract(StatDefOf.ArmorRating_Heat);
                }
                catch (Exception)
                {
                //    Log.Message(string.Format("armour"));
                }
                try
                {
                    MeleeHitChance = thingDef.GetStatValueAbstract(StatDefOf.MeleeHitChance);
                    MeleeDodgeChance = thingDef.GetStatValueAbstract(StatDefOf.MeleeDodgeChance);
                }
                catch (Exception)
                {
                //    Log.Message(string.Format("melee chances"));
                }
                try
                {
                    Humanlike = thingDef.race.Humanlike;
                }
                catch (Exception)
                {
                //    Log.Message(string.Format("Humanlike"));
                }
                try
                {
                    Predator = thingDef.race.predator;
                }
                catch (Exception)
                {
                //    Log.Message(string.Format("predator"));
                }
            }
            num = 1f;
            if (Humanlike)
            {
                num += 0.5f;
            }
            if (Predator)
            {
                num += 0.75f;
            }
            if (factionLeader)
            {
                num += 1f;
            }
            num += (float)intelligence;
            basescore = num;
            meleescore = (MeleeDPS * meleeDamageFactor) * (1 * (MeleeHitChance + MeleeDodgeChance));
            healthscore = (baseHealthScale * healthScaleFactor);
            sizescore = (baseBodySize * bodySizeFactor) * 0.75f;
            armourscore = (ArmorRating_Blunt + ArmorRating_Sharp + ArmorRating_Heat) / 3;
            num = meleescore + ((basescore) + ((healthscore + sizescore) * (1 + armourscore)));
            // num *= (((MeleeDPS * meleeDamageFactor) * ((baseHealthScale * healthScaleFactor) + (baseBodySize * bodySizeFactor))) * (1 * ((ArmorRating_Blunt + ArmorRating_Sharp + ArmorRating_Heat) + (MeleeHitChance + MeleeDodgeChance))));

            //    Log.Message(string.Format("markscore for thing"));
            return num;
        }

        private float TryDrawOverallArmor(Pawn p, StatDef stat)
        {
            float num = 0f;
            float num2 = Mathf.Clamp01(p.GetStatValue(stat, true) / 2f);
            List<BodyPartRecord> allParts = p.RaceProps.body.AllParts;
            List<Apparel> list = (p.apparel == null) ? null : p.apparel.WornApparel;
            for (int i = 0; i < allParts.Count; i++)
            {
                float num3 = 1f - num2;
                if (list != null)
                {
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (list[j].def.apparel.CoversBodyPart(allParts[i]))
                        {
                            float num4 = Mathf.Clamp01(list[j].GetStatValue(stat, true) / 2f);
                            num3 *= 1f - num4;
                        }
                    }
                }
                num += allParts[i].coverageAbs * (1f - num3);
            }
            num = Mathf.Clamp(num * 2f, 0f, 2f);
            return num;
        }
        
        // Token: 0x06003967 RID: 14695 RVA: 0x001B4310 File Offset: 0x001B2710
        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            ThingDef thingDef = req.Def as ThingDef;
            if (thingDef == null)
            {
                return null;
            }
            if (thingDef.race == null)
            {
                return null;
            }
            /*
            float num = 1f;
            if (Humanlike)
            {
                num += 0.5f;
            }
            if (Predator)
            {
                num += 2f;
            }
            if (factionLeader)
            {
                num += 3f;
            }
            num += (float)intelligence;
            basescore = num;
            meleescore = (MeleeDPS * meleeDamageFactor) * (1 * (MeleeHitChance + MeleeDodgeChance));
            healthscore = (baseHealthScale * healthScaleFactor);
            sizescore = (baseBodySize * bodySizeFactor);
            armourscore = (ArmorRating_Blunt + ArmorRating_Sharp + ArmorRating_Heat)  / 3;
            num = meleescore + ((basescore) + ((healthscore + sizescore) * (1 + armourscore)));
            */
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(string.Format("    {0} = 1" + " "+ intelligence.ToString() + " +"+ (int)intelligence + (Humanlike ? " Humanlike +0.5" : "") + (Predator ? " Predator +2" : "") + (factionLeader ? " Faction Leader +3" : ""), "HMS_MarkScore_Base".Translate(basescore.ToString("F1"))));
            stringBuilder.AppendLine(string.Format("    {0} = " + "(" + MeleeDPS + " * " + meleeDamageFactor + ") X ( 1 X (" + MeleeHitChance + " + " + MeleeDodgeChance + "))", "HMS_MarkScore_Melee".Translate(meleescore.ToString("F1"))));
            stringBuilder.AppendLine(string.Format("    {0} = "+ "(" + baseHealthScale + " X " + healthScaleFactor  + ")" , "HMS_MarkScore_Health".Translate(healthscore.ToString("F1"))));
            stringBuilder.AppendLine(string.Format("    {0} = " + "(" + baseBodySize + " X "+ bodySizeFactor + ")", "HMS_MarkScore_Size".Translate(sizescore.ToString("F1"))));
            stringBuilder.AppendLine(string.Format("    {0} = " + "("+ ArmorRating_Blunt+" + "+ArmorRating_Sharp + " + " + ArmorRating_Heat + ") / 3", "HMS_MarkScore_Armour".Translate(armourscore.ToString("F1"))));
            stringBuilder.AppendLine(string.Format("    " + meleescore + "(("+ basescore+") + (("+healthscore + " + " + sizescore + ") X (1" + " + " + armourscore + ")))" + " = " + GetValueUnfinalized(req)));
            stringBuilder.AppendLine();
            return stringBuilder.ToString();
        }
    }
}
