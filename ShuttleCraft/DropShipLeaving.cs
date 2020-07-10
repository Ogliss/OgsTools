using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI.Group;

namespace Dropships
{
    public class DropShipLeaving : DropPodLeaving, IActiveDropPod, IThingHolder
    {
        // Token: 0x170005F0 RID: 1520
        // Token: 0x0600271D RID: 10013 RVA: 0x00129CD4 File Offset: 0x001280D4
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<int>(ref this.groupID, "groupID", 0, false);
            Scribe_Values.Look<int>(ref this.destinationTile, "destinationTile", 0, false);
            Scribe_Deep.Look<TransportPodsArrivalAction>(ref this.arrivalAction, "arrivalAction", new object[0]);
            Scribe_Values.Look<bool>(ref this.alreadyLeft, "alreadyLeft", false, false);
        }

        // Token: 0x0600271E RID: 10014 RVA: 0x00129D34 File Offset: 0x00128134
        protected override void LeaveMap()
        {
            if (this.groupID < 0 && this.destinationTile < 0)
            {
                this.Destroy(DestroyMode.Vanish);
                return;
            }
            if (this.alreadyLeft)
            {
                base.LeaveMap();
                return;
            }
            if (this.groupID < 0)
            {
                Log.Error("Drop pod left the map, but its group ID is " + this.groupID, false);
                this.Destroy(DestroyMode.Vanish);
                return;
            }
            if (this.destinationTile < 0)
            {
                Log.Error("Drop pod left the map, but its destination tile is " + this.destinationTile, false);
                this.Destroy(DestroyMode.Vanish);
                return;
            }
            Lord lord = TransporterUtility.FindLord(this.groupID, base.Map);
            if (lord != null)
            {
                base.Map.lordManager.RemoveLord(lord);
            }
            TravelingTransportPods travelingTransportPods = (TravelingTransportPods)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("AvP_USCM_TravelingDropshipUD4L", true));
            travelingTransportPods.Tile = base.Map.Tile;
            travelingTransportPods.SetFaction(Faction.OfPlayer);
            travelingTransportPods.destinationTile = this.destinationTile;
            travelingTransportPods.arrivalAction = this.arrivalAction;
            Find.WorldObjects.Add(travelingTransportPods);
            DropShipLeaving.tmpActiveDropPods.Clear();
            DropShipLeaving.tmpActiveDropPods.AddRange(base.Map.listerThings.ThingsInGroup(ThingRequestGroup.ActiveDropPod));
            for (int i = 0; i < DropShipLeaving.tmpActiveDropPods.Count; i++)
            {
                DropShipLeaving DropshipLeaving = DropShipLeaving.tmpActiveDropPods[i] as DropShipLeaving;
                if (DropshipLeaving != null && DropshipLeaving.groupID == this.groupID)
                {
                    DropshipLeaving.alreadyLeft = true;
                    travelingTransportPods.AddPod(DropshipLeaving.Contents, true);
                    DropshipLeaving.Contents = null;
                    DropshipLeaving.Destroy(DestroyMode.Vanish);
                }
            }
        }

        // Token: 0x04001627 RID: 5671
        private bool alreadyLeft;

        // Token: 0x04001628 RID: 5672
        private static List<Thing> tmpActiveDropPods = new List<Thing>();
    }
}
