using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace TrapsRearmable
{
    // Token: 0x02000008 RID: 8
    public class Designator_Rearm : Designator
    {
        // Token: 0x0600002D RID: 45 RVA: 0x00002A38 File Offset: 0x00000C38
        public Designator_Rearm()
        {
            this.defaultLabel = Translator.Translate("AvP_DesignatorRearmTrap");
            this.defaultDesc = Translator.Translate("AvP_DesignatorRearmTrapDesc");
            this.icon = ContentFinder<Texture2D>.Get("UI/Designators/RearmTrap", true);
            this.soundDragSustain = SoundDefOf.Designate_DragStandard;
            this.soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            this.useMouseIcon = true;
            this.soundSucceeded = SoundDefOf.Designate_Claim;
        }

        // Token: 0x17000005 RID: 5
        // (get) Token: 0x0600002E RID: 46 RVA: 0x00002AA4 File Offset: 0x00000CA4
        public override int DraggableDimensions
        {
            get
            {
                return 2;
            }
        }

        // Token: 0x0600002F RID: 47 RVA: 0x00002AA7 File Offset: 0x00000CA7
        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!GenGrid.InBounds(c, base.Map))
            {
                return false;
            }
            if (!this.RearmablesInCell(c).Any<Thing>())
            {
                return false;
            }
            return true;
        }

        // Token: 0x06000030 RID: 48 RVA: 0x00002ADC File Offset: 0x00000CDC
        public override void DesignateSingleCell(IntVec3 c)
        {
            foreach (Thing thing in this.RearmablesInCell(c))
            {
                this.DesignateThing(thing);
            }
        }

        // Token: 0x06000031 RID: 49 RVA: 0x00002B2C File Offset: 0x00000D2C
        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            Building_TrapRearmable building_TrapRearmable = t as Building_TrapRearmable;
            return building_TrapRearmable != null && !building_TrapRearmable.Armed && base.Map.designationManager.DesignationOn(building_TrapRearmable, TrapsDefOf.TR_RearmTrap) == null;
        }

        // Token: 0x06000032 RID: 50 RVA: 0x00002B6C File Offset: 0x00000D6C
        public override void DesignateThing(Thing t)
        {
            base.Map.designationManager.AddDesignation(new Designation(t, TrapsDefOf.TR_RearmTrap));
        }

        // Token: 0x06000033 RID: 51 RVA: 0x00002B8E File Offset: 0x00000D8E
        private IEnumerable<Thing> RearmablesInCell(IntVec3 c)
        {
            if (GridsUtility.Fogged(c, base.Map))
            {
                yield break;
            }
            List<Thing> thingList = GridsUtility.GetThingList(c, base.Map);
            int num;
            for (int i = 0; i < thingList.Count; i = num + 1)
            {
                if (this.CanDesignateThing(thingList[i]).Accepted)
                {
                    yield return thingList[i];
                }
                num = i;
            }
            yield break;
        }
    }
}
