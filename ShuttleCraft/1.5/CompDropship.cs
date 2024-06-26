﻿using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Dropships
{
    // Token: 0x02000861 RID: 2145
    public class CompProperties_Dropship : CompProperties
    {
        // Token: 0x06003429 RID: 13353 RVA: 0x0011B149 File Offset: 0x00119349
        public CompProperties_Dropship()
        {
            this.compClass = typeof(CompDropship);
        }
        public float fuelConsumptionPerHour = 10f;
        public float tilesPerHour = 2f;
        public ThingDef active = null;
        public ThingDef dropship = null;
        public ThingDef incomming = null;
        public ThingDef leaving = null;
    }
    // Token: 0x02000CFD RID: 3325
    [StaticConstructorOnStartup]
    public class CompDropship : ThingComp
    {
        public CompProperties_Dropship Props => this.props as CompProperties_Dropship;
        public int dustoffdelay = 120;
        public bool autodustoff = false;

        public bool LoadingInProgressOrReadyToLaunch
        {
            get
            {
                return this.Transporter.LoadingInProgressOrReadyToLaunch || this.autodustoff || this.Transporter.innerContainer.Any(x => x.GetType() == typeof(Pawn) && ((Pawn)x).story != null);
            }
        }

        public List<CompTransporter> TransportersInGroup
        {
            get
            {
                List<CompTransporter> result = new List<CompTransporter>();
                result.Add(this.parent.TryGetComp<CompTransporter>());
                return result;//this.Transporter.TransportersInGroup(this.parent.Map);
            }
        }

        public bool AnyInGroupIsUnderRoof
        {
            get
            {
                List<CompTransporter> transportersInGroup = this.TransportersInGroup;
                for (int i = 0; i < transportersInGroup.Count; i++)
                {
                    if (transportersInGroup[i].parent.Position.Roofed(this.parent.Map))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public CompTransporter Transporter
        {
            get
            {
                if (this.cachedCompTransporter == null)
                {
                    this.cachedCompTransporter = this.parent.GetComp<CompTransporter>();
                }
                return this.cachedCompTransporter;
            }
        }

        private bool Autoloadable
        {
            get
            {
                if (this.cachedTransporterList == null)
                {
                    this.cachedTransporterList = new List<CompTransporter>
                    {
                        this.Transporter
                    };
                }
                foreach (Pawn thing in TransporterUtility.AllSendablePawns(this.cachedTransporterList, this.parent.Map, false))
                {
                    if (!this.IsRequired(thing))
                    {
                        return false;
                    }
                }
                foreach (Thing thing2 in TransporterUtility.AllSendableItems(this.cachedTransporterList, this.parent.Map, false))
                {
                    if (!this.IsRequired(thing2))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public bool AllRequiredThingsLoaded
        {
            get
            {
                ThingOwner innerContainer = this.Transporter.innerContainer;
                for (int i = 0; i < this.requiredPawns.Count; i++)
                {
                    if (!innerContainer.Contains(this.requiredPawns[i]))
                    {
                        return false;
                    }
                }
                if (this.requiredColonistCount > 0)
                {
                    int num = 0;
                    for (int j = 0; j < innerContainer.Count; j++)
                    {
                        Pawn pawn = innerContainer[j] as Pawn;
                        if (pawn != null && pawn.IsFreeColonist)
                        {
                            num++;
                        }
                    }
                    if (num < this.requiredColonistCount)
                    {
                        return false;
                    }
                }
                CompDropship.tmpRequiredItemsWithoutDuplicates.Clear();
                for (int k = 0; k < this.requiredItems.Count; k++)
                {
                    bool flag = false;
                    for (int l = 0; l < CompDropship.tmpRequiredItemsWithoutDuplicates.Count; l++)
                    {
                        if (CompDropship.tmpRequiredItemsWithoutDuplicates[l].ThingDef == this.requiredItems[k].ThingDef)
                        {
                            CompDropship.tmpRequiredItemsWithoutDuplicates[l] = CompDropship.tmpRequiredItemsWithoutDuplicates[l].WithCount(CompDropship.tmpRequiredItemsWithoutDuplicates[l].Count + this.requiredItems[k].Count);
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        CompDropship.tmpRequiredItemsWithoutDuplicates.Add(this.requiredItems[k]);
                    }
                }
                for (int m = 0; m < CompDropship.tmpRequiredItemsWithoutDuplicates.Count; m++)
                {
                    int num2 = 0;
                    for (int n = 0; n < innerContainer.Count; n++)
                    {
                        if (innerContainer[n].def == CompDropship.tmpRequiredItemsWithoutDuplicates[m].ThingDef)
                        {
                            num2 += innerContainer[n].stackCount;
                        }
                    }
                    if (num2 < CompDropship.tmpRequiredItemsWithoutDuplicates[m].Count)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
            {
                yield return gizmo;
            }
            if (this.Autoloadable)
            {
                yield return new Command_Toggle
                {
                    defaultLabel = "CommandAutoloadTransporters".Translate(),
                    defaultDesc = "CommandAutoloadTransportersDesc".Translate(),
                    icon = CompDropship.AutoloadToggleTex,
                    isActive = (() => this.autoload),
                    toggleAction = delegate ()
                    {
                        this.autoload = !this.autoload;
                        if (this.autoload && !this.LoadingInProgressOrReadyToLaunch)
                        {
                            TransporterUtility.InitiateLoading(Gen.YieldSingle<CompTransporter>(this.Transporter));
                        }
                        this.CheckAutoload();
                    }
                };
            }
            Command_Action command_Action = new Command_Action();
            command_Action.defaultLabel = "CommandSendShuttle".Translate();
            command_Action.defaultDesc = "CommandSendShuttleDesc".Translate();
            command_Action.icon = CompDropship.SendCommandTex;
            command_Action.alsoClickIfOtherInGroupClicked = false;
            command_Action.action = delegate ()
            {
                this.StartChoosingDestination();
            };
            if (!this.AllFuelingPortSourcesInGroupHaveAnyFuel)
            {
                command_Action.Disable("CommandLaunchGroupFailNoFuel".Translate());
            }
            else
            if (!this.LoadingInProgressOrReadyToLaunch || !this.AllRequiredThingsLoaded)
            {
                command_Action.Disable("CommandSendShuttleFailMissingRequiredThing".Translate());
            }
            else
            if (this.AnyInGroupIsUnderRoof)
            {
                command_Action.Disable("CommandSendShuttleFailUnderRoof".Translate());
            }
            yield return command_Action;
            /*
            foreach (Gizmo gizmo2 in QuestUtility.GetQuestRelatedGizmos(this.parent))
            {
                yield return gizmo2;
            }
            */
            yield break;
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            if (!selPawn.CanReach(this.parent, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn))
            {
                yield break;
            }
            string text = "EnterShuttle".Translate();
            if (!this.IsAllowed(selPawn))
            {
                yield return new FloatMenuOption(text + " (" + "NotAllowed".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null);
                yield break;
            }
            yield return new FloatMenuOption(text, delegate ()
            {
                if (!this.LoadingInProgressOrReadyToLaunch)
                {
                    TransporterUtility.InitiateLoading(Gen.YieldSingle<CompTransporter>(this.Transporter));
                }
                Job job = JobMaker.MakeJob(JobDefOf.EnterTransporter, this.parent);
                selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }, MenuOptionPriority.Default, null, null, 0f, null, null);
            yield break;
        }

        public IEnumerable<FloatMenuOption> GetTransportPodsFloatMenuOptionsAt(int tile, Caravan car = null)
        {
            bool anything = false;
            IEnumerable<IThingHolder> pods = this.TransportersInGroup.Cast<IThingHolder>();
            if (car != null)
            {
                List<Caravan> rliss = new List<Caravan>();
                rliss.Add(car);
                pods = rliss.Cast<IThingHolder>();

            }
            if (car == null)
            {
                if (TransportPodsArrivalAction_FormCaravan.CanFormCaravanAt(pods, tile) && !Find.WorldObjects.AnySettlementBaseAt(tile) && !Find.WorldObjects.AnySiteAt(tile))
                {
                    anything = true;
                    yield return new FloatMenuOption("FormCaravanHere".Translate(), delegate
                    {
                        this.TryLaunch(tile, new TransportPodsArrivalAction_FormCaravan(), car);

                    }, MenuOptionPriority.Default, null, null, 0f, null, null);
                }
            }
            else
            {
                if (!Find.WorldObjects.AnySettlementBaseAt(tile) && !Find.WorldObjects.AnySiteAt(tile) && !Find.World.Impassable(tile))
                {
                    anything = true;
                    yield return new FloatMenuOption("FormCaravanHere".Translate(), delegate
                    {
                        this.TryLaunch(tile, new TransportPodsArrivalAction_FormCaravan(), car);

                    }, MenuOptionPriority.Default, null, null, 0f, null, null);
                }
            }

            List<WorldObject> worldObjects = Find.WorldObjects.AllWorldObjects;
            for (int i = 0; i < worldObjects.Count; i++)
            {
                if (worldObjects[i].Tile == tile)
                {
                    IEnumerable<FloatMenuOption> nowre = getFM(worldObjects[i], pods, this, car);
                    if (nowre.ToList().Count < 1)
                    {
                        yield return new FloatMenuOption("FormCaravanHere".Translate(), delegate
                        {
                            this.TryLaunch(tile, new TransportPodsArrivalAction_FormCaravan(), car);
                        }, MenuOptionPriority.Default, null, null, 0f, null, null);
                    }
                    else
                    {
                        foreach (FloatMenuOption o in nowre)//worldObjects[i].GetTransportPodsFloatMenuOptions(this.TransportersInGroup.Cast<IThingHolder>(), this))
                        {
                            anything = true;
                            yield return o;
                        }
                    }
                }
            }

            if (!anything && !Find.World.Impassable(tile))
            {
                yield return new FloatMenuOption("TransportPodsContentsWillBeLost".Translate(), delegate
                {
                    this.TryLaunch(tile, null);
                }, MenuOptionPriority.Default, null, null, 0f, null, null);
            }

            yield break;
        }

        public static IEnumerable<FloatMenuOption> getFM(WorldObject wobj, IEnumerable<IThingHolder> ih, CompDropship comp, Caravan car)
        {
            if (wobj is Caravan)
            {
                return Enumerable.Empty<FloatMenuOption>();
            }
            else if (wobj is Site)
            {
                return GetSite(wobj as Site, ih, comp, car);
            }
            else if (wobj is Settlement)
            {
                return GetSettle(wobj as Settlement, ih, comp, car);
            }
            else if (wobj is MapParent)
            {
                return GetMapParent(wobj as MapParent, ih, comp, car);
            }
            else
            {
                return Enumerable.Empty<FloatMenuOption>();
            }

        }

        public static IEnumerable<FloatMenuOption> GetMapParent(MapParent mapparent, IEnumerable<IThingHolder> pods, CompDropship representative, Caravan car)
        {
            /*
            foreach (FloatMenuOption o in mapparent.GetFloatMenuOptions())
            {
                yield return o;
            }
            */


            if (TransportPodsArrivalAction_LandInSpecificCell.CanLandInSpecificCell(pods, mapparent))
            {
                yield return new FloatMenuOption("LandInExistingMap".Translate(mapparent.Label), delegate
                {

                    Map myMap;
                    if (car == null)
                        myMap = representative.parent.Map;
                    else
                        myMap = null;

                    Map map = mapparent.Map;
                    Current.Game.CurrentMap = map;
                    CameraJumper.TryHideWorld();
                    Find.Targeter.BeginTargeting(TargetingParameters.ForDropPodsDestination(), delegate (LocalTargetInfo x)
                    {
                        representative.TryLaunch(mapparent.Tile, new TransportPodsArrivalAction_LandInSpecificCell(mapparent, x.Cell), car);
                    }, null, delegate
                    {

                        if (myMap != null && Find.Maps.Contains(myMap))
                        {
                            Current.Game.CurrentMap = myMap;
                        }

                    }, CompLaunchable.TargeterMouseAttachment);
                }, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            else
                yield break;
        }

        public static IEnumerable<FloatMenuOption> GetSite(Site site, IEnumerable<IThingHolder> pods, CompDropship representative, Caravan car)
        {
            foreach (FloatMenuOption o in GetMapParent(site, pods, representative, car))
            {
                yield return o;
            }
            foreach (FloatMenuOption o2 in GetVisitSite(representative, pods, site, car))
            {
                yield return o2;
            }
            yield break;
        }

        public static IEnumerable<FloatMenuOption> GetVisitSite(CompDropship representative, IEnumerable<IThingHolder> pods, Site site, Caravan car)
        {
            foreach (FloatMenuOption f in DropShipArrivalActionUtility.GetFloatMenuOptions<TransportPodsArrivalAction_VisitSite>(() => TransportPodsArrivalAction_VisitSite.CanVisit(pods, site), () => new TransportPodsArrivalAction_VisitSite(site, PawnsArrivalModeDefOf.EdgeDrop), "DropAtEdge".Translate(), representative, site.Tile, car))
            {
                yield return f;
            }
            foreach (FloatMenuOption f2 in DropShipArrivalActionUtility.GetFloatMenuOptions<TransportPodsArrivalAction_VisitSite>(() => TransportPodsArrivalAction_VisitSite.CanVisit(pods, site), () => new TransportPodsArrivalAction_VisitSite(site, PawnsArrivalModeDefOf.CenterDrop), "DropInCenter".Translate(), representative, site.Tile, car))
            {
                yield return f2;
            }
            yield break;
        }

        public static IEnumerable<FloatMenuOption> GetSettle(Settlement bs, IEnumerable<IThingHolder> pods, CompDropship representative, Caravan car)
        {
            foreach (FloatMenuOption o in GetMapParent(bs, pods, representative, car))
            {
                yield return o;
            }

            foreach (FloatMenuOption f in DropShipArrivalActionUtility.GetVisitFloatMenuOptions(representative, pods, bs, car))
            {
                yield return f;
            }

            foreach (FloatMenuOption f2 in DropShipArrivalActionUtility.GetGIFTFloatMenuOptions(representative, pods, bs, car))
            {
                yield return f2;
            }
            foreach (FloatMenuOption f3 in DropShipArrivalActionUtility.GetATKFloatMenuOptions(representative, pods, bs, car))
            {
                yield return f3;
            }
            yield break;
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Required".Translate() + ": ");
            CompDropship.tmpRequiredLabels.Clear();
            if (this.requiredColonistCount > 0)
            {
                CompDropship.tmpRequiredLabels.Add(this.requiredColonistCount + " " + ((this.requiredColonistCount > 1) ? Faction.OfPlayer.def.pawnsPlural : Faction.OfPlayer.def.pawnSingular));
            }
            for (int i = 0; i < this.requiredPawns.Count; i++)
            {
                CompDropship.tmpRequiredLabels.Add(this.requiredPawns[i].LabelShort);
            }
            for (int j = 0; j < this.requiredItems.Count; j++)
            {
                CompDropship.tmpRequiredLabels.Add(this.requiredItems[j].Label);
            }
            if (CompDropship.tmpRequiredLabels.Any<string>())
            {
                stringBuilder.Append(CompDropship.tmpRequiredLabels.ToCommaList(true).CapitalizeFirst());
            }
            else
            {
                stringBuilder.Append("Nothing".Translate());
            }
            return stringBuilder.ToString();
        }

        private bool ChoseWorldTarget(GlobalTargetInfo target)
        {

            if (this.carr == null)
                if (!this.LoadingInProgressOrReadyToLaunch)
                {
                    return true;
                }

            if (!target.IsValid)
            {
                Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }
            /*
            if (!target.HasWorldObject)
            {
                Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }
            */


            int myTile = -2;
            if (this.carr == null)
            {
                myTile = this.parent.Map.Tile;
            }
            else
            {
                myTile = carr.Tile;
            }

            int num = Find.WorldGrid.TraversalDistanceBetween(myTile, target.Tile, true, int.MaxValue);
            if (num > this.MaxLaunchDistance)
            {
                Messages.Message("MessageTransportPodsDestinationIsTooFar".Translate(CompDropship.FuelNeededToLaunchAtDist((float)num).ToString("0.#")), MessageTypeDefOf.RejectInput, false);
                return false;
            }
            if (Find.WorldGrid[target.Tile].biome.impassable || Find.World.Impassable(target.Tile))
            {

                Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            MapParent mapParent = Find.WorldObjects.MapParentAt(target.Tile);


            IEnumerable<FloatMenuOption> transportPodsFloatMenuOptionsAt = this.GetTransportPodsFloatMenuOptionsAt(target.Tile, carr);


            if (!transportPodsFloatMenuOptionsAt.Any<FloatMenuOption>())
            {
                if (Find.WorldGrid[target.Tile].biome.impassable || Find.World.Impassable(target.Tile))
                {

                    Messages.Message("MessageTransportPodsDestinationIsInvalid".Translate(), MessageTypeDefOf.RejectInput, false);
                    return false;
                }
                this.TryLaunch(target.Tile, null, null);
                return true;
            }
            else
            {
                if (transportPodsFloatMenuOptionsAt.Count<FloatMenuOption>() == 1)
                {
                    if (!transportPodsFloatMenuOptionsAt.First<FloatMenuOption>().Disabled)
                    {
                        transportPodsFloatMenuOptionsAt.First<FloatMenuOption>().action();
                    }
                    return false;
                }
                Find.WindowStack.Add(new FloatMenu(transportPodsFloatMenuOptionsAt.ToList<FloatMenuOption>()));
                return false;
            }


        }

        private void StartChoosingDestination()
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(this.parent));
            Find.WorldSelector.ClearSelection();
            int tile = this.parent.Map.Tile;
            this.carr = null;
            Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(this.ChoseWorldTarget), true, CompDropship.TargeterMouseAttachment, true, delegate
            {
                GenDraw.DrawWorldRadiusRing(tile, this.MaxLaunchDistance);
            }, delegate (GlobalTargetInfo target)
            {
                if (!target.IsValid)
                {
                    return null;
                }
                int num = Find.WorldGrid.TraversalDistanceBetween(tile, target.Tile, true, int.MaxValue);
                if (num > this.MaxLaunchDistance)
                {
                    GUI.color = Color.red;
                    if (num > this.MaxLaunchDistanceEverPossible)
                    {
                        return "TransportPodDestinationBeyondMaximumRange".Translate();
                    }
                    return "TransportPodNotEnoughFuel".Translate();
                }
                else
                {
                    IEnumerable<FloatMenuOption> transportPodsFloatMenuOptionsAt = this.GetTransportPodsFloatMenuOptionsAt(target.Tile);
                    if (!transportPodsFloatMenuOptionsAt.Any<FloatMenuOption>())
                    {
                        if (Find.WorldGrid[target.Tile].biome.impassable || Find.World.Impassable(target.Tile))
                        {

                            return ("MessageTransportPodsDestinationIsInvalid".Translate());

                        }
                        return string.Empty;
                    }
                    if (transportPodsFloatMenuOptionsAt.Count<FloatMenuOption>() == 1)
                    {
                        if (transportPodsFloatMenuOptionsAt.First<FloatMenuOption>().Disabled)
                        {
                            GUI.color = Color.red;
                        }
                        return transportPodsFloatMenuOptionsAt.First<FloatMenuOption>().Label;
                    }
                    MapParent mapParent = target.WorldObject as MapParent;
                    if (mapParent != null)
                    {
                        return "ClickToSeeAvailableOrders_WorldObject".Translate(mapParent.LabelCap);
                    }
                    return "ClickToSeeAvailableOrders_Empty".Translate();
                }
            });
        }

        public void WorldStartChoosingDestination(Caravan car)
        {
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(car));
            //    Find.WorldSelector.ClearSelection();
            int tile = car.Tile;
            this.carr = car;
            Find.WorldTargeter.BeginTargeting(new Func<GlobalTargetInfo, bool>(this.ChoseWorldTarget), true, CompDropship.TargeterMouseAttachment, false, delegate
            {
                GenDraw.DrawWorldRadiusRing(car.Tile, this.MaxLaunchDistance);
            }, delegate (GlobalTargetInfo target)
            {
                if (!target.IsValid)
                {
                    return null;
                }
                int num = Find.WorldGrid.TraversalDistanceBetween(tile, target.Tile, true, int.MaxValue);
                if (num > this.MaxLaunchDistance)
                {
                    GUI.color = Color.red;
                    if (num > this.MaxLaunchDistanceEverPossible)
                    {
                        return "TransportPodDestinationBeyondMaximumRange".Translate();
                    }
                    return "TransportPodNotEnoughFuel".Translate();
                }
                else
                {

                    IEnumerable<FloatMenuOption> transportPodsFloatMenuOptionsAt = this.GetTransportPodsFloatMenuOptionsAt(target.Tile, car);
                    if (!transportPodsFloatMenuOptionsAt.Any<FloatMenuOption>())
                    {
                        if (Find.WorldGrid[target.Tile].biome.impassable || Find.World.Impassable(target.Tile))
                        {
                            return ("MessageTransportPodsDestinationIsInvalid".Translate());

                        }
                        return string.Empty;
                    }
                    if (transportPodsFloatMenuOptionsAt.Count<FloatMenuOption>() == 1)
                    {
                        if (transportPodsFloatMenuOptionsAt.First<FloatMenuOption>().Disabled)
                        {
                            GUI.color = Color.red;
                        }
                        return transportPodsFloatMenuOptionsAt.First<FloatMenuOption>().Label;
                    }
                    MapParent mapParent = target.WorldObject as MapParent;
                    if (mapParent != null)
                    {
                        return "ClickToSeeAvailableOrders_WorldObject".Translate(mapParent.LabelCap);
                    }
                    return "ClickToSeeAvailableOrders_Empty".Translate();

                    //return "DI!";

                }
            });
        }

        public void TryLaunch(int destinationTile, TransportPodsArrivalAction arrivalAction, Caravan cafr = null)
        {
            //Log.Warning("CARR:" + this.carr+"/"+cafr);
            if (cafr == null)
                if (!this.parent.Spawned)
                {
                    Log.Error("Tried to launch " + this.parent + ", but it's unspawned.");
                    return;
                }
            /*
            List<CompTransporter> transportersInGroup = this.TransportersInGroup;
            if (transportersInGroup == null)
            {
                Log.Error("Tried to launch " + this.parent + ", but it's not in any group.", false);
                return;
            }
            */
            if (this.parent.Spawned)
            {
                if (!this.LoadingInProgressOrReadyToLaunch)
                {
                    return;
                }
            }
            if (!this.AllInGroupConnectedToFuelingPort || !this.AllFuelingPortSourcesInGroupHaveAnyFuel)
            {

                return;
            }
            if (cafr == null)
            {
                Map map = this.parent.Map;
                int num = Find.WorldGrid.TraversalDistanceBetween(map.Tile, destinationTile, true, int.MaxValue);
                if (num > this.MaxLaunchDistance)
                {
                    return;
                }
                this.Transporter.TryRemoveLord(map);
                if (this.Transporter.groupID < 0)
                {
                    this.Transporter.groupID = this.parent.thingIDNumber;
                }
                int groupID = this.Transporter.groupID;
                float amount = Mathf.Max(FuelNeededToLaunchAtDist((float)num), 1f);
                //for (int i = 0; i < transportersInGroup.Count; i++)

                CompTransporter compTransporter = Transporter;//transportersInGroup[i];
                Building fuelingPortSource = this.FuelingPortSource;//compTransporter.Launchable.FuelingPortSource;
                if (fuelingPortSource != null)
                {
                    fuelingPortSource.TryGetComp<CompRefuelable>().ConsumeFuel(amount);
                }
                ThingOwner directlyHeldThings = compTransporter.GetDirectlyHeldThings();

                Thing dropship = ThingMaker.MakeThing(Props.dropship);
                dropship.SetFactionDirect(Faction.OfPlayer);

                CompRefuelable compr = dropship.TryGetComp<CompRefuelable>();
                Type tcr = compr.GetType();
                FieldInfo finfos = tcr.GetField("fuel", BindingFlags.NonPublic | BindingFlags.Instance);
                finfos.SetValue(compr, fuelingPortSource.TryGetComp<CompRefuelable>().Fuel);

                dropship.stackCount = 1;
                directlyHeldThings.TryAddOrTransfer(dropship);

                ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(Props.active, null);
                activeDropPod.Contents = new ActiveDropPodInfo();
                activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);
                DropShipLeaving dropPodLeaving = (DropShipLeaving)SkyfallerMaker.MakeSkyfaller(Props.leaving, activeDropPod);
                dropPodLeaving.groupID = groupID;
                dropPodLeaving.destinationTile = destinationTile;
                dropPodLeaving.arrivalAction = arrivalAction;
                compTransporter.CleanUpLoadingVars(map);
                //compTransporter.parent
                IntVec3 poc = fuelingPortSource.Position;
                // fuelingPortSource.Destroy(DestroyMode.Vanish);
                DropshipDestroy(fuelingPortSource, DestroyMode.Vanish);
                GenSpawn.Spawn(dropPodLeaving, poc, map, WipeMode.Vanish);

                CameraJumper.TryHideWorld();
            }
            else
            {
                int num = Find.WorldGrid.TraversalDistanceBetween(carr.Tile, destinationTile, true, int.MaxValue);
                if (num > this.MaxLaunchDistance)
                {
                    return;
                }
                float amount = Mathf.Max(FuelNeededToLaunchAtDist((float)num), 1f);
                if (FuelingPortSource != null)
                {
                    FuelingPortSource.TryGetComp<CompRefuelable>().ConsumeFuel(amount);
                }


                ThingOwner<Pawn> directlyHeldThings = (ThingOwner<Pawn>)cafr.GetDirectlyHeldThings();
                //    Thing dropship = null;
                //    CompUSCMDropship dropship1 = null;
                List<Pawn> lpto = directlyHeldThings.AsEnumerable<Pawn>().ToList();

                /*
                foreach (Pawn pawn in directlyHeldThings.InnerListForReading)
                {
                    Pawn_InventoryTracker pinv = pawn.inventory;
                    for (int i = 0; i < pinv.innerContainer.Count; i++)
                    {
                        if (pinv.innerContainer[i].def == USCMDefOf.AvP_USCM_DropshipUD4L)
                        {
                            dropship = pinv.innerContainer[i];
                            dropship1 = dropship.TryGetComp<CompUSCMDropship>();
                            pinv.innerContainer[i].holdingOwner.Remove(pinv.innerContainer[i]);

                            break;
                        }
                    }
                }

                ThingOwner<Thing> finalto = new ThingOwner<Thing>();
                List<Pawn> lpto = directlyHeldThings.AsEnumerable<Pawn>().ToList();
                foreach (Pawn p in lpto)
                {
                    finalto.TryAddOrTransfer(p);
                }


                if (dropship != null)
                {
                    // Log.Warning("TRY ADD"+helicopter);
                    if (dropship.holdingOwner == null)
                        //Log.Warning("NULL");
                        //directlyHeldThings.
                        finalto.TryAddOrTransfer(dropship, false);
                }
                */

                ActiveDropPod activeDropPod = (ActiveDropPod)ThingMaker.MakeThing(Props.active, null);
                activeDropPod.Contents = new ActiveDropPodInfo();
                foreach (var item in carr.AllThings)
                {
                    //    Log.Message(string.Format("carr.AllThings: {0}", item));
                }


                activeDropPod.Contents.innerContainer.TryAddRangeOrTransfer(
                    //directlyHeldThings
                    carr.Goods, true, true);
                foreach (Pawn p in lpto)
                {
                    activeDropPod.Contents.innerContainer.TryAddOrTransfer(p);
                }
                foreach (var item in activeDropPod.Contents.innerContainer)
                {
                    //    Log.Message(string.Format("activeDropPod: {0}", item));
                }
                cafr.RemoveAllPawns();
                if (cafr.Spawned)
                {
                    Find.WorldObjects.Remove(cafr);
                }

                TravelingTransportPods travelingTransportPods = (TravelingTransportPods)WorldObjectMaker.MakeWorldObject(DefDatabase<WorldObjectDef>.GetNamed("AvP_USCM_TravelingDropshipUD4L", true));
                travelingTransportPods.Tile = cafr.Tile;
                travelingTransportPods.SetFaction(Faction.OfPlayer);
                travelingTransportPods.destinationTile = destinationTile;
                travelingTransportPods.arrivalAction = arrivalAction;
                Find.WorldObjects.Add(travelingTransportPods);
                travelingTransportPods.AddPod(activeDropPod.Contents, true);
                //    activeDropPod.Contents = null;
                //    activeDropPod.Destroy(DestroyMode.Vanish);
                // CameraJumper.TryHideWorld();
                Find.WorldTargeter.StopTargeting();
            }

        }

        public static void DropshipDestroy(Thing thing, DestroyMode mode = DestroyMode.Vanish)
        {
            if (!Thing.allowDestroyNonDestroyable && !thing.def.destroyable)
            {
                Log.Error("Tried to destroy non-destroyable thing " + thing);
                return;
            }
            if (thing.Destroyed)
            {
                Log.Error("Tried to destroy already-destroyed thing " + thing);
                return;
            }
            bool spawned = thing.Spawned;
            Map map = thing.Map;
            if (thing.Spawned)
            {
                thing.DeSpawn(mode);
            }
            Type typ = typeof(Thing);
            FieldInfo finfo = typ.GetField("mapIndexOrState", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            SByte sbt = -2;
            finfo.SetValue(thing, sbt);



            if (thing.def.DiscardOnDestroyed)
            {
                thing.Discard(false);
            }

            if (thing.holdingOwner != null)
            {
                thing.holdingOwner.Notify_ContainedItemDestroyed(thing);
            }
            MethodInfo minfo = typ.GetMethod("RemoveAllReservationsAndDesignationsOnThis", BindingFlags.NonPublic | BindingFlags.Instance);
            minfo.Invoke(thing, null);

            if (!(thing is Pawn))
            {
                thing.stackCount = 0;
            }
        }

        public Building FuelingPortSource
        {
            get
            {
                return (Building)this.parent;
            }
        }

        public static float FuelNeededToLaunchAtDist(float dist)
        {
            return 2.25f * dist;
        }

        public bool AllFuelingPortSourcesInGroupHaveAnyFuel
        {
            get
            {
                List<CompTransporter> transportersInGroup = this.TransportersInGroup;
                try
                {
                    for (int i = 0; i < transportersInGroup.Count; i++)
                    {
                        CompDropship dropship = transportersInGroup[i].parent.TryGetComp<CompDropship>();
                        if (!dropship.FuelingPortSourceHasAnyFuel)
                        {
                            return false;
                        }
                    }
                }
                catch (Exception)
                {
                    //    Log.Message("TransportersInGroup false");
                    throw;
                }
                return true;
            }
        }

        public bool AllInGroupConnectedToFuelingPort
        {
            get
            {
                /*
                List<CompTransporter> transportersInGroup = this.TransportersInGroup;
                for (int i = 0; i < transportersInGroup.Count; i++)
                {
                    if (!transportersInGroup[i].Launchable.ConnectedToFuelingPort)
                    {
                        return false;
                    }
                }
                */
                return true;
            }
        }

        public bool ConnectedToFuelingPort
        {
            get
            {
                return this.FuelingPortSource != null;
            }
        }

        public bool FuelingPortSourceHasAnyFuel
        {
            get
            {
                return this.ConnectedToFuelingPort && this.FuelingPortSource.GetComp<CompRefuelable>().HasFuel;
            }
        }

        public float FuelingPortSourceFuel
        {
            get
            {
                if (!this.ConnectedToFuelingPort)
                {
                    return 0f;
                }
                return this.FuelingPortSource.GetComp<CompRefuelable>().Fuel;
            }
        }

        private float FuelInLeastFueledFuelingPortSource
        {
            get
            {
                //List<CompTransporter> transportersInGroup = this.TransportersInGroup;
                float num = 0f;
                bool flag = false;
                //  for (int i = 0; i < transportersInGroup.Count; i++)
                //  {
                float fuelingPortSourceFuel = //transportersInGroup[i].Launchable.
                FuelingPortSourceFuel;
                if (!flag || fuelingPortSourceFuel < num)
                {
                    num = fuelingPortSourceFuel;
                    flag = true;
                }
                //  }
                if (!flag)
                {
                    return 0f;
                }
                return num;
            }
        }

        private int MaxLaunchDistance
        {
            get
            {
                return MaxLaunchDistanceAtFuelLevel(this.FuelInLeastFueledFuelingPortSource);
            }
        }

        private int MaxLaunchDistanceEverPossible
        {
            get
            {
                if (!this.LoadingInProgressOrReadyToLaunch)
                {
                    return 0;
                }
                List<CompTransporter> transportersInGroup = this.TransportersInGroup;
                float num = 0f;
                for (int i = 0; i < transportersInGroup.Count; i++)
                {
                    Building fuelingPortSource = transportersInGroup[i].Launchable.FuelingPortSource;
                    if (fuelingPortSource != null)
                    {
                        num = Mathf.Max(num, fuelingPortSource.GetComp<CompRefuelable>().Props.fuelCapacity);
                    }
                }
                return CompLaunchable.MaxLaunchDistanceAtFuelLevel(num);
            }
        }

        public static int MaxLaunchDistanceAtFuelLevel(float fuelLevel)
        {
            return Mathf.FloorToInt(fuelLevel / 2.25f);
        }

        private bool PodsHaveAnyPotentialCaravanOwner
        {
            get
            {
                List<CompTransporter> transportersInGroup = this.TransportersInGroup;
                for (int i = 0; i < transportersInGroup.Count; i++)
                {
                    ThingOwner innerContainer = transportersInGroup[i].innerContainer;
                    for (int j = 0; j < innerContainer.Count; j++)
                    {
                        Pawn pawn = innerContainer[j] as Pawn;
                        if (pawn != null && CaravanUtility.IsOwner(pawn, Faction.OfPlayer))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public void Send()
        {
            //    Log.Message("1 ");
            if (this.sending)
            {
                return;
            }
            //    Log.Message("2 ");
            if (!this.parent.Spawned)
            {
                Log.Error("Tried to send " + this.parent + ", but it's unspawned.");
                return;
            }
            //    Log.Message("3 ");
            List<CompTransporter> transportersInGroup = this.TransportersInGroup;
            //    Log.Message("4 ");
            if (this.Transporter.groupID < 0)
            {
                this.Transporter.groupID = this.parent.thingIDNumber;
            }
            if (!this.autodustoff)
            {
                if (transportersInGroup == null)
                {
                    Log.Error("Tried to send " + this.parent + ", but it's not in any group.");
                    return;
                }
            }
            //    Log.Message("5 ");
            if (!this.LoadingInProgressOrReadyToLaunch)
            {
                //    Log.Message("5 1");
                return;
            }
            //    Log.Message("6 ");
            if (!this.AllRequiredThingsLoaded)
            {
                //    Log.Message("7 ");
                if (this.dropEverythingIfUnsatisfied)
                {
                    this.Transporter.CancelLoad();
                }
                else if (this.dropNonRequiredIfUnsatisfied)
                {
                    for (int i = 0; i < transportersInGroup.Count; i++)
                    {
                        for (int j = transportersInGroup[i].innerContainer.Count - 1; j >= 0; j--)
                        {
                            Thing thing = transportersInGroup[i].innerContainer[j];
                            Pawn pawn;
                            if (!this.IsRequired(thing) && (this.requiredColonistCount <= 0 || (pawn = (thing as Pawn)) == null || !pawn.IsColonist))
                            {
                                Thing thing2;
                                transportersInGroup[i].innerContainer.TryDrop(thing, ThingPlaceMode.Near, out thing2, null, null);
                            }
                        }
                    }
                }
            }
            //    Log.Message("8 ");
            this.sending = true;
            bool allRequiredThingsLoaded = this.AllRequiredThingsLoaded;
            //    Log.Message("9 ");
            Map map = this.parent.Map;
            this.Transporter.TryRemoveLord(map);
            string signalPart = allRequiredThingsLoaded ? "SentSatisfied" : "SentUnsatisfied";
            //    Log.Message("10 ");
            /*
            for (int k = 0; k < transportersInGroup.Count; k++)
            {
                QuestUtility.SendQuestTargetSignals(transportersInGroup[k].parent.questTags, signalPart, transportersInGroup[k].parent.Named("SUBJECT"), transportersInGroup[k].innerContainer.ToList<Thing>().Named("SENT"));
            }
            */
            List<Pawn> list = new List<Pawn>();

            //    Log.Message("11 ");
            Transporter.innerContainer.ClearAndDestroyContentsOrPassToWorld(DestroyMode.Vanish);
            Thing newThing = ThingMaker.MakeThing(Props.leaving, null);
            Transporter.CleanUpLoadingVars(map);
            Transporter.parent.Destroy(DestroyMode.QuestLogic);
            GenSpawn.Spawn(newThing, Transporter.parent.Position, map, WipeMode.Vanish);
            /*
            if (list.Count != 0)
            {
                for (int n = 0; n < transportersInGroup.Count; n++)
                {
                    QuestUtility.SendQuestTargetSignals(transportersInGroup[n].parent.questTags, "SentWithExtraColonists", transportersInGroup[n].parent.Named("SUBJECT"), list.Named("SENTCOLONISTS"));
                }
            }
            */
            this.sending = false;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!autodustoff)
            {

                if (this.parent.IsHashIntervalTick(120))
                {
                    this.CheckAutoload();
                }
                if (this.leaveASAP && this.parent.Spawned)
                {
                    if (!this.LoadingInProgressOrReadyToLaunch)
                    {
                        TransporterUtility.InitiateLoading(Gen.YieldSingle<CompTransporter>(this.Transporter));
                    }
                    this.Send();
                }
                if (this.leaveImmediatelyWhenSatisfied && this.AllRequiredThingsLoaded)
                {
                    this.Send();
                }
            }
            else
            {
                if (parent.Map != null)
                {
                    dustoffdelay--;
                    if (dustoffdelay == 60)
                    {
                        if (!this.Transporter.innerContainer.NullOrEmpty())
                        {
                            this.Transporter.innerContainer.TryDropAll(parent.Position, parent.Map, ThingPlaceMode.Near);
                        }
                    }
                    if (dustoffdelay <= 0)
                    {
                        this.Send();
                        autodustoff = false;
                    }
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look<ThingDefCount>(ref this.requiredItems, "requiredItems", LookMode.Deep, Array.Empty<object>());
            Scribe_Collections.Look<Pawn>(ref this.requiredPawns, "requiredPawns", LookMode.Reference, Array.Empty<object>());
            Scribe_Values.Look<int>(ref this.requiredColonistCount, "requiredColonistCount", 0, false);
            Scribe_Values.Look<bool>(ref this.acceptColonists, "acceptColonists", false, false);
            Scribe_Values.Look<bool>(ref this.onlyAcceptColonists, "onlyAcceptColonists", false, false);
            Scribe_Values.Look<bool>(ref this.leaveImmediatelyWhenSatisfied, "leaveImmediatelyWhenSatisfied", false, false);
            Scribe_Values.Look<bool>(ref this.autoload, "autoload", false, false);
            Scribe_Values.Look<bool>(ref this.dropEverythingIfUnsatisfied, "dropEverythingIfUnsatisfied", false, false);
            Scribe_Values.Look<bool>(ref this.dropNonRequiredIfUnsatisfied, "dropNonRequiredIfUnsatisfied", false, false);
            Scribe_Values.Look<bool>(ref this.leaveASAP, "leaveASAP", false, false);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.requiredPawns.RemoveAll((Pawn x) => x == null);
            }
        }

        private void CheckAutoload()
        {
            if (!this.autoload || !this.LoadingInProgressOrReadyToLaunch || !this.parent.Spawned)
            {
                return;
            }
            CompDropship.tmpRequiredItems.Clear();
            CompDropship.tmpRequiredItems.AddRange(this.requiredItems);
            CompDropship.tmpRequiredPawns.Clear();
            CompDropship.tmpRequiredPawns.AddRange(this.requiredPawns);
            ThingOwner innerContainer = this.Transporter.innerContainer;
            for (int i = 0; i < innerContainer.Count; i++)
            {
                Pawn pawn = innerContainer[i] as Pawn;
                if (pawn != null)
                {
                    CompDropship.tmpRequiredPawns.Remove(pawn);
                }
                else
                {
                    int num = innerContainer[i].stackCount;
                    for (int j = 0; j < CompDropship.tmpRequiredItems.Count; j++)
                    {
                        if (CompDropship.tmpRequiredItems[j].ThingDef == innerContainer[i].def)
                        {
                            int num2 = Mathf.Min(CompDropship.tmpRequiredItems[j].Count, num);
                            if (num2 > 0)
                            {
                                CompDropship.tmpRequiredItems[j] = CompDropship.tmpRequiredItems[j].WithCount(CompDropship.tmpRequiredItems[j].Count - num2);
                                num -= num2;
                            }
                        }
                    }
                }
            }
            for (int k = CompDropship.tmpRequiredItems.Count - 1; k >= 0; k--)
            {
                if (CompDropship.tmpRequiredItems[k].Count <= 0)
                {
                    CompDropship.tmpRequiredItems.RemoveAt(k);
                }
            }
            if (CompDropship.tmpRequiredItems.Any<ThingDefCount>() || CompDropship.tmpRequiredPawns.Any<Pawn>())
            {
                if (this.Transporter.leftToLoad != null)
                {
                    this.Transporter.leftToLoad.Clear();
                }
                CompDropship.tmpAllSendablePawns.Clear();
                CompDropship.tmpAllSendablePawns.AddRange(TransporterUtility.AllSendablePawns(this.TransportersInGroup, this.parent.Map, false));
                CompDropship.tmpAllSendableItems.Clear();
                CompDropship.tmpAllSendableItems.AddRange(TransporterUtility.AllSendableItems(this.TransportersInGroup, this.parent.Map, false));
                CompDropship.tmpAllSendableItems.AddRange(TransporterUtility.ThingsBeingHauledTo(this.TransportersInGroup, this.parent.Map));
                CompDropship.tmpRequiredPawnsPossibleToSend.Clear();
                for (int l = 0; l < CompDropship.tmpRequiredPawns.Count; l++)
                {
                    if (CompDropship.tmpAllSendablePawns.Contains(CompDropship.tmpRequiredPawns[l]))
                    {
                        TransferableOneWay transferableOneWay = new TransferableOneWay();
                        transferableOneWay.things.Add(CompDropship.tmpRequiredPawns[l]);
                        this.Transporter.AddToTheToLoadList(transferableOneWay, 1);
                        CompDropship.tmpRequiredPawnsPossibleToSend.Add(CompDropship.tmpRequiredPawns[l]);
                    }
                }
                for (int m = 0; m < CompDropship.tmpRequiredItems.Count; m++)
                {
                    if (CompDropship.tmpRequiredItems[m].Count > 0)
                    {
                        int num3 = 0;
                        for (int n = 0; n < CompDropship.tmpAllSendableItems.Count; n++)
                        {
                            if (CompDropship.tmpAllSendableItems[n].def == CompDropship.tmpRequiredItems[m].ThingDef)
                            {
                                num3 += CompDropship.tmpAllSendableItems[n].stackCount;
                            }
                        }
                        if (num3 > 0)
                        {
                            TransferableOneWay transferableOneWay2 = new TransferableOneWay();
                            for (int num4 = 0; num4 < CompDropship.tmpAllSendableItems.Count; num4++)
                            {
                                if (CompDropship.tmpAllSendableItems[num4].def == CompDropship.tmpRequiredItems[m].ThingDef)
                                {
                                    transferableOneWay2.things.Add(CompDropship.tmpAllSendableItems[num4]);
                                }
                            }
                            int count = Mathf.Min(CompDropship.tmpRequiredItems[m].Count, num3);
                            this.Transporter.AddToTheToLoadList(transferableOneWay2, count);
                        }
                    }
                }
                TransporterUtility.MakeLordsAsAppropriate(CompDropship.tmpRequiredPawnsPossibleToSend, this.TransportersInGroup, this.parent.Map);
                CompDropship.tmpAllSendablePawns.Clear();
                CompDropship.tmpAllSendableItems.Clear();
                CompDropship.tmpRequiredItems.Clear();
                CompDropship.tmpRequiredPawns.Clear();
                CompDropship.tmpRequiredPawnsPossibleToSend.Clear();
                return;
            }
            if (this.Transporter.leftToLoad != null)
            {
                this.Transporter.leftToLoad.Clear();
            }
            TransporterUtility.MakeLordsAsAppropriate(CompDropship.tmpRequiredPawnsPossibleToSend, this.TransportersInGroup, this.parent.Map);
        }

        public bool IsRequired(Thing thing)
        {
            Pawn pawn = thing as Pawn;
            if (pawn != null)
            {
                return this.requiredPawns.Contains(pawn);
            }
            for (int i = 0; i < this.requiredItems.Count; i++)
            {
                if (this.requiredItems[i].ThingDef == thing.def && this.requiredItems[i].Count != 0)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsAllowed(Thing t)
        {
            if (this.IsRequired(t))
            {
                return true;
            }
            if (this.acceptColonists)
            {
                Pawn pawn = t as Pawn;
                if (pawn != null && (pawn.IsColonist || (!this.onlyAcceptColonists && pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer)) && (!this.onlyAcceptColonists || !pawn.IsQuestLodger()))
                {
                    return true;
                }
            }
            return false;
        }

        public List<ThingDefCount> requiredItems = new List<ThingDefCount>();

        public List<Pawn> requiredPawns = new List<Pawn>();

        public int requiredColonistCount;

        public bool acceptColonists;

        public bool onlyAcceptColonists;

        public bool dropEverythingIfUnsatisfied;

        public bool dropNonRequiredIfUnsatisfied = true;

        public bool leaveImmediatelyWhenSatisfied;

        private bool autoload;

        public bool leaveASAP;

        private CompTransporter cachedCompTransporter;

        private List<CompTransporter> cachedTransporterList;

        private bool sending;

        private const int CheckAutoloadIntervalTicks = 120;

        private static readonly Texture2D AutoloadToggleTex = ContentFinder<Texture2D>.Get("UI/Commands/Autoload", true);

        private static readonly Texture2D SendCommandTex = CompLaunchable.LaunchCommandTex;

        private static List<ThingDefCount> tmpRequiredItemsWithoutDuplicates = new List<ThingDefCount>();

        private static List<string> tmpRequiredLabels = new List<string>();

        private static List<ThingDefCount> tmpRequiredItems = new List<ThingDefCount>();

        private static List<Pawn> tmpRequiredPawns = new List<Pawn>();

        private static List<Pawn> tmpAllSendablePawns = new List<Pawn>();

        private static List<Thing> tmpAllSendableItems = new List<Thing>();

        private static List<Pawn> tmpRequiredPawnsPossibleToSend = new List<Pawn>();

        private Caravan carr;
        private const float FuelPerTile = 2.25f;
        public static readonly Texture2D TargeterMouseAttachment = ContentFinder<Texture2D>.Get("UI/Overlays/LaunchableMouseAttachment", true);
    }
}
