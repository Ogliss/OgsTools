using HunterMarkingSystem;
using System;
using Verse;
namespace RimWorld
{
    // Token: 0x0200054F RID: 1359
    public class HMS_Thought_SituationalSocial : Thought_SituationalSocial
    {

        public Comp_Markable Markable
        {
            get
            {
                return pawn.TryGetComp<Comp_Markable>();
            }
        }

        public Comp_Markable Other_Markable
        {
            get
            {
                return otherPawn.TryGetComp<Comp_Markable>();
            }
        }

        // Token: 0x170003A3 RID: 931
        // (get) Token: 0x06001983 RID: 6531 RVA: 0x0004EFE5 File Offset: 0x0004D3E5
        public override bool VisibleInNeedsTab
        {
            get
            {
                return base.VisibleInNeedsTab && this.MoodOffset() != 0f;
            }
        }


        // Token: 0x06001985 RID: 6533 RVA: 0x0004F00D File Offset: 0x0004D40D
        public override float OpinionOffset()
        {
            if (base.CurStage.baseOpinionOffset>=0)
            {
                return (Other_Markable.MarkScore / 10) + base.CurStage.baseOpinionOffset;
            }
            else
            {
                return -(Other_Markable.MarkScore / 10) + base.CurStage.baseOpinionOffset;
            }
        }
        
        // Token: 0x06001987 RID: 6535 RVA: 0x0004F055 File Offset: 0x0004D455
        protected override ThoughtState CurrentStateInternal()
        {
            return this.def.Worker.CurrentSocialState(this.pawn, this.otherPawn);
        }
        
    }
}
