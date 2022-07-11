using Sandbox.Common.ObjectBuilders;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace Faction_TerritoriesV2
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Beacon), false, "ClaimBlock")]
    public class ClaimLogic : MyGameLogicComponent
    {
        public IMyBeacon beacon;
        private bool isServer;
        private Vector3 BlockColor = new Vector3(0, 0, 0);
        private MyStringHash texture;


        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void UpdateOnceBeforeFrame()
        {
            isServer = MyAPIGateway.Session.IsServer;
            beacon = Entity as IMyBeacon;
            if (beacon == null) return;

            if (beacon?.CubeGrid?.Physics == null)
            {
                beacon?.CubeGrid?.Close();
                return;
            }

            Events.BeaconSetup(beacon);
            BlockColor = beacon.SlimBlock.ColorMaskHSV;
            texture = beacon.SlimBlock.SkinSubtypeId;
            //AddInventory();
        }

        public override void UpdateBeforeSimulation100()
        {
            if (!isServer) return;
            if (beacon.SlimBlock.ColorMaskHSV != BlockColor)
            {
                BlockColor = beacon.SlimBlock.ColorMaskHSV;

                ClaimBlockSettings settings;
                if (!Session.Instance.claimBlocks.TryGetValue(beacon.EntityId, out settings)) return;
                IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
                Utils.SetScreen(beacon, blockFaction, true);

                settings.BlockEmissive = settings.BlockEmissive;
            }

            if (beacon.SlimBlock.SkinSubtypeId != texture)
            {
                texture = beacon.SlimBlock.SkinSubtypeId;

                ClaimBlockSettings settings;
                if (!Session.Instance.claimBlocks.TryGetValue(beacon.EntityId, out settings)) return;
                IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
                Utils.SetScreen(beacon, blockFaction, true);

                settings.BlockEmissive = settings.BlockEmissive;
            }
        }

        private void AddInventory()
        {
            MyDefinitionId token = new MyDefinitionId(typeof(MyObjectBuilder_Component), "ZoneChip");
            MyInventory inventory = new MyInventory(0.05f, new Vector3(1, 1, 1), MyInventoryFlags.CanReceive | MyInventoryFlags.CanSend);
            MyInventoryConstraint constraint = new MyInventoryConstraint("ClaimBlock");
            constraint.Add(token);
            inventory.Constraint = constraint;
            beacon.Components.Add<MyInventoryBase>(inventory);

            var inv = (MyInventory)beacon.GetInventory(0);
            if (inventory == null)
            {
                return;
            }

            inv.SetFlags(MyInventoryFlags.CanReceive | MyInventoryFlags.CanSend);
        }

        public override void OnRemovedFromScene()
        {
            if (Entity == null) return;
            if (beacon?.CubeGrid?.Physics == null) return;
            if (beacon == null || beacon.MarkedForClose) return;
            //Unregister any handlers here
            beacon.AppendingCustomInfo -= Events.UpdateCustomInfo;

            if (isServer)
            {
                MyLog.Default.WriteLineAndConsole($"Beacon Block Removed From Scene - Session Null? > {Session.Instance == null}");
                beacon.OwnershipChanged -= Events.CheckOwnership;
                beacon.CubeGrid.OnIsStaticChanged -= Events.CheckGridStatic;

                ClaimBlockSettings settings;
                if (Session.Instance.claimBlocks.TryGetValue(beacon.EntityId, out settings))
                {
                    foreach(var item in settings.GetInstallations)
                    {
                        IMyEntity ent;
                        if (!MyAPIGateway.Entities.TryGetEntityById(item.GridEntityId, out ent)) continue;

                        IMyCubeGrid grid = ent as IMyCubeGrid;
                        if (grid == null) continue;

                        grid.OnGridSplit -= Events.OnInstallationSplit;
                    }
                }

                /*ClaimBlockSettings settings;
                Session.Instance.claimBlocks.TryGetValue(beacon.EntityId, out settings);
                if (settings != null)
                {
                    Session.Instance.UpdateParticlesToRun(settings, false);
                }*/

                MyVisualScriptLogicProvider.RemoveTrigger(beacon.EntityId.ToString());
                Triggers.RemoveTriggerData(beacon.EntityId);
                Session.Instance.claimBlocks.Remove(beacon.EntityId);
                Comms.SendRemoveBlockToOthers(beacon.EntityId);

                //MyVisualScriptLogicProvider.ShowNotificationToAll($"Beacon Removed", 20000);
            }
            else
            {
                ClaimBlockSettings settings;
                if (!Session.Instance.claimBlocks.TryGetValue(beacon.EntityId, out settings)) return;

                settings.Block = null;
            }
        }
    }
}
