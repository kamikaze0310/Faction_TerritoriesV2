using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using SpaceEngineers.Game.Entities.Weapons;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using Sandbox.ModAPI.Interfaces.Terminal;
using Sandbox.ModAPI.Interfaces;

namespace Faction_TerritoriesV2
{
    public static class Events
    {
        private static bool controlsInitBeacon;
        private static bool controlsInitJumpDrive;

        public static void InitProduction(MyEntity entity)
        {
            if (entity as IMyProductionBlock != null)
            {
                var production = entity as IMyProductionBlock;
                Utils.SetDefaultProduction(production as MyCubeBlock);
                if (Session.Instance.isServer) return;
                foreach (var setting in Session.Instance.claimBlocks.Values)
                {
                    if (!setting.Enabled || !setting.IsClaimed) continue;
                    if (Vector3D.Distance(setting.TerritoryConfig._territoryOptions._centerOnPlanet ? setting.PlanetCenter : setting.BlockPos, production.GetPosition()) > setting.TerritoryConfig._territoryRadius) continue;

                    IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(production.OwnerId);
                    if (blockFaction == null) return;
                    if (setting.AllowTerritoryAllies)
                    {
                        if (Utils.IsFactionEnemy(setting, blockFaction)) return;
                    }
                    else { if (blockFaction.Tag != setting.ClaimedFaction) return; }

                    Utils.AddProductionPerk(setting, production as MyCubeBlock);
                    return;
                }
            }
        }

        public static void EntityAdd(IMyEntity entity)
        {
            MyAPIGateway.Parallel.StartBackground(() =>
            {
                MyCubeGrid grid = entity as MyCubeGrid;
                if (grid != null)
                {
                    foreach (var block in grid.GetFatBlocks())
                    {
                        if (block as IMyProductionBlock != null)
                        {
                            MyAPIGateway.Utilities.InvokeOnGameThread(() => InitProduction(block as MyEntity));
                            continue;
                        }
                    }

                    return;
                }

                MySafeZone zone = entity as MySafeZone;
                if (zone != null)
                {
                    if (zone.DisplayName != null)
                        if (zone.DisplayName.Contains("(FactionTerritory)"))
                        {
                            var split = zone.DisplayName.Split('_');
                            if (split.Length == 3)
                            {
                                long claimId;
                                ClaimBlockSettings settings;
                                long.TryParse(split[2], out claimId);
                                if (!Session.Instance.claimBlocks.TryGetValue(claimId, out settings))
                                {
                                    return;
                                }

                                zone.Radius = Utils.GetSafeZoneRadius(settings);
                                MyAPIGateway.Utilities.InvokeOnGameThread(() => zone.RecreatePhysics());
                            }

                            return;
                        }

                    if (!MyAPIGateway.Multiplayer.IsServer) return;
                    if (!Session.Instance.init) return;

                    /*foreach (var settings in Session.Instance.claimBlocks.Values)
                    {
                        Installations installation = settings.GetInstallationByGridId(entity.EntityId);
                        if (installation != null)
                        {
                            if (installation.Enabled && installation._installationParticle == null && installation.Integrity < 25)
                                Comms.SyncParticleEffect("ExhaustFire", settings.BlockPos, settings, installation, 3f);
                        }
                    }*/

                    foreach (var claim in Session.Instance.claimBlocks.Values)
                    {
                        if (Vector3D.Distance(claim.BlockPos, zone.PositionComp.GetPosition()) <= 10000)
                        {
                            long blockId = zone.SafeZoneBlockId;
                            if (blockId == 0)
                            {
                                MyAPIGateway.Utilities.InvokeOnGameThread(() => entity.Close());
                                return;
                            }

                            IMySafeZoneBlock block = null;
                            Session.Instance.safeZoneBlocks.TryGetValue(blockId, out block);
                            if (block != null)
                            {
                                MyAPIGateway.Parallel.Sleep(5);
                                MyAPIGateway.Utilities.InvokeOnGameThread(() =>
                                {
                                    var prop = block.GetProperty("SafeZoneCreate");
                                    var prop2 = prop as ITerminalProperty<bool>;

                                    if (prop2 != null)
                                    {
                                        prop2.SetValue(block, false);
                                    }
                                    return;
                                });

                                return;
                            }

                            MyAPIGateway.Utilities.InvokeOnGameThread(() => entity.Close());
                            return;
                        }

                        if (claim.IsClaimed)
                        {
                            if (Vector3D.Distance(claim.TerritoryConfig._territoryOptions._centerOnPlanet ? claim.PlanetCenter : claim.BlockPos, zone.PositionComp.GetPosition()) <= claim.TerritoryConfig._territoryRadius)
                            {
                                if (zone.DisplayName != null && zone.DisplayName.Contains("FSZ"))
                                {
                                    var split = zone.DisplayName.Split('_');
                                    IMyFaction zoneFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(split[1]);
                                    IMyFaction claimFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(claim.ClaimedFaction);
                                    if (zoneFaction == null)
                                    {
                                        MyAPIGateway.Utilities.InvokeOnGameThread(() => entity.Close());
                                        return;
                                    }

                                    if (claimFaction == zoneFaction) return;
                                }
                                else
                                {
                                    if (claim.GetSafeZones.Contains(zone.EntityId)) return;

                                    long zoneBlockId = zone.SafeZoneBlockId;
                                    IMySafeZoneBlock zoneBlock = null;

                                    Session.Instance.safeZoneBlocks.TryGetValue(zoneBlockId, out zoneBlock);
                                    if (zoneBlock != null)
                                    {
                                        IMyFaction zoneFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(zoneBlock.OwnerId);
                                        if (zoneFaction != null)
                                        {
                                            if (zoneFaction.Tag == claim.ClaimedFaction) return;
                                        }

                                        foreach (var item in claim.GetZonesDelayRemove)
                                        {
                                            if (item.zoneId == zoneBlockId) return;
                                        }

                                        MyAPIGateway.Parallel.Sleep(5);
                                        MyAPIGateway.Utilities.InvokeOnGameThread(() =>
                                        {
                                            var prop = zoneBlock.GetProperty("SafeZoneCreate");
                                            var prop2 = prop as ITerminalProperty<bool>;

                                            if (prop2 != null)
                                            {
                                                prop2.SetValue(zoneBlock, false);
                                            }
                                            return;
                                        });

                                        return;
                                    }
                                }

                                MyAPIGateway.Utilities.InvokeOnGameThread(() => entity.Close());
                                return;
                            }
                        }
                    }
                }
            });
        }

        public static void EntityCreate(MyEntity entity)
        {
            MyAPIGateway.Parallel.StartBackground(() =>
            {
                MyAPIGateway.Parallel.Sleep(2000);
                IMyTextPanel panel = entity as IMyTextPanel;
                if (panel == null)
                {
                    return;
                }


                if (panel.CubeGrid?.Physics == null) return;

                if (panel.CustomName.Contains("[Territory]"))
                {
                    if (!Session.Instance.activeLCDs.Contains(panel))
                        Session.Instance.activeLCDs.Add(panel);

                    Utils.GetTerritoriesStatus(panel);
                }

                panel.CustomNameChanged += CheckBlockName;
            });
        }

        public static void EntityRemoved(MyEntity entity)
        {
            IMyEntity ent = entity as IMyEntity;
            if (ent == null) return;
            IMySlimBlock slim = ent as IMySlimBlock;
            if (slim == null)
            {
                MyVisualScriptLogicProvider.ShowNotificationToAll("Slim is null", 20000);
                return;
            }
            IMyCubeBlock block = slim.FatBlock;
            if (block == null) return;
            IMyTerminalBlock t = block as IMyTerminalBlock;
            if (t == null) return;
            IMyTextSurface panel = t as IMyTextSurface;
            if (panel == null) return;

            if (Session.Instance.activeLCDs.Contains(panel))
                Session.Instance.activeLCDs.Remove(panel);

            t.CustomNameChanged -= CheckBlockName;
        }

        public static void CheckBlockName(IMyTerminalBlock block)
        {
            MyVisualScriptLogicProvider.ShowNotificationToAll("Custom Name Event Fired", 8000);
            IMyTextSurface panel = block as IMyTextSurface;
            if (panel == null) return;

            if (block.CustomName.Contains("[Territory]"))
            {
                MyVisualScriptLogicProvider.ShowNotificationToAll("Found Territory LCD", 8000);
                if (!Session.Instance.activeLCDs.Contains(panel))
                    Session.Instance.activeLCDs.Add(panel);

                Utils.GetTerritoriesStatus(block);
            }
            else if (Session.Instance.activeLCDs.Contains(panel))
                Session.Instance.activeLCDs.Remove(panel);
        }

        public static void FatBlockAdded(MyCubeBlock block)
        {
            MyAPIGateway.Parallel.StartBackground(() =>
            {
                MyAPIGateway.Parallel.Sleep(5);
                if (block as IMyShipController != null ||
                block as IMyPowerProducer != null ||
                block as IMyShipToolBase != null ||
                block as IMyShipDrill != null ||
                block as IMyProductionBlock != null)
                {
                    ClaimBlockSettings settings = null;
                    foreach (var data in Session.Instance.claimBlocks.Values)
                    {
                        if (!data.Enabled || !data.IsClaimed) continue;
                        if (Vector3D.Distance(block.PositionComp.GetPosition(), data.TerritoryConfig._territoryOptions._centerOnPlanet ? data.PlanetCenter : data.BlockPos) > data.TerritoryConfig._territoryRadius) continue;
                        settings = data;
                        break;
                    }

                    if (settings == null) return;
                    MyCubeGrid grid = block.CubeGrid;
                    settings.UpdatesBlocksMonitored(grid, block, true);
                }
            });
        }

        public static void FatBlockClosed(MyCubeBlock block)
        {
            if (block as IMyShipController != null ||
                block as IMyPowerProducer != null ||
                block as IMyShipToolBase != null ||
                block as IMyShipDrill != null ||
                block as IMyProductionBlock != null)
            {
                ClaimBlockSettings settings = null;
                foreach (var data in Session.Instance.claimBlocks.Values)
                {
                    if (!data.Enabled || !data.IsClaimed) continue;
                    if (Vector3D.Distance(block.PositionComp.GetPosition(), data.TerritoryConfig._territoryOptions._centerOnPlanet ? data.PlanetCenter : data.BlockPos) > data.TerritoryConfig._territoryRadius) continue;
                    settings = data;
                    break;
                }

                if (settings == null) return;
                MyCubeGrid grid = block.CubeGrid;
                settings.UpdatesBlocksMonitored(grid, block, false);
                CheckForWorkingBlock(block, settings);
                //MyVisualScriptLogicProvider.ShowNotificationToAll($"Fat Block Closed | {MyAPIGateway.Multiplayer.IsServer}", 5000, "Green");
            }
        }

        /*public static void FatBlockCheckFunctional(IMySlimBlock slim)
        {
            IMyCubeBlock block = slim.FatBlock;
            if (block == null) return;
   
            if (block as IMyShipController != null ||
                block as IMyPowerProducer != null ||
                block as IMyShipToolBase != null ||
                block as IMyShipDrill != null ||
                block as IMyProductionBlock != null)
            {
                if (block.IsFunctional) return;
                ClaimBlockSettings settings = null;
                foreach (var data in Session.Instance.claimBlocks.Values)
                {
                    if (!data.Enabled || !data.IsClaimed) continue;
                    if (Vector3D.Distance(block.PositionComp.GetPosition(), data.BlockPos) > data.ClaimRadius) continue;
                    settings = data;
                    break;
                }

                if (settings == null) return;
                //MyCubeGrid grid = block.CubeGrid;
                //settings.UpdatesBlocksMonitored(grid, block, false);
                CheckForWorkingBlock(block, settings);
                MyVisualScriptLogicProvider.ShowNotificationToAll($"Block is no longer functional!!! | {MyAPIGateway.Multiplayer.IsServer}", 5000, "Green");
            }
        }*/

        public static void CheckForWorkingBlock(IMyCubeBlock block, ClaimBlockSettings settings)
        {
            try
            {
                if (settings == null) return;

                bool functional = false;
                MyCubeGrid grid = block.CubeGrid as MyCubeGrid;
                IMyShipController controller = block as IMyShipController;
                if (controller != null)
                {
                    functional = controller.IsFunctional;

                    if (!functional)
                    {
                        if (!settings._server._gridsInside.ContainsKey(grid.EntityId))
                        {
                            MyLog.Default.WriteLineAndConsole($"Territories: Key does not exist for controllers --{grid.EntityId}--");
                            return;
                        }
                        foreach (var t in settings._server._gridsInside[grid.EntityId].blocksMonitored.controllers)
                        {
                            functional = t.IsFunctional;
                            if (functional)
                            {
                                IMyFaction blockOwner = MyAPIGateway.Session.Factions.TryGetPlayerFaction(t.OwnerId);
                                if (blockOwner != null)
                                {
                                    functional = blockOwner.Tag == settings.ClaimedFaction;
                                    break;
                                }
                                else
                                {
                                    functional = false;
                                }

                            }
                        }
                    }

                    settings.UpdateGridData(grid.EntityId, ClaimBlockSettings.GridChangeType.Controller, functional);
                    return;
                }

                IMyPowerProducer power = block as IMyPowerProducer;
                if (power != null)
                {
                    functional = power.IsFunctional;

                    if (!functional)
                    {
                        if (!settings._server._gridsInside.ContainsKey(grid.EntityId))
                        {
                            MyLog.Default.WriteLineAndConsole($"Territories: Key does not exist for power --{grid.EntityId}--");
                            return;
                        }
                        foreach (var t in settings._server._gridsInside[grid.EntityId].blocksMonitored.powers)
                        {
                            functional = t.IsFunctional;
                            if (functional)
                            {
                                IMyFaction blockOwner = MyAPIGateway.Session.Factions.TryGetPlayerFaction(t.OwnerId);
                                if (blockOwner != null)
                                {
                                    functional = blockOwner.Tag == settings.ClaimedFaction;
                                    break;
                                }
                                else
                                {
                                    functional = false;
                                }

                            }
                        }
                    }

                    settings.UpdateGridData(grid.EntityId, ClaimBlockSettings.GridChangeType.Power, functional);
                    return;
                }
            }
            catch (Exception ex)
            {
                MyLog.Default.WriteLineAndConsole($"Territories: Crash Logged --- {ex.StackTrace}");
            }

        }

        public static void IsWorkingChanged(IMyCubeBlock block)
        {
            if (block as IMyShipController != null || block as IMyPowerProducer != null || block as IMyShipToolBase != null || block as IMyShipDrill != null || block as IMyProductionBlock != null)
            {
                IMyFunctionalBlock fBlock = block as IMyFunctionalBlock;
                ClaimBlockSettings settings = null;
                foreach (var data in Session.Instance.claimBlocks.Values)
                {
                    if (!data.Enabled || !data.IsClaimed) continue;
                    if (Vector3D.Distance(block.GetPosition(), data.TerritoryConfig._territoryOptions._centerOnPlanet ? data.PlanetCenter : data.BlockPos) > data.TerritoryConfig._territoryRadius) continue;
                    settings = data;
                    break;
                }

                if (settings == null) return;
                IMyShipToolBase tool = block as IMyShipToolBase;
                IMyShipDrill drill = block as IMyShipDrill;
                //IMyProductionBlock production = block as IMyProductionBlock;

                if (fBlock != null)
                {
                    if (tool != null || drill != null)
                    {
                        if (settings.TerritoryConfig._territoryOptions._allowTools) return;
                        if (block is IMyShipDrill && settings.TerritoryConfig._territoryOptions._allowDrilling) return;
                        if (block is IMyShipWelder && settings.TerritoryConfig._territoryOptions._allowWelding) return;
                        if (block is IMyShipGrinder && settings.TerritoryConfig._territoryOptions._allowGrinding) return;

                        IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.OwnerId);
                        if (blockFaction != null)
                        {
                            if (blockFaction.Tag == settings.ClaimedFaction) return;

                            if (settings.AllowTerritoryAllies || settings.AllowSafeZoneAllies)
                            {
                                if (!Utils.IsFactionEnemy(settings, blockFaction)) return;
                            }
                        }

                        fBlock.Enabled = false;
                        return;
                    }

                    /*if (production != null)
                    {
                        if (!fBlock.Enabled) return;
                        Utils.AddAllProductionMultipliers(settings, block as MyCubeBlock, true);
                        return;
                    }*/
                }

                CheckForWorkingBlock(block, settings);
            }
        }

        public static void PbWatcher(IMyCubeBlock block)
        {
            if (block == null) return;

            IMyProgrammableBlock pb = block as IMyProgrammableBlock;
            if (pb == null) return;

            if (pb.Enabled)
            {
                foreach (var settings in Session.Instance.claimBlocks.Values)
                {
                    if (Vector3D.Distance(block.GetPosition(), settings.BlockPos) > settings.TerritoryConfig._safeZoneRadius) continue;
                    pb.Enabled = false;
                }
            }
        }

        public static void CheckOwnership(IMyTerminalBlock block)
        {
            //MyAPIGateway.Parallel.StartBackground(() =>
            //{
            //MyAPIGateway.Parallel.Sleep(20);
            /* MyCubeBlock cubeblock = block as MyCubeBlock;
             if (cubeblock == null) return;

             ClaimBlockSettings settings;
             if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

             if (settings.IsClaimed)
             {
                 IMyFaction blockOwner = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.OwnerId);
                 if (blockOwner == null || blockOwner.Tag != settings.ClaimedFaction)
                 {
                     IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
                     if (faction != null)
                         cubeblock.ChangeBlockOwnerRequest(faction.FounderId, VRage.Game.MyOwnershipShareModeEnum.Faction);
                     else
                         cubeblock.ChangeBlockOwnerRequest(Session.Instance.blockOwner.FounderId, VRage.Game.MyOwnershipShareModeEnum.All);
                 }
             }
             else
             {
                 cubeblock.ChangeBlockOwnerRequest(Session.Instance.blockOwner.FounderId, VRage.Game.MyOwnershipShareModeEnum.Faction);
             }*/

            //var share = cubeblock.IDModule.ShareMode;
            //if (share == VRage.Game.MyOwnershipShareModeEnum.All) return;

            /*IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.OwnerId);
            if (faction == null || !faction.IsEveryoneNpc())
            {
                IMyFaction npcFaction = Session.Instance.blockOwner;
                if (npcFaction == null)
                {
                    Session.Instance.blockOwner = MyAPIGateway.Session.Factions.TryGetFactionByTag("SPRT");
                    npcFaction = Session.Instance.blockOwner;
                }
            }*/


            //});
        }

        public static void ProductionOwnershipChanged(IMyTerminalBlock block)
        {
            if (block == null) return;
            IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.OwnerId);
            foreach (var item in Session.Instance.claimBlocks.Values)
            {
                if (!item.Enabled || !item.IsClaimed) continue;
                if (Vector3D.Distance(block.GetPosition(), item.TerritoryConfig._territoryOptions._centerOnPlanet ? item.PlanetCenter : item.BlockPos) > item.TerritoryConfig._territoryRadius) continue;

                if (block.OwnerId == 0 || blockFaction == null)
                {
                    Utils.RemoveProductionPerk(item, block as MyCubeBlock);
                    return;
                }

                if (item.ClaimedFaction == blockFaction.Tag)
                {
                    Utils.AddProductionPerk(item, block as MyCubeBlock);
                    return;
                }

                if (item.AllowTerritoryAllies && !Utils.IsFactionEnemy(item, blockFaction))
                {
                    Utils.AddProductionPerk(item, block as MyCubeBlock);
                    return;
                }
                else
                {
                    Utils.RemoveProductionPerk(item, block as MyCubeBlock);
                }
            }
        }

        public static void CheckGridStatic(IMyCubeGrid cubeGrid, bool isStatic)
        {
            if (!isStatic)
                cubeGrid.IsStatic = true;
        }

        private static bool IsAdmin(long owner)
        {
            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach (var player in players)
            {
                if (player.IdentityId == owner)
                {
                    if (player.PromoteLevel == MyPromoteLevel.Owner || player.PromoteLevel == MyPromoteLevel.Admin) return true;
                    break;
                }
            }

            return false;
        }

        public static void BeaconSetup(IMyBeacon beacon)
        {
            if (!controlsInitBeacon)
            {
                controlsInitBeacon = true;
                MyAPIGateway.TerminalControls.CustomControlGetter += ActionControls.CreateControlsBeaconNew;
            }

            if (MyAPIGateway.Session.IsServer)
            {
                MyCubeGrid cubeGrid = beacon.CubeGrid as MyCubeGrid;

                beacon.Enabled = false;
                beacon.CubeGrid.IsStatic = true;
                cubeGrid.Editable = false;
                cubeGrid.DestructibleBlocks = false;
                beacon.CubeGrid.CustomName = "Faction Territory Claim [NPC-IGNORE]";
                beacon.CustomName = "Faction Territory Claim";
                beacon.OwnershipChanged += CheckOwnership;
                beacon.CubeGrid.OnIsStaticChanged += CheckGridStatic;
                //CheckOwnership(beacon);

                ClaimBlockSettings data = Session.Instance.LoadClaimData(beacon);
                if (data != null)
                {
                    Comms.SendClientsSettings(data);

                    if (data.IsClaimed)
                        Utils.SetOwner(beacon, data);
                    else
                        Utils.SetOwner(beacon, data);

                    foreach (var item in data.GetInstallations)
                    {
                        IMyEntity ent;
                        if (!MyAPIGateway.Entities.TryGetEntityById(item.GridEntityId, out ent)) continue;
                        IMyCubeGrid grid = ent as IMyCubeGrid;
                        if (grid == null) continue;

                        grid.OnGridSplit += OnInstallationSplit;
                    }
                }
                else
                {
                    Utils.SetOwner(beacon, data);
                }


                Triggers.CreateNewTriggers(beacon);


                //MyVisualScriptLogicProvider.ShowNotification($"Count = {Session.Instance.claimBlocks.Count}", 5000);
            }

            beacon.AppendingCustomInfo += UpdateCustomInfo;
            ClaimBlockSettings settings = null;
            if (!Session.Instance.claimBlocks.TryGetValue(beacon.EntityId, out settings)) return;
            settings.Block = beacon as IMyTerminalBlock;
            Utils.SetEmissive(settings.BlockEmissive, beacon);

            if (settings.IsClaimed)
            {
                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
                Utils.SetScreen(beacon, faction);
            }
            else
            {
                Utils.SetScreen(beacon);
            }
        }

        public static void JumpDriveSetup(IMyJumpDrive jd)
        {
            if (!controlsInitJumpDrive)
            {
                controlsInitJumpDrive = true;
                MyAPIGateway.TerminalControls.CustomControlGetter += ActionControls.CreateControlsJumpdriveNew;
                MyAPIGateway.TerminalControls.CustomActionGetter += ActionControls.CreateActionsJumpdriveNew;
            }

            jd.AppendingCustomInfo += UpdateCustomInfo;
        }

        public static void SafeZoneBlockSetup(IMySafeZoneBlock zoneBlock)
        {
            if (MyAPIGateway.Session.IsServer)
            {
                if (Session.Instance.safeZoneBlocks.ContainsKey(zoneBlock.EntityId)) return;
                Session.Instance.safeZoneBlocks.Add(zoneBlock.EntityId, zoneBlock);
            }


            //MyVisualScriptLogicProvider.ShowNotification("Found Safezone block", 5000);
        }

        public static void OnInstallationSplit(IMyCubeGrid originalGrid, IMyCubeGrid newGrid)
        {
            List<IMyBeacon> beacons = new List<IMyBeacon>();
            MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(newGrid).GetBlocksOfType(beacons);
            if (beacons.Count == 0) return;

            foreach (var settings in Session.Instance.claimBlocks.Values)
            {
                if (!settings.Enabled || !settings.IsClaimed) continue;
                //if (Vector3D.Distance(originalGrid.GetPosition(), settings.BlockPos) > settings.TerritoryConfig._territoryRadius) continue;

                Installations installation = settings.GetInstallationByGridId(originalGrid.EntityId);
                if (installation == null) continue;

                MyCubeGrid cubeGrid = newGrid as MyCubeGrid;

                installation.GridEntityId = newGrid.EntityId;
                newGrid.OnGridSplit += OnInstallationSplit;
                cubeGrid.Editable = false;
                newGrid.IsStatic = true;
                originalGrid.OnGridSplit -= OnInstallationSplit;


                newGrid.CustomName = ($"{settings.UnclaimName} Territory - {installation.Type}(Installation) [NPC-IGNORE]");
                return;
            }
        }

        public static void OnInstallationTurretsClosing(IMyEntity entity)
        {
            if (Session.Instance == null) return;

            foreach(var settings in Session.Instance.claimBlocks.Values)
            {
                foreach(var installation in settings.GetInstallations)
                {
                    for (int i = installation.GetTurrets().Count - 1; i >= 0; i--)
                    {
                        if (installation.GetTurrets()[i].EntityId == entity.EntityId)
                        {
                            installation.GetTurrets().RemoveAt(i);
                            return;
                        }
                    }
                }
            }
        }

        public static void UpdateCustomInfo(IMyTerminalBlock block, StringBuilder sb)
        {
            if (block as IMyBeacon != null)
            {
                ClaimBlockSettings data;
                Session.Instance.claimBlocks.TryGetValue(block.EntityId, out data);
                if (data == null) return;

                sb.Clear();
                sb.Append(new StringBuilder(data.DetailInfo));
            }

            if (block as IMyJumpDrive != null)
            {
                sb.Clear();
                sb.AppendStringBuilder(Controls.sb);
                //sb.Append(new StringBuilder(Controls.text));
            }
        }

        public static void PlayerConnected(long playerId)
        {
            /*if (!Session.Instance.init)
            {
                MyLog.Default.WriteLineAndConsole("Territories: Client Connected, Init False, Returning");
                return;
            }

            MyLog.Default.WriteLineAndConsole("Territories: Client Connected, Init True, Clearing Data/Reloading");*/
            ulong steamId = MyAPIGateway.Players.TryGetSteamId(playerId);
            if (steamId == 0) return;

            Comms.ResetClientModData(steamId);
            //foreach (var item in Session.Instance.claimBlocks.Values)
            //Comms.SendClientsSettings(item, steamId);
        }

        public static void PlayerDisconnected(long playerId)
        {
            //GPS.RemoveCachedGps(playerId, GpsType.Tag);
            /*if (Session.Instance.connectedPlayers.ContainsKey(playerId))
                Session.Instance.connectedPlayers.Remove(playerId);*/
        }
    }
}
