using Sandbox.Game;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;

namespace Faction_TerritoriesV2
{
    public static class Counters
    {
        public static void IsClaimingCounter(ClaimBlockSettings settings)
        {
            settings.Timer--;
            if (!Utils.CheckPlayerandBlock(settings))
            {
                if (settings.RecoveryTimer == 0)
                {
                    IMyPlayer playerClaim = Triggers.GetPlayerFromId(settings.PlayerClaimingId);
                    if (playerClaim != null)
                        MyVisualScriptLogicProvider.SendChatMessageColored($"WARNING - Return to range of claim area within 60 seconds or claiming will fail", Color.Violet, "[Faction Territories]", settings.PlayerClaimingId, "Red");
                }

                settings.RecoveryTimer++;
                if (settings.RecoveryTimer == 60)
                {
                    IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(settings.PlayerClaimingId);
                    if (faction != null)
                        new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"({faction.Tag}) Failed To Claim: {settings.UnclaimName}", "[Faction Territories]", Color.Red);

                    Utils.ResetClaim(settings);
                }
            }
            else
            {
                settings.RecoveryTimer = 0;
            }

            if (settings.Timer % 60 == 0 && settings.Timer != 0)
            {
                var pos = settings.BlockPos;
                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(settings.PlayerClaimingId);

                if (faction != null)
                    new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"({faction.Tag}) Claiming Territory: {settings.UnclaimName} - X:{Math.Ceiling((decimal)pos.X)}, Y:{Math.Ceiling((decimal)pos.Y)}, Z:{Math.Ceiling((decimal)pos.Z)} with {TimeSpan.FromSeconds(settings.Timer)} to claim", "[Faction Territories]", Color.Red);
            }

            if (settings.Timer == 3)
            {
                Comms.SyncParticleEffect("Claimed", settings.BlockPos);
            }

            if (settings.Timer <= 0)
            {
                if (!Utils.CheckPlayerandBlock(settings))
                {
                    IMyFaction factionA = MyAPIGateway.Session.Factions.TryGetPlayerFaction(settings.PlayerClaimingId);
                    if (factionA != null)
                        new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"({factionA.Tag}) Failed To Claim: {settings.UnclaimName}", "[Faction Territories]", Color.Red);

                    Utils.ResetClaim(settings);
                    return;
                }

                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(settings.PlayerClaimingId);

                settings.RecoveryTimer = 0;
                settings.IsClaiming = false;
                settings.IsClaimed = true;
                settings.ClaimZoneName = settings.UnclaimName;
                settings.Timer = settings.TerritoryConfig._territoryMaintenance._consumptionTime;
                settings.BlockEmissive = EmissiveState.Claimed;
                if (faction != null)
                {
                    settings.ClaimedFaction = faction.Tag;
                    settings.FactionId = faction.FactionId;

                    var icon = faction.FactionIcon;
                    Utils.SetScreen(settings.Block as IMyBeacon, faction, true);
                }

                Utils.SetOwner(settings.Block, settings);
                Utils.AddAllInstallations(settings);
                Utils.SetRelationWithNpc(settings, MyRelationsBetweenFactions.Friends);
                Utils.AddSafeZone(settings);
                Utils.GetSurroundingSafeZones(settings);
                Utils.TagEnemyGrids(settings);
                Utils.TagEnemyPlayers(settings);
                Utils.StopHandTools();
                //Utils.AddPerks(settings);
                GPS.UpdateBlockText(settings, $"Claimed Territory: {settings.ClaimZoneName} ({settings.ClaimedFaction})");
                settings.GetTerritoryStatus = TerritoryStatus.Claimed;

                var pos = settings.BlockPos;
                if (faction != null)
                {
                    new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"({faction.Tag}) Claimed Territory: {settings.UnclaimName} - X:{Math.Ceiling((decimal)pos.X)}, Y:{Math.Ceiling((decimal)pos.Y)}, Z:{Math.Ceiling((decimal)pos.Z)}", "[Faction Territories]", Color.Red);
                    new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"WARNING - Claimed Territory: {settings.ClaimZoneName} is out of tokens, {TimeSpan.FromSeconds(settings.TerritoryConfig._territoryMaintenance._consumptionTime)} until territory is unclaimed", "[Faction Territories]", Color.Red);
                }
            }
        }

        public static void IsClaimedCounter(ClaimBlockSettings settings)
        {
            if (!settings.IsSieging)
                settings.Timer--;

            if (settings.Timer <= 0)
            {
                if (!Utils.ConsumeToken(settings))
                {
                    Utils.RemoveSafeZone(settings);
                    Utils.ResetClaim(settings);
                    return;
                }

                settings.Timer = settings.TerritoryConfig._territoryMaintenance._consumptionTime;
            }

            if (!settings.IsSieging)
            {
                foreach (var installation in settings.GetInstallations)
                {
                    if (!installation.Enabled && installation.RebuildCooldown > 0)
                        installation.RebuildCooldown--;

                    if (installation.Type == InstallationType.Resource)
                    {
                        if (installation.Enabled && installation.ResourceTimer > 0)
                            installation.ResourceTimer--;
                        else if (installation.Enabled && installation.ResourceTimer <= 0)
                        {
                            Utils.SpawnResourceGrid(settings, installation);
                            installation.ResourceTimer = settings.TerritoryConfig._perkConfig._resourceConfig._secondsToSpawn;
                        }

                        continue;
                    }

                    if (installation.Type == InstallationType.Research)
                    {
                        if (installation.Enabled && installation.ResearchTimer > 0)
                            installation.ResearchTimer--;
                        else if (installation.Enabled && installation.ResearchTimer <= 0)
                        {
                            Utils.SpawnResourceGrid(settings, installation);
                            installation.ResearchTimer = settings.TerritoryConfig._perkConfig._researchConfig._secondsToSpawn;
                        }

                        continue;
                    }
                }
            }

            settings.DetailInfo = Utils.GetCounterDetails(settings);
            GPS.ValidateGps(settings);
        }

        public static void IsSiegingCounter(ClaimBlockSettings settings)
        {
            settings.SiegeTimer--;
            if (!Utils.CheckPlayerandBlock(settings))
            {
                if (settings.RecoveryTimer == 0)
                {
                    IMyPlayer playerSiege = Triggers.GetPlayerFromId(settings.PlayerSiegingId);
                    if (playerSiege != null)
                        MyVisualScriptLogicProvider.SendChatMessageColored($"WARNING - Return to range of siege area within 60 seconds or sieging will fail", Color.Violet, "[Faction Territories]", settings.PlayerSiegingId, "Red");
                }

                settings.RecoveryTimer++;
                if (settings.RecoveryTimer == 60)
                {
                    IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(settings.PlayerSiegingId);
                    if (faction != null)
                    {
                        if (!settings.IsSieged)
                            new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"({faction.Tag}) Failed To Siege: {settings.ClaimZoneName}", "[Faction Territories]", Color.Red);
                        else
                            new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"({faction.Tag}) Failed To Siege: {settings.ClaimZoneName} - Siege Cooldown Started {TimeSpan.FromSeconds(settings.TerritoryConfig._siegingConfig._siegeCooldown)}", "[Faction Territories]", Color.Red);
                    }

                    settings.SiegeTimer = settings.TerritoryConfig._siegingConfig._siegeCooldown;
                    settings.IsSiegeCooling = true;
                    Utils.ResetSiegeData(settings);
                    settings.GetTerritoryStatus = TerritoryStatus.FailedSiegeCooldown;
                    return;
                }
            }
            else
            {
                settings.RecoveryTimer = 0;
            }

            if ((settings.SiegeTimer % (10 * 60) == 0 && settings.SiegeTimer != 0) || settings.SiegeTimer == 60)
            {
                var pos = settings.BlockPos;
                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(settings.PlayerSiegingId);

                if (faction != null)
                    new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"({faction.Tag}) Sieging Territory: {settings.ClaimZoneName} - X:{Math.Ceiling((decimal)pos.X)}, Y:{Math.Ceiling((decimal)pos.Y)}, Z:{Math.Ceiling((decimal)pos.Z)} with {TimeSpan.FromSeconds(settings.SiegeTimer)} to siege", "[Faction Territories]", Color.Red);
                else
                {
                    IMyPlayer playerSiege = Triggers.GetPlayerFromId(settings.PlayerSiegingId);
                    new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"({playerSiege?.DisplayName}) Sieging Territory: {settings.ClaimZoneName} - X:{Math.Ceiling((decimal)pos.X)}, Y:{Math.Ceiling((decimal)pos.Y)}, Z:{Math.Ceiling((decimal)pos.Z)} with {TimeSpan.FromHours(settings.SiegeTimer)} to siege", "[Faction Territories]", Color.Red);
                }
            }

            if (settings.SiegeTimer <= 0)
            {
                if (!Utils.CheckPlayerandBlock(settings))
                {
                    IMyFaction factionA = MyAPIGateway.Session.Factions.TryGetPlayerFaction(settings.PlayerSiegingId);
                    if (factionA != null)
                        new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"({factionA.Tag}) Failed To Siege: {settings.ClaimZoneName}", "[Faction Territories]", Color.Red);

                    settings.SiegeTimer = settings.TerritoryConfig._siegingConfig._siegeCooldown;
                    settings.IsSiegeCooling = true;
                    Utils.ResetSiegeData(settings);
                    settings.GetTerritoryStatus = TerritoryStatus.FailedSiegeCooldown;
                    return;
                }

                IMyFaction factionB = MyAPIGateway.Session.Factions.TryGetPlayerFaction(settings.PlayerSiegingId);

                if (factionB != null)
                    new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"({factionB.Tag}) Successfully Sieged: {settings.ClaimZoneName} - Territory can be claimed for {TimeSpan.FromSeconds(settings.TerritoryConfig._territoryOptions._cooldown)}", "[Faction Territories]", Color.Red);

                Utils.SetRelationWithNpc(settings, MyRelationsBetweenFactions.Enemies);
                settings.RecoveryTimer = 0;
                settings.IsSieged = true;
                settings.IsSieging = false;
                settings.IsClaimed = false;
                settings.IsCooling = true;
                settings.ClaimedFaction = "";
                settings.FactionId = 0;
                settings.Timer = settings.TerritoryConfig._territoryOptions._cooldown;
                settings.BlockEmissive = EmissiveState.Sieged;
                Utils.RemoveSafeZone(settings);
                Utils.RemoveAllInstallations(settings);
                GPS.UpdateBlockText(settings, $"Territory Cooldown: {settings.UnclaimName} - Unclaimable while cooling");
                settings.GetTerritoryStatus = TerritoryStatus.CooldownToNeutral;

                Utils.MonitorSafeZonePBs(settings, true);
                Comms.SyncParticleEffect("Claimed", settings.BlockPos);
            }
        }

        /*public static void IsSiegedCounter(ClaimBlockSettings settings)
        {
            settings.SiegeTimer--;
            if (settings.SiegeTimer == 0 && settings.IsSieged && !settings.ReadyToSiege)
            {
                settings.SiegeTimer = settings.TimeframeToSiege;
                settings.ReadyToSiege = true;
                new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"Territory Sieged Init: {settings.UnclaimName} - Territory is ready to be sieged by ({settings.SiegedBy}), time left to final siege {TimeSpan.FromSeconds(settings.SiegeTimer)}", "[Faction Territories]", Color.Red);
                //new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"Territory Sieged Init: {settings.UnclaimName} - Territory is ready to be sieged by ({settings.SiegedBy}), time left to final siege {TimeSpan.FromSeconds(settings.SiegeTimer)}", "[Faction Territories]", Color.Red);
                //Utils.RemoveSafeZone(settings);
                //Utils.ResetClaim(settings);
                //new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"Territory is now unclaimed: {settings.UnclaimName}", "[Faction Territories]", Color.Red);
                //return;
            }

            if (settings.ReadyToSiege)
            {
                if (settings.SiegeTimer % 300 == 0 && settings.SiegeTimer != 0)
                {
                    new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"Territory Sieged Init: {settings.UnclaimName} - Territory is ready to be sieged by ({settings.SiegedBy}), time left to final siege {TimeSpan.FromSeconds(settings.SiegeTimer)}", "[Faction Territories]", Color.Red);
                }

                if (settings.SiegeTimer == 0)
                {
                    Utils.ResetSiegeData(settings, false);
                    new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"Territory Siege Failed: {settings.UnclaimName} - Territory siege has been reset", "[Faction Territories]", Color.Red);

                    return;
                }

                //settings.DetailInfo = $"\n[Territory Can Be Final Sieged For]:\n{TimeSpan.FromSeconds(settings.SiegeTimer)}\n";
            }

            if (settings.IsSieged && !settings.ReadyToSiege && !settings.IsSiegingFinal)
            {
                if (settings.SiegeTimer % 3600 == 0)
                {
                    new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"Territory Sieged Init: {settings.UnclaimName} - Territory has been initial sieged, time until it can be final sieged {TimeSpan.FromSeconds(settings.SiegeTimer)}", "[Faction Territories]", Color.Red);
                }

                //settings.DetailInfo = $"\n[Time Until Territory Can Be Final Sieged]:\n{TimeSpan.FromSeconds(settings.SiegeTimer)}\n";
                //settings.DetailInfo += $"\n[Siege Time Extended]: {settings.HoursToDelay * settings.SiegedDelayedHit} hrs ({settings.SiegedDelayedHit}/{settings.SiegeDelayAllow}) used\n";
            }
        }*/

        public static void IsCoolingCounter(ClaimBlockSettings settings)
        {
            settings.Timer--;
            if (settings.Timer <= 0)
            {
                Utils.ResetClaim(settings);
                new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"Territory: {settings.ClaimZoneName} - Cooldown timer has expired, territory can now be claimed", "[Faction Territories]", Color.Red);
            }
        }

        public static void IsSiegeCoolingCounter(ClaimBlockSettings settings)
        {
            settings.SiegeTimer--;
            if (settings.SiegeTimer <= 0)
            {
                settings.IsSiegeCooling = false;
                settings.GetTerritoryStatus = TerritoryStatus.Claimed;
                new ModMessage(settings.DiscordRoleId, settings.UnclaimName, $"Territory {settings.ClaimZoneName}: Siege cooldown has expired and can now be init sieged", "[Faction Territories]", Color.Red);
            }
        }

        public static void CheckSafeZoneDelay()
        {
            foreach (var item in Session.Instance.claimBlocks.Values)
            {
                if (!item.Enabled || !item.IsClaimed) continue;
                if (item.GetZonesDelayRemove.Count == 0) continue;

                for (int i = item.GetZonesDelayRemove.Count - 1; i >= 0; i--)
                {
                    var zoneData = item.GetZonesDelayRemove[i];
                    IMySafeZoneBlock zoneBlock = null;
                    Session.Instance.safeZoneBlocks.TryGetValue(zoneData.zoneId, out zoneBlock);
                    if (zoneBlock == null) continue;

                    var elapsed = DateTime.Now - zoneData.time;
                    TimeSpan totalTime = new TimeSpan(1, 0, 0, 0);
                    TimeSpan timeLeft = totalTime.Subtract(elapsed);
                    if (timeLeft <= TimeSpan.Zero)
                    {
                        item.UpdateZonesDelayRemove(zoneData.zoneId, DateTime.Now, false);
                        //item.UpdateSafeZoneBlocks(zoneData.zoneId, true);
                        //zoneBlock.EnableSafeZone(false);
                        var prop = zoneBlock.GetProperty("SafeZoneCreate");
                        var prop2 = prop as ITerminalProperty<bool>;

                        if (prop2 != null)
                        {
                            prop2.SetValue(zoneBlock, false);
                        }
                        continue;
                    }

                    if (!zoneBlock.IsSafeZoneEnabled())
                    {
                        item.UpdateZonesDelayRemove(zoneData.zoneId, DateTime.Now, false);
                        //item.UpdateSafeZoneBlocks(zoneData.zoneId, true);
                    }
                }

            }
        }
    }
}
