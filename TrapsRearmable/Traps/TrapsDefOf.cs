using System;
using Verse;

namespace RimWorld
{
	// Token: 0x02000956 RID: 2390
	[DefOf]
	public static class TrapsDefOf
    {
		// Token: 0x06003781 RID: 14209 RVA: 0x001A8393 File Offset: 0x001A6793
		static TrapsDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(TrapsDefOf));
		}
        
        // Yautja WorkTypeDefs 
    //    public static WorkTypeDef TR_Rearm;

        // Yautja DesignationDefs
        public static DesignationDef TR_RearmTrap;

        // Yautja RecordDefs
        public static RecordDef TR_TrapsRearmed;

        // TrapsRearmable JobDefs
        public static JobDef TR_RearmTrapJob;
    }
}
