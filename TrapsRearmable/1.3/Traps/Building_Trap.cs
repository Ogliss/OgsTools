using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace TrapsRearmable
{
    // Token: 0x02000006 RID: 6
    public abstract class Building_Trap : Building
    {
        // Token: 0x06000012 RID: 18
        protected abstract void SpringSub(Pawn p);

        // Token: 0x17000002 RID: 2
        // (get) Token: 0x06000013 RID: 19 RVA: 0x00002414 File Offset: 0x00000614
        public virtual bool Armed
        {
            get
            {
                return true;
            }
        }

        // Token: 0x06000014 RID: 20 RVA: 0x00002417 File Offset: 0x00000617
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<Pawn>(ref this.touchingPawns, "testees", LookMode.Reference, new object[0]);
        }

        // Token: 0x06000015 RID: 21 RVA: 0x00002438 File Offset: 0x00000638
        public override void Tick()
        {
            if (this.Armed)
            {
                List<Thing> thingList = GridsUtility.GetThingList(base.Position, base.Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    Pawn pawn = thingList[i] as Pawn;
                    if (pawn != null && !this.touchingPawns.Contains(pawn))
                    {
                        this.touchingPawns.Add(pawn);
                        this.CheckSpring(pawn);
                    }
                }
            }
            for (int j = 0; j < this.touchingPawns.Count; j++)
            {
                Pawn pawn2 = this.touchingPawns[j];
                if (!pawn2.Spawned || pawn2.Position != base.Position)
                {
                    this.touchingPawns.Remove(pawn2);
                }
            }
            base.Tick();
        }


        // Token: 0x06000016 RID: 22 RVA: 0x000024F8 File Offset: 0x000006F8
        protected virtual float SpringChance(Pawn p)
        {
            float num;
            if (this.KnowsOfTrap(p))
            {
                num = 0.004f;
            }
            else
            {
                num = StatExtension.GetStatValue(this, StatDefOf.TrapSpringChance, true);
            }
            num *= GenMath.LerpDouble(0.4f, 0.8f, 0f, 1f, p.BodySize);
            return Mathf.Clamp01(num);
        }

        // Token: 0x06000017 RID: 23 RVA: 0x000025B0 File Offset: 0x000007B0
        private void CheckSpring(Pawn p)
        {
            Rand.PushState();
            if (Rand.Value < this.SpringChance(p))
            {
                this.Spring(p);
                if (p.Faction == Faction.OfPlayer || p.HostFaction == Faction.OfPlayer)
                {
                    Find.LetterStack.ReceiveLetter(TranslatorFormattedStringExtensions.Translate("LetterFriendlyTrapSprungLabel", p.LabelShort, p), TranslatorFormattedStringExtensions.Translate("LetterFriendlyTrapSprung", p.LabelShort, p), LetterDefOf.NegativeEvent, new TargetInfo(base.Position, base.Map, false), null, null);
                }
            }
            Rand.PopState();
        }

        // Token: 0x06000018 RID: 24 RVA: 0x0000264C File Offset: 0x0000084C
        public bool KnowsOfTrap(Pawn p)
        {
            if (p.Faction != null && !FactionUtility.HostileTo(p.Faction, base.Faction))
            {
                return true;
            }
            if (p.guest != null && p.guest.Released)
            {
                return true;
            }
            Lord lord = LordUtility.GetLord(p);
            return p.RaceProps.Humanlike && lord != null && lord.LordJob is LordJob_FormAndSendCaravan;
        }

        // Token: 0x06000019 RID: 25 RVA: 0x000026E1 File Offset: 0x000008E1
        public override ushort PathFindCostFor(Pawn p)
        {
            if (!this.Armed)
            {
                return 0;
            }
            if (this.KnowsOfTrap(p))
            {
                return 800;
            }
            return 0;
        }

        // Token: 0x0600001A RID: 26 RVA: 0x000026FD File Offset: 0x000008FD
        public override ushort PathWalkCostFor(Pawn p)
        {
            if (!this.Armed)
            {
                return 0;
            }
            if (this.KnowsOfTrap(p))
            {
                return 30;
            }
            return 0;
        }

        // Token: 0x0600001B RID: 27 RVA: 0x00002716 File Offset: 0x00000916
        public override bool IsDangerousFor(Pawn p)
        {
            return this.Armed && this.KnowsOfTrap(p);
        }

        // Token: 0x0600001C RID: 28 RVA: 0x0000272C File Offset: 0x0000092C
        public override string GetInspectString()
        {
            string text = base.GetInspectString();
            if (!GenText.NullOrEmpty(text))
            {
                text += "\n";
            }
            if (this.Armed)
            {
                text += Translator.Translate("AvP_TrapArmed");
            }
            else
            {
                text += Translator.Translate("AvP_TrapNotArmed");
            }
            return text;
        }

        // Token: 0x0600001D RID: 29 RVA: 0x00002781 File Offset: 0x00000981
        public void Spring(Pawn p)
        {
            SoundStarter.PlayOneShot(SoundDef.Named("DeadfallSpring"), new TargetInfo(base.Position, base.Map, false));
            this.SpringSub(p);
        }

        // Token: 0x0400000A RID: 10
        private List<Pawn> touchingPawns = new List<Pawn>();

        // Token: 0x0400000B RID: 11
        private const float KnowerSpringChance = 0.004f;

        // Token: 0x0400000C RID: 12
        private const ushort KnowerPathFindCost = 800;

        // Token: 0x0400000D RID: 13
        private const ushort KnowerPathWalkCost = 30;

        // Token: 0x0400000E RID: 14
        private const float AnimalSpringChanceFactor = 0.1f;
    }
}
