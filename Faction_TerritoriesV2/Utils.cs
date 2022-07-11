using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Components;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace Faction_TerritoriesV2
{
    [Flags]
    public enum CustomSafeZoneAction
    {
        Damage = 1,
        Shooting = 2,
        Drilling = 4,
        Welding = 8,
        Grinding = 16,
        VoxelHand = 32,
        Building = 64,
        LandingGearLock = 128,
        ConvertToStation = 256,
        BuildingProjections = 512,
        All = 1023,
        AdminIgnore = 894
    }

    public enum EmissiveState
    {
        Online,
        Claimed,
        Sieged,
        Offline
    }

    public enum MyTextEnum
    {
        SafeZoneAllies,
        TerritoryAllies
    }


    public static class EnumFlagExtensions
    {
        public static TEnum NewFlag<TEnum>(this TEnum enumVar, int newFlag)
            where TEnum : struct
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), newFlag);
        }
    }

    public static class Utils
    {
        private readonly static string emissiveName = "Emissive";
        private readonly static Color Online = Color.Green;
        private readonly static Color Offline = Color.Red;
        private readonly static Color Claimed = Color.Aqua;
        private readonly static Color Sieged = Color.Orange;
        //private readonly static string UnClaimedLogo = $@"{MyAPIGateway.Utilities.GamePaths.ContentPath}\244850\2187340824\Textures\Logo\GVLogoScaled.dds";
        private readonly static string UnClaimedLogo = $@"{Session.Instance.modPath}\Textures\FactionLogo\GVLogoScaled.dds";
        //private readonly static string UnClaimedLogo = $@"Textures\FactionLogo\PirateIcon.dds";
        public static int territoryStatusDelay;
        public static List<IMySlimBlock> blocks = new List<IMySlimBlock>();

        public static Random Rnd = new Random();

        public static bool TakeTokens(IMyEntity entity, ClaimBlockSettings settings)
        {
            var jd = entity as IMyJumpDrive;
            if (jd == null) return false;

            IMyCubeGrid cubeGrid = jd.CubeGrid;
            if (cubeGrid == null) return false;

            int tokensToRemove = 0;
            if (!settings.IsClaimed)
                tokensToRemove = settings.TerritoryConfig._claimingConfig._tokensToClaim;
            else
                tokensToRemove = settings.TerritoryConfig._siegingConfig._tokensToSiege;

            if (tokensToRemove == 0) return true;

            MyDefinitionId tokenId;
            if (!MyDefinitionId.TryParse(settings.TerritoryConfig._token, out tokenId)) return false;

            List<IMyInventory> cachedInventory = new List<IMyInventory>();
            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
            MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(cubeGrid).GetBlocksOfType(Blocks, x => x.HasInventory);
            MyFixedPoint tokens = 0;

            foreach (var tblock in Blocks)
            {
                tokens = 0;
                IMyInventory blockInv = tblock.GetInventory();
                tokens = blockInv.GetItemAmount(tokenId);

                if (tokens != 0)
                {
                    tokensToRemove -= (int)tokens;
                    if (cachedInventory.Contains(blockInv)) continue;
                    cachedInventory.Add(blockInv);
                }

                if (tokensToRemove <= 0) break;
            }

            if (tokensToRemove > 0) return false;

            if (!settings.IsClaimed)
                tokensToRemove = settings.TerritoryConfig._claimingConfig._tokensToClaim;
            else
                tokensToRemove = settings.TerritoryConfig._siegingConfig._tokensToSiege;
            foreach (MyInventory inventory in cachedInventory)
            {
                var removed = (int)inventory.RemoveItemsOfType(tokensToRemove, tokenId);
                tokensToRemove -= removed;
                if (tokensToRemove <= 0) return true;
            }

            if (tokensToRemove <= 0) return true;
            return false;
        }

        /*public static string GetPendingPerksAdd(ClaimBlockSettings settings)
        {
            string result = "";
            if (settings == null) return "";
            foreach (var perk in settings.GetPerks.Keys)
            {
                PerkBase perkBase;
                if (!settings.GetPerks.TryGetValue(perk, out perkBase)) return "";
                if (perk == PerkType.Production)
                {
                    for (int i = 0; i < perkBase.perk.productionPerk.GetPendingAddUpgrades.Count; i++)
                    {
                        result += perkBase.perk.productionPerk.GetPendingAddUpgrades[i];
                        if (i < perkBase.perk.productionPerk.GetPendingAddUpgrades.Count - 1)
                            result += ", ";
                    }
                }
            }

            return result;
        }*/

        /*public static bool AnyPerksEnabled(ClaimBlockSettings settings)
        {
            if (settings == null) return false;
            if (settings.GetPerks.Count == 0) return false;

            foreach (var perk in settings.GetPerks.Keys)
            {
                if (settings.GetPerks[perk].Enable) return true;
            }

            return false;
        }*/

        /*public static string GetPendingPerksRemove(ClaimBlockSettings settings)
        {
            string result = "";
            if (settings == null) return result;
            foreach (var perk in settings.GetPerks.Keys)
            {
                PerkBase perkBase;
                if (!settings.GetPerks.TryGetValue(perk, out perkBase)) return result;
                if (perk == PerkType.Production)
                {
                    for (int i = 0; i < perkBase.perk.productionPerk.GetPendingRemoveUpgrades.Count; i++)
                    {
                        result += perkBase.perk.productionPerk.GetPendingRemoveUpgrades[i];
                        if (i < perkBase.perk.productionPerk.GetPendingRemoveUpgrades.Count - 1)
                            result += ", ";
                    }
                }
            }

            return result;
        }*/

        /*public static string GetActivePerks(ClaimBlockSettings settings)
        {
            string result = "";
            if (settings == null) return result;
            foreach (var perk in settings.GetPerks.Keys)
            {
                PerkBase perkBase;
                if (!settings.GetPerks.TryGetValue(perk, out perkBase)) return result;
                if (perk == PerkType.Production)
                {
                    for (int i = 0; i < perkBase.perk.productionPerk.GetActiveUpgrades.Count; i++)
                    {
                        result += perkBase.perk.productionPerk.GetActiveUpgrades[i];
                        if (i < perkBase.perk.productionPerk.GetActiveUpgrades.Count - 1)
                            result += ", ";
                    }
                }
            }

            return result;
        }*/

        /*public static int GetPerkCost(ClaimBlockSettings settings)
        {
            int cost = 0;
            if (settings == null) return 0;
            if (!settings.Enabled || !settings.IsClaimed) return 0;

            foreach (var perk in settings.GetPerks.Keys)
            {
                PerkBase perkBase;
                if (!settings.GetPerks.TryGetValue(perk, out perkBase)) return 0;

                cost += perkBase.PendingPerkCost;
            }

            return cost;
        }*/

        /*public static int GetActivePerkCost(ClaimBlockSettings settings)
        {
            int cost = 0;
            if (settings == null) return 0;
            if (!settings.Enabled || !settings.IsClaimed) return 0;

            foreach (var perk in settings.GetPerks.Keys)
            {
                PerkBase perkBase;
                if (!settings.GetPerks.TryGetValue(perk, out perkBase)) return 0;

                cost += perkBase.ActivePerkCost;
            }

            return cost;
        }*/

        /*public static int GetPendingPerkCost(ClaimBlockSettings settings)
        {
            int cost = 0;
            if (settings == null) return cost;
            if (!settings.Enabled || !settings.IsClaimed) return cost;

            foreach (var perk in settings.GetPerks.Keys)
            {
                PerkBase perkBase;
                if (!settings.GetPerks.TryGetValue(perk, out perkBase)) return cost;

                cost += perkBase.PendingPerkCost;
            }

            return cost;
        }*/

        public static bool ConsumeToken(ClaimBlockSettings settings)
        {
            int cost = settings.TerritoryConfig._territoryMaintenance._consumptionAmt;
            MyFixedPoint tokensNeeded = (MyFixedPoint)cost;
            MyDefinitionId tokenId;
            MyDefinitionId.TryParse(settings.TerritoryConfig._token, out tokenId);
            if (tokenId == null) return false;

            if (tokensNeeded == 0) return true;

            IMyInventory blockInv = settings.Block.GetInventory();
            if (blockInv == null) return false;

            MyFixedPoint tokens = blockInv.GetItemAmount(tokenId);

            if (tokens >= tokensNeeded)
                blockInv.RemoveItemsOfType(tokensNeeded, tokenId);
            else { return false; }

            if (!settings.Server._perkWarning)
            {
                int perkWarningCost = cost * 24;

                if (perkWarningCost >= tokens)
                {
                    settings.Server._perkWarning = true;
                    new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"WARNING - Claimed Territory: {settings.ClaimZoneName} has less than 24 hours worth of tokens left before territory is reset.", "[Faction Territories]", Color.Red);

                }
            }

            return true;
        }

        /*public static bool DelaySiegeTokenConsumption(ClaimBlockSettings settings, bool consume = false)
        {
            MyFixedPoint tokensNeeded = (MyFixedPoint)settings.TokensSiegeDelay;
            MyDefinitionId tokenId;
            if (!MyDefinitionId.TryParse(settings.TerritoryConfig._token, out tokenId)) return false;
            IMyInventory blockInv = settings.Block.GetInventory();
            if (blockInv == null) return false;

            MyFixedPoint tokens = blockInv.GetItemAmount(tokenId);

            if (!consume)
                if (tokens >= tokensNeeded) return true;
                else return false;

            if (tokens >= tokensNeeded)
                blockInv.RemoveItemsOfType(tokensNeeded, tokenId);
            else { return false; }

            int perkCost = GetPerkCost(settings) + 1;
            if (!settings.Server._perkWarning)
            {
                int perkWarningCost = perkCost * 24;

                if (perkWarningCost >= tokens)
                {
                    settings.Server._perkWarning = true;
                    new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"WARNING - Claimed Territory: {settings.ClaimZoneName} has less than 24 hours worth of tokens left before territory is reset.", "[Faction Territories]", Color.Red);
                }
            }

            new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"Sieged Territory: {settings.ClaimZoneName} - Siege time has been exteneded additional {settings.HoursToDelay} hours, time to final siege is now {TimeSpan.FromSeconds(settings.SiegeTimer + settings.HoursToDelay * 3600)}", "[Faction Territories]", Color.Red);
            return true;
        }*/

        public static void AddSafeZone(ClaimBlockSettings settings, bool allowActions = true)
        {
            if (!settings.IsClaimed) return;
            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
            var ob = new MyObjectBuilder_SafeZone();

            ob.PositionAndOrientation = new MyPositionAndOrientation(settings.BlockPos, Vector3.Forward, Vector3.Up);
            ob.PersistentFlags = MyPersistentEntityFlags2.InScene;
            if (faction != null)
            {
                if (settings.AllowSafeZoneAllies)
                {
                    var factions = MyAPIGateway.Session.Factions.Factions;
                    List<long> friendlies = new List<long>();
                    foreach (var thisFaction in factions.Values)
                    {
                        if (thisFaction == faction)
                        {
                            friendlies.Add(thisFaction.FactionId);
                            continue;
                        }

                        if (IsFactionEnemy(settings, thisFaction)) continue;
                        friendlies.Add(thisFaction.FactionId);
                    }

                    ob.Factions = friendlies.ToArray();
                }
                else
                {
                    ob.Factions = new long[] { faction.FactionId };
                }

                ob.AccessTypeFactions = MySafeZoneAccess.Whitelist;
            }
            else
            {
                ob.Players = new long[] { settings.PlayerClaimingId };
                ob.AccessTypePlayers = MySafeZoneAccess.Whitelist;
            }

            ob.Shape = MySafeZoneShape.Sphere;
            ob.Radius = GetSafeZoneRadius(settings);
            ob.DisplayName = "(FactionTerritory)" + "_" + settings.UnclaimName + "_" + settings.EntityId.ToString();
            ob.Enabled = true;

            if (allowActions && !settings.IsSieging)
            {
                int allowedFlags = (int)ob.AllowedActions;
                allowedFlags = allowedFlags | (int)CustomSafeZoneAction.Grinding;
                allowedFlags = allowedFlags | (int)CustomSafeZoneAction.Welding;
                allowedFlags = allowedFlags | (int)CustomSafeZoneAction.Building;
                allowedFlags = allowedFlags | (int)CustomSafeZoneAction.Drilling;
                allowedFlags = allowedFlags | (int)CustomSafeZoneAction.ConvertToStation;
                allowedFlags = allowedFlags | (int)CustomSafeZoneAction.BuildingProjections;
                allowedFlags = allowedFlags | (int)CustomSafeZoneAction.LandingGearLock;
                ob.AllowedActions = MySessionComponentSafeZones.AllowedActions.NewFlag(allowedFlags);
            }

            var zoneEntity = MyEntities.CreateFromObjectBuilderAndAdd(ob, true);
            MySafeZone zone = zoneEntity as MySafeZone;
            if (zone == null) return;

            zone.Radius = GetSafeZoneRadius(settings);
            //zone.DisplayName = "(FactionTerritory)" + "_" + zone.EntityId.ToString();
            zone.RecreatePhysics();

            settings.SafeZoneEntity = zone.EntityId;
        }

        public static void RemoveSafeZone(ClaimBlockSettings settings)
        {
            IMyEntity entity;
            if (!MyAPIGateway.Entities.TryGetEntityById(settings.SafeZoneEntity, out entity))
            {
                var zones = MySessionComponentSafeZones.SafeZones;
                foreach (var sz in zones)
                {
                    if (sz == null || sz.MarkedForClose) continue;
                    if (sz.DisplayName == null) continue;
                    if (sz.DisplayName.Contains(settings.UnclaimName))
                    {
                        entity = sz as IMyEntity;
                        break;
                    }
                }
            }

            settings.SafeZoneEntity = 0;
            if (entity == null) return;
            MySafeZone zone = entity as MySafeZone;
            if (zone == null || zone.MarkedForClose) return;

            zone.Close();
        }

        public static float GetSafeZoneRadius(ClaimBlockSettings settings)
        {
            float radius = settings.TerritoryConfig._safeZoneRadius;
            foreach(var installation in settings.GetInstallations)
            {
                if (!installation.Enabled) continue;
                if (installation.Type == InstallationType.SafeZone)
                    return radius * settings.TerritoryConfig._perkConfig._safeZoneConfig._safeZoneExtensionMultiplier;
            }

            return radius;
        }

        public static void ResetClaim(ClaimBlockSettings settings)
        {
            //RemovePerks(settings);
            SetRelationWithNpc(settings, MyRelationsBetweenFactions.Enemies);
            settings.RecoveryTimer = 0;
            settings.IsClaiming = false;
            settings.IsClaimed = false;
            settings.IsSieging = false;
            settings.IsSieged = false;
            settings.PlayerClaimingId = 0;
            settings.PlayerSiegingId = 0;
            settings.JDClaimingId = 0;
            settings.JDClaiming = null;
            settings.JDSieging = null;
            settings.JDSiegingId = 0;
            settings.ClaimedFaction = " ";
            settings.FactionId = 0;
            settings.SafeZoneEntity = 0;
            settings.ClaimZoneName = settings.UnclaimName;
            settings.DetailInfo = " ";
            settings._safeZones?.Clear();
            settings._zonesDelay?.Clear();
            settings.AllowSafeZoneAllies = false;
            settings.AllowTerritoryAllies = false;
            settings.IsCooling = false;
            settings.IsSiegeCooling = false;
            settings.BlockEmissive = settings.Enabled ? EmissiveState.Online : EmissiveState.Offline;
            settings.GetTerritoryStatus = TerritoryStatus.Neutral;
            //GPS.RemoveCachedGps(0, GpsType.Tag, settings);
            //GPS.RemoveCachedGps(0, GpsType.Player, settings);
            RemoveAllInstallations(settings);
            GPS.UpdateBlockText(settings, $"Unclaimed Territory: {settings.UnclaimName}");
            //SetScreen(settings.Block as IMyBeacon, null, true);
            //SetOwner(settings.Block);
            MonitorSafeZonePBs(settings, true);
            ActionControls.RefreshControls(settings.Block);
            SetOwner(settings.Block, settings);
        }

        public static void ResetSiegeData(ClaimBlockSettings settings, bool resetSafeZone = true)
        {
            settings.RecoveryTimer = 0;
            settings.IsSieging = false;
            settings.IsSieged = false;
            settings.JDSieging = null;
            settings.JDSiegingId = 0;
            settings.PlayerSiegingId = 0;
            settings.BlockEmissive = EmissiveState.Claimed;
            MonitorSafeZonePBs(settings, true);

            if (!resetSafeZone) return;
            RemoveSafeZone(settings);
            AddSafeZone(settings);
        }

        public static void DrainAllJDs(IMyEntity entity)
        {
            if (entity == null) return;
            IMyJumpDrive jd = entity as IMyJumpDrive;
            if (jd == null) return;
            IMyCubeGrid cubeGrid = jd.CubeGrid;
            if (cubeGrid == null) return;

            List<IMyJumpDrive> Blocks = new List<IMyJumpDrive>();
            MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(cubeGrid).GetBlocksOfType(Blocks, x => x.IsFunctional);

            foreach (var item in Blocks)
            {
                item.CurrentStoredPower = 0f;
            }

        }

        public static bool CheckPlayerandBlock(ClaimBlockSettings settings)
        {
            IMyEntity targetEntity = settings.IsClaiming ? settings.JDClaiming : settings.JDSieging;
            double targetDistance = settings.IsClaiming ? settings.TerritoryConfig._claimingConfig._distanceToClaim : settings.TerritoryConfig._siegingConfig._distanceToSiege;

            if (targetEntity == null)
            {
                long targetId = settings.IsClaiming ? settings.JDClaimingId : settings.JDSiegingId;
                if (!MyAPIGateway.Entities.TryGetEntityById(targetId, out targetEntity)) return false;

                if (settings.IsClaiming)
                    settings.JDClaiming = targetEntity;
                else
                    settings.JDSieging = targetEntity;
            }

            IMyJumpDrive jd = targetEntity as IMyJumpDrive;
            if (jd == null || jd.MarkedForClose) return false;

            if (!jd.IsFunctional) return false;
            if (Vector3D.Distance(jd.GetPosition(), settings.BlockPos) > targetDistance) return false;
            return true;
        }

        public static void TagEnemyGrids(ClaimBlockSettings settings)
        {
            if (!settings.IsClaimed) return;
            if (settings.GetGridsInside.Count == 0) return;
            foreach (var data in settings.GetGridsInside.Values)
            {

                MyCubeGrid cubeGrid = data.cubeGrid;
                if (cubeGrid == null) continue;

                if (!data.hasController || !data.hasPower) continue;
                var owner = cubeGrid.BigOwners.FirstOrDefault();
                //IMyFaction claimFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(owner);

                if (faction != null)
                {
                    //MyVisualScriptLogicProvider.ShowNotification($"my faction = {faction.Tag}, claim faction = {settings.ClaimedFaction}", 10000);
                    if (settings.ClaimedFaction != faction.Tag)
                    {
                        if (settings.AllowSafeZoneAllies || settings.AllowTerritoryAllies)
                        {
                            if (!Utils.IsFactionEnemy(settings, faction)) return;
                        }

                        GPS.TagEnemyGrid(cubeGrid, settings);
                    }

                    return;
                }

                GPS.TagEnemyGrid(cubeGrid, settings);
            }
        }

        public static void TagEnemyPlayers(ClaimBlockSettings settings)
        {
            if (!settings.IsClaimed) return;
            if (settings.GetPlayersInside.Count == 0) return;
            foreach (var data in settings.GetPlayersInside.Keys)
            {
                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(data);
                IMyPlayer player = Triggers.GetPlayerFromId(data);
                if (player == null) continue;

                if (faction != null)
                {
                    if (settings.ClaimedFaction != faction.Tag)
                    {
                        if (settings.AllowSafeZoneAllies || settings.AllowTerritoryAllies)
                        {
                            if (!Utils.IsFactionEnemy(settings, faction)) continue;
                        }

                        GPS.TagEnemyPlayer(player, settings);
                    }

                    return;
                }

                GPS.TagEnemyPlayer(player, settings);
            }
        }

        public static void CheckGridsToTag(long playerId)
        {
            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(playerId);
            if (faction == null) return;

            foreach (var item in Session.Instance.claimBlocks.Values)
            {
                if (item.ClaimedFaction == faction.Tag)
                {
                    foreach (var gridData in item.GetGridsInside.Values)
                    {
                        MyCubeGrid cubeGrid = gridData.cubeGrid;
                        if (cubeGrid == null) continue;
                        if (!gridData.hasController || !gridData.hasPower) continue;

                        var owner = cubeGrid.BigOwners.FirstOrDefault();
                        IMyFaction gridFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(owner);

                        if (gridFaction != null)
                        {
                            if (gridFaction.Tag != faction.Tag)
                            {
                                if (item.AllowSafeZoneAllies || item.AllowTerritoryAllies)
                                {
                                    if (!IsFactionEnemy(item, gridFaction)) continue;
                                }

                                GPS.TagEnemyGrid(cubeGrid, item);
                            }

                            continue;
                        }

                        GPS.TagEnemyGrid(cubeGrid, item);

                    }
                }
            }
        }

        public static void CheckPlayersToTag(long playerId)
        {
            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(playerId);
            if (faction == null) return;

            foreach (var item in Session.Instance.claimBlocks.Values)
            {
                if (item.ClaimedFaction == faction.Tag)
                {
                    foreach (var player in item.GetPlayersInside.Keys)
                    {
                        IMyFaction playerFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(player);
                        IMyPlayer p = Triggers.GetPlayerFromId(player);
                        if (p == null) continue;

                        if (playerFaction != null)
                        {
                            if (playerFaction.Tag != faction.Tag)
                            {
                                if (item.AllowSafeZoneAllies || item.AllowTerritoryAllies)
                                {
                                    if (!Utils.IsFactionEnemy(item, playerFaction)) continue;
                                }

                                GPS.TagEnemyPlayer(p, item);
                            }

                            continue;
                        }

                        GPS.TagEnemyPlayer(p, item);

                    }
                }
            }
        }

        public static void StopHandTools()
        {
            List<IMyPlayer> players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);

            foreach (var player in players)
            {
                if (player.SteamUserId <= 0 || player.IsBot) continue;
                Session.Instance.ToolEquipped(player.IdentityId, "", "");
            }
        }

        public static void RemoveGridData(ClaimBlockSettings settings, MyCubeGrid cubeGrid = null)
        {
            var keys = settings.GetGridsInside.Keys.ToList();
            //MyVisualScriptLogicProvider.ShowNotification($"Grid Count Before = {keys.Count}", 10000);
            if (cubeGrid == null)
            {
                for (int i = keys.Count - 1; i >= 0; i--)
                {
                    settings.UpdateGridsInside(keys[i], settings._server._gridsInside[keys[i]].cubeGrid, false);
                }
            }
            else
            {
                for (int i = keys.Count - 1; i >= 0; i--)
                {
                    if (settings._server._gridsInside[keys[i]].cubeGrid == cubeGrid)
                    {
                        settings.UpdateGridsInside(keys[i], settings._server._gridsInside[keys[i]].cubeGrid, false);
                        break;
                    }
                }
            }

            //MyVisualScriptLogicProvider.ShowNotification($"Grid Count After = {Session.Instance.claimBlocks[settings.EntityId].GetGridsInside.Count}", 10000);
        }

        public static void GetSurroundingSafeZones(ClaimBlockSettings settings)
        {
            BoundingSphereD sphere = new BoundingSphereD(settings.TerritoryConfig._territoryOptions._centerOnPlanet ? settings.PlanetCenter : settings.BlockPos, settings.TerritoryConfig._territoryRadius);
            List<IMyEntity> entities = MyAPIGateway.Entities.GetTopMostEntitiesInSphere(ref sphere);

            foreach (var entity in entities)
            {
                MySafeZone zone = entity as MySafeZone;
                if (zone == null) continue;

                long zoneBlockId = zone.SafeZoneBlockId;
                settings.UpdateSafeZones(zone.EntityId, true);
                if (zoneBlockId == 0) continue;

                IMySafeZoneBlock zoneBlock = null;
                Session.Instance.safeZoneBlocks.TryGetValue(zoneBlockId, out zoneBlock);
                if (zoneBlock == null) continue;

                IMyFaction claimFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
                IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(zoneBlock.OwnerId);

                if (claimFaction != null && claimFaction != blockFaction)
                {
                    //MyVisualScriptLogicProvider.ShowNotification("Added Safe Zone Info", 5000);
                    if (zoneBlock.IsSafeZoneEnabled())
                        settings.UpdateZonesDelayRemove(zoneBlockId, DateTime.Now, true);
                    //else
                    //settings.UpdateSafeZoneBlocks(zoneBlockId, true);
                }
            }
        }

        public static void CheckTools(ClaimBlockSettings settings)
        {
            if (settings.GetGridsInside.Count == 0 || settings.TerritoryConfig._territoryOptions._allowTools) return;

            foreach (var data in settings.GetGridsInside.Values)
            {
                if (!settings.TerritoryConfig._territoryOptions._allowDrilling)
                {
                    foreach (var drill in data.blocksMonitored.drills)
                    {
                        if (drill == null || drill.MarkedForClose) continue;
                        IMyFaction toolFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(drill.OwnerId);
                        if (toolFaction != null)
                        {
                            if (toolFaction.Tag == settings.ClaimedFaction) continue;
                            if (settings.AllowSafeZoneAllies || settings.AllowTerritoryAllies)
                            {
                                if (!Utils.IsFactionEnemy(settings, toolFaction)) continue;
                            }
                        }

                        var toolbase = (IMyGunObject<MyToolBase>)drill;
                        if (toolbase == null) continue;
                        if (!toolbase.IsShooting) continue;
                        toolbase.EndShoot(MyShootActionEnum.PrimaryAction);
                        toolbase.EndShoot(MyShootActionEnum.SecondaryAction);

                        var player = MyAPIGateway.Players.GetPlayerControllingEntity(drill.CubeGrid);
                        if (player == null) continue;

                        AudioPackage audioPackage = new AudioPackage(AudioClips.RealHudUnable, InstallationType.SafeZone, player.IdentityId, AudioType.Character, player);
                    }
                }
                
                foreach (var tool in data.blocksMonitored.tools)
                {
                    if (settings.TerritoryConfig._territoryOptions._allowWelding && tool is IMyShipWelder) continue;
                    if (settings.TerritoryConfig._territoryOptions._allowGrinding && tool is IMyShipGrinder) continue;

                    if (tool == null || tool.MarkedForClose) continue;
                    IMyFaction toolFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(tool.OwnerId);
                    if (toolFaction != null)
                    {
                        if (toolFaction.Tag == settings.ClaimedFaction) continue;
                        if (settings.AllowSafeZoneAllies || settings.AllowTerritoryAllies)
                        {
                            if (!Utils.IsFactionEnemy(settings, toolFaction)) continue;
                        }
                    }

                    var toolbase = (IMyGunObject<MyToolBase>)tool;
                    if (toolbase == null) continue;
                    if (!toolbase.IsShooting) continue;
                    toolbase.EndShoot(MyShootActionEnum.PrimaryAction);

                    var player = MyAPIGateway.Players.GetPlayerControllingEntity(tool.CubeGrid);
                    if (player == null) continue;

                    AudioPackage audioPackage = new AudioPackage(AudioClips.RealHudUnable, InstallationType.SafeZone, player.IdentityId, AudioType.Character, player);
                    //Comms.SendAudioToClient(player, player.IdentityId, "RealHudUnable");
                }
            }
        }

        public static void SetEmissive(EmissiveState state, IMyBeacon myBeacon)
        {
            if (myBeacon == null) return;
            if (state == EmissiveState.Online)
            {
                myBeacon.SetEmissiveParts(emissiveName, Online, 1f);
                return;
            }

            if (state == EmissiveState.Offline)
            {
                myBeacon.SetEmissiveParts(emissiveName, Offline, 1f);
                return;
            }

            if (state == EmissiveState.Claimed)

            {
                myBeacon.SetEmissiveParts(emissiveName, Claimed, 1f);
                return;
            }

            if (state == EmissiveState.Sieged)
            {
                myBeacon.SetEmissiveParts(emissiveName, Sieged, 1f);
                return;
            }
        }

        public static void SetScreen(IMyBeacon beacon, IMyFaction faction = null, bool sync = false)
        {
            try
            {
                return;
                if (beacon == null) return;
                var screenAreaRender = beacon.Render.GetAs<MyRenderComponentScreenAreas>();

                if (screenAreaRender == null)
                {
                    var renderId = beacon.Render.RenderObjectIDs[0];
                    MyVisualScriptLogicProvider.ShowNotification($"Render Id Before = {beacon.Render.RenderObjectIDs[0]}", 25000);
                    screenAreaRender = new MyRenderComponentScreenAreas((MyEntity)beacon);
                    beacon.Render.Container.Add(screenAreaRender);
                    //beacon.Render.SetRenderObjectID(0, renderId);
                    beacon.Render.AddRenderObjects();
                    screenAreaRender = beacon.Render.GetAs<MyRenderComponentScreenAreas>();

                    //screenAreaRender.AddRenderObjects();
                    MyVisualScriptLogicProvider.ShowNotification($"Render Id AFter = {beacon.Render.RenderObjectIDs[0]}", 25000);
                    //screenAreaRender.AddScreenArea(beacon.Render.RenderObjectIDs, "ScreenArea");
                    //screenAreaRender.UpdateModelProperties();

                    //IMyCubeBlock block = beacon as IMyCubeBlock;
                    //var useObject = block.UseObjectsComponent;
                    //useObject.LoadDetectorsFromModel();
                    //block.ReloadDetectors();


                }

                if (screenAreaRender != null)
                {
                    if (faction == null)
                    {
                        //MyVisualScriptLogicProvider.ShowNotification($"Logo = {UnClaimedLogo}", 15000);
                        screenAreaRender.ChangeTexture(0, UnClaimedLogo);
                    }
                    else
                    {
                        screenAreaRender.ChangeTexture(0, $"{faction.FactionIcon}");
                        //MyAPIGateway.Utilities.ShowNotification($"{faction.FactionIcon}", 25000, "Green");
                    }
                }

                if (sync)
                {
                    if (faction != null)
                        Comms.SyncBillBoard(beacon.EntityId, MyAPIGateway.Session.LocalHumanPlayer, faction?.Tag);
                    else
                        Comms.SyncBillBoard(beacon.EntityId, MyAPIGateway.Session.LocalHumanPlayer);
                }

            }
            catch (Exception ex)
            {
                MyVisualScriptLogicProvider.ShowNotification($"{ex.ToString()}", 25000);
                //MyLog.Default.WriteLineAndConsole($"{ex.StackTrace}");
            }

        }

        public static void PlayParticle(string effect, Vector3D pos, ClaimBlockSettings settings, Installations installation, float size = 0)
        {
            MatrixD hitParticleMatrix = MatrixD.CreateWorld(pos, Vector3.Forward, Vector3.Up);
            MyParticleEffect particle = null;
            MyParticlesManager.TryCreateParticleEffect(effect, ref hitParticleMatrix, ref pos, uint.MaxValue, out particle);
            if (particle == null) return;

            if (effect.Contains("Claiming") || effect.Contains("Sieging"))
                particle.UserScale = 10f;
            else if (size == 0) return;

            particle.UserScale = size;
            if (effect == "ExhaustFire")
            {
                if (installation == null || settings == null) return;
                ClaimBlockSettings mySettings;
                Session.Instance.claimBlocks.TryGetValue(settings.EntityId, out mySettings);
                if (mySettings == null) return;

                Installations myInstallation = mySettings.GetInstallationByType(installation.Type);
                if (myInstallation == null) return;

                IMyEntity ent;
                MyAPIGateway.Entities.TryGetEntityById(myInstallation.BlockEntityId, out ent);
                if (ent == null) return;

                myInstallation._installationParticle = particle;
            }
        }

        public static void AddProductionPerk(ClaimBlockSettings settings, MyCubeBlock block)
        {
            if (settings == null) return;
            Installations installation = settings.GetInstallationByType(InstallationType.Production);
            if (installation == null || !installation.Enabled) return;

            if (block != null)
            {
                float speed = settings.TerritoryConfig._perkConfig._productionConfig._speed / 100;
                float yield = settings.TerritoryConfig._perkConfig._productionConfig._yield / 100;
                float energy = settings.TerritoryConfig._perkConfig._productionConfig._energy / 100;

                float num3 = GetAttachedUpgradeModules(block, "Productivity");
                float num4 = GetAttachedUpgradeModules(block, "Effectiveness");
                float num5 = GetAttachedUpgradeModules(block, "PowerEfficiency");

                SetUpgradeValue(block, "Productivity", speed + num3, true);
                SetUpgradeValue(block, "Effectiveness", yield + num4 + 1, true);
                SetUpgradeValue(block, "PowerEfficiency", energy + num5 + 1, true);
            }
            else
            {
                foreach (var grids in settings.GetGridsInside.Values)
                {
                    foreach (var production in grids.blocksMonitored.production)
                    {
                        AddProductionPerk(settings, production as MyCubeBlock);
                    }
                }
            }
            
        }

        public static void RemoveProductionPerk(ClaimBlockSettings settings, MyCubeBlock block)
        {
            if (settings == null) return;

            if (block != null)
            {
                float num = GetAttachedUpgradeModules(block, "Productivity");
                float num1 = GetAttachedUpgradeModules(block, "Effectiveness");
                float num2 = GetAttachedUpgradeModules(block, "PowerEfficiency");

                if (num > 0)
                    SetUpgradeValue(block, "Productivity", num, true);
                else
                    SetUpgradeValue(block, "Productivity", 0f, true);

                if (num1 > 0)
                    SetUpgradeValue(block, "Effectiveness", num1, true);
                else
                    SetUpgradeValue(block, "Effectiveness", 1f, true);

                if (num2 > 0)
                    SetUpgradeValue(block, "PowerEfficiency", num2, true);
                else
                    SetUpgradeValue(block, "PowerEfficiency", 1f, true);
            }
            else
            {
                foreach(var grids in settings.GetGridsInside.Values)
                {
                    foreach(var production in grids.blocksMonitored.production)
                    {
                        RemoveProductionPerk(settings, production as MyCubeBlock);
                    }
                }
            }
            
        }

        public static float GetAttachedUpgradeModules(MyCubeBlock block, string type)
        {
            float num = 0;
            var modules = block.CurrentAttachedUpgradeModules;
            if (modules == null) return 0f;
            if (modules.Count == 0) return 0f;
            foreach (var module in modules.Values)
            {
                if (!module.Block.IsWorking) continue;
                int slotCount = module.SlotCount;
                //uint connections = module.Block.Connections;
                //uint upgradeCount = module.Block.UpgradeCount;

                var upgradeModule = module.Block as MyCubeBlock;
                var def = upgradeModule.BlockDefinition;
                var defModule = def as MyUpgradeModuleDefinition;
                if (defModule != null)
                {
                    var upgrades = defModule.Upgrades;
                    foreach (var upgrade in upgrades)
                    {
                        if (type == upgrade.UpgradeType)
                        {
                            if (upgrade.ModifierType == MyUpgradeModifierType.Additive)
                                num += upgrade.Modifier * slotCount;
                            else
                                num *= upgrade.Modifier * slotCount;
                        }
                    }

                }
            }

            return num;
        }

        /*public static float GetCurrentUpgradeValue(MyCubeBlock block, string type)
        {
            float num = 0;
            var upgradeValues = block.UpgradeValues;
            foreach (var upgrade in upgradeValues.Keys)
            {
                if (upgrade == type)
                {
                    num += upgradeValues[upgrade];
                }
            }

            return num;
        }*/

        public static void SetUpgradeValue(MyCubeBlock block, string type, float value, bool sync = false)
        {
            if (!block.UpgradeValues.ContainsKey(type)) return;
            block.UpgradeValues[type] = value;
            block.CommitUpgradeValues();

            if (sync)
                Comms.SyncProductionPerk(block.EntityId, type, value);
        }

        /*public static void AddPerks(ClaimBlockSettings settings, MyCubeBlock block = null)
        {
            if (settings == null) return;
            if (settings.GetPerks == null || settings.GetPerks.Count == 0) return;

            foreach (var item in settings.GetPerks.Values)
            {
                if (item.type == PerkType.Production)
                {
                    AddAllProductionMultipliers(settings, block, true);
                }
            }
        }*/

        /*public static void RemovePerks(ClaimBlockSettings settings)
        {
            if (settings == null) return;
            if (settings.GetPerks == null || settings.GetPerks.Count == 0)
            {
                return;
            }

            foreach (var item in settings.GetPerks.Values)
            {
                if (item.type == PerkType.Production)
                {
                    RemoveProductionMultipliers(settings, null, true);
                    settings.GetPerks[PerkType.Production].perk.productionPerk.pendingAddUpgrades.Clear();
                    settings.GetPerks[PerkType.Production].perk.productionPerk.pendingRemoveUpgrades.Clear();
                    settings.GetPerks[PerkType.Production].perk.productionPerk.activeUpgrades.Clear();
                    settings.GetPerks[PerkType.Production].perk.productionPerk.enableClientControlSpeed = false;
                    settings.GetPerks[PerkType.Production].perk.productionPerk.enableClientControlYield = false;
                    settings.GetPerks[PerkType.Production].perk.productionPerk.enableClientControlEnergy = false;
                }
            }
        }*/

        /*public static void RemovePerksFromGrid(ClaimBlockSettings settings, MyCubeGrid grid)
        {
            if (settings == null) return;
            if (settings.GetGridsInside.Count == 0 || settings.GetGridsInside.ContainsKey(grid.EntityId)) return;

            foreach (var item in settings.GetPerks.Values)
            {
                if (item.type == PerkType.Production)
                {
                    foreach (var gridData in settings.GetGridsInside.Values)
                    {
                        foreach (var production in gridData.blocksMonitored.production)
                        {
                            RemoveProductionMultipliers(settings, production, true);
                        }
                    }
                }
            }
        }*/

        /*public static void RemovePerkType(ClaimBlockSettings settings, PerkType perkType)
        {
            if (settings == null) return;
            if (settings.GetPerks == null || settings.GetPerks.Count == 0) return;

            foreach (var item in settings.GetPerks.Values)
            {
                if (item.type == perkType)
                {
                    RemoveProductionMultipliers(settings, null, true);
                    settings.GetPerks[PerkType.Production].perk.productionPerk.pendingAddUpgrades.Clear();
                    settings.GetPerks[PerkType.Production].perk.productionPerk.pendingRemoveUpgrades.Clear();
                    settings.GetPerks[PerkType.Production].perk.productionPerk.activeUpgrades.Clear();
                    settings.GetPerks[PerkType.Production].perk.productionPerk.enableClientControlSpeed = false;
                    settings.GetPerks[PerkType.Production].perk.productionPerk.enableClientControlYield = false;
                    settings.GetPerks[PerkType.Production].perk.productionPerk.enableClientControlEnergy = false;
                }
            }

            settings.Server.Sync = true;
        }*/

        /*public static void UpdateSingleProductionMultiplier(ClaimBlockSettings settings, MyCubeBlock block, string upgradeName, bool add = false, bool sync = false)
        {
            if (settings == null) return;
            if (settings.GetGridsInside.Count == 0) return;
            if (!settings.GetPerks.ContainsKey(PerkType.Production)) return;
            if (!settings.GetPerks[PerkType.Production].enabled) return;

            float speed = settings.GetPerks[PerkType.Production].perk.productionPerk.Speed;
            float yield = settings.GetPerks[PerkType.Production].perk.productionPerk.Yield;
            float energy = settings.GetPerks[PerkType.Production].perk.productionPerk.Energy;

            if (block != null)
            {
                if (!block.IsFunctional) return;
                IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.OwnerId);
                if (blockFaction == null) return;

                if (settings.ClaimedFaction == blockFaction.Tag)
                {

                    float num3 = GetAttachedUpgradeModules(block, "Productivity");
                    float num4 = GetAttachedUpgradeModules(block, "Effectiveness");
                    float num5 = GetAttachedUpgradeModules(block, "PowerEfficiency");

                    if (!settings.GetPerks[PerkType.Production].perk.productionPerk.allowStandAlone)
                    {
                        if (upgradeName == "Productivity")
                        {
                            if (add)
                                SetUpgradeValue(block, "Productivity", speed + num3, sync);
                            else
                                SetUpgradeValue(block, "Productivity", num3, sync);
                        }


                        if (upgradeName == "Effectiveness")
                        {
                            if (add)
                                SetUpgradeValue(block, "Effectiveness", yield + num4 + 1, sync);
                            else
                                SetUpgradeValue(block, "Effectiveness", num4 + 1, sync);
                        }

                        if (upgradeName == "PowerEfficiency")
                        {
                            if (add)
                                SetUpgradeValue(block, "PowerEfficiency", energy + num5 + 1, sync);
                            else
                                SetUpgradeValue(block, "PowerEfficiency", num5 + 1, sync);
                        }

                        if (settings.GetPerks[PerkType.Production].perk.productionPerk.GetActiveUpgrades.Count == 0)
                            settings.GetPerks[PerkType.Production].perk.productionPerk.UpdateAttachedEntities(block.EntityId, false);
                        else
                            settings.GetPerks[PerkType.Production].perk.productionPerk.UpdateAttachedEntities(block.EntityId, true);

                        Comms.SyncSettingType(settings, MyAPIGateway.Session.LocalHumanPlayer, SyncType.SyncProductionAttached);
                    }
                }

                return;
            }

            foreach (var gridData in settings.GetGridsInside.Values)
            {
                if (gridData.blocksMonitored.production.Count == 0) continue;
                foreach (var production in gridData.blocksMonitored.production)
                {
                    if (!production.IsFunctional) return;
                    IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(production.OwnerId);
                    if (blockFaction == null) continue;

                    if (settings.ClaimedFaction == blockFaction.Tag)
                    {
                        float num3 = GetAttachedUpgradeModules(production as MyCubeBlock, "Productivity");
                        float num4 = GetAttachedUpgradeModules(production as MyCubeBlock, "Effectiveness");
                        float num5 = GetAttachedUpgradeModules(production as MyCubeBlock, "PowerEfficiency");

                        if (!settings.GetPerks[PerkType.Production].perk.productionPerk.allowStandAlone)
                        {
                            if (upgradeName == "Productivity")
                            {
                                if (add)
                                    SetUpgradeValue(production as MyCubeBlock, "Productivity", speed + num3, sync);
                                else
                                    SetUpgradeValue(production as MyCubeBlock, "Productivity", num3, sync);
                            }


                            if (upgradeName == "Effectiveness")
                            {
                                if (add)
                                    SetUpgradeValue(production as MyCubeBlock, "Effectiveness", yield + num4 + 1, sync);
                                else
                                    SetUpgradeValue(production as MyCubeBlock, "Effectiveness", num4 + 1, sync);
                            }

                            if (upgradeName == "PowerEfficiency")
                            {
                                if (add)
                                    SetUpgradeValue(production as MyCubeBlock, "PowerEfficiency", energy + num5 + 1, sync);
                                else
                                    SetUpgradeValue(production as MyCubeBlock, "PowerEfficiency", num5 + 1, sync);
                            }

                            if (settings.GetPerks[PerkType.Production].perk.productionPerk.GetActiveUpgrades.Count == 0)
                            {
                                settings.GetPerks[PerkType.Production].perk.productionPerk.UpdateAttachedEntities(production.EntityId, false);
                            }

                            else
                            {
                                settings.GetPerks[PerkType.Production].perk.productionPerk.UpdateAttachedEntities(production.EntityId, true);
                            }

                        }

                        continue;
                    }
                }
            }

            Comms.SyncSettingType(settings, MyAPIGateway.Session.LocalHumanPlayer, SyncType.SyncProductionAttached);
        }*/

        /*public static void AddProductionMultipliersToClient(MyCubeBlock block, ClaimBlockSettings settings)
        {
            if (block == null || settings == null) return;
            if (!block.IsFunctional) return;
            if (!settings.GetPerks.ContainsKey(PerkType.Production)) return;
            if (!settings.GetPerks[PerkType.Production].enabled) return;

            float speed = settings.GetPerks[PerkType.Production].perk.productionPerk.Speed;
            float yield = settings.GetPerks[PerkType.Production].perk.productionPerk.Yield;
            float energy = settings.GetPerks[PerkType.Production].perk.productionPerk.Energy;

            float num3 = GetAttachedUpgradeModules(block, "Productivity");
            float num4 = GetAttachedUpgradeModules(block, "Effectiveness");
            float num5 = GetAttachedUpgradeModules(block, "PowerEfficiency");

            if (settings.GetPerks[PerkType.Production].perk.productionPerk.allowStandAlone)
            {
                SetUpgradeValue(block, "Productivity", speed + num3);
                SetUpgradeValue(block, "Effectiveness", yield + num4 + 1);
                SetUpgradeValue(block, "PowerEfficiency", energy + num5 + 1);
            }
            else
            {
                if (settings.GetPerks[PerkType.Production].perk.productionPerk.ProductionRunning)
                {
                    foreach (var upgrade in settings.GetPerks[PerkType.Production].perk.productionPerk.GetActiveUpgrades)
                    {
                        if (upgrade == "Productivity")
                            SetUpgradeValue(block, "Productivity", speed + num3);

                        if (upgrade == "Effectiveness")
                            SetUpgradeValue(block, "Effectiveness", yield + num4 + 1);

                        if (upgrade == "PowerEfficiency")
                            SetUpgradeValue(block, "PowerEfficiency", energy + num5 + 1);
                    }
                }
            }
        }*/

        /*public static void AddAllProductionMultipliers(ClaimBlockSettings settings, MyCubeBlock block, bool sync = false)
        {
            if (settings == null) return;
            if (settings.GetGridsInside.Count == 0)
            {
                return;
            }
            if (!settings.GetPerks.ContainsKey(PerkType.Production))
            {
                return;
            }

            if (!settings.GetPerks[PerkType.Production].enabled) return;

            float speed = settings.GetPerks[PerkType.Production].perk.productionPerk.Speed;
            float yield = settings.GetPerks[PerkType.Production].perk.productionPerk.Yield;
            float energy = settings.GetPerks[PerkType.Production].perk.productionPerk.Energy;

            if (block != null)
            {
                if (!block.IsFunctional) return;
                IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.OwnerId);
                if (blockFaction == null)
                {
                    return;
                }

                if (settings.ClaimedFaction == blockFaction.Tag)
                {

                    float num3 = GetAttachedUpgradeModules(block, "Productivity");
                    float num4 = GetAttachedUpgradeModules(block, "Effectiveness");
                    float num5 = GetAttachedUpgradeModules(block, "PowerEfficiency");

                    if (settings.GetPerks[PerkType.Production].perk.productionPerk.allowStandAlone)
                    {
                        SetUpgradeValue(block, "Productivity", speed + num3, sync);
                        SetUpgradeValue(block, "Effectiveness", yield + num4 + 1, sync);
                        SetUpgradeValue(block, "PowerEfficiency", energy + num5 + 1, sync);
                    }
                    else
                    {
                        if (settings.GetPerks[PerkType.Production].perk.productionPerk.GetActiveUpgrades.Count == 0) return;
                        if (!settings.GetPerks[PerkType.Production].perk.productionPerk.ProductionRunning) return;
                        foreach (var item in settings.GetPerks[PerkType.Production].perk.productionPerk.GetActiveUpgrades)
                        {
                            if (item == "Productivity")
                                SetUpgradeValue(block, "Productivity", speed + num3, sync);

                            if (item == "Effectiveness")
                                SetUpgradeValue(block, "Effectiveness", yield + num4 + 1, sync);

                            if (item == "PowerEfficiency")
                                SetUpgradeValue(block, "PowerEfficiency", energy + num5 + 1, sync);

                        }
                    }


                    settings.GetPerks[PerkType.Production].perk.productionPerk.UpdateAttachedEntities(block.EntityId, true);
                    Comms.SyncSettingType(settings, MyAPIGateway.Session.LocalHumanPlayer, SyncType.SyncProductionAttached);
                }

                return;
            }

            foreach (var gridData in settings.GetGridsInside.Values)
            {
                if (gridData.blocksMonitored.production.Count == 0)
                {
                    continue;
                }
                foreach (var production in gridData.blocksMonitored.production)
                {
                    if (!production.IsFunctional) continue;
                    IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(production.OwnerId);
                    if (blockFaction == null) continue;

                    if (settings.ClaimedFaction == blockFaction.Tag)
                    {
                        float num3 = GetAttachedUpgradeModules(production as MyCubeBlock, "Productivity");
                        float num4 = GetAttachedUpgradeModules(production as MyCubeBlock, "Effectiveness");
                        float num5 = GetAttachedUpgradeModules(production as MyCubeBlock, "PowerEfficiency");

                        if (settings.GetPerks[PerkType.Production].perk.productionPerk.allowStandAlone)
                        {
                            SetUpgradeValue(production as MyCubeBlock, "Productivity", speed + num3, sync);
                            SetUpgradeValue(production as MyCubeBlock, "Effectiveness", yield + num4 + 1, sync);
                            SetUpgradeValue(production as MyCubeBlock, "PowerEfficiency", energy + num5 + 1, sync);
                        }
                        else
                        {
                            if (settings.GetPerks[PerkType.Production].perk.productionPerk.GetActiveUpgrades.Count == 0) return;
                            if (!settings.GetPerks[PerkType.Production].perk.productionPerk.ProductionRunning) return;
                            foreach (var item in settings.GetPerks[PerkType.Production].perk.productionPerk.GetActiveUpgrades)
                            {
                                if (item == "Productivity")
                                    SetUpgradeValue(production as MyCubeBlock, "Productivity", speed + num3, sync);

                                if (item == "Effectiveness")
                                    SetUpgradeValue(production as MyCubeBlock, "Effectiveness", yield + num4 + 1, sync);

                                if (item == "PowerEfficiency")
                                    SetUpgradeValue(production as MyCubeBlock, "PowerEfficiency", energy + num5 + 1, sync);

                            }
                        }


                        settings.GetPerks[PerkType.Production].perk.productionPerk.UpdateAttachedEntities(production.EntityId, true);


                        continue;
                    }
                }
            }

            Comms.SyncSettingType(settings, MyAPIGateway.Session.LocalHumanPlayer, SyncType.SyncProductionAttached);
        }*/

        /*public static void UpdateProductionMultipliers(ClaimBlockSettings settings)
        {
            if (!settings.IsClaimed) return;
            if (!settings.GetPerks.ContainsKey(PerkType.Production)) return;
            if (settings.GetPerks[PerkType.Production].perk.productionPerk.attachedEntities.Count == 0) return;
            if (!settings.GetPerks[PerkType.Production].enabled) return;

            foreach (var productionId in settings.GetPerks[PerkType.Production].perk.productionPerk.attachedEntities)
            {
                IMyEntity entity;
                if (!MyAPIGateway.Entities.TryGetEntityById(productionId, out entity)) continue;

                float num3 = GetAttachedUpgradeModules(entity as MyCubeBlock, "Productivity");
                float num4 = GetAttachedUpgradeModules(entity as MyCubeBlock, "Effectiveness");
                float num5 = GetAttachedUpgradeModules(entity as MyCubeBlock, "PowerEfficiency");

                float speed = settings.GetPerks[PerkType.Production].perk.productionPerk.Speed;
                float yield = settings.GetPerks[PerkType.Production].perk.productionPerk.Yield;
                float energy = settings.GetPerks[PerkType.Production].perk.productionPerk.Energy;

                if (settings.GetPerks[PerkType.Production].perk.productionPerk.allowStandAlone)
                {
                    SetUpgradeValue(entity as MyCubeBlock, "Productivity", speed + num3, true);
                    SetUpgradeValue(entity as MyCubeBlock, "Effectiveness", yield + num4 + 1, true);
                    SetUpgradeValue(entity as MyCubeBlock, "PowerEfficiency", energy + num5 + 1, true);
                }
                else
                {
                    if (settings.GetPerks[PerkType.Production].perk.productionPerk.GetActiveUpgrades.Count == 0) return;
                    if (!settings.GetPerks[PerkType.Production].perk.productionPerk.ProductionRunning) return;
                    foreach (var item in settings.GetPerks[PerkType.Production].perk.productionPerk.GetActiveUpgrades)
                    {
                        if (item == "Productivity")
                            SetUpgradeValue(entity as MyCubeBlock, "Productivity", speed + num3, true);

                        if (item == "Effectiveness")
                            SetUpgradeValue(entity as MyCubeBlock, "Effectiveness", yield + num4 + 1, true);

                        if (item == "PowerEfficiency")
                            SetUpgradeValue(entity as MyCubeBlock, "PowerEfficiency", energy + num5 + 1, true);

                    }
                }
            }
        }*/

        /*public static void RemoveProductionMultipliers(ClaimBlockSettings settings, IMyProductionBlock block = null, bool sync = false)
        {
            if (settings == null) return;
            if (settings.GetGridsInside.Count == 0) return;
            if (!settings.GetPerks.ContainsKey(PerkType.Production)) return;

            if (block != null)
            {
                if (!settings.GetPerks[PerkType.Production].perk.productionPerk.attachedEntities.Contains(block.EntityId)) return;
                float num = GetAttachedUpgradeModules(block as MyCubeBlock, "Productivity");
                float num1 = GetAttachedUpgradeModules(block as MyCubeBlock, "Effectiveness");
                float num2 = GetAttachedUpgradeModules(block as MyCubeBlock, "PowerEfficiency");

                if (num > 0)
                    SetUpgradeValue(block as MyCubeBlock, "Productivity", num, sync);
                else
                    SetUpgradeValue(block as MyCubeBlock, "Productivity", 0f, sync);

                if (num1 > 0)
                    SetUpgradeValue(block as MyCubeBlock, "Effectiveness", num1, sync);
                else
                    SetUpgradeValue(block as MyCubeBlock, "Effectiveness", 1f, sync);

                if (num2 > 0)
                    SetUpgradeValue(block as MyCubeBlock, "PowerEfficiency", num2, sync);
                else
                    SetUpgradeValue(block as MyCubeBlock, "PowerEfficiency", 1f, sync);

                settings.GetPerks[PerkType.Production].perk.productionPerk.UpdateAttachedEntities(block.EntityId, false);
                Comms.SyncSettingType(settings, MyAPIGateway.Session.LocalHumanPlayer, SyncType.SyncProductionAttached);
                //settings.Server.Sync = true;
                return;
            }

            foreach (var gridData in settings.GetGridsInside.Values)
            {
                if (gridData.blocksMonitored.production.Count == 0) continue;
                foreach (var production in gridData.blocksMonitored.production)
                {
                    if (!settings.GetPerks[PerkType.Production].perk.productionPerk.attachedEntities.Contains(production.EntityId)) continue;
                    float num = GetAttachedUpgradeModules(production as MyCubeBlock, "Productivity");
                    float num1 = GetAttachedUpgradeModules(production as MyCubeBlock, "Effectiveness");
                    float num2 = GetAttachedUpgradeModules(production as MyCubeBlock, "PowerEfficiency");

                    if (num > 0)
                        SetUpgradeValue(production as MyCubeBlock, "Productivity", num, sync);
                    else
                        SetUpgradeValue(production as MyCubeBlock, "Productivity", 0f, sync);

                    if (num1 > 0)
                        SetUpgradeValue(production as MyCubeBlock, "Effectiveness", num1, sync);
                    else
                        SetUpgradeValue(production as MyCubeBlock, "Effectiveness", 1f, sync);

                    if (num2 > 0)
                        SetUpgradeValue(production as MyCubeBlock, "PowerEfficiency", num2, sync);
                    else
                        SetUpgradeValue(production as MyCubeBlock, "PowerEfficiency", 1f, sync);

                    settings.GetPerks[PerkType.Production].perk.productionPerk.UpdateAttachedEntities(production.EntityId, false);

                    continue;
                }
            }

            settings.GetPerks[PerkType.Production].perk.productionPerk.ProductionRunning = false;
            Comms.SyncSettingType(settings, MyAPIGateway.Session.LocalHumanPlayer, SyncType.SyncProductionAttached);
            Comms.SyncSettingType(settings, MyAPIGateway.Session.LocalHumanPlayer, SyncType.SyncProductionRunning);
        }*/

        /*public static void RemoveProductionMultipliersByPlayer(ClaimBlockSettings settings, long playerId, bool sync = false)
        {
            if (settings == null) return;
            if (settings.GetGridsInside.Count == 0) return;
            if (!settings.GetPerks.ContainsKey(PerkType.Production)) return;

            foreach (var gridData in settings.GetGridsInside.Values)
            {
                if (gridData.blocksMonitored.production.Count == 0) continue;
                foreach (var production in gridData.blocksMonitored.production)
                {
                    if (!settings.GetPerks[PerkType.Production].perk.productionPerk.attachedEntities.Contains(production.EntityId)) continue;
                    if (production.OwnerId != playerId) continue;

                    float num = GetAttachedUpgradeModules(production as MyCubeBlock, "Productivity");
                    float num1 = GetAttachedUpgradeModules(production as MyCubeBlock, "Effectiveness");
                    float num2 = GetAttachedUpgradeModules(production as MyCubeBlock, "PowerEfficiency");

                    if (num > 0)
                        SetUpgradeValue(production as MyCubeBlock, "Productivity", num, sync);
                    else
                        SetUpgradeValue(production as MyCubeBlock, "Productivity", 0f, sync);

                    if (num1 > 0)
                        SetUpgradeValue(production as MyCubeBlock, "Effectiveness", num1, sync);
                    else
                        SetUpgradeValue(production as MyCubeBlock, "Effectiveness", 1f, sync);

                    if (num2 > 0)
                        SetUpgradeValue(production as MyCubeBlock, "PowerEfficiency", num2, sync);
                    else
                        SetUpgradeValue(production as MyCubeBlock, "PowerEfficiency", 1f, sync);

                    settings.GetPerks[PerkType.Production].perk.productionPerk.UpdateAttachedEntities(production.EntityId, false);
                    continue;
                }
            }

            Comms.SyncSettingType(settings, MyAPIGateway.Session.LocalHumanPlayer, SyncType.SyncProductionAttached);

            if (settings.GetPerks[PerkType.Production].perk.productionPerk.attachedEntities.Count == 0)
            {
                settings.GetPerks[PerkType.Production].perk.productionPerk.ProductionRunning = false;
                Comms.SyncSettingType(settings, MyAPIGateway.Session.LocalHumanPlayer, SyncType.SyncProductionRunning);
            }
        }*/

        public static void SetDefaultProduction(MyCubeBlock block)
        {
            if (block == null) return;
            float num = GetAttachedUpgradeModules(block as MyCubeBlock, "Productivity");
            float num1 = GetAttachedUpgradeModules(block as MyCubeBlock, "Effectiveness");
            float num2 = GetAttachedUpgradeModules(block as MyCubeBlock, "PowerEfficiency");

            if (num > 0)
                SetUpgradeValue(block as MyCubeBlock, "Productivity", num);
            else
                SetUpgradeValue(block as MyCubeBlock, "Productivity", 0f);

            if (num1 > 0)
                SetUpgradeValue(block as MyCubeBlock, "Effectiveness", num1);
            else
                SetUpgradeValue(block as MyCubeBlock, "Effectiveness", 1f);

            if (num2 > 0)
                SetUpgradeValue(block as MyCubeBlock, "PowerEfficiency", num2);
            else
                SetUpgradeValue(block as MyCubeBlock, "PowerEfficiency", 1f);
        }

        /*public static void UpdatePlayerPerks(ClaimBlockSettings settings)
        {
            if (settings == null) return;

            foreach (var perk in settings.GetPerks.Keys)
            {
                if (perk == PerkType.Production)
                {
                    PerkBase perkbase;
                    if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) continue;

                    if (!perkbase.Enable) continue;
                    if (perkbase.perk.productionPerk.allowStandAlone) continue;
                    if (perkbase.perk.productionPerk.GetPendingAddUpgrades.Count != 0)
                    {
                        foreach (var upgrade in perkbase.perk.productionPerk.GetPendingAddUpgrades)
                        {
                            if (perkbase.perk.productionPerk.GetActiveUpgrades.Contains(upgrade)) continue;

                            settings.GetPerks[PerkType.Production].perk.productionPerk.ActiveUprades(upgrade, true);
                            UpdateSingleProductionMultiplier(settings, null, upgrade, true, true);
                        }
                    }

                    if (perkbase.perk.productionPerk.GetPendingRemoveUpgrades.Count != 0)
                    {
                        foreach (var upgrade in perkbase.perk.productionPerk.GetPendingRemoveUpgrades)
                        {
                            if (!perkbase.perk.productionPerk.GetActiveUpgrades.Contains(upgrade)) continue;

                            settings.GetPerks[PerkType.Production].perk.productionPerk.ActiveUprades(upgrade, false);
                            UpdateSingleProductionMultiplier(settings, null, upgrade, false, true);
                        }
                    }

                    perkbase.perk.productionPerk.pendingAddUpgrades.Clear();
                    perkbase.perk.productionPerk.pendingRemoveUpgrades.Clear();

                    if (perkbase.perk.productionPerk.GetActiveUpgrades.Count != 0)
                        perkbase.perk.productionPerk.ProductionRunning = true;
                    else
                        perkbase.perk.productionPerk.ProductionRunning = false;

                    settings.Server.Sync = true;
                    Comms.SyncSettingType(settings, MyAPIGateway.Session.LocalHumanPlayer, SyncType.SyncProductionRunning);
                }
            }
        }*/

        /*public static void UpdateActiveStandAlonePerks(ClaimBlockSettings settings)
        {
            if (settings == null) return;
            foreach (var perk in settings.GetPerks.Keys)
            {
                if (perk == PerkType.Production)
                {
                    PerkBase perkbase;
                    if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) continue;
                    perkbase.perk.productionPerk.activeUpgrades.Clear();

                    if (perkbase.perk.productionPerk.allowStandAlone)
                    {
                        if (perkbase.perk.productionPerk.Speed != 0)
                            settings.GetPerks[PerkType.Production].perk.productionPerk.ActiveUprades("Productivity", true);

                        if (perkbase.perk.productionPerk.Yield != 0)
                            settings.GetPerks[PerkType.Production].perk.productionPerk.ActiveUprades("Effectiveness", true);

                        if (perkbase.perk.productionPerk.Energy != 0)
                            settings.GetPerks[PerkType.Production].perk.productionPerk.ActiveUprades("PowerEfficiency", true);
                    }

                    if (perkbase.perk.productionPerk.GetActiveUpgrades.Count != 0)
                        perkbase.perk.productionPerk.ProductionRunning = true;
                    else
                        perkbase.perk.productionPerk.ProductionRunning = false;

                    settings.Server.Sync = true;
                    Comms.SyncSettingType(settings, MyAPIGateway.Session.LocalHumanPlayer, SyncType.SyncProductionRunning);
                }
            }
        }*/

        public static string GetCounterDetails(ClaimBlockSettings settings)
        {
            string result = "";
            var idx = settings.TerritoryConfig._token.IndexOf('/') + 1;

            result += $"\n A Token = {settings.TerritoryConfig._token.Substring(idx)}\n";
            result += $"[Territory Radius]: {settings.TerritoryConfig._territoryRadius}m\n";
            result += $"[Territory Upkeep Cost]: {settings.TerritoryConfig._territoryMaintenance._consumptionAmt} Token(s)\n";
            if (settings.TerritoryConfig._territoryOptions._centerOnPlanet)
                result += $"[Territory Centered Around]: {settings.PlanetName}\n";

            if (settings.IsSiegeCooling)
            {
                result += $"\n[Siege Cooldown]: {TimeSpan.FromSeconds(settings.SiegeTimer)}\n";
            }

            if (settings.IsClaimed)
            {
                result += $"\n[Time Until Token Consumption]: ";
                result += $"{TimeSpan.FromSeconds(settings.Timer)}\n";
                result += $"\n[Installation Type] : [Status]\n";
                foreach (var installation in settings.GetInstallations)
                {
                    string status = installation.Enabled ? $"Active: {installation.Integrity}%" : $"Rebuild Cooldown: {TimeSpan.FromSeconds(installation.RebuildCooldown)}";
                    if (!installation.Enabled && installation.RebuildCooldown <= 0)
                        status = "Ready To Purchase";

                    if (installation._searchingTerrain)
                        status = "Searching Terrain...";

                    if (installation._failedToSpawn)
                        status = "Failed Spawn, Try Again";

                    result += $"{installation.Type} - {status}\n";
                }
            }
            

            

            

            return result;
        }

        public static void ManuallySetTerritory(ClaimBlockSettings settings, string tag)
        {
            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(tag);
            if (faction == null) return;

            settings.RecoveryTimer = 0;
            settings.IsClaiming = false;
            settings.IsClaimed = true;
            settings.ClaimZoneName = settings.UnclaimName;
            settings.Timer = settings.TerritoryConfig._territoryMaintenance._consumptionTime;
            settings.BlockEmissive = EmissiveState.Claimed;
            settings.ClaimedFaction = faction.Tag;
            settings.FactionId = faction.FactionId;
            settings.GetTerritoryStatus = TerritoryStatus.Claimed;

            //var icon = faction.FactionIcon;
            //Utils.SetScreen(settings.Block as IMyBeacon, faction, true);

            Utils.SetOwner(settings.Block, settings);
            Utils.AddSafeZone(settings);
            Utils.GetSurroundingSafeZones(settings);
            Utils.TagEnemyGrids(settings);
            Utils.TagEnemyPlayers(settings);
            Utils.StopHandTools();
            Utils.AddAllInstallations(settings);
            Utils.SetRelationWithNpc(settings, MyRelationsBetweenFactions.Friends);
            //Utils.AddPerks(settings);
            GPS.UpdateBlockText(settings, $"Claimed Territory: {settings.ClaimZoneName} ({settings.ClaimedFaction})");
        }

        public static void DisableBlock(MyCubeBlock block, ClaimBlockSettings settings)
        {
            if (settings.TerritoryConfig._territoryOptions._allowTools) return;
            IMyFunctionalBlock fBlock = block as IMyFunctionalBlock;
            if (fBlock == null) return;

            if (block is IMyShipDrill && settings.TerritoryConfig._territoryOptions._allowDrilling) return;
            if (block is IMyShipWelder && settings.TerritoryConfig._territoryOptions._allowWelding) return;
            if (block is IMyShipGrinder && settings.TerritoryConfig._territoryOptions._allowGrinding) return;

            IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.OwnerId);
            if (blockFaction == null)
            {
                fBlock.Enabled = false;
                return;
            }

            if (settings.IsClaimed)
            {
                if (settings.ClaimedFaction != blockFaction.Tag)
                {
                    if (settings.AllowTerritoryAllies || settings.AllowSafeZoneAllies)
                    {
                        IMyFaction claimFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
                        if (claimFaction != null)
                        {
                            if (IsFactionEnemy(settings, blockFaction))
                            {
                                fBlock.Enabled = false;
                                return;
                            }

                            return;
                        }

                    }

                    fBlock.Enabled = false;
                    return;
                }
            }
        }

        public static bool IsFactionEnemy(ClaimBlockSettings settings, IMyFaction faction2)
        {
            if (settings == null || faction2 == null) return true;
            IMyFaction claimFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
            if (claimFaction == null) return true;

            var relation = MyAPIGateway.Session.Factions.GetRelationBetweenFactions(claimFaction.FactionId, faction2.FactionId);
            if (relation == MyRelationsBetweenFactions.Enemies) return true;

            return false;
        }

        public static void SetOwner(IMyTerminalBlock block, ClaimBlockSettings settings)
        {
            MyCubeBlock cubeblock = block as MyCubeBlock;
            if (cubeblock == null) return;

            cubeblock.ChangeBlockOwnerRequest(0, MyOwnershipShareModeEnum.Faction);

            if (settings == null)
            {
                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionByTag("SPRT");
                cubeblock.ChangeBlockOwnerRequest(faction.FounderId, MyOwnershipShareModeEnum.Faction);
                return;
            }

            IMyFaction npc = null;
            if (settings.BlockOwner != null)
                npc = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.BlockOwner);

            if (npc == null)
            {
                npc = MyAPIGateway.Session.Factions.TryGetFactionByTag("SPRT");
                cubeblock.ChangeBlockOwnerRequest(npc.FounderId, MyOwnershipShareModeEnum.Faction);
                return;
            }

            if (settings.IsClaimed)
                cubeblock.ChangeBlockOwnerRequest(npc.FounderId, MyOwnershipShareModeEnum.All);
            else
                cubeblock.ChangeBlockOwnerRequest(npc.FounderId, MyOwnershipShareModeEnum.Faction);

            /*if (myFaction != null)
                cubeblock.ChangeBlockOwnerRequest(myFaction.FounderId, MyOwnershipShareModeEnum.Faction);
            else
            {
                IMyFaction npc = MyAPIGateway.Session.Factions.TryGetFactionByTag("SPRT");
                if (npc != null)
                    cubeblock.ChangeBlockOwnerRequest(npc.FounderId, MyOwnershipShareModeEnum.Faction);

            }*/

        }

        public static void SetRelationWithNpc(ClaimBlockSettings settings, MyRelationsBetweenFactions relation)
        {
            IMyFaction ownerFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.ClaimedFaction);
            IMyFaction npc = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.BlockOwner);
            if (ownerFaction == null || npc == null) return;

            if (relation == MyRelationsBetweenFactions.Friends)
            {
                MyAPIGateway.Session.Factions.SendPeaceRequest(ownerFaction.FactionId, npc.FactionId);
                MyAPIGateway.Session.Factions.AcceptPeace(ownerFaction.FactionId, npc.FactionId);

                var members = ownerFaction.Members;
                foreach (var member in members.Keys)
                {
                    MyAPIGateway.Session.Factions.SetReputationBetweenPlayerAndFaction(member, npc.FactionId, 1500);
                    Comms.SetReputationWithFaction(member, npc.FactionId, 1500);
                }

                var factions = MyAPIGateway.Session.Factions.Factions;
                foreach (var faction in factions.Values)
                {
                    if (faction.IsEveryoneNpc() || faction.Tag.Length > 3 || faction == ownerFaction) continue;
                    if (!IsFactionEnemy(settings, faction))
                    {
                        SetAlliesRelationWithNpc(settings, faction, MyRelationsBetweenFactions.Friends);
                        continue;
                    }

                    MyAPIGateway.Session.Factions.DeclareWar(faction.FactionId, npc.FactionId);
                }

                return;
            }

            if (relation == MyRelationsBetweenFactions.Enemies)
            {
                var factions = MyAPIGateway.Session.Factions.Factions;
                foreach (var faction in factions.Values)
                {
                    if (faction.IsEveryoneNpc() || faction.Tag.Length > 3) continue;
                    MyAPIGateway.Session.Factions.DeclareWar(faction.FactionId, npc.FactionId);
                }
            }
        }

        public static void SetReputationWithFaction(long playerId, long factionId, int amt)
        {
            MyAPIGateway.Session.Factions.SetReputationBetweenPlayerAndFaction(playerId, factionId, amt);
        }

        public static void SetAlliesRelationWithNpc(ClaimBlockSettings settings, IMyFaction ally, MyRelationsBetweenFactions relation)
        {
            //if (!settings.AllowTerritoryAllies) return;
            IMyFaction npc = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.TerritoryConfig._npcTag);
            if (npc == null) return;

            if (relation == MyRelationsBetweenFactions.Friends)
            {
                if (ally == null)
                {
                    var factions = MyAPIGateway.Session.Factions.Factions;
                    foreach(var f in factions.Values)
                    {
                        if (f.IsEveryoneNpc() || f.Tag.Length > 3 || f.Tag == settings.ClaimedFaction) continue;
                        if (!IsFactionEnemy(settings, f))
                        {
                            var factionMembers = f.Members;
                            foreach (var member in factionMembers.Keys)
                            {
                                MyAPIGateway.Session.Factions.SetReputationBetweenPlayerAndFaction(member, npc.FactionId, 1500);
                                Comms.SetReputationWithFaction(member, npc.FactionId, 1500);
                            }
                        }
                    }

                    return;
                }
                
                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionById(ally.FactionId);
                if (faction == null) return;

                var members = faction.Members;
                foreach (var member in members.Keys)
                {
                    MyAPIGateway.Session.Factions.SetReputationBetweenPlayerAndFaction(member, npc.FactionId, 1500);
                    Comms.SetReputationWithFaction(member, npc.FactionId, 1500);
                }

                return;
            }

            if (relation == MyRelationsBetweenFactions.Enemies)
            {
                if (ally == null)
                {
                    var factions = MyAPIGateway.Session.Factions.Factions;
                    foreach (var f in factions.Values)
                    {
                        if (f.IsEveryoneNpc() || f.Tag.Length > 3 || f.Tag == settings.ClaimedFaction) continue;
                        MyAPIGateway.Session.Factions.DeclareWar(f.FactionId, npc.FactionId);
                    }

                    return;
                }

                MyAPIGateway.Session.Factions.DeclareWar(ally.FactionId, npc.FactionId);
            }
        }

        public static void MonitorSafeZonePBs(ClaimBlockSettings settings, bool disable = false)
        {
            if (disable)
            {
                foreach (var pb in settings.Server._pbList)
                    pb.IsWorkingChanged -= Events.PbWatcher;

                settings.Server._pbList.Clear();
                return;
            }

            if (settings.SafeZoneEntity == 0) return;
            BoundingSphereD sphere = new BoundingSphereD(settings.BlockPos, settings.TerritoryConfig._safeZoneRadius);
            var ents = MyAPIGateway.Entities.GetTopMostEntitiesInSphere(ref sphere);
            foreach (var ent in ents)
            {
                if (ent == null) continue;
                IMyCubeGrid cubeGrid = ent as IMyCubeGrid;
                if (cubeGrid == null) continue;

                List<IMyProgrammableBlock> blocks = new List<IMyProgrammableBlock>();
                MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(cubeGrid).GetBlocksOfType(blocks);
                foreach (var pb in blocks)
                {
                    if (pb == null) continue;
                    pb.Enabled = false;
                    pb.IsWorkingChanged += Events.PbWatcher;
                    settings.Server._pbList.Add(pb);
                }
            }
        }

        public static string GetPopupText(MyTextEnum text)
        {
            StringBuilder builder = new StringBuilder();

            if (text == MyTextEnum.SafeZoneAllies)
            {
                builder.Append("\n            ****** WARNING ******\n\n");
                builder.Append(" Enabling this will give all of your faction's allies access inside the safe zone with the ability to drill/weld/grind.");
                builder.Append(" Enabling this option will also force enable 'Allow Territory Allies'. Which means that will be able to access your whole territory to drill/weld/grind without being detected.");
                builder.Append(" Allies will NOT be able to access the claim block to change settings.\n\n");
                builder.Append("This option is NOT dynamic, so if a faction is newly allied to you after this option is enabled, the safe zone whitelist does NOT automatically update its whitelist.");
                builder.Append(" You will need to disable/enable this option to refresh the safe zone with your current allies. Same goes with a faction that is no longer an allie with you.");
                return builder.ToString();
            }

            if (text == MyTextEnum.TerritoryAllies)
            {
                builder.Append("\n            ****** WARNING ******\n\n");
                builder.Append(" Enabling this option will allow all of your faction's allies to access your whole territory, they will be able to drill/weld/grind without being detected.");
                builder.Append(" Allies will NOT be able to access the claim block to change settings.");
                return builder.ToString();
            }

            return builder.ToString();
        }

        public static void EnterTerritoryMessage(ClaimBlockSettings settings, IMyPlayer player)
        {
            if (settings == null) return;

            MyVisualScriptLogicProvider.SendChatMessageColored($"Claimed territories prevent your free offline safezone, or safezone generators, from working unless you are the owner or ally. You will be seen in real-time by the territory owners. Proceed with caution!", Color.Violet, "[Faction Territories]", player.IdentityId, "Red");

            if (settings.TerritoryConfig._territoryOptions._allowTools) return;
            if (!settings.TerritoryConfig._territoryOptions._allowDrilling  && !settings.TerritoryConfig._territoryOptions._allowGrinding && !settings.TerritoryConfig._territoryOptions._allowWelding)
            {
                MyVisualScriptLogicProvider.SendChatMessageColored($"All ship/hand tools are NOT allowed inside claimed territories!", Color.Violet, "[Faction Territories]", player.IdentityId, "Red");
                return;
            }

            if (!settings.TerritoryConfig._territoryOptions._allowDrilling)
                MyVisualScriptLogicProvider.SendChatMessageColored($"Drilling is NOT allowed inside claimed territories!", Color.Violet, "[Faction Territories]", player.IdentityId, "Red");

            if (!settings.TerritoryConfig._territoryOptions._allowGrinding)
                MyVisualScriptLogicProvider.SendChatMessageColored($"Grinding is NOT allowed inside claimed territories!", Color.Violet, "[Faction Territories]", player.IdentityId, "Red");

            if (!settings.TerritoryConfig._territoryOptions._allowWelding)
                MyVisualScriptLogicProvider.SendChatMessageColored($"Welding is NOT allowed inside claimed territories!", Color.Violet, "[Faction Territories]", player.IdentityId, "Red");
        }

        public static void GetTerritoriesStatus(IMyTerminalBlock block = null)
        {
            MyAPIGateway.Parallel.StartBackground(() =>
            {
                if (Session.Instance.IsNexusInstalled)
                {
                    Session.Instance.crossServerClaimSettings.Clear();
                    MyAPIGateway.Utilities.InvokeOnGameThread(() => NexusComms.RequestTerritoryStatus());
                    MyAPIGateway.Parallel.Sleep(2000);
                }

                string status = GetStatus();

                if (block != null)
                {
                    IMyTextSurface panel = block as IMyTextSurface;
                    if (panel == null) return;
                    MyAPIGateway.Utilities.InvokeOnGameThread(() => panel.WriteText(status));
                }
                else
                {
                    foreach (IMyTextSurface panel in Session.Instance.activeLCDs)
                        MyAPIGateway.Utilities.InvokeOnGameThread(() => panel.WriteText(status));
                }
            });
        }

        public static string GetStatus()
        {
            /*if (panel == null) return;
            string title = " Territory Status\n ------------------\n";
            panel.WriteText(title);*/

            string text = "";
            string t2 = "";
            IMyFaction faction = null;
            text = " Territory Status\n ------------------\n";

            foreach (var item in Session.Instance.claimBlocks.Values)
            {
                if (!item.Enabled) continue;
                faction = null;

                TerritoryStatus status = item.GetTerritoryStatus;
                if (status == TerritoryStatus.Claiming)
                {
                    var block = item.JDClaiming as IMyTerminalBlock;
                    if (block == null) continue;
                    faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.OwnerId);
                    if (faction == null) continue;
                    t2 = $"{faction.Tag}";
                }
                if (status == TerritoryStatus.Sieging)
                {
                    var block = item.JDSieging as IMyTerminalBlock;
                    if (block == null) continue;
                    faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.OwnerId);
                    if (faction == null) continue;
                    t2 = $"{faction.Tag}";
                }

                string claimFaction = item.IsClaimed ? $" - {item.ClaimedFaction}" : "";

                text += $"\n {item.UnclaimName}: {status}{claimFaction}\n";
                if (status == TerritoryStatus.Claiming)
                    text += $"Currently Being Claimed | Time To Claim = {TimeSpan.FromSeconds(item.Timer)}\n";
                if (status == TerritoryStatus.Sieging)
                    text += $"Currently Under Siege By `{t2}` | Siege Time Left = {TimeSpan.FromSeconds(item.SiegeTimer)}\n";
                if (status == TerritoryStatus.FailedSiegeCooldown)
                    text += $"Failed Siege | Cooldown Time = {TimeSpan.FromSeconds(item.SiegeTimer)}\n";
                if (status == TerritoryStatus.CooldownToNeutral)
                    text += $"Successfull Siege | Cooldown Time To Neutral = {TimeSpan.FromSeconds(item.Timer)}\n";

                if (status == TerritoryStatus.Claimed)
                {
                    text += "~ Active Installations ~\n";
                    foreach (var installation in item.GetInstallations)
                    {
                        if (!installation.Enabled) continue;
                        text += $"{installation.Type} - Active: {installation.Integrity}\n";
                    }

                }

                text += "---------------------------------------\n";
            }

            foreach (var item in Session.Instance.crossServerClaimSettings)
            {
                if (!item.Enabled) continue;
                faction = null;
                IMyEntity ent;

                TerritoryStatus status = item.GetTerritoryStatus;
                if (status == TerritoryStatus.Claiming)
                {
                    if (!MyAPIGateway.Entities.TryGetEntityById(item.JDClaimingId, out ent)) continue;
                    var block = ent as IMyTerminalBlock;
                    if (block == null) continue;
                    faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.OwnerId);
                    if (faction == null) continue;
                    t2 = $"{faction.Tag}";
                }
                if (status == TerritoryStatus.Sieging)
                {
                    if (!MyAPIGateway.Entities.TryGetEntityById(item.JDSiegingId, out ent)) continue;
                    var block = ent as IMyTerminalBlock;
                    if (block == null) continue;
                    faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.OwnerId);
                    if (faction == null) continue;
                    t2 = $"{faction.Tag}";
                }

                string claimFaction = item.IsClaimed ? $" - {item.ClaimedFaction}" : "";

                text += $"\n {item.UnclaimName}: {status}{claimFaction}\n";
                if (status == TerritoryStatus.Claiming)
                    text += $"Currently Being Claimed | Time To Claim = {TimeSpan.FromSeconds(item.Timer)}\n";
                if (status == TerritoryStatus.Sieging)
                    text += $"Currently Under Siege By `{t2}` | Siege Time Left = {TimeSpan.FromSeconds(item.SiegeTimer)}\n";
                if (status == TerritoryStatus.FailedSiegeCooldown)
                    text += $"Failed Siege | Cooldown Time = {TimeSpan.FromSeconds(item.SiegeTimer)}\n";
                if (status == TerritoryStatus.CooldownToNeutral)
                    text += $"Successfull Siege | Cooldown Time To Neutral = {TimeSpan.FromSeconds(item.Timer)}\n";

                if (status == TerritoryStatus.Claimed)
                {
                    text += "~ Active Installations ~\n";
                    foreach (var installation in item.GetInstallations)
                    {
                        if (!installation.Enabled) continue;
                        text += $"{installation.Type} - Active: {installation.Integrity}\n";
                    }

                }

                text += "---------------------------------------\n";
            }

            return text;
        }

        public static void UpdateTerritoryStatus(bool updateToNexus = false)
        {
            if (Session.Instance.activeLCDs.Count == 0) return;

            string status = GetStatus();
            foreach (IMyTextSurface panel in Session.Instance.activeLCDs)
                panel.WriteText(status);

            if (updateToNexus && Session.Instance.IsNexusInstalled)
                NexusComms.UpdateTerritoryStatusToAll();
        }

        public static int GetTotalSiegeTime(ClaimBlockSettings settings)
        {
            if (settings == null) return 0;
            int time = settings.TerritoryConfig._siegingConfig._siegingTime;
            foreach(var config in settings.GetInstallations)
            {
                time += config.TerritoryInstallations._addSiegeTimeAmt;
            }

            return time;
        }

        public static void DestroyInstallation(ClaimBlockSettings settings, Installations installation)
        {
            if (installation._installationParticle != null)
            {
                installation._installationParticle.Close();
                installation._installationParticle = null;
            }
                

            if (!MyAPIGateway.Session.IsServer) return;
            IMyEntity entity;
            if (!MyAPIGateway.Entities.TryGetEntityById(installation.GridEntityId, out entity)) return;


            /*foreach(var config in settings.TerritoryConfig._territoryInstallations)
            {
                if (config._installationType == installation.type)
                {
                    installation.rebuildCooldown = config._rebuildCooldown;
                    break;
                }
            }*/

            RemoveInstallationPerk(settings, installation);
            installation.RebuildCooldown = installation.TerritoryInstallations._rebuildCooldown;
            installation.GridEntityId = 0;
            installation.BlockEntityId = 0;
            installation.Enabled = false;
            
            //RemoveInstallation(settings, installation);

            IMyCubeGrid grid = entity as IMyCubeGrid;
            if (grid == null) return;

            List<IMySlimBlock> mySlimBlocks = new List<IMySlimBlock>();
            grid.GetBlocks(mySlimBlocks);

            float blockRatio = (float)MathHelper.Clamp(mySlimBlocks.Count / 500, .1, 1);
            float explosionCt = MathHelper.Lerp(10, 30, blockRatio);
            if (mySlimBlocks.Count < 10)
                explosionCt = 3;

            MyVisualScriptLogicProvider.ShowNotification($"Number of explosions max {(int)explosionCt}", 10000);
            InstallationExplosion item = new InstallationExplosion()
            {
                grid = grid,
                blocks = new List<IMySlimBlock>(mySlimBlocks),
                explosions = 0,
                explosionMax = (int)explosionCt
            };

            Session.Instance.explosionsList.Add(item);
        }

        /*public static void RemoveInstallation(ClaimBlockSettings settings, Installations installation)
        {
            if (settings == null) return;
            settings.InstallationRemove(installation);
        }*/

        /*public static void InstallationAdd(ClaimBlockSettings settings, Installations installation)
        {
            if (settings == null) return;
            settings.InstallationAdd(installation);
        }*/

        public static void CreateInstallation(ClaimBlockSettings settings, InstallationType type, bool isAdmin)
        {
            MyAPIGateway.Parallel.StartBackground(() =>
            {
                TerritoryInstallations territoryInstallation = new TerritoryInstallations();
                Installations installation = settings.GetInstallationByType(type);
                if (installation == null)
                {
                    foreach (var config in settings.TerritoryConfig._territoryInstallations)
                    {
                        if (config._installationType == type)
                        {
                            territoryInstallation = config;
                            break;
                        }
                    }

                    installation = new Installations();
                    installation._territoryInstallation = territoryInstallation;
                    installation._type = type;
                }

                installation._failedToSpawn = false;
                Vector3D spawn = FindValidSpawnLocation(settings, installation);
                installation._searchingTerrain = false;
                if (spawn == new Vector3D())
                {
                    installation._failedToSpawn = true;
                    return;
                }

                if (BuyInstallation(settings, installation, isAdmin))
                {
                    if (SpawnInstallation(settings, installation, spawn))
                    {
                        //AddInstallationPerk(settings, installation);
                    }
                }
            });
        }

        public static Vector3D FindValidSpawnLocation(ClaimBlockSettings settings, Installations installation)
        {
            if (settings == null || installation == null) return new Vector3D();
            installation._searchingTerrain = true;

            IMyEntity planetEntity = null;
            Vector3D planetCenter = new Vector3D();
            bool inGravity = false;
            IMyEntity myEntity = settings.Block as IMyEntity;
            MyPlanet planet = MyGamePruningStructure.GetClosestPlanet(myEntity.GetPosition());
            int loops = 0;
            float safeZoneExtension = settings.TerritoryConfig._safeZoneRadius * settings.TerritoryConfig._perkConfig._safeZoneConfig._safeZoneExtensionMultiplier;
            if (planet != null)
            {
                planetEntity = planet as IMyEntity; //Get planet as Entity so we can get the position later.
                inGravity = planetEntity.Components.Get<MyGravityProviderComponent>().IsPositionInRange(myEntity.GetPosition());
                planetCenter = planetEntity.GetPosition();

            }

            var prefab = MyDefinitionManager.Static.GetPrefabDefinition(installation.TerritoryInstallations._prefabName);
            if (prefab == null) return new Vector3D();

            Vector3D spawnCoords = new Vector3D(0, 0, 0);
            bool levelGround = false;

            int stepDistanceMin = 300;
            int stepDistanceMax = 1000;

            int stepDistanceUp = 3000;

            if (inGravity)
            {
                while (loops <= 5000)
                {
                    //if (loops == 5000) break;
                    if (loops % 100 == 0)
                    {
                        //loops = 0;
                        stepDistanceMin += 100;
                        stepDistanceMax += 100;
                    }

                    loops++;
                    Vector3D upDirPlayer = new Vector3D(0, 0, 0);
                    Vector3D randomDirPlayer = new Vector3D(0, 0, 0);
                    Vector3D roughSpawnArea = new Vector3D(0, 0, 0);
                    Vector3D forwardDir = new Vector3D(0, 0, 0);
                    Vector3D upDir = new Vector3D(0, 0, 0);

                    List<Vector3D> surfacePoints = new List<Vector3D>();
                    Vector3D[] corners = new Vector3D[8];
                    BoundingBoxD bb = prefab.BoundingBox;
                    double slopeThreshold = 4;

                    float gravityInterference = 0;
                    upDirPlayer = -MyAPIGateway.Physics.CalculateNaturalGravityAt(myEntity.GetPosition(), out gravityInterference);
                    /*forwardDir = Vector3D.CalculatePerpendicularVector(upDir);
                    MatrixD alignGravityMatrix = MatrixD.CreateWorld(bb.Center, forwardDir, upDir);

                    var obb = MyOrientedBoundingBoxD.Create(bb, alignGravityMatrix);
                    bb = obb.GetAABB*/

                    upDir = Vector3D.Normalize(myEntity.GetPosition() - planetCenter); //Get Player Up Direction By Comparing Planet center to Player Location
                    randomDirPlayer = MyUtils.GetRandomPerpendicularVector(ref upDirPlayer); //Using Player Up Direction as Reference, Get Random Direction Perpendicular from Player.
                    roughSpawnArea = randomDirPlayer * Session.Instance.rnd.Next((int)safeZoneExtension + stepDistanceMin , (int)safeZoneExtension + stepDistanceMax) + myEntity.GetPosition(); //Draw line from player in random direction we created above. Line distance is randomized based on min/max values in script header.
                    spawnCoords = planet.GetClosestSurfacePointGlobal(ref roughSpawnArea);

                    if (installation.Type == InstallationType.Radar)
                    {
                        randomDirPlayer = MyUtils.GetRandomPerpendicularVector(ref upDir); //Using Player Up Direction as Reference, Get Random Direction Perpendicular from Player.
                        roughSpawnArea = randomDirPlayer * Session.Instance.rnd.Next((int)safeZoneExtension + stepDistanceMin, (int)safeZoneExtension + stepDistanceMax) + myEntity.GetPosition(); //Draw line from player in random direction we created above. Line distance is randomized based on min/max values in script header.
                        var surface = planet.GetClosestSurfacePointGlobal(ref roughSpawnArea);
                        spawnCoords = upDir * stepDistanceUp + surface; //Draw a line upwards from the surface. This is where the pod will spawn.

                        /*float gravityInterference = 0;
                        MyAPIGateway.Physics.CalculateNaturalGravityAt(myEntity.GetPosition(), out gravityInterference);

                        if (gravityInterference > .9)
                        {
                            stepDistanceUp += 500;
                            continue;
                        }

                        if (gravityInterference < .05)
                        {
                            stepDistanceUp -= 500;
                            continue;
                        }*/
                    }

                    
                    if (Vector3D.Distance(spawnCoords, settings.BlockPos) > 5000 && installation.Type != InstallationType.Radar)
                    {
                        stepDistanceMin = 300;
                        stepDistanceMax = 1000;
                        continue;
                    }

                    bb.Translate(spawnCoords - bb.Center);

                    List<IMyEntity> entities = MyAPIGateway.Entities.GetTopMostEntitiesInBox(ref bb);
                    bool isblocked = false;
                    foreach (var entity in entities)
                    {
                        if (entity as IMyCubeGrid != null || entity as IMyCharacter != null)
                        {
                            isblocked = true;
                            break;
                        }
                    }

                    if (isblocked) continue;

                    bool toClose = false;
                    foreach(var config in settings.GetInstallations)
                    {
                        if (Vector3D.Distance(config.Coords, spawnCoords) < 1000)
                        {
                            toClose = true;
                            break;
                        }
                    }

                    if (toClose) continue;

                    if (installation.Type != InstallationType.Radar)
                    {
                        corners = bb.GetCorners();
                        surfacePoints.Clear();

                        for (int i = 0; i < corners.Length; i++)
                        {
                            if (corners[i] == null) continue;
                            var surface = planet.GetClosestSurfacePointGlobal(ref corners[i]);
                            surfacePoints.Add(surface);
                        }

                        List<double> dist = new List<double>();
                        foreach (var points in surfacePoints)
                        {
                            dist.Add(Math.Round(Vector3D.Distance(points, planetCenter), 2));
                        }

                        dist.Sort();
                        dist.Reverse();

                        if (dist[0] - dist[dist.Count - 1] > slopeThreshold) continue;
                        levelGround = true;
                        break;
                    }
                    else
                    {
                        levelGround = true;
                        break;
                    }
                        
                }

                if (levelGround)
                    return spawnCoords;
                else
                    return new Vector3D();
            }
            else
            {
                bool blocked = false;
                while (loops <= 100)
                {
                    loops++;
                    blocked = false;
                    var randomDir = Vector3D.Normalize(MyUtils.GetRandomVector3D());
                    spawnCoords = randomDir * Session.Instance.rnd.Next((int)safeZoneExtension + 300, (int)safeZoneExtension + 1000) + myEntity.GetPosition();

                    BoundingBoxD bb = prefab.BoundingBox;
                    bb.Translate(spawnCoords - bb.Center);
                    List<IMyEntity> entities = MyAPIGateway.Entities.GetTopMostEntitiesInBox(ref bb);
                    

                    foreach (var entity in entities)
                    {
                        if (entity as IMyCubeGrid != null || entity as IMyVoxelBase != null || entity as IMyCharacter != null)
                        {
                            blocked = true;
                            break;
                        }
                    }

                    if (blocked) continue;
                    break;
                }

                if (!blocked)
                    return spawnCoords;
                else
                    return new Vector3D();
            }
        }

        public static bool BuyInstallation(ClaimBlockSettings settings, Installations installation, bool isAdmin)
        {
            if (isAdmin) return true;
            MyFixedPoint tokensNeeded = (MyFixedPoint)installation.TerritoryInstallations._installationCost;
            if (tokensNeeded == 0) return true;

            MyDefinitionId tokenId;
            MyDefinitionId.TryParse(settings.TerritoryConfig._token, out tokenId);
            if (tokenId == null) return false;

            IMyInventory blockInv = settings.Block.GetInventory();
            if (blockInv == null) return false;

            MyFixedPoint tokens = blockInv.GetItemAmount(tokenId);

            if (tokens >= tokensNeeded)
                MyAPIGateway.Utilities.InvokeOnGameThread(() => blockInv.RemoveItemsOfType(tokensNeeded, tokenId));
            else { return false; }

            return true;
        }

        public static bool SpawnInstallation(ClaimBlockSettings settings, Installations installation, Vector3D spawnCoords)
        {
            var prefab = MyDefinitionManager.Static.GetPrefabDefinition(installation.TerritoryInstallations._prefabName);
            if (prefab == null) return false;

            var roughOB = prefab.CubeGrids[0];
            var cloneOB = roughOB.Clone();
            var gridOB = cloneOB as MyObjectBuilder_CubeGrid;

            IMyFaction npc = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.BlockOwner);
            if (npc == null) return false;

            for (int i = gridOB.CubeBlocks.Count - 1; i >= 0; i--)
            {
                var block = gridOB.CubeBlocks[i];
                block.Owner = npc.FounderId;
                block.BuiltBy = npc.FounderId;

                if (block.SubtypeName.Contains("Armor"))
                {
                    block.DeformationRatio = 1f;
                    //MyAPIGateway.Utilities.ShowNotification($"Deformation Ratio = {block.DeformationRatio}", 20000, "Red");
                }

                if (block.TypeId == typeof(MyObjectBuilder_Reactor) && (block.SubtypeName == "LargeBlockLargeGenerator" || block.SubtypeName == "ProprietaryLargeBlockLargeGenerator"))
                {
                    var id = block.GetId();

                    var replaceId = new MyDefinitionId();
                    MyDefinitionId.TryParse("MyObjectBuilder_Beacon/InstallationBeacon", out replaceId);

                    var targetBlockDef = MyDefinitionManager.Static.GetCubeBlockDefinition(id);
                    var newBlockDef = MyDefinitionManager.Static.GetCubeBlockDefinition(replaceId);
                    if (targetBlockDef == null || newBlockDef == null) continue;

                    var newBlockBuilder = MyObjectBuilderSerializer.CreateNewObject(newBlockDef.Id) as MyObjectBuilder_CubeBlock;
                    if (newBlockBuilder == null) continue;

                    newBlockBuilder.BlockOrientation = block.BlockOrientation;
                    newBlockBuilder.Min = block.Min;
                    newBlockBuilder.ColorMaskHSV = block.ColorMaskHSV;
                    newBlockBuilder.Owner = block.Owner;

                    gridOB.CubeBlocks.RemoveAt(i);
                    gridOB.CubeBlocks.Add(newBlockBuilder);
                } 
            }

            /*foreach (var block in gridOB.CubeBlocks)
            {
                if (block.TypeId == typeof(MyObjectBuilder_Beacon))
                {
                    block.SubtypeName = "InstallationBeacon";
                }

                block.Owner = npc.FounderId;
                block.BuiltBy = npc.FounderId;
                
                if (block.SubtypeName.Contains("Armor"))
                {
                    block.DeformationRatio = 1f;
                    //MyAPIGateway.Utilities.ShowNotification($"Deformation Ratio = {block.DeformationRatio}", 20000, "Red");
                }
            }*/

            IMyEntity planetEntity = null;
            bool inGravity = false;
            MyPlanet planet = MyGamePruningStructure.GetClosestPlanet(spawnCoords);
            if (planet != null)
            {
                planetEntity = planet as IMyEntity; //Get planet as Entity so we can get the position later.
                inGravity = planetEntity.Components.Get<MyGravityProviderComponent>().IsPositionInRange(spawnCoords);

            }

            Matrix matrix = settings.Block.WorldMatrix;
            Vector3D upDir = new Vector3D(0, 0, 0);
            Vector3D forDir = new Vector3D(0, 0, 0);

            if (inGravity)
            {
                float gravityInterference = 0;
                upDir = -MyAPIGateway.Physics.CalculateNaturalGravityAt(spawnCoords, out gravityInterference);
                forDir = Vector3D.CalculatePerpendicularVector(upDir);
            }
            else
            {
                upDir = matrix.Up;
                forDir = matrix.Forward;
            }
            

            gridOB.PositionAndOrientation = new MyPositionAndOrientation(spawnCoords, forDir, upDir);
            MyAPIGateway.Entities.RemapObjectBuilder(gridOB);

            MyCallbacks myCallbacks = new MyCallbacks(settings, installation, spawnCoords);
            IMyEntity ent = MyAPIGateway.Entities.CreateFromObjectBuilderParallel(gridOB, true, myCallbacks.InstallationSpawnCallback);
            //MyAPIGateway.Parallel.Sleep(2000);

            
            return true;
        }

        public static void SpawnResourceGrid(ClaimBlockSettings settings, Installations installation)
        {
            if (!installation.Enabled) return;
            IMyFaction npc = MyAPIGateway.Session.Factions.TryGetFactionByTag(settings.BlockOwner);
            if (npc == null) return;

            ResourceConfig resourceConfig = settings.TerritoryConfig._perkConfig._resourceConfig;
            ResearchConfig researchConfig = settings.TerritoryConfig._perkConfig._researchConfig;

            IMyEntity ent;
            MyAPIGateway.Entities.TryGetEntityById(installation.Type == InstallationType.Resource ? installation.ResourceGridId : installation.ResearchGridId, out ent);

            if (ent == null || Vector3D.Distance(ent.GetPosition(), installation.Coords) > 50)
            {
                var prefab = MyDefinitionManager.Static.GetPrefabDefinition(installation.Type == InstallationType.Resource ? resourceConfig._resourcePrefab : researchConfig._researchPrefab);
                if (prefab == null) return;

                var roughOB = prefab.CubeGrids[0];
                var cloneOB = roughOB.Clone();
                var gridOB = cloneOB as MyObjectBuilder_CubeGrid;
                BoundingBoxD bb = prefab.BoundingBox;

                foreach (var block in gridOB.CubeBlocks)
                {
                    block.Owner = npc.FounderId;
                    block.BuiltBy = npc.FounderId;
                }

                IMyEntity installationEnt;
                MyAPIGateway.Entities.TryGetEntityById(installation.GridEntityId, out installationEnt);
                if (installationEnt == null) return;

                IMyCubeGrid grid = installationEnt as IMyCubeGrid;
                if (grid == null) return;

                List<IMySlimBlock> blocks = new List<IMySlimBlock>();
                IMyShipConnector connector = null;
                grid.GetBlocks(blocks);

                foreach(var block in blocks)
                {
                    IMyCubeBlock cube = block.FatBlock;
                    if (cube == null) continue;

                    connector = cube as IMyShipConnector;
                    if (connector == null) continue;
                    break;
                }

                if (connector == null) return;
                Matrix matrix = connector.WorldMatrix;
                var offset = new Vector3D(0, 0, -2);
                var spawnarea = Vector3D.Transform(offset, matrix);

                bb.Translate(spawnarea - bb.Center);

                List<IMyEntity> entities = MyAPIGateway.Entities.GetTopMostEntitiesInBox(ref bb);
                bool isblocked = false;
                foreach (var entity in entities)
                {
                    //isblocked = true;
                    break;
                }

                if (isblocked) return;

                gridOB.PositionAndOrientation = new MyPositionAndOrientation(spawnarea, matrix.Backward, matrix.Up);
                MyAPIGateway.Entities.RemapObjectBuilder(gridOB);
                ent = MyAPIGateway.Entities.CreateFromObjectBuilderAndAdd(gridOB);
                //MyAPIGateway.Parallel.Sleep(1000);

                if (ent == null) return;
                if (installation.Type == InstallationType.Resource)
                    installation.ResourceGridId = ent.EntityId;
                else
                    installation.ResearchGridId = ent.EntityId;

                IMyCubeGrid cubeGrid = ent as IMyCubeGrid;
                if (cubeGrid == null) return;
                cubeGrid.ChangeGridOwnership(npc.FounderId, MyOwnershipShareModeEnum.All);
                cubeGrid.CustomName += " [NPC-IGNORE]";

                SpawnResource(settings, installation, ent);
            }
            else
            {
                SpawnResource(settings, installation, ent);
            }
        }

        public static void SpawnResource(ClaimBlockSettings settings, Installations installation, IMyEntity entity)
        {
            if (entity == null) return;
            IMyCubeGrid grid = entity as IMyCubeGrid;
            if (grid == null) return;

            ResourceConfig resourceConfig = settings.TerritoryConfig._perkConfig._resourceConfig;
            ResearchConfig researchConfig = settings.TerritoryConfig._perkConfig._researchConfig;

            List<IMyCargoContainer> containers = new List<IMyCargoContainer>();
            List<object> ballot = new List<object>();
            MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid).GetBlocksOfType(containers);
            if (containers.Count == 0)
            {
                grid.Close();

                if (installation.Type == InstallationType.Resource)
                    installation.ResourceGridId = 0;
                else
                    installation.ResearchGridId = 0;

                return;
            }

            foreach(var item in installation.Type == InstallationType.Resource ? resourceConfig._resourceItems : researchConfig._resourceItems)
            {
                int i = 0;
                while (i < item._frequency)
                {
                    ResourceItem resourceItem = new ResourceItem();
                    resourceItem = item;
                    ballot.Add(resourceItem);
                    i++;
                }
            }

            int roll = Session.Instance.rnd.Next(0, ballot.Count);
            ResourceItem _resourceItem = (ResourceItem)ballot[roll];
            int itemCount = Session.Instance.rnd.Next(_resourceItem._amountMin, _resourceItem._amountMax);
            MyDefinitionId defId;
            MyDefinitionId.TryParse(_resourceItem._resourceId, out defId);
            if (defId == null) return;

            foreach(var cargo in containers)
            {
                var inventory = cargo.GetInventory();
                MyInventory myInventory = inventory as MyInventory;
                if (myInventory == null) return;
                MyFixedPoint amountMFP = myInventory.ComputeAmountThatFits(defId);
                if (amountMFP == 0) continue;

                MyFixedPoint toAdd = itemCount < amountMFP ? itemCount : amountMFP;

                var content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(defId);
                MyObjectBuilder_InventoryItem inventoryItem = new MyObjectBuilder_InventoryItem { Amount = toAdd, Content = content };
                inventory.AddItems(toAdd, inventoryItem.Content);

                itemCount -= (int)amountMFP;
                if (itemCount <= 0) break;
            }
        }

        public static void AddInstallationPerk(ClaimBlockSettings settings, Installations installation)
        {
            //MyAPIGateway.Parallel.Sleep(2000);
            MyAPIGateway.Utilities.InvokeOnGameThread(() =>
            {
                if (installation.Type == InstallationType.SafeZone)
                {
                    RemoveSafeZone(settings);
                    AddSafeZone(settings, true);
                    return;
                }

                if (installation.Type == InstallationType.Radar)
                {
                    TagEnemyGrids(settings);
                    TagEnemyPlayers(settings);
                    return;
                }

                if (installation.Type == InstallationType.Drone)
                {
                    if (!AddMESPlayerKnownLocation(settings)) return;
                    return;
                }

                if (installation.Type == InstallationType.Production)
                {
                    AddProductionPerk(settings, null);
                    return;
                }

                if (installation.Type == InstallationType.Resource)
                {
                    installation.ResourceTimer = settings.TerritoryConfig._perkConfig._resourceConfig._secondsToSpawn;
                    return;
                }

                if (installation.Type == InstallationType.Research)
                {
                    installation.ResearchTimer = settings.TerritoryConfig._perkConfig._researchConfig._secondsToSpawn;
                    return;
                }

                return;
            });
        }

        public static void UpdateInstallationPerks()
        {
            foreach(var item in Session.Instance.claimBlocks.Values)
            {
                if (!item.Enabled || !item.IsClaimed) continue;

                foreach(var installation in item.GetInstallations)
                {
                    if (installation.Type == InstallationType.Drone && installation.Enabled)
                    {
                        AddInstallationPerk(item, installation);
                        break;
                    }
                }
            }
        }

        public static void RemoveInstallationPerk(ClaimBlockSettings settings, Installations installation)
        {
            if (!installation.Enabled) return;
            if (installation.Type == InstallationType.SafeZone)
            {
                RemoveSafeZone(settings);
                if (settings.IsClaimed)
                    AddSafeZone(settings, true);

                return;
            }

            if (installation.Type == InstallationType.Radar)
            {
                GPS.RemoveCachedGps(0, GpsType.Tag, settings);
                GPS.RemoveCachedGps(0, GpsType.Player, settings);
                return;
            }

            if (installation.Type == InstallationType.Drone)
            {
                RemoveMESPlayerKnownLocation(settings);
                return;
            }

            if (installation.Type == InstallationType.Production)
            {
                RemoveProductionPerk(settings, null);
                return;
            }

            if (installation.Type == InstallationType.Resource)
            {
                //if (!installation.Enabled) return;
                IMyEntity ent;
                if (!MyAPIGateway.Entities.TryGetEntityById(installation.ResourceGridId, out ent)) return;

                if (Vector3D.Distance(installation.Coords, ent.GetPosition()) < 500)
                    ent.Close();
            }

            if (installation.Type == InstallationType.Research)
            {
                //if (!installation.Enabled) return;
                IMyEntity ent;
                if (!MyAPIGateway.Entities.TryGetEntityById(installation.ResearchGridId, out ent)) return;

                if (Vector3D.Distance(installation.Coords, ent.GetPosition()) < 500)
                    ent.Close();
            }
        }

        public static void RemoveAllInstallations(ClaimBlockSettings settings)
        {
            if (settings == null) return;
            for (int i = settings.GetInstallations.Count - 1; i >= 0; i--)
            {
                IMyEntity entity;
                if (!MyAPIGateway.Entities.TryGetEntityById(settings.GetInstallations[i].GridEntityId, out entity))
                {

                }
                else
                {
                    entity.Close();
                }

                RemoveInstallationPerk(settings, settings.GetInstallations[i]);
                settings.InstallationRemove(settings.GetInstallations[i]);
            }
        }

        public static void AddAllInstallations(ClaimBlockSettings settings)
        {
            if (settings == null) return;
            foreach(var item in settings.TerritoryConfig._territoryInstallations)
            {
                Installations installation = new Installations()
                {
                    _type = item._installationType,
                    _territoryInstallation = item,
                    _blockEntityId = 0,
                    _coords = new Vector3D(),
                    _damageNotificationCooldown = 0,
                    _enabled = false,
                    _gridEntityId = 0,
                    _integrity = 0,
                    _integrityWarning = false,
                    _parentId = settings.EntityId,
                    _rebuildCooldown = 0,
                    _resourceGridId = 0,
                    _resourceTimer = 0,
                    _researchTimer = 0,
                    _turrets = new List<IMyTerminalBlock>()
                };

                settings.InstallationAdd(installation);
            }
        }

        public static void SetBlockConfigs(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;
            settings.UnclaimName = settings.TerritoryConfig._territoryName;
            settings.DiscordRoleId = settings.TerritoryConfig._discordRoleId;
            settings.BlockOwner = settings.TerritoryConfig._npcTag;

            if (settings.TerritoryConfig._territoryOptions._centerOnPlanet)
            {
                MyPlanet planet = MyGamePruningStructure.GetClosestPlanet(settings.BlockPos);
                if (planet == null) return;

                settings.PlanetCenter = planet.PositionComp.WorldAABB.Center;
                settings.PlanetName = planet.Generator.Id.SubtypeName;
            }
            else
            {
                settings.PlanetCenter = new Vector3D();
                settings.PlanetName = " ";
            }
        }

        public static bool AddMESPlayerKnownLocation(ClaimBlockSettings settings)
        {
            Installations installation = settings.GetInstallationByType(InstallationType.Drone);
            if (installation == null) return false;

            Session.Instance.MES.AddKnownPlayerLocation(settings.BlockPos, settings.BlockOwner, settings.TerritoryConfig._perkConfig._droneConfig._radius, 99999, settings.TerritoryConfig._perkConfig._droneConfig._maxSpawns, settings.TerritoryConfig._perkConfig._droneConfig._minThreat);
            return true;
        }

        public static void RemoveMESPlayerKnownLocation(ClaimBlockSettings settings)
        {
            Session.Instance.MES.RemoveKnownPlayerLocation(settings.BlockPos, "", true);
        }

        public static void GetInstallationTurrets(Installations installation, IMyCubeGrid grid)
        {
            if (installation == null || grid == null) return;

            blocks.Clear();
            grid.GetBlocks(blocks);

            foreach(var block in blocks)
            {
                IMyCubeBlock cubeBlock = block.FatBlock;
                if (cubeBlock == null) continue;

                if (cubeBlock as IMyUserControllableGun != null || cubeBlock as IMyConveyorSorter != null)
                {
                    CheckAmmo(cubeBlock as IMyTerminalBlock);
                    cubeBlock.OnClose += Events.OnInstallationTurretsClosing;
                    installation.AddTurret(cubeBlock as IMyTerminalBlock);
                }
            }
        }

        public static void CheckAmmo(IMyTerminalBlock gun)
        {
            if (!gun.IsWorking) return;
            IMyFaction gunFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(gun.OwnerId);
            IMyFaction gridFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(gun.CubeGrid.BigOwners.FirstOrDefault());
            if (gunFaction == null || gridFaction == null) return;
            if (!gunFaction.IsEveryoneNpc() || gunFaction.Tag.Length <= 3 || !gridFaction.IsEveryoneNpc() || gridFaction.Tag.Length <= 3) return;

            var inventory = gun.GetInventory();
            MyInventory myInventory = inventory as MyInventory;
            if (myInventory == null) return;

            if (myInventory.Empty())
            {
                if (!myInventory.IsConstrained) return;
                var constraint = myInventory.Constraint;
                if (constraint == null) return;

                HashSetReader<MyDefinitionId> ids = constraint.ConstrainedIds;
                MyFixedPoint amountMFP = myInventory.ComputeAmountThatFits(ids.First());
                if (amountMFP <= 0) return;

                var content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(ids.First());
                MyObjectBuilder_InventoryItem inventoryItem = new MyObjectBuilder_InventoryItem { Amount = amountMFP, Content = content };
                inventory.AddItems(amountMFP, inventoryItem.Content);
            }
        }

        public static void RefreshConfig()
        {
            foreach (var claimSettings in Session.Instance.claimBlocks.Values)
            {
                for (int j = 0; j < Session.Instance.territoryConfigs.Count; j++)
                {
                    if (Session.Instance.config._config[j]._territoryName == claimSettings.TerritoryConfig._territoryName)
                    {
                        claimSettings.TerritoryConfig = Session.Instance.config._config[j];

                        for (int i = 0; i < claimSettings.GetInstallations.Count; i++)
                        {
                            foreach (var installationConfig in Session.Instance.config._config[j]._territoryInstallations)
                            {
                                if (claimSettings.GetInstallations[i].Type == installationConfig._installationType)
                                {
                                    claimSettings.GetInstallations[i].TerritoryInstallations = installationConfig;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static float RandomFloat(float minValue, float maxValue)
        {
            var minInflatedValue = (float)Math.Round(minValue, 3) * 1000;
            var maxInflatedValue = (float)Math.Round(maxValue, 3) * 1000;
            var randomValue = (float)Rnd.Next((int)minInflatedValue, (int)maxInflatedValue) / 1000;
            return randomValue;

        }
    }
}
