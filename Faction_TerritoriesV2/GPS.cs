using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
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

namespace Faction_TerritoriesV2
{
    public struct DetectionSize
    {
        public string name;
        public float size;
    }

    public enum GpsType
    {
        Tag,
        Block,
        Player,
        All
    }

    public class GpsData
    {
        public long playerId;
        public string gpsName;
        public IMyEntity entity;
        public IMyGps gps;
        public GpsType gpsType;
        public IMyPlayer playerGps;
    }

    public static class GPS
    {
        public static List<DetectionSize> detections = new List<DetectionSize>();

        public static void AddBlockLocation(Vector3D pos, long playerId, ClaimBlockSettings settings)
        {
            IMyGps gps = null;
            if (settings.IsClaimed)
            {
                gps = CreateNewGps($"Claimed Territory: {settings.ClaimZoneName} ({settings.ClaimedFaction})", $"{settings.UnclaimName} (ClaimBlock)", pos, playerId, Color.Orange);
                if (gps != null)
                {
                    AddGpsData(gps, gps.Name, settings.Block.CubeGrid, playerId, settings, GpsType.Block);
                }
            }
            else
            {
                gps = CreateNewGps($"Unclaimed Territory: {settings.UnclaimName}", $"{settings.UnclaimName} (ClaimBlock)", pos, playerId, Color.Orange);
                if (gps != null)
                {
                    AddGpsData(gps, gps.Name, settings.Block.CubeGrid, playerId, settings, GpsType.Block);
                }
            }
        }

        public static void UpdateBlockText(ClaimBlockSettings settings, string text, long playerId = 0)
        {
            IMyCubeGrid grid = settings.Block?.CubeGrid;
            if (grid == null) return;
            bool foundgps = false;
            if (settings.GetGridsInside.Count != 0 && settings.GetGridsInside.ContainsKey(grid.EntityId))
            {
                List<GpsData> gpsData = settings.GetGridsInside[grid.EntityId].gpsData;
                for (int i = gpsData.Count - 1; i >= 0; i--)
                {
                    if (playerId != 0 && playerId != gpsData[i].playerId) continue;
                    if (gpsData[i].gpsType == GpsType.Block)
                    {
                        foundgps = true;
                        var gps = gpsData[i].gps;
                        gps.Name = text;
                        gps.Coords = settings.BlockPos;
                        gps.Description = $"{settings.UnclaimName} (ClaimBlock)";
                        MyAPIGateway.Session.GPS.ModifyGps(gpsData[i].playerId, gps);
                        MyVisualScriptLogicProvider.SetGPSColor(gps.Name, Color.Orange, gpsData[i].playerId);
                        gps.UpdateHash();
                        //Session.Instance.claimBlocks[settings.EntityId]._gridsInside[grid.EntityId].gpsData[i].gps = gps;
                        settings._server._gridsInside[grid.EntityId].gpsData[i].gps = gps;
                    }
                }
            }

            if (!foundgps)
            {
                bool foundInList = false;
                List<IMyGps> gpsList = MyAPIGateway.Session.GPS.GetGpsList(playerId);
                foreach (var gps in gpsList)
                {
                    if (gps.Description.Contains(settings.UnclaimName))
                    {
                        foundInList = true;
                        gps.Name = text;
                        gps.Coords = settings.BlockPos;
                        gps.Description = $"{settings.UnclaimName} (ClaimBlock)";
                        MyAPIGateway.Session.GPS.ModifyGps(playerId, gps);
                        MyVisualScriptLogicProvider.SetGPSColor(gps.Name, Color.Orange, playerId);
                        gps.UpdateHash();

                        AddGpsData(gps, gps.Name, settings.Block.CubeGrid, playerId, settings, GpsType.Block);
                        //MyVisualScriptLogicProvider.ShowNotificationToAll($"Add Gps", 15000, "Green");
                        break;
                    }
                }

                if (!foundInList)
                    AddBlockLocation(settings.BlockPos, playerId, settings);
            }

        }

        public static void TagEnemyPlayer(IMyPlayer enemy, ClaimBlockSettings settings, long playerId = 0L)
        {
            Installations installation = settings.GetInstallationByType(InstallationType.Radar);
            if (installation == null || !installation.Enabled) return;

            if (enemy == null || enemy.Character == null) return;
            string description = enemy.DisplayName;

            if (playerId != 0L)
            {
                IMyGps gps = CreateNewGps(description, "Faction Territories (Player)", enemy.GetPosition(), playerId, Color.Red);
                if (gps != null)
                {
                    AddGpsData(gps, description, enemy.Character, playerId, settings, GpsType.Player, enemy);
                    AudioPackage audioPackage = new AudioPackage(AudioClips.EnemyDetected, InstallationType.SafeZone, playerId, AudioType.Character, null);
                    //Comms.SendAudioToClient(null, playerId, "EnemyDetected");
                }
                return;
            }

            IMyFaction claimFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
            if (claimFaction != null)
            {
                List<IMyPlayer> players = new List<IMyPlayer>();
                MyAPIGateway.Players.GetPlayers(players);

                foreach (var player in players)
                {
                    IMyFaction playerFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(player.IdentityId);
                    if (playerFaction != claimFaction)
                    {
                        if (settings.AllowSafeZoneAllies || settings.AllowTerritoryAllies)
                        {
                            if (Utils.IsFactionEnemy(settings, playerFaction)) continue;
                        }
                        else continue;
                    }

                    IMyGps gps = CreateNewGps(description, "Faction Territories (Player)", enemy.GetPosition(), player.IdentityId, Color.Red);
                    if (gps != null)
                    {
                        AddGpsData(gps, description, enemy.Character, player.IdentityId, settings, GpsType.Player, enemy);
                        AudioPackage audioPackage = new AudioPackage(AudioClips.EnemyDetected, InstallationType.SafeZone, player.IdentityId, AudioType.Character, player);
                        //Comms.SendAudioToClient(player, 0, "EnemyDetected");
                    }
                }
            }
        }

        public static void TagEnemyGrid(MyCubeGrid grid, ClaimBlockSettings settings, long playerId = 0L)
        {
            IMyFaction gridFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(grid.BigOwners.FirstOrDefault());
            if (gridFaction == null || gridFaction.IsEveryoneNpc() || gridFaction.Tag.Length > 3) return;

            Installations installation = settings.GetInstallationByType(InstallationType.Radar);
            if (installation == null || !installation.Enabled) return;

            if (detections.Count == 0)
                BuildSizeDetections();

            if (!settings.IsClaimed) return;

            string description = GetEntityObjectSize(grid as IMyEntity, settings);
            if (playerId != 0L)
            {
                IMyGps gps = CreateNewGps(description, "Faction Territories (Tag)", grid.PositionComp.GetPosition(), playerId, Color.Red);
                if (gps != null)
                {
                    AddGpsData(gps, description, grid, playerId, settings, GpsType.Tag);
                    AudioPackage audioPackage = new AudioPackage(AudioClips.EnemyDetected, InstallationType.SafeZone, playerId, AudioType.Character, null);
                    //Comms.SendAudioToClient(null, playerId, "EnemyDetected");
                }
                //Comms.CreateGpsOnClient(description, grid.PositionComp.GetPosition(), grid.EntityId, playerId, settings);
                return;
            }

            IMyFaction claimFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
            if (claimFaction != null)
            {
                List<IMyPlayer> players = new List<IMyPlayer>();
                MyAPIGateway.Players.GetPlayers(players);

                foreach (var player in players)
                {
                    IMyFaction playerFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(player.IdentityId);
                    if (playerFaction != claimFaction)
                    {
                        if (settings.AllowSafeZoneAllies || settings.AllowTerritoryAllies)
                        {
                            if (Utils.IsFactionEnemy(settings, playerFaction)) continue;
                        }
                        else continue;
                    }

                    IMyGps gps = CreateNewGps(description, "Faction Territories (Tag)", grid.PositionComp.GetPosition(), player.IdentityId, Color.Red);
                    if (gps != null)
                    {
                        AddGpsData(gps, description, grid, player.IdentityId, settings, GpsType.Tag);
                        AudioPackage audioPackage = new AudioPackage(AudioClips.EnemyDetected, InstallationType.SafeZone, player.IdentityId, AudioType.Character, player);
                        //Comms.SendAudioToClient(player, 0, "EnemyDetected");
                    }
                    //Comms.CreateGpsOnClient(description, grid.PositionComp.GetPosition(), grid, player.IdentityId, settings);
                }
                //Comms.CreateGpsOnClient(description, grid.PositionComp.GetPosition(), grid.EntityId, settings.PlayerClaimingId, settings);
            }
            /*else
            {
                List<IMyPlayer> players = new List<IMyPlayer>();
                MyAPIGateway.Players.GetPlayers(players);

                foreach (var player in players)
                {
                    
                    IMyFaction playerFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(player.IdentityId);
                    if (playerFaction != claimFaction) continue;
                    Comms.CreateGpsOnClient(description, grid.PositionComp.GetPosition(), grid.EntityId, player.IdentityId, settings);
                }
            } */
        }

        public static void AddGpsData(IMyGps gps, string description, IMyEntity entity, long playerId, ClaimBlockSettings settings, GpsType type, IMyPlayer playerGps = null)
        {
            GpsData data = new GpsData();
            data.playerId = playerId;
            data.gpsName = description;
            data.entity = entity;
            data.gps = gps;
            data.gpsType = type;

            if (playerGps != null)
                data.playerGps = playerGps;

            settings.UpdateGpsData(data, true);
        }

        public static void ValidateGps(ClaimBlockSettings settings)
        {
            try
            {
                Installations installation = settings.GetInstallationByType(InstallationType.Radar);
                if (installation == null || !installation.Enabled) return;

                foreach (var item in settings.GetGridsInside.Values)
                {
                    if (item.hasController && item.hasPower && item.gpsData.Count == 0)
                    {
                        if (item.cubeGrid == null || item.cubeGrid.MarkedForClose) continue;

                        long owner = item.cubeGrid.BigOwners.FirstOrDefault();
                        IMyFaction gridFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(owner);
                        if (gridFaction != null)
                        {
                            if (settings.ClaimedFaction != gridFaction.Tag)
                            {
                                if (settings.AllowSafeZoneAllies || settings.AllowTerritoryAllies)
                                {
                                    if (!Utils.IsFactionEnemy(settings, gridFaction)) continue;
                                }

                                TagEnemyGrid(item.cubeGrid, settings);
                            }
                        }
                        else
                        {
                            TagEnemyGrid(item.cubeGrid, settings);
                        }

                        continue;
                    }

                    if (item.gpsData.Count != 0)
                    {
                        if (item.cubeGrid == null || item.cubeGrid.MarkedForClose) continue;

                        if (!item.hasController || !item.hasPower)
                            RemoveCachedGps(0, GpsType.Tag, settings, item.cubeGrid.EntityId);

                        IMyFaction gridFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(item.cubeGrid.BigOwners.FirstOrDefault());
                        if (gridFaction != null)
                        {
                            if (gridFaction.Tag == settings.ClaimedFaction)
                            {
                                RemoveCachedGps(0, GpsType.Tag, settings, item.cubeGrid.EntityId);
                                continue;
                            }

                            if (settings.AllowTerritoryAllies || settings.AllowSafeZoneAllies)
                            {
                                if (!Utils.IsFactionEnemy(settings, gridFaction))
                                    RemoveCachedGps(0, GpsType.Tag, settings, item.cubeGrid.EntityId);

                                continue;
                            }
                        }
                    }
                }

                foreach (var item in settings.GetPlayersInside.Values)
                {
                    IMyPlayer player = Triggers.GetPlayerFromId(item.playerId);
                    if (player == null) continue;

                    IMyFaction playerFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(item.playerId);

                    if (item.gpsData.Count == 0)
                    {

                        if (playerFaction != null)
                        {
                            if (settings.ClaimedFaction != playerFaction.Tag)
                            {
                                if (settings.AllowSafeZoneAllies || settings.AllowTerritoryAllies)
                                {
                                    if (!Utils.IsFactionEnemy(settings, playerFaction)) continue;
                                }

                                TagEnemyPlayer(player, settings);
                            }
                        }
                        else
                        {
                            TagEnemyPlayer(player, settings);
                        }
                    }
                    else
                    {
                        if (playerFaction != null)
                        {
                            if (settings.ClaimedFaction == playerFaction.Tag)
                            {
                                RemoveCachedGps(0, GpsType.Player, settings, 0, item.playerId);
                                continue;
                            }

                            if (settings.AllowSafeZoneAllies || settings.AllowTerritoryAllies)
                            {
                                if (!Utils.IsFactionEnemy(settings, playerFaction))
                                    RemoveCachedGps(0, GpsType.Player, settings, 0, item.playerId);

                                continue;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MyLog.Default.WriteLineAndConsole($"Faction Territories: {ex}");
            }
        }

        public static void UpdateGPS()
        {
            try
            {
                if (Session.Instance.claimBlocks.Count == 0) return;
                var claimKeys = Session.Instance.claimBlocks.Keys.ToList();

                for (int i = claimKeys.Count - 1; i >= 0; i--)
                {

                    var item = Session.Instance.claimBlocks[claimKeys[i]];
                    if (item.TerritoryConfig._perkConfig._radarConfig._updateFreq != 0 && Session.Instance.ticks % (item.TerritoryConfig._perkConfig._radarConfig._updateFreq * 60) != 0) continue;
                    if (!item.Enabled || !item.IsClaimed) continue;
                    if (item.GetGridsInside.Count == 0) continue;

                    var gridKeys = item.GetGridsInside.Keys.ToList();
                    var playerKeys = item.GetPlayersInside.Keys.ToList();

                    for (int k = gridKeys.Count - 1; k >= 0; k--)
                    {
                        var gridData = item.GetGridsInside[gridKeys[k]];
                        if (gridData.gpsData.Count == 0) continue;

                        for (int j = gridData.gpsData.Count - 1; j >= 0; j--)
                        {
                            var gpsData = gridData.gpsData[j];
                            if (gpsData.gpsType == GpsType.Tag)
                            {
                                IMyGps myGps = gpsData.gps;
                                myGps.Coords = gpsData.entity.GetPosition();
                                if (Vector3D.Distance(myGps.Coords, item.TerritoryConfig._territoryOptions._centerOnPlanet ? item.PlanetCenter : item.BlockPos) > item.TerritoryConfig._territoryRadius || myGps.Coords == new Vector3D(0, 0, 0))
                                {
                                    RemoveGpsData(gpsData.playerId, gpsData.gps, item, gpsData);
                                    continue;
                                }

                                MyAPIGateway.Session.GPS.ModifyGps(gpsData.playerId, myGps);
                                MyVisualScriptLogicProvider.SetGPSColor(myGps.Name, Color.Red, gpsData.playerId);
                                myGps.UpdateHash();
                                Session.Instance.claimBlocks[claimKeys[i]]._server._gridsInside[gridKeys[k]].gpsData[j].gps = myGps;
                            }
                        }
                    }

                    for (int l = playerKeys.Count - 1; l >= 0; l--)
                    {
                        var playerData = item.GetPlayersInside[playerKeys[l]];
                        if (playerData.gpsData.Count == 0) continue;

                        for (int m = playerData.gpsData.Count - 1; m >= 0; m--)
                        {
                            var gpsData = playerData.gpsData[m];
                            if (gpsData.gpsType == GpsType.Player)
                            {
                                IMyGps myGps = gpsData.gps;
                                IMyPlayer targetGps = gpsData.playerGps;
                                if (targetGps == null || targetGps.GetPosition() == new Vector3D(0, 0, 0) || Vector3D.Distance(targetGps.GetPosition(), item.TerritoryConfig._territoryOptions._centerOnPlanet ? item.PlanetCenter : item.BlockPos) > item.TerritoryConfig._territoryRadius)
                                {
                                    RemoveGpsData(gpsData.playerId, gpsData.gps, item, gpsData);
                                    continue;
                                }

                                myGps.Coords = targetGps.GetPosition();
                                MyAPIGateway.Session.GPS.ModifyGps(gpsData.playerId, myGps);
                                MyVisualScriptLogicProvider.SetGPSColor(myGps.Name, Color.Red, gpsData.playerId);
                                myGps.UpdateHash();
                                Session.Instance.claimBlocks[claimKeys[i]]._server._playersInside[playerKeys[l]].gpsData[m].gps = myGps;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //MyVisualScriptLogicProvider.ShowNotification($"Error on updating gps {ex}", 20000);
            }

        }

        public static IMyGps CreateNewGps(string name, string description, Vector3D pos, long playerId, Color color)
        {
            IMyGps gps = null;
            gps = MyAPIGateway.Session.GPS.Create(name, description, pos, true, true);
            MyAPIGateway.Session.GPS.AddGps(playerId, gps);
            MyVisualScriptLogicProvider.SetGPSColor(name, color, playerId);

            return gps;
        }

        public static void RemoveCachedGps(long playerId, GpsType type, ClaimBlockSettings settings = null, long gridId = 0, long playerRemoved = 0)
        {
            if (settings != null)
            {
                if (type != GpsType.Player)
                {
                    var keys = settings._server._gridsInside.Keys.ToList();
                    for (int i = keys.Count - 1; i >= 0; i--)
                    {
                        var gridData = gridId == 0 ? settings._server._gridsInside[keys[i]] : settings._server._gridsInside[gridId];
                        if (gridData.gpsData.Count == 0) continue;
                        for (int j = gridData.gpsData.Count - 1; j >= 0; j--)
                        {
                            if (type == GpsType.All && playerId == 0)
                            {
                                RemoveGpsData(gridData.gpsData[j].playerId, gridData.gpsData[j].gps, settings, gridData.gpsData[j]);
                                continue;
                            }


                            if (gridData.gpsData[j].gpsType == type && playerId == 0)
                            {
                                RemoveGpsData(gridData.gpsData[j].playerId, gridData.gpsData[j].gps, settings, gridData.gpsData[j]);
                                continue;
                            }


                            if (gridData.gpsData[j].gpsType == type && gridData.gpsData[j].playerId == playerId)
                            {
                                RemoveGpsData(gridData.gpsData[j].playerId, gridData.gpsData[j].gps, settings, gridData.gpsData[j]);
                                continue;
                            }

                        }

                        if (gridId != 0) return;
                    }
                }
                else
                {
                    var keys = settings._server._playersInside.Keys.ToList();
                    for (int c = keys.Count - 1; c >= 0; c--)
                    {
                        var data = settings._server._playersInside[keys[c]];
                        if (data.gpsData.Count == 0) continue;
                        for (int e = data.gpsData.Count - 1; e >= 0; e--)
                        {
                            if (playerRemoved == 0)
                            {
                                if (playerId == 0)
                                {
                                    RemoveGpsData(data.gpsData[e].playerId, data.gpsData[e].gps, settings, data.gpsData[e]);
                                    continue;
                                }


                                if (playerId == data.gpsData[e].playerId)
                                {
                                    RemoveGpsData(data.gpsData[e].playerId, data.gpsData[e].gps, settings, data.gpsData[e]);
                                    continue;
                                }
                            }
                            else
                            {
                                if (playerId == 0 && data.gpsData[e].playerGps.IdentityId == playerRemoved)
                                {
                                    RemoveGpsData(data.gpsData[e].playerId, data.gpsData[e].gps, settings, data.gpsData[e]);
                                    continue;
                                }


                                if (playerId == data.gpsData[e].playerId && data.gpsData[e].playerGps.IdentityId == playerRemoved)
                                {
                                    RemoveGpsData(data.gpsData[e].playerId, data.gpsData[e].gps, settings, data.gpsData[e]);
                                    continue;
                                }

                            }
                        }
                    }
                }
            }

            if (settings == null)
            {
                if (playerId != 0)
                {
                    if (Session.Instance.claimBlocks.Count == 0) return;
                    var claimKeys = Session.Instance.claimBlocks.Keys.ToList();
                    for (int i = claimKeys.Count - 1; i >= 0; i--)
                    {
                        ClaimBlockSettings claimSettings = Session.Instance.claimBlocks[claimKeys[i]];
                        var gridKeys = claimSettings._server._gridsInside.Keys.ToList();
                        var playerKeys = claimSettings._server._playersInside.Keys.ToList();
                        for (int j = gridKeys.Count - 1; j >= 0; j--)
                        {
                            var gridData = claimSettings._server._gridsInside[gridKeys[j]];
                            if (gridData.gpsData.Count == 0) continue;
                            for (int k = gridData.gpsData.Count - 1; k >= 0; k--)
                            {
                                if (gridData.gpsData[k].playerId == playerId && type == GpsType.Tag)
                                {
                                    RemoveGpsData(playerId, gridData.gpsData[k].gps, claimSettings, gridData.gpsData[k]);
                                    continue;
                                }

                            }
                        }

                        for (int f = playerKeys.Count - 1; f >= 0; f--)
                        {
                            var playerData = claimSettings._server._playersInside[playerKeys[f]];
                            if (playerData.gpsData.Count == 0) continue;
                            for (int g = playerData.gpsData.Count - 1; g >= 0; g--)
                            {
                                if (playerData.gpsData[g].playerId == playerId)
                                {
                                    RemoveGpsData(playerId, playerData.gpsData[g].gps, claimSettings, playerData.gpsData[g]);
                                    continue;
                                }

                            }
                        }
                    }
                }
            }






            /*if (settings != null && playerRemoved == 0 && type == GpsType.Player)
            {
                var keys = settings._playersInside.Keys.ToList();
                for (int c = keys.Count - 1; c >= 0; c--)
                {
                    var data = settings._playersInside[keys[c]];
                    if (data.gpsData.Count == 0) continue;
                    for (int e = data.gpsData.Count - 1; e >= 0; e--)
                    {
                        if (playerId == 0)
                        {
                            MyAPIGateway.Session.GPS.RemoveGps(data.gpsData[e].playerId, data.gpsData[e].gps);
                            settings.UpdateGpsData(data.gpsData[e], false);
                            continue;
                        }

                        if (playerId == data.gpsData[e].playerId)
                        {
                            MyAPIGateway.Session.GPS.RemoveGps(data.gpsData[e].playerId, data.gpsData[e].gps);
                            settings.UpdateGpsData(data.gpsData[e], false);
                            continue;
                        }
                    }
                }
            }

            if (settings != null && playerRemoved != 0 && type == GpsType.Player)
            {
                var keys = settings._playersInside.Keys.ToList();
                for (int c = keys.Count - 1; c >= 0; c--)
                {
                    var data = settings._playersInside[keys[c]];
                    if (data.gpsData.Count == 0) continue;
                    for (int e = data.gpsData.Count - 1; e >= 0; e--)
                    {
                        if (playerId == 0 && data.gpsData[e].playerGps.IdentityId == playerRemoved)
                        {
                            MyAPIGateway.Session.GPS.RemoveGps(data.gpsData[e].playerId, data.gpsData[e].gps);
                            settings.UpdateGpsData(data.gpsData[e], false);
                            continue;
                        }

                        if (playerId == data.gpsData[e].playerId && data.gpsData[e].playerGps.IdentityId == playerRemoved)
                        {
                            MyAPIGateway.Session.GPS.RemoveGps(data.gpsData[e].playerId, data.gpsData[e].gps);
                            settings.UpdateGpsData(data.gpsData[e], false);
                            continue;
                        }
                    }
                }
            }

            if (settings != null && gridId == 0 && type != GpsType.Player)
            {
                var keys = settings._gridsInside.Keys.ToList();
                for (int i = keys.Count - 1; i >= 0; i--)
                {
                    var gridData = settings._gridsInside[keys[i]];
                    if (gridData.gpsData.Count == 0) continue;
                    for (int j = gridData.gpsData.Count - 1; j >= 0; j--)
                    {
                        if (type == GpsType.All && playerId == 0)
                        {
                            MyAPIGateway.Session.GPS.RemoveGps(gridData.gpsData[j].playerId, gridData.gpsData[j].gps);
                            settings.UpdateGpsData(gridData.gpsData[j], false);
                            //settings._gridsInside[gridKeys[i]].gpsData.RemoveAtFast(j);
                            continue;
                        }

                        if (gridData.gpsData[j].gpsType == type && playerId == 0)
                        {
                            MyAPIGateway.Session.GPS.RemoveGps(gridData.gpsData[j].playerId, gridData.gpsData[j].gps);
                            settings.UpdateGpsData(gridData.gpsData[j], false);
                            continue;
                        }

                        if (gridData.gpsData[j].playerId == playerId && gridData.gpsData[j].gpsType == type)
                        {
                            MyAPIGateway.Session.GPS.RemoveGps(gridData.gpsData[j].playerId, gridData.gpsData[j].gps);
                            settings.UpdateGpsData(gridData.gpsData[j], false);
                            //settings._gridsInside[gridKeys[i]].gpsData.RemoveAtFast(j);
                        }
                    }
                }
            }

            if (settings != null && gridId != 0 && type != GpsType.Player)
            {
                if (!settings._gridsInside.ContainsKey(gridId)) return;
                if (settings._gridsInside[gridId].gpsData.Count == 0) return;

                for (int i = settings._gridsInside[gridId].gpsData.Count - 1; i >= 0; i--)
                {
                    if (playerId == 0 && settings._gridsInside[gridId].gpsData[i].gpsType == type)
                    {
                        MyAPIGateway.Session.GPS.RemoveGps(settings._gridsInside[gridId].gpsData[i].playerId, settings._gridsInside[gridId].gpsData[i].gps);
                        settings.UpdateGpsData(settings._gridsInside[gridId].gpsData[i], false);
                    }
                }
            }

            if (settings == null)
            {
                if (Session.Instance.claimBlocks.Count == 0) return;
                var claimKeys = Session.Instance.claimBlocks.Keys.ToList();
                for (int i = claimKeys.Count - 1; i >= 0; i--)
                {
                    ClaimBlockSettings claimSettings = Session.Instance.claimBlocks[claimKeys[i]];
                    var gridKeys = claimSettings._gridsInside.Keys.ToList();
                    var playerKeys = claimSettings._playersInside.Keys.ToList();
                    for (int j = gridKeys.Count - 1; j >= 0; j--)
                    {
                        var gridData = claimSettings._gridsInside[gridKeys[j]];
                        if (gridData.gpsData.Count == 0) continue;
                        for (int k = gridData.gpsData.Count - 1; k >= 0; k--)
                        {
                            if(gridData.gpsData[k].playerId == playerId && type == GpsType.Tag)
                            {
                                MyAPIGateway.Session.GPS.RemoveGps(playerId, gridData.gpsData[k].gps);
                                claimSettings.UpdateGpsData(gridData.gpsData[k], false);
                            }
                        }
                    }

                    for (int f = playerKeys.Count - 1; f >= 0; f--)
                    {
                        var playerData = claimSettings._playersInside[playerKeys[f]];
                        if (playerData.gpsData.Count == 0) continue;
                        for (int g = playerData.gpsData.Count - 1; g >= 0; g--)
                        {
                            if(playerData.gpsData[g].playerId == playerId)
                            {
                                MyAPIGateway.Session.GPS.RemoveGps(playerId, playerData.gpsData[g].gps);
                                claimSettings.UpdateGpsData(playerData.gpsData[g], false);
                            }
                        }
                    }
                }
            }*/
        }

        private static void RemoveGpsData(long playerId, IMyGps gps, ClaimBlockSettings settings, GpsData data)
        {
            MyAPIGateway.Session.GPS.RemoveGps(playerId, gps);
            settings.UpdateGpsData(data, false);
        }

        private static void BuildSizeDetections()
        {
            AddScanItem("Massive Hostile ", 500f);
            AddScanItem("Huge Hostile ", 250f);
            AddScanItem("Large Hostile ", 100f);
            AddScanItem("Medium Hostile ", 50f);
            AddScanItem("Small Hostile ", 25f);
            AddScanItem("Tiny Hostile ", 0f);
        }

        private static void AddScanItem(string name, float size)
        {
            DetectionSize data = new DetectionSize();
            data.name = name;
            data.size = size;
            detections.Add(data);
        }

        private static string GetEntityObjectSize(IMyEntity entity, ClaimBlockSettings settings)
        {
            foreach (var gridData in settings.GetGridsInside.Values)
            {
                if (gridData.gpsData.Count == 0) continue;

                foreach (var data in gridData.gpsData)
                {
                    if (data.entity == entity) return data.gpsName;
                }
            }

            int num = Utils.Rnd.Next(0, 999);
            if (entity == null) return "Unknown Hostile " + num;
            double entitySize = entity.PositionComp.WorldAABB.Size.AbsMax();
            foreach (DetectionSize item in detections)
            {
                if (entitySize >= item.size)
                {
                    return item.name + num;
                }
            }

            return "Unknown Hostile " + num;
        }
    }
}
