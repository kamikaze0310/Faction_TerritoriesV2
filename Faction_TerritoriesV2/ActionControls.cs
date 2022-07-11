using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;

namespace Faction_TerritoriesV2
{
    public static class ActionControls
    {
        private static string BlockSubtype = "ClaimBlock";
        private static bool ValidTokens = false;
        private static bool EnemyNearby = true;
        private static bool InClaimRange = false;
        private static bool ShowErrors = false;
        private static bool IsBeingClaimed = false;
        private static bool ValidEnergy = false;
        private static long ClaimBlockId = 0;
        private static bool IsInFaction = false;
        private static bool IsPlayerNear = false;
        private static bool InSiegeRange = false;
        private static bool IsBeingSieged = false;
        private static bool AlreadySieged = false;
        private static bool IsStatic = false;
        //private static bool InVoxel = false;
        private static bool IsUnderground = false;
        private static string factionSelected = "";
        private static bool ReadyToSiege = false;
        private static bool IsCoolingError = false;
        private static bool IsSiegeCoolingError = false;
        private static string SelectedInstallation = ""; 
        //public static IMyTerminalControlOnOffSwitch safeZoneSwitch = null;

        //public static int inVoxelDelay = 0;

        private static Dictionary<MyTerminalControlComboBoxItem, TerritoryConfig> ConfigPairs = new Dictionary<MyTerminalControlComboBoxItem, TerritoryConfig>();

        public static void CreateControlsBeaconNew(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (block as IMyBeacon != null)
            {
                Controls.CreateBeaconControls(block, controls);
            }
        }

        public static void CreateControlsJumpdriveNew(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (block as IMyJumpDrive != null)
            {
                Controls.CreateJumpdriveControls(block, controls);
            }
        }

        public static void CreateActionsJumpdriveNew(IMyTerminalBlock block, List<IMyTerminalAction> actions)
        {
            if (block as IMyJumpDrive != null)
            {
                Controls.CreateJumpdriveActions(block, actions);
            }
        }

        public static bool HideVanilla(IMyTerminalBlock block, IMyTerminalControl control)
        {
            if (block as IMyBeacon != null)
            {
                if (block.BlockDefinition.SubtypeName.Contains(BlockSubtype)) return false;
            }

            return true;
        }

        public static void GetTerritoryConfigs(List<MyTerminalControlComboBoxItem> listItems)
        {
            ConfigPairs.Clear();
            int key = 1;
            var dummy = new MyTerminalControlComboBoxItem();
            TerritoryConfig territoryConfig = new TerritoryConfig();
            dummy.Key = 0;
            dummy.Value = MyStringId.GetOrCompute(territoryConfig._territoryName);
            listItems.Add(dummy);
            ConfigPairs.Add(dummy, territoryConfig);

            foreach (var config in Session.Instance.territoryConfigs)
            {
                dummy = new MyTerminalControlComboBoxItem();

                dummy.Key = key;
                dummy.Value = MyStringId.GetOrCompute(config._territoryName);
                listItems.Add(dummy);
                ConfigPairs.Add(dummy, config);
                key++;
            }
        }

        public static long GetSelectedConfig(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 0;

            foreach(var config in ConfigPairs.Keys)
            {
                if (config.Value.ToString() == settings.TerritoryConfig._territoryName)
                    return config.Key;
            }

            return 0;
        }

        public static void SetSelectedConfig(IMyTerminalBlock block, long value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            foreach(var config in ConfigPairs.Keys)
            {
                if (config.Key == (int)value)
                {
                    settings.TerritoryConfig = ConfigPairs[config];
                    break;
                }
            }

            RefreshControls(block);
        }

        /*public static bool IsPerkControls(IMyTerminalBlock block, PerkTypeList perkType = PerkTypeList.Production)
        {
            if (!IsClaimAndAdmin(block)) return false;
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            return settings.UI == UIControls.Perks;
        }

        public static bool IsPerkType(IMyTerminalBlock block, PerkTypeList perkType)
        {
            if (!IsClaimAndAdmin(block)) return false;
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;
            if (!IsPerkControls(block)) return false;

            return settings.UIPerkList == perkType;
        }

        public static bool IsMiscControls(IMyTerminalBlock block)
        {
            if (!IsClaimAndAdmin(block)) return false;
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            return settings.UI == UIControls.Misc;
        }

        public static bool IsClaimingControls(IMyTerminalBlock block)
        {
            if (!IsClaimAndAdmin(block)) return false;
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            return settings.UI == UIControls.Claiming;
        }

        public static bool IsTerritoryOptionsControls(IMyTerminalBlock block)
        {
            if (!IsClaimAndAdmin(block)) return false;
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            return settings.UI == UIControls.TerritoryOptions;
        }

        public static bool IsAllowToolsEnabled(IMyTerminalBlock block)
        {
            if (!IsClaimAndAdmin(block)) return false;
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            return !settings.AllowTools;
        }

        public static bool GetCenterToggle(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            return settings.CenterToPlanet;
        }

        public static void SetCenterToggle(IMyTerminalBlock block, bool value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.CenterToPlanet = value;
            if (!value)
            {
                settings.PlanetName = "";
                RefreshControls(block);
                return;
            }

            MyPlanet planet = MyGamePruningStructure.GetClosestPlanet(settings.BlockPos);
            if (planet == null) return;

            settings.PlanetCenter = planet.PositionComp.WorldAABB.Center;
            settings.PlanetName = planet.Generator.Id.SubtypeName;
            RefreshControls(block);
        }

        public static bool IsCenterToPlanetEnabled(IMyTerminalBlock block)
        {
            if (!IsTerritoryOptionsControls(block)) return false;
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            return settings.CenterToPlanet;
        }

        public static MyStringId GetPlanetName(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return MyStringId.GetOrCompute("");

            if (!settings.CenterToPlanet)
                return MyStringId.GetOrCompute("");
            else
                return MyStringId.GetOrCompute(settings.PlanetName);
        }

        /*public static bool GetAdminSafeZoneAllies(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            return settings.AdminAllowSafeZoneAllies;
        }

        public static void SetAdminSafeZoneAllies(IMyTerminalBlock block, bool value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.AdminAllowSafeZoneAllies = value;
            if (!value)
                settings.AllowSafeZoneAllies = false;
            else
                settings.AdminAllowTerritoryAllies = true;

            RefreshControls(block);
        }

        public static bool GetAdminTerritoryAllies(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            return settings.AdminAllowTerritoryAllies;
        }

        public static void SetAdminTerritoryAllies(IMyTerminalBlock block, bool value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.AdminAllowTerritoryAllies = value;
            if (!value)
                settings.AllowTerritoryAllies = false;

            RefreshControls(block);
        }

        public static bool GetAllowTools(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            return settings.AllowTools;
        }

        public static void SetAllowTools(IMyTerminalBlock block, bool value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.AllowTools = value;
            RefreshControls(block);
        }

        public static bool GetAllowDrilling(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            return settings.AllowDrilling;
        }

        public static void SetAllowDrilling(IMyTerminalBlock block, bool value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.AllowDrilling = value;
        }

        public static bool GetAllowWelding(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            return settings.AllowWelding;
        }

        public static void SetAllowWelding(IMyTerminalBlock block, bool value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.AllowWelding = value;
        }

        public static bool GetAllowGrinding(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            return settings.AllowGrinding;
        }

        public static void SetAllowGrinding(IMyTerminalBlock block, bool value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.AllowGrinding = value;
        }

        public static bool IsSiegeControls(IMyTerminalBlock block)
        {
            if (!IsClaimAndAdmin(block)) return false;
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            return settings.UI == UIControls.Sieging;
        }*/

        public static bool IsClaimBlock(IMyTerminalBlock block)
        {
            if (block as IMyBeacon != null)
            {
                if (block.BlockDefinition.SubtypeName.Contains(BlockSubtype)) return true;
            }

            return false;
        }

        public static bool IsClaimAndAdmin(IMyTerminalBlock block)
        {
            if (!IsClaimBlock(block)) return false;
            if (!IsAdmin(block)) return false;

            return true;
        }

        public static bool IsAdmin(IMyTerminalBlock block)
        {
            if (!IsClaimBlock(block)) return false;
            IMyPlayer player = MyAPIGateway.Session.LocalHumanPlayer;
            if (player.PromoteLevel == MyPromoteLevel.Owner || player.PromoteLevel == MyPromoteLevel.Admin) return true;
            return false;
        }

        /*public static bool IsAdminValidItem(IMyTerminalBlock block)
        {
            if (!IsAdmin(block)) return false;
            if (IsConsumptionItemValid(block)) return false;

            return true;
        }*/

        public static bool IsAdminAndEnabled(IMyTerminalBlock block)
        {
            if (!IsClaimBlock(block)) return false;
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            if (settings.Enabled) return false;
            IMyPlayer player = MyAPIGateway.Session.LocalHumanPlayer;
            if (player.PromoteLevel == MyPromoteLevel.Owner || player.PromoteLevel == MyPromoteLevel.Admin)
            {
                if (!settings.Enabled) return true;
            }

            return false;
        }

        public static bool IsAdminAndCheckEnabled(IMyTerminalBlock block)
        {
            if (!IsClaimBlock(block)) return false;
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            IMyPlayer player = MyAPIGateway.Session.LocalHumanPlayer;
            if (player.PromoteLevel == MyPromoteLevel.Owner || player.PromoteLevel == MyPromoteLevel.Admin)
            {
                if (!settings.Enabled) return false;
                else return true;
            }

            return false;
        }

        public static bool IsFactionSelected(IMyTerminalBlock block)
        {
            if (!IsAdminAndCheckEnabled(block)) return false;
            if (string.IsNullOrEmpty(factionSelected)) return false;
            if (factionSelected.Contains("-Select Faction Below-")) return false;

            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;
            if (settings.IsClaimed) return false;

            return true;
        }

        public static bool GetSwitchState(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            return settings.Enabled;
        }

        public static void SetSwitchState(IMyTerminalBlock block, bool state)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;
            settings.Enabled = state;

            if (state)
            {
                //Triggers.CreateNewTriggers(block as IMyBeacon);
                Utils.SetBlockConfigs(block);
                Comms.SendTriggerToServer(block, settings);
                RefreshControls(block);
                settings.BlockEmissive = EmissiveState.Online;
            }
            else
            {
                //Triggers.RemoveTriggerData(block.EntityId);
                settings.TriggerInit = false;
                Comms.SendRemoveTriggerToServer(block);
                RefreshControls(block);
                settings.BlockEmissive = EmissiveState.Offline;
            }
        }

        public static void ResetTerritory(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;
            if (!settings.Enabled) return;

            Comms.SendResetToServer(settings);
        }

        public static void GetInstallationList(IMyTerminalBlock block, List<MyTerminalControlListBoxItem> listItems, List<MyTerminalControlListBoxItem> selectedItems)
        {
            if (!IsClaimBlock(block)) return;
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            string objectText = "abc";
            /*var dummy = new MyTerminalControlListBoxItem(MyStringId.GetOrCompute("-Select Installation Below-"), MyStringId.GetOrCompute("-Select Installation Below-"), objectText);
            listItems.Add(dummy);

            if (SelectedInstallation == "-Select Installation Below-")
                selectedItems.Add(dummy);*/

            foreach (var item in settings.GetInstallations)
            {
                //if (item.Enabled || item.RebuildCooldown > 0) continue;

                var toList = new MyTerminalControlListBoxItem(MyStringId.GetOrCompute($"{item.Type} - {item.TerritoryInstallations._installationCost} Token(s)"), MyStringId.GetOrCompute(GetTooltip(item)), objectText);
                if (SelectedInstallation.Contains(item.Type.ToString()))
                    selectedItems.Add(toList);

                listItems.Add(toList);
            }
        }

        public static void SetSelectedInstallation(IMyTerminalBlock block, List<MyTerminalControlListBoxItem> listItems)
        {
            if (listItems.Count == 0) return;

            SelectedInstallation = listItems[0].Text.ToString();
            RefreshControls(block);
        }

        public static string GetTooltip(Installations installation)
        {
            if (installation.Type == InstallationType.Drone)
                return "Installation that spawns AI drones to help defend your territory";
            if (installation.Type == InstallationType.Production)
                return "Installation that increases production speed, yield, power";
            if (installation.Type == InstallationType.Radar)
                return "Installation that adds gps locations on enemies";
            if (installation.Type == InstallationType.Research)
                return "Installation that will slowly generate random research components";
            if (installation.Type == InstallationType.Resource)
                return "Installation that will slowly genereate random resources";
            if (installation.Type == InstallationType.SafeZone)
                return "Installation that extends the safe zone";

            return "";
        }

        public static void GetFactionList(IMyTerminalBlock block, List<MyTerminalControlListBoxItem> listItems, List<MyTerminalControlListBoxItem> selectedItems)
        {
            if (!IsClaimAndAdmin(block)) return;
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;
            //if (settings.UI != UIControls.Misc) return;

            List<string> factionNames = new List<string>();
            var factions = MyAPIGateway.Session.Factions.Factions;
            foreach (var faction in factions.Values)
            {
                if (faction.IsEveryoneNpc()) continue;
                factionNames.Add(faction.Name);
            }

            factionNames.Sort();

            string objectText = "abc";
            var dummy = new MyTerminalControlListBoxItem(MyStringId.GetOrCompute("-Select Faction Below-"), MyStringId.GetOrCompute("-Select Faction Below-"), objectText);
            listItems.Add(dummy);
            if (factionNames.Count == 0 || factionSelected.Contains("-Select Faction Below-"))
            {
                selectedItems.Add(dummy);
            }

            foreach (var name in factionNames)
            {
                if (string.IsNullOrEmpty(name)) continue;
                var toList = new MyTerminalControlListBoxItem(MyStringId.GetOrCompute(name), MyStringId.GetOrCompute(name), objectText);
                if (factionSelected == (name))
                {
                    selectedItems.Add(toList);
                }

                listItems.Add(toList);
            }
        }

        public static void SetSelectedFaction(IMyTerminalBlock block, List<MyTerminalControlListBoxItem> listItems)
        {
            if (listItems.Count == 0) return;
            //if (string.IsNullOrEmpty(factionSelected)) return;

            factionSelected = listItems[0].Text.ToString();
            RefreshControls(block);
        }

        public static void SetManualTerritory(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;
            if (!settings.Enabled) return;

            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetFactionByName(factionSelected);
            if (faction == null) return;

            Comms.ManualTerritorySet(settings, faction.Tag);
        }

        public static void RefreshConfig(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            Comms.RefreshConfig(block.EntityId);
        }

        /*public static float GetSafeZoneSlider(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 1000f;
            return settings.SafeZoneSize;
        }

        public static void SetSafeZoneSlider(IMyTerminalBlock block, float value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.SafeZoneSize = value;
        }

        public static float GetClaimAreaSlider(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 50000f;
            return settings.ClaimRadius;
        }

        public static void SetClaimAreaSlider(IMyTerminalBlock block, float value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.ClaimRadius = value;
            //settings.Sync = true;

            //Session.Instance.claimBlocks[block] = settings;
        }

        public static void GetControlsContent(List<MyTerminalControlComboBoxItem> listItems)
        {
            int key = 0;
            foreach (var type in Enum.GetNames(typeof(UIControls)))
            {
                var dummy = new MyTerminalControlComboBoxItem();

                dummy.Key = key;
                dummy.Value = MyStringId.GetOrCompute(type);
                listItems.Add(dummy);
                key++;
            }
        }

        public static long GetSelectedControl(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 0;

            if (settings.UI == UIControls.Misc) return 0;
            if (settings.UI == UIControls.Claiming) return 1;
            if (settings.UI == UIControls.Sieging) return 2;
            if (settings.UI == UIControls.Perks) return 3;
            if (settings.UI == UIControls.TerritoryOptions) return 4;

            return 0;
        }

        public static void SetSelectedControl(IMyTerminalBlock block, long value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            if (value == 0)
                settings.UI = UIControls.Misc;

            if (value == 1)
                settings.UI = UIControls.Claiming;

            if (value == 2)
                settings.UI = UIControls.Sieging;

            if (value == 3)
                settings.UI = UIControls.Perks;

            if (value == 4)
                settings.UI = UIControls.TerritoryOptions;

            RefreshControls(block);
        }

        public static float GetToClaimTime(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 300f;
            return settings.ToClaimTimer;
        }

        public static void SetToClaimTime(IMyTerminalBlock block, float value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.ToClaimTimer = (int)value;
        }

        public static float GetToSiegeTime(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 3600f;
            return settings.ToSiegeTimer;
        }

        public static void SetToSiegeTime(IMyTerminalBlock block, float value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.ToSiegeTimer = (int)value;
        }

        public static float GetToSiegeTimeFinal(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 3600f;
            return settings.SiegeFinalTimer;
        }

        public static void SetToSiegeTimeFinal(IMyTerminalBlock block, float value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.SiegeFinalTimer = (int)value;
        }

        public static float GetTokensToSiegeFinal(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 3600f;
            return settings.TokensSiegeFinal;
        }

        public static void SetTokensToSiegeFinal(IMyTerminalBlock block, float value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.TokensSiegeFinal = (int)value;
        }

        public static float GetTokensToSiege(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 1000f;
            return settings.TokensToSiege;
        }

        public static void SetTokensToSiege(IMyTerminalBlock block, float value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.TokensToSiege = (int)value;
        }

        public static float GetTokensToClaim(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 1000f;
            return settings.TokensToClaim;
        }

        public static void SetTokensToClaim(IMyTerminalBlock block, float value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.TokensToClaim = (int)value;
        }

        public static float GetTimeToConsumeToken(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 3600f;
            return settings.ConsumeTokenTimer;
        }

        public static void SetTimeToConsumeToken(IMyTerminalBlock block, float value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.ConsumeTokenTimer = (int)value;
        }

        public static float GetDistanceToClaim(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 3000f;
            return (float)settings.DistanceToClaim;
        }

        public static void SetDistanceToClaim(IMyTerminalBlock block, float value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.DistanceToClaim = value;
        }

        public static float GetDistanceToSiege(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 3000f;
            return (float)settings.DistanceToSiege;
        }

        public static void SetDistanceToSiege(IMyTerminalBlock block, float value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.DistanceToSiege = value;
        }

        public static float GetDeactivationTime(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 1440f;
            return settings.ZoneDeactivationTimer / 60;
        }

        public static void SetDeactivationTime(IMyTerminalBlock block, float value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.ZoneDeactivationTimer = (int)value * 60;
        }

        public static float GetSiegeGapTime(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 30f;
            return settings.TimeframeToSiege / 60;
        }

        public static void SetSiegeGapTime(IMyTerminalBlock block, float value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.TimeframeToSiege = (int)value * 60;
        }

        public static float GetCooldownTime(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 1440f;
            return settings.CooldownTimer / 60;
        }

        public static void SetCooldownTime(IMyTerminalBlock block, float value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.CooldownTimer = (int)value * 60;
        }

        public static float GetSiegeCooldownTime(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 7200f;
            return settings.SiegeCoolingTime / 60;
        }

        public static void SetSiegeCooldownTime(IMyTerminalBlock block, float value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.SiegeCoolingTime = (int)value * 60;
        }

        public static float GetTokenSiegeDelay(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 100f;
            return settings.TokensSiegeDelay;
        }

        public static void SetTokenSiegeDelay(IMyTerminalBlock block, float value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.TokensSiegeDelay = (int)value;
        }

        public static float GetSiegeDelayCount(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 3f;
            return settings.SiegeDelayAllow;
        }

        public static void SetSiegeDelayCount(IMyTerminalBlock block, float value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.SiegeDelayAllow = (int)value;
        }

        public static float GetGpsUpdate(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 30f;
            return settings.GpsUpdateDelay;
        }

        public static void SetGpsUpdate(IMyTerminalBlock block, float value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.GpsUpdateDelay = (int)value;
        }

        public static float GetSiegeNotificationFreq(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 30f;
            return settings.SiegeNoficationFreq;
        }

        public static void SetSiegeNotificationFreq(IMyTerminalBlock block, float value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.SiegeNoficationFreq = (int)value;
        }

        public static void SetAreaTrigger(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            //Comms.CreateNewAreaTrigger(block);
            if (settings.TriggerInit) return;
            MyVisualScriptLogicProvider.RemoveTrigger(block.EntityId.ToString());
            MyVisualScriptLogicProvider.CreateAreaTriggerOnPosition(block.GetPosition(), settings.ClaimRadius, block.EntityId.ToString());
            settings.TriggerInit = true;

        }

        public static StringBuilder GetDefaultName(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return new StringBuilder();

            StringBuilder newbuilder = new StringBuilder();
            newbuilder.Append(settings.UnclaimName);

            return newbuilder;
        }

        public static void SetDefaultName(IMyTerminalBlock block, StringBuilder builder)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.UnclaimName = builder.ToString();
        }

        public static StringBuilder GetDiscordId(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return new StringBuilder();

            StringBuilder newbuilder = new StringBuilder();
            newbuilder.Append(settings.DiscordRoleId.ToString());

            return newbuilder;
        }

        public static void SetDiscordId(IMyTerminalBlock block, StringBuilder builder)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            long id = 0;
            if (!long.TryParse(builder.ToString(), out id)) return;

            settings.DiscordRoleId = id;
        }

        public static StringBuilder GetNPC(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return new StringBuilder();

            StringBuilder newbuilder = new StringBuilder();
            newbuilder.Append(settings.BlockOwner);

            return newbuilder;
        }

        public static void SetNPC(IMyTerminalBlock block, StringBuilder builder)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.BlockOwner = builder.ToString();
        }

        public static void GetPerkTypeContent(List<MyTerminalControlComboBoxItem> listItems)
        {
            int key = 0;
            foreach (var type in Enum.GetNames(typeof(PerkTypeList)))
            {
                var dummy = new MyTerminalControlComboBoxItem();

                dummy.Key = key;
                dummy.Value = MyStringId.GetOrCompute(type);
                listItems.Add(dummy);
                key++;
            }
        }

        public static long GetPerkType(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 0;

            if (settings.UIPerkList == PerkTypeList.Production) return 0;

            return 0;
        }

        public static void SetPerkType(IMyTerminalBlock block, long value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            if (value == 0)
                settings.UIPerkList = PerkTypeList.Production;

            RefreshControls(block);
        }

        public static bool IsAdminProductionEnabled(IMyTerminalBlock block)
        {
            if (!IsClaimAndAdmin(block)) return false;
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            if (settings.GetPerks == null || settings.GetPerks.Count == 0) return false;
            if (settings.GetPerks.ContainsKey(PerkType.Production))
            {
                if (!IsPerkControls(block)) return false;
                if (!IsPerkType(block, PerkTypeList.Production)) return false;
                return settings.GetPerks[PerkType.Production].enabled;
            }

            return false;
        }

        public static bool GetProductionEnabled(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            if (settings.GetPerks == null || settings.GetPerks.Count == 0) return false;
            if (settings.GetPerks.ContainsKey(PerkType.Production))
            {
                //if (!IsPerkControls(block)) return false;
                //if (!IsPerkType(block, PerkTypeList.Production)) return false;
                return settings.GetPerks[PerkType.Production].enabled;
            }

            return false;
        }

        public static void SetProductionEnabled(IMyTerminalBlock block, bool value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            if (value)
            {
                if (settings.GetPerks == null || !settings.GetPerks.ContainsKey(PerkType.Production))
                {
                    settings.UpdatePerks(PerkType.Production, true);
                }
                else
                {
                    settings.GetPerks[PerkType.Production].enabled = true;
                    Comms.SyncSettingType(settings, MyAPIGateway.Session.LocalHumanPlayer, SyncType.EnableProductionPerk);
                    if (settings.IsClaimed && GetStandAloneEnabled(block))
                        Comms.SendApplyProductionPerkToServer(settings);

                }
            }
            else
            {
                if (settings.GetPerks.ContainsKey(PerkType.Production))
                {
                    settings.GetPerks[PerkType.Production].enabled = false;
                    Comms.SyncSettingType(settings, MyAPIGateway.Session.LocalHumanPlayer, SyncType.DisableProductionPerk);
                    settings.UpdatePerks(PerkType.Production, false);
                }
            }

            RefreshControls(block);
        }

        public static float GetProductionSpeed(IMyTerminalBlock block)
        {
            if (block == null) return 0f;
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 0f;

            if (settings.GetPerks == null) return 0f;
            if (!settings.GetPerks.ContainsKey(PerkType.Production)) return 0f;

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return 0f;

            return perkbase.perk.productionPerk.Speed * 100;
        }

        public static void SetProductionSpeed(IMyTerminalBlock block, float value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return;

            perkbase.perk.productionPerk.Speed = value / 100;
            if (perkbase.perk.productionPerk.allowStandAlone)
            {
                if (!settings.Server.Sync)
                    Utils.UpdateActiveStandAlonePerks(settings);
            }

            settings.Server.Sync = true;
        }

        public static float GetProductionYield(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 0f;

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return 0f;

            return perkbase.perk.productionPerk.Yield * 100;
        }

        public static void SetProductionYield(IMyTerminalBlock block, float value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return;

            perkbase.perk.productionPerk.Yield = value / 100;
            if (perkbase.perk.productionPerk.allowStandAlone)
            {
                if (!settings.Server.Sync)
                    Utils.UpdateActiveStandAlonePerks(settings);
            }

            settings.Server.Sync = true;
        }

        public static float GetProductionEnergy(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 0f;

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return 0f;

            return perkbase.perk.productionPerk.Energy * 100;
        }

        public static void SetProductionEnergy(IMyTerminalBlock block, float value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return;

            perkbase.perk.productionPerk.Energy = value / 100;
            if (perkbase.perk.productionPerk.allowStandAlone)
            {
                if (!settings.Server.Sync)
                    Utils.UpdateActiveStandAlonePerks(settings);
            }

            settings.Server.Sync = true;
        }

        public static void SetProduction(IMyTerminalBlock block)
        {
            Comms.UpdateProductionMultipliers(block.EntityId);
        }

        public static bool GetStandAloneEnabled(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return false;

            return perkbase.perk.productionPerk.allowStandAlone;
        }

        public static void SetStandAloneEnabled(IMyTerminalBlock block, bool value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return;

            perkbase.perk.productionPerk.allowStandAlone = value;
            Comms.SyncSettingsToOthers(settings, MyAPIGateway.Session.LocalHumanPlayer);
            RefreshControls(block);

            if (value)
            {
                if (settings.IsClaimed)
                {
                    Comms.SendApplyProductionPerkToServer(settings);
                }
            }
            else
            {
                if (settings.IsClaimed)
                {
                    Comms.SendRemoveProductionPerkToServer(settings);
                }
            }

            Utils.UpdateActiveStandAlonePerks(settings);
            //settings.Server.Sync = true;

        }

        public static bool CheckPlayerControls(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return false;

            if (perkbase.perk.productionPerk.allowClientControlEnergy) return false;
            if (perkbase.perk.productionPerk.allowClientControlSpeed) return false;
            if (perkbase.perk.productionPerk.allowClientControlYield) return false;

            return true;
        }

        public static bool CheckStandAloneEnabled(IMyTerminalBlock block)
        {
            if (!IsAdmin(block)) return false;
            if (GetStandAloneEnabled(block)) return false;

            return true;
        }

        public static bool GetPlayerControlSpeed(IMyTerminalBlock block)
        {
            if (!IsClaimAndAdmin(block)) return false;
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return false;

            if (settings.UI != UIControls.Perks && settings.UIPerkList != PerkTypeList.Production) return false;
            if (!IsAdminProductionEnabled(block)) return false;
            return perkbase.perk.productionPerk.allowClientControlSpeed;
        }

        public static void SetPlayerControlSpeed(IMyTerminalBlock block, bool value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return;

            if (!value)
            {
                if (perkbase.perk.productionPerk.allowClientControlSpeed)
                {
                    SetPlayerToggleSpeed(block, false);
                    //Utils.UpdatePlayerPerks(settings);
                }
            }

            perkbase.perk.productionPerk.allowClientControlSpeed = value;

            if (perkbase.perk.productionPerk.allowStandAlone)
                perkbase.perk.productionPerk.allowStandAlone = false;

            settings.Server.Sync = true;
            RefreshControls(block);
        }

        public static StringBuilder GetSpeedTokens(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return new StringBuilder();

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return new StringBuilder();

            StringBuilder newbuilder = new StringBuilder();
            newbuilder.Append(perkbase.perk.productionPerk.speedTokens);

            return newbuilder;
        }

        public static void SetSpeedTokens(IMyTerminalBlock block, StringBuilder builder)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return;

            int tokens = 0;
            int.TryParse(builder.ToString(), out tokens);

            perkbase.perk.productionPerk.speedTokens = tokens;
            settings.Server.Sync = true;
            //RefreshControls(block);
        }

        public static bool GetPlayerControlYield(IMyTerminalBlock block)
        {
            if (!IsClaimAndAdmin(block)) return false;
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return false;

            if (settings.UI != UIControls.Perks && settings.UIPerkList != PerkTypeList.Production) return false;
            if (!IsAdminProductionEnabled(block)) return false;
            return perkbase.perk.productionPerk.allowClientControlYield;
        }

        public static void SetPlayerControlYield(IMyTerminalBlock block, bool value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return;

            if (!value)
            {
                if (perkbase.perk.productionPerk.allowClientControlYield)
                {
                    SetPlayerToggleYield(block, false);
                    //Utils.UpdatePlayerPerks(settings);
                }
            }

            perkbase.perk.productionPerk.allowClientControlYield = value;

            if (perkbase.perk.productionPerk.allowStandAlone)
                perkbase.perk.productionPerk.allowStandAlone = false;

            settings.Server.Sync = true;
            RefreshControls(block);
        }

        public static StringBuilder GetYieldTokens(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return new StringBuilder();

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return new StringBuilder();

            StringBuilder newbuilder = new StringBuilder();
            newbuilder.Append(perkbase.perk.productionPerk.yieldTokens);

            return newbuilder;
        }

        public static void SetYieldTokens(IMyTerminalBlock block, StringBuilder builder)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return;

            int tokens = 0;
            int.TryParse(builder.ToString(), out tokens);

            perkbase.perk.productionPerk.yieldTokens = tokens;
            settings.Server.Sync = true;
        }

        public static bool GetPlayerControlEnergy(IMyTerminalBlock block)
        {
            if (!IsClaimAndAdmin(block)) return false;
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return false;

            if (settings.UI != UIControls.Perks && settings.UIPerkList != PerkTypeList.Production) return false;
            if (!IsAdminProductionEnabled(block)) return false;
            return perkbase.perk.productionPerk.allowClientControlEnergy;
        }

        public static void SetPlayerControlEnergy(IMyTerminalBlock block, bool value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return;

            perkbase.perk.productionPerk.allowClientControlEnergy = value;

            if (!value)
            {
                if (perkbase.perk.productionPerk.enableClientControlEnergy)
                {
                    SetPlayerToggleEnergy(block, false);
                    //Utils.UpdatePlayerPerks(settings);
                }
            }

            if (perkbase.perk.productionPerk.allowStandAlone)
                perkbase.perk.productionPerk.allowStandAlone = false;

            settings.Server.Sync = true;
            RefreshControls(block);
        }

        public static StringBuilder GetEnergyTokens(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return new StringBuilder();

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return new StringBuilder();

            StringBuilder newbuilder = new StringBuilder();
            newbuilder.Append(perkbase.perk.productionPerk.energyTokens);

            return newbuilder;
        }

        public static void SetEnergyTokens(IMyTerminalBlock block, StringBuilder builder)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return;

            int tokens = 0;
            int.TryParse(builder.ToString(), out tokens);

            perkbase.perk.productionPerk.energyTokens = tokens;
            settings.Server.Sync = true;
        }*/

        /*public static void GetPerkTypeContentPlayer(List<MyTerminalControlComboBoxItem> listItems)
        {
            int key = 0;
            foreach (var type in Enum.GetNames(typeof(PlayerPerks)))
            {
                var dummy = new MyTerminalControlComboBoxItem();

                dummy.Key = key;
                dummy.Value = MyStringId.GetOrCompute(type);
                listItems.Add(dummy);
                key++;
            }
        }*/

        /*public static long GetPerkTypePlayer(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 0;

            if (settings.UIPlayerPerks == PlayerPerks.Production) return 0;

            return 0;
        }*/

        /*public static void SetPerkTypePlayer(IMyTerminalBlock block, long value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            if (value == 0)
                settings.UIPlayerPerks = PlayerPerks.Production;

            RefreshControls(block);
        }*/

        /*public static bool IsPlayerProductionTypeAllowed(IMyTerminalBlock block, string name)
        {
            if (!IsClaimBlock(block)) return false;
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return false;

            //if (settings.UIPlayerPerks != PlayerPerks.Production) return false;
            //if (!IsAdminProductionEnabled(block)) return false;
            if (!GetProductionEnabled(block)) return false;
            if (settings.UIPlayerPerks != PlayerPerks.Production) return false;

            if (name == "Speed")
                return perkbase.perk.productionPerk.allowClientControlSpeed;

            if (name == "Yield")
                return perkbase.perk.productionPerk.allowClientControlYield;

            if (name == "Energy")
                return perkbase.perk.productionPerk.allowClientControlEnergy;

            return false;
        }*/

        /*public static bool IsPlayerPerkType(IMyTerminalBlock block, PlayerPerks playerPerks)
        {
            if (!IsClaimBlock(block)) return false;
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            return settings.UIPlayerPerks == playerPerks;
        }*/

        /*public static bool GetPlayerToggleSpeed(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return false;

            return perkbase.perk.productionPerk.enableClientControlSpeed;
        }*/

        /*public static void SetPlayerToggleSpeed(IMyTerminalBlock block, bool value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return;

            perkbase.perk.productionPerk.enableClientControlSpeed = value;

            if (value)
            {
                if (!perkbase.perk.productionPerk.GetActiveUpgrades.Contains("Productivity"))
                {
                    if (!perkbase.perk.productionPerk.GetPendingAddUpgrades.Contains("Productivity"))
                        perkbase.perk.productionPerk.PendingAddUpgrades("Productivity", true);
                }

                if (perkbase.perk.productionPerk.GetPendingRemoveUpgrades.Contains("Productivity"))
                    perkbase.perk.productionPerk.PendingRemoveUpgrades("Productivity", false);
            }
            else
            {
                if (perkbase.perk.productionPerk.GetActiveUpgrades.Contains("Productivity"))
                {
                    if (!perkbase.perk.productionPerk.GetPendingRemoveUpgrades.Contains("Productivity"))
                        perkbase.perk.productionPerk.PendingRemoveUpgrades("Productivity", true);
                }

                if (perkbase.perk.productionPerk.GetPendingAddUpgrades.Contains("Productivity"))
                    perkbase.perk.productionPerk.PendingAddUpgrades("Productivity", false);

            }

            //settings.Server.Sync = true;
            Comms.SyncSettingsToOthers(settings, MyAPIGateway.Session.LocalHumanPlayer);
            RefreshControls(block);
        }*/

        /*public static bool GetPlayerToggleYield(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return false;

            return perkbase.perk.productionPerk.enableClientControlYield;
        }*/

        /*public static void SetPlayerToggleYield(IMyTerminalBlock block, bool value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return;

            perkbase.perk.productionPerk.enableClientControlYield = value;

            if (value)
            {
                if (!perkbase.perk.productionPerk.GetActiveUpgrades.Contains("Effectiveness"))
                {
                    if (!perkbase.perk.productionPerk.GetPendingAddUpgrades.Contains("Effectiveness"))
                        perkbase.perk.productionPerk.PendingAddUpgrades("Effectiveness", true);
                }

                if (perkbase.perk.productionPerk.GetPendingRemoveUpgrades.Contains("Effectiveness"))
                    perkbase.perk.productionPerk.PendingRemoveUpgrades("Effectiveness", false);
            }
            else
            {
                if (perkbase.perk.productionPerk.GetActiveUpgrades.Contains("Effectiveness"))
                {
                    if (!perkbase.perk.productionPerk.GetPendingRemoveUpgrades.Contains("Effectiveness"))
                        perkbase.perk.productionPerk.PendingRemoveUpgrades("Effectiveness", true);
                }

                if (perkbase.perk.productionPerk.GetPendingAddUpgrades.Contains("Effectiveness"))
                    perkbase.perk.productionPerk.PendingAddUpgrades("Effectiveness", false);

            }

            //settings.Server.Sync = true;
            Comms.SyncSettingsToOthers(settings, MyAPIGateway.Session.LocalHumanPlayer);
            RefreshControls(block);
        }*/

        /*public static bool GetPlayerToggleEnergy(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return false;

            return perkbase.perk.productionPerk.enableClientControlEnergy;
        }*/

        /*public static void SetPlayerToggleEnergy(IMyTerminalBlock block, bool value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            PerkBase perkbase;
            if (!settings.GetPerks.TryGetValue(PerkType.Production, out perkbase)) return;

            perkbase.perk.productionPerk.enableClientControlEnergy = value;

            if (value)
            {
                if (!perkbase.perk.productionPerk.GetActiveUpgrades.Contains("PowerEfficiency"))
                {
                    if (!perkbase.perk.productionPerk.GetPendingAddUpgrades.Contains("PowerEfficiency"))
                        perkbase.perk.productionPerk.PendingAddUpgrades("PowerEfficiency", true);
                }

                if (perkbase.perk.productionPerk.GetPendingRemoveUpgrades.Contains("PowerEfficiency"))
                    perkbase.perk.productionPerk.PendingRemoveUpgrades("PowerEfficiency", false);
            }
            else
            {
                if (perkbase.perk.productionPerk.GetActiveUpgrades.Contains("PowerEfficiency"))
                {
                    if (!perkbase.perk.productionPerk.GetPendingRemoveUpgrades.Contains("PowerEfficiency"))
                        perkbase.perk.productionPerk.PendingRemoveUpgrades("PowerEfficiency", true);
                }

                if (perkbase.perk.productionPerk.GetPendingAddUpgrades.Contains("PowerEfficiency"))
                    perkbase.perk.productionPerk.PendingAddUpgrades("PowerEfficiency", false);

            }

            //settings.Server.Sync = true;
            Comms.SyncSettingsToOthers(settings, MyAPIGateway.Session.LocalHumanPlayer);
            RefreshControls(block);
        }*/

        public static void TriggerCustomName(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            Comms.UpdateBlockText(settings);
        }

        public static bool IsClaimedAndFaction(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;
            if (!settings.IsClaimed) return false;
            if (IsAdmin(block)) return true;

            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(MyAPIGateway.Session.LocalHumanPlayer.IdentityId);
            if (faction == null) return false;

            if (settings.ClaimedFaction == faction.Tag) return true;
            return false;
        }

        public static StringBuilder GetClaimedName(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return new StringBuilder();

            StringBuilder newbuilder = new StringBuilder();
            newbuilder.Append(settings.ClaimZoneName);

            return newbuilder;
        }

        public static void SetClaimedName(IMyTerminalBlock block, StringBuilder builder)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.ClaimZoneName = builder.ToString();
        }

        /*public static bool CheckIsSieged(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            if (settings.IsSieged && settings.SiegeTimer > 3600)
            {
                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(MyAPIGateway.Session.LocalHumanPlayer.IdentityId);
                if (faction == null) return false;

                if (faction.Tag == settings.ClaimedFaction)
                {
                    if (settings.SiegedDelayedHit >= settings.SiegeDelayAllow) return false;
                    if (Utils.DelaySiegeTokenConsumption(settings)) return true;
                }
            }
            return false;
        }*/

        /*public static void DelaySiege(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            if (settings.SiegedDelayedHit >= settings.SiegeDelayAllow) return;
            Comms.ConsumeDelayTokens(settings);

            settings.SiegedDelayedHit++;
            settings.SiegeTimer += settings.HoursToDelay * 3600;

            //settings.DetailInfo = $"\n[Time Until Territory Can Be Final Sieged]:\n{TimeSpan.FromSeconds(settings.SiegeTimer)}\n";
            //settings.DetailInfo += $"\n[Siege Time Extended]: {settings.HoursToDelay * settings.SiegedDelayedHit} hrs ({settings.SiegedDelayedHit}/{settings.SiegeDelayAllow}) used\n";
            //settings.DetailInfo += Utils.GetCounterDetails(settings);
            settings.DetailInfo = Utils.GetCounterDetails(settings);

            RefreshControls(block);
        }*/

        /*public static int GetDelayCost(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 0;

            return settings.TokensSiegeDelay;
        }*/

        /*public static int GetSiegeDelayTime(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 0;

            return settings.HoursToDelay;
        }*/

        /*public static void SetSiegeDelayTime(IMyTerminalBlock block, float value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.HoursToDelay = (int)value;
        }*/

        /*public static bool IsAdminAllowAlliesEnabledSZ(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            return settings.AdminAllowSafeZoneAllies;
        }*/

        /*public static bool IsAdminAllowAlliesEnabledTerritory(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            return settings.AdminAllowTerritoryAllies;
        }*/

        public static bool GetPlayerAllowAlliesSZ(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            return settings.AllowSafeZoneAllies;
        }

        public static void SetPlayerAllowAlliesSZ(IMyTerminalBlock block, bool value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.AllowSafeZoneAllies = value;
            if (value)
            {
                settings.AllowTerritoryAllies = value;
                MyAPIGateway.Utilities.ShowMissionScreen("SafeZone Allies Warning", "", null, Utils.GetPopupText(MyTextEnum.SafeZoneAllies), null, "Ok");
            }

            RefreshControls(block);
        }

        public static bool GetPlayerAllowAlliesTerritory(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            return settings.AllowTerritoryAllies;
        }

        public static void SetPlayerAllowAlliesTerritory(IMyTerminalBlock block, bool value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.AllowTerritoryAllies = value;
            if (value)
                MyAPIGateway.Utilities.ShowMissionScreen("Territory Allies Warning", "", null, Utils.GetPopupText(MyTextEnum.TerritoryAllies), null, "Ok");

            RefreshControls(block);
        }

        public static bool IsPlayerSafezoneAllowed(IMyTerminalBlock block)
        {
            if (!IsClaimedAndFaction(block)) return false;

            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            return !settings.AllowSafeZoneAllies;
        }

        /*public static void SetTokenTimer(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.Timer = settings.ConsumeTokenTimer;
        }*/

        public static bool IsOffAndSieging(IMyTerminalBlock block)
        {
            if (!IsClaimedAndFaction(block)) return false;
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            if (settings.IsSieging)
                return settings.SafeZoneEntity != 0;

            if (settings.SafeZoneEntity == 0)
                return !CheckEnemyNearby(block);

            return true;
        }

        public static bool IsSafeZoneValid(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return true;

            return settings.SafeZoneEntity != 0;
        }

        public static void SetSafeZoneState(IMyTerminalBlock block, bool value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            if (value)
            {
                if (settings.IsSieging) return;
                settings.SafeZoneEntity = 1;
                Comms.EnableSafeZoneToServer(settings);
            }
            else
            {
                settings.SafeZoneEntity = 0;
                Comms.DisableSafeZoneToServer(settings);
                if (settings.IsSieging)
                    Comms.DisablePBMonitor(settings);
            }

            RefreshControls(block);
        }

        public static bool CheckEnemyNearby(IMyTerminalBlock block)
        {
            BoundingSphereD sphere = new BoundingSphereD(block.GetPosition(), 3000);
            List<IMyEntity> entities = MyAPIGateway.Entities.GetTopMostEntitiesInSphere(ref sphere);

            long owner = MyAPIGateway.Session.LocalHumanPlayer.IdentityId;
            IMyFaction ownerFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(owner);

            if (owner == 0) return false;
            foreach (var entity in entities)
            {
                if (entity as IMyCubeGrid != null)
                {
                    var cubeGrid = entity as IMyCubeGrid;
                    if (cubeGrid.Physics == null) continue;
                    List<IMySlimBlock> blocks = new List<IMySlimBlock>();
                    cubeGrid.GetBlocks(blocks);
                    if (blocks.Count < 30) continue;
                    if (cubeGrid.GridSizeEnum == VRage.Game.MyCubeSize.Small) continue;
                    if (cubeGrid.IsStatic) continue;
                    if (MyVisualScriptLogicProvider.IsUnderGround(cubeGrid.GetPosition())) continue;

                    var gridOwner = cubeGrid.BigOwners.FirstOrDefault();
                    IMyFaction gridFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(gridOwner);

                    if (gridOwner != owner)
                    {
                        if (ownerFaction != null && gridFaction != null && ownerFaction.Tag == gridFaction.Tag) continue;
                        if (ownerFaction != null && gridFaction != null && ownerFaction.Tag != gridFaction.Tag)
                        {
                            ClaimBlockSettings settings;
                            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) continue;

                            if (settings.AllowSafeZoneAllies || settings.AllowTerritoryAllies)
                                return Utils.IsFactionEnemy(settings, gridFaction);

                            return true;
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        /*public static StringBuilder GetConsumptionItem(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return new StringBuilder();

            return new StringBuilder(settings.ConsumptionItem);
        }

        public static void SetConsumptionItem(IMyTerminalBlock block, StringBuilder sb)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.ConsumptionItem = sb.ToString();
            RefreshControls(block);
        }

        public static bool IsConsumptionItemValid(IMyTerminalBlock block)
        {
            if (!IsClaimAndAdmin(block)) return false;
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;

            MyDefinitionId itemId;
            if (!MyDefinitionId.TryParse(settings.ConsumptionItem, out itemId)) return true;
            if (itemId == null) return true;

            MyDefinitionBase defBase;
            if (!MyDefinitionManager.Static.TryGetDefinition(itemId, out defBase)) return true;
            return false;
        }

        public static float GetMaintainCost(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 1;

            return settings.ConsumptinAmt;
        }

        public static void SetMaintainCost(IMyTerminalBlock block, float value)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            settings.ConsumptinAmt = (int)value;
        }*/

        public static bool CanBuyInstallation(IMyTerminalBlock block)
        {
            if (Session.Instance.installationBuyCooldown > 0) return false;
            if (!IsClaimedAndFaction(block)) return false;

            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return false;
            if (settings.IsSieging) return false;
            if (string.IsNullOrEmpty(SelectedInstallation)) return false;
            Installations installation = settings.GetInstallationByType(SelectedInstallation);
            if (installation != null)
            {
                if (installation.Enabled) return false;
                if (installation.RebuildCooldown > 0) return false;
            }
            else return false;

            bool foundInstallationConfig = false;
            foreach(var config in settings.TerritoryConfig._territoryInstallations)
            {
                if (!SelectedInstallation.Contains(config._installationType.ToString())) continue;
                foundInstallationConfig = true;
                break;
            }

            if (!foundInstallationConfig) return false;

            /*foreach (var config in settings.GetInstallations)
            {
                if (config.Type == installationType)
                {
                    if (config.Enabled) return false;
                    if (config.RebuildCooldown > 0) return false;
                    break;
                }
            }*/

            if (IsAdmin(block)) return true;

            MyFixedPoint amountNeeded = 0;
            MyDefinitionId tokenId;

            foreach (var config in settings.TerritoryConfig._territoryInstallations)
            {
                if (SelectedInstallation.Contains(config._installationType.ToString()))
                {
                    amountNeeded = config._installationCost;
                    break;
                }
            }

            MyDefinitionId.TryParse(settings.TerritoryConfig._token, out tokenId);
            if (tokenId == null) return false;

            IMyInventory blockInv = block.GetInventory();
            if (blockInv == null) return false;
            MyFixedPoint tokens = blockInv.GetItemAmount(tokenId);
            if (tokens >= amountNeeded)
                return true;
            else { return false; }
        }

        public static void BuyInstallation(IMyTerminalBlock block)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return;

            Installations installation = settings.GetInstallationByType(SelectedInstallation);
            if (installation == null) return;

            Comms.BuyAndAddInstallation(block, installation.Type);
            Session.Instance.installationBuyCooldown = 2;
            RefreshControls(block);
        }

        /*public static int GetInstallationCost(IMyTerminalBlock block, InstallationType type)
        {
            ClaimBlockSettings settings;
            if (!Session.Instance.claimBlocks.TryGetValue(block.EntityId, out settings)) return 0;

            Installations installation = settings.GetInstallationByType(type);
            if (installation == null) return 0;

            return installation.TerritoryInstallations._installationCost;
        }*/

        //JumpDrive Actions

        public static bool IsJumpDriveBlock(IMyTerminalBlock block)
        {
            if (block as IMyJumpDrive != null)
            {
                if (block.BlockDefinition.SubtypeName.Contains("LargeJumpDrive")) return true;
            }

            return false;
        }

        public static bool IsNearClaim(IMyTerminalBlock block)
        {
            if (!IsJumpDriveBlock(block)) return false;
            foreach (var claim in Session.Instance.claimBlocks.Values)
            {
                if (Vector3D.Distance(claim.BlockPos, block.GetPosition()) > claim.TerritoryConfig._territoryRadius) continue;
                if (claim.IsClaimed) continue;
                return true;
            }

            return false;
        }

        public static bool IsNearClaimed(IMyTerminalBlock block)
        {
            if (!IsJumpDriveBlock(block)) return false;
            foreach (var claim in Session.Instance.claimBlocks.Values)
            {
                if (Vector3D.Distance(claim.BlockPos, block.GetPosition()) > claim.TerritoryConfig._territoryRadius) continue;
                if (claim.IsClaimed)
                {
                    IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(MyAPIGateway.Session.LocalHumanPlayer.IdentityId);
                    if (blockFaction == null) return true;

                    if (blockFaction.Tag != claim.ClaimedFaction) return true;
                    return false;
                }

            }

            return false;
        }

        public static bool IsCooling(IMyTerminalBlock block)
        {
            IsCoolingError = false;
            if (!IsJumpDriveBlock(block)) return false;
            if (!ShowErrors) return false;
            ClaimBlockSettings claimBlock;
            if (!Session.Instance.claimBlocks.TryGetValue(ClaimBlockId, out claimBlock)) return false;

            IsCoolingError = claimBlock.IsCooling;
            return claimBlock.IsCooling;
        }

        public static bool IsSiegeCooling(IMyTerminalBlock block)
        {
            IsSiegeCoolingError = false;
            if (!IsJumpDriveBlock(block)) return false;
            if (!ShowErrors) return false;
            ClaimBlockSettings claimBlock;
            if (!Session.Instance.claimBlocks.TryGetValue(ClaimBlockId, out claimBlock)) return false;

            IsSiegeCoolingError = claimBlock.IsSiegeCooling;
            return IsSiegeCoolingError;
        }

        public static bool AllowClaimEnable(IMyTerminalBlock block)
        {
            if (!IsJumpDriveBlock(block)) return false;
            if (!ShowErrors) return false;
            if (!InClaimRange) return false;
            if (!IsInFaction) return false;
            if (!IsPlayerNear) return false;
            if (EnemyNearby) return false;
            if (!ValidTokens) return false;
            if (!ValidEnergy) return false;
            if (IsStatic) return false;
            //if (InVoxel) return false;
            if (IsUnderground) return false;
            if (IsCoolingError) return false;

            return true;
        }

        public static bool AllowSiegeEnable(IMyTerminalBlock block)
        {
            if (!IsJumpDriveBlock(block)) return false;
            if (!ShowErrors) return false;
            if (!InSiegeRange) return false;
            if (!IsInFaction) return false;
            if (!IsPlayerNear) return false;
            if (AlreadySieged) return false;
            if (!ValidTokens) return false;
            if (!ValidEnergy) return false;
            if (IsBeingSieged) return false;
            if (IsUnderground) return false;
            if (ReadyToSiege) return false;
            if (IsSiegeCoolingError) return false;

            ClaimBlockSettings claimBlock;
            if (!Session.Instance.claimBlocks.TryGetValue(ClaimBlockId, out claimBlock)) return false;
            if (claimBlock.IsSieged) return false;

            return true;
        }

        /*public static bool AllowFinalSiegeEnable(IMyTerminalBlock block)
        {
            if (!IsJumpDriveBlock(block)) return false;
            if (!ShowErrors) return false;
            if (!InSiegeRange) return false;
            if (!IsInFaction) return false;
            if (!IsPlayerNear) return false;
            if (AlreadySieged) return false;
            if (!ValidTokens) return false;
            if (!ValidEnergy) return false;
            if (IsBeingSieged) return false;
            if (IsUnderground) return false;

            if (ReadyToSiege) return true;
            return false;
        }*/

        public static bool IsInClaimOrSiegeRange(IMyTerminalBlock block)
        {
            ClaimBlockId = 0;
            InClaimRange = false;
            InSiegeRange = false;
            ShowErrors = false;
            if (!IsJumpDriveBlock(block)) return false;

            foreach (var claim in Session.Instance.claimBlocks.Values)
            {
                if (!claim.Enabled) continue;
                if (Vector3D.Distance(claim.BlockPos, block.GetPosition()) <= claim.TerritoryConfig._territoryRadius)
                {
                    ShowErrors = true;
                    ClaimBlockId = claim.EntityId;

                    if (!claim.IsClaimed)
                    {
                        if (Vector3D.Distance(claim.BlockPos, block.GetPosition()) > claim.TerritoryConfig._claimingConfig._distanceToClaim) continue;
                        InClaimRange = true;
                        return false;
                    }
                    else
                    {
                        if (Vector3D.Distance(claim.BlockPos, block.GetPosition()) > claim.TerritoryConfig._siegingConfig._distanceToSiege) continue;
                        InSiegeRange = true;
                        return false;
                    }
                }
            }

            if (ShowErrors) return true;
            return false;
        }

        public static bool CheckForFaction(IMyTerminalBlock block)
        {
            IsInFaction = false;
            if (!IsJumpDriveBlock(block)) return false;
            if (!ShowErrors) return false;
            //if (InSiegeRange) return false;

            long playerId = MyAPIGateway.Session.LocalHumanPlayer.IdentityId;
            IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(playerId);
            if (faction == null) return true;
            else
            {
                IsInFaction = true;
                return false;
            }
        }

        public static bool CheckForPlayer(IMyTerminalBlock block)
        {
            IsPlayerNear = false;
            if (!IsJumpDriveBlock(block)) return false;
            if (!ShowErrors) return false;

            IMyPlayer client = MyAPIGateway.Session.LocalHumanPlayer;
            foreach (var claim in Session.Instance.claimBlocks.Values)
            {
                if (claim.EntityId != ClaimBlockId) continue;
                if (claim.IsClaimed)
                {
                    if (Vector3D.Distance(client.GetPosition(), claim.BlockPos) > claim.TerritoryConfig._siegingConfig._distanceToSiege) return true;
                }
                else
                {
                    if (Vector3D.Distance(client.GetPosition(), claim.BlockPos) > claim.TerritoryConfig._claimingConfig._distanceToClaim) return true;
                }

                IsPlayerNear = true;
                return false;
            }

            return false;
        }

        public static bool IsClaiming(IMyTerminalBlock block)
        {
            IsBeingClaimed = false;
            if (!IsJumpDriveBlock(block)) return false;
            if (!ShowErrors) return false;
            if (!IsInFaction) return false;
            if (!IsPlayerNear) return false;

            if (InClaimRange)
            {
                foreach (var claim in Session.Instance.claimBlocks.Values)
                {
                    if (claim.IsClaiming && claim.EntityId == ClaimBlockId)
                    {
                        IsBeingClaimed = true;
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsSieging(IMyTerminalBlock block)
        {
            IsBeingSieged = false;
            if (!IsJumpDriveBlock(block)) return false;
            if (!ShowErrors) return false;
            if (!IsPlayerNear) return false;

            if (InSiegeRange)
            {
                foreach (var claim in Session.Instance.claimBlocks.Values)
                {
                    if (claim.IsSieging && claim.EntityId == ClaimBlockId)
                    {
                        IsBeingSieged = true;
                        return true;
                    }
                }
            }

            return false;
        }

        /*public static bool IsSieged(IMyTerminalBlock block)
        {
            AlreadySieged = false;
            if (!IsJumpDriveBlock(block)) return false;
            if (!ShowErrors) return false;
            if (!IsPlayerNear) return false;

            if (InSiegeRange)
            {
                foreach (var claim in Session.Instance.claimBlocks.Values)
                {
                    if (claim.IsSieged && claim.EntityId == ClaimBlockId)
                    {
                        IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(MyAPIGateway.Session.LocalHumanPlayer.IdentityId);
                        if (blockFaction != null)
                        {
                            if (blockFaction.Tag == claim.SiegedBy) return false;
                        }
                        else
                        {
                            if (block.OwnerId.ToString() == claim.SiegedBy) return false;
                        }

                        AlreadySieged = true;
                        return true;
                    }
                }
            }

            return false;
        }*/

        /*public static bool ReadyToFinalSiege(IMyTerminalBlock block)
        {
            ReadyToSiege = false;
            if (!IsJumpDriveBlock(block)) return false;
            if (!ShowErrors) return false;
            if (!IsPlayerNear) return false;

            if (InSiegeRange && !AlreadySieged)
            {
                foreach (var claim in Session.Instance.claimBlocks.Values)
                {
                    if (claim.IsSieged && claim.EntityId == ClaimBlockId)
                    {
                        IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(MyAPIGateway.Session.LocalHumanPlayer.IdentityId);
                        if (blockFaction != null)
                        {
                            if (blockFaction.Tag == claim.SiegedBy && claim.ReadyToSiege)
                            {
                                ReadyToSiege = true;
                                return false;
                            }
                        }
                        else
                        {
                            if (block.OwnerId.ToString() == claim.SiegedBy && claim.ReadyToSiege)
                            {
                                ReadyToSiege = true;
                                return false;
                            }
                        }

                        //ReadyToSiege = true;
                        return true;
                    }
                }
            }

            return false;
        }*/

        public static bool IsEnemyNearby(IMyTerminalBlock block)
        {
            EnemyNearby = true;
            if (!IsJumpDriveBlock(block)) return false;
            if (!ShowErrors) return false;
            //if (!IsInFaction) return false;
            //if (!IsPlayerNear) return false;


            if (InClaimRange && !IsBeingClaimed)
            {
                ClaimBlockSettings claimBlock;
                Session.Instance.claimBlocks.TryGetValue(ClaimBlockId, out claimBlock);
                if (claimBlock == null) return false;

                BoundingSphereD sphere = new BoundingSphereD(claimBlock.BlockPos, 3000);
                List<IMyEntity> entities = MyAPIGateway.Entities.GetTopMostEntitiesInSphere(ref sphere);

                long owner = MyAPIGateway.Session.LocalHumanPlayer.IdentityId;
                IMyFaction ownerFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(owner);

                if (owner == 0) return false;
                foreach (var entity in entities)
                {
                    if (entity as IMyCubeGrid != null)
                    {
                        var cubeGrid = entity as IMyCubeGrid;
                        if (cubeGrid.Physics == null) continue;
                        List<IMySlimBlock> blocks = new List<IMySlimBlock>();
                        cubeGrid.GetBlocks(blocks);
                        if (blocks.Count < 30) continue;
                        if (cubeGrid.GridSizeEnum == VRage.Game.MyCubeSize.Small) continue;
                        if (cubeGrid.IsStatic) continue;
                        if (MyVisualScriptLogicProvider.IsUnderGround(cubeGrid.GetPosition())) continue;

                        var gridOwner = cubeGrid.BigOwners.FirstOrDefault();
                        IMyFaction gridFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(gridOwner);

                        if (gridOwner != owner)
                        {
                            if (ownerFaction != null && gridFaction != null && ownerFaction.Tag == gridFaction.Tag) continue;
                            if (ownerFaction != null && gridFaction != null && ownerFaction.Tag != gridFaction.Tag) return true;

                            return true;
                        }
                    }
                }

                EnemyNearby = false;
                return false;
            }

            return false;
        }

        public static bool CheckForClaimTokens(IMyTerminalBlock block)
        {
            ValidTokens = false;
            if (!IsJumpDriveBlock(block)) return false;
            if (!ShowErrors) return false;
            //if (!IsInFaction) return false;
            //if (!IsPlayerNear) return false;

            if (InClaimRange)
            {
                //if (!IsInFaction) return false;
                ClaimBlockSettings claimBlock;
                Session.Instance.claimBlocks.TryGetValue(ClaimBlockId, out claimBlock);
                if (claimBlock == null) return false;

                if (claimBlock.TerritoryConfig._claimingConfig._tokensToClaim == 0)
                {
                    ValidTokens = true;
                    return false;
                }

                MyDefinitionId tokenId;
                MyDefinitionId.TryParse(claimBlock.TerritoryConfig._token, out tokenId);
                if (tokenId == null) return false;

                IMyCubeGrid cubeGrid = block.CubeGrid;
                if (cubeGrid == null) return false;

                List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
                MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(cubeGrid).GetBlocksOfType(Blocks, x => x.HasInventory);
                MyFixedPoint tokens = 0;

                foreach (var tblock in Blocks)
                {
                    var blockInv = tblock.GetInventory();
                    tokens += blockInv.GetItemAmount(tokenId);

                    if (tokens >= claimBlock.TerritoryConfig._claimingConfig._tokensToClaim) break;
                }

                if (tokens >= claimBlock.TerritoryConfig._claimingConfig._tokensToClaim)
                {
                    ValidTokens = true;
                    return false;
                }
            }

            if (InSiegeRange)
            {
                ClaimBlockSettings claimBlock;
                Session.Instance.claimBlocks.TryGetValue(ClaimBlockId, out claimBlock);
                if (claimBlock == null) return false;

                if (claimBlock.TerritoryConfig._siegingConfig._tokensToSiege == 0)
                {
                    ValidTokens = true;
                    return false;
                }

                MyDefinitionId tokenId;
                MyDefinitionId.TryParse(claimBlock.TerritoryConfig._token, out tokenId);
                if (tokenId == null) return false;

                IMyCubeGrid cubeGrid = block.CubeGrid;
                if (cubeGrid == null) return false;

                List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
                MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(cubeGrid).GetBlocksOfType(Blocks, x => x.HasInventory);
                MyFixedPoint tokens = 0;

                foreach (var tblock in Blocks)
                {
                    var blockInv = tblock.GetInventory();
                    tokens += blockInv.GetItemAmount(tokenId);

                    if (tokens >= claimBlock.TerritoryConfig._siegingConfig._tokensToSiege) break;
                }

                if (tokens >= claimBlock.TerritoryConfig._siegingConfig._tokensToSiege)
                {
                    ValidTokens = true;
                    return false;
                }
            }

            return true;
        }

        public static bool CheckForEnergy(IMyTerminalBlock block)
        {
            ValidEnergy = false;
            if (!IsJumpDriveBlock(block)) return false;
            if (!ShowErrors) return false;
            //if (InClaimRange && !IsInFaction) return false;
            //if (!IsPlayerNear) return false;

            if (!block.IsWorking) return true;
            IMyJumpDrive jd = block as IMyJumpDrive;
            if (jd == null) return false;

            if (jd.CurrentStoredPower != jd.MaxStoredPower) return true;
            ValidEnergy = true;
            return false;
        }

        public static bool IsGridStatic(IMyTerminalBlock block)
        {
            IsStatic = false;
            if (!IsJumpDriveBlock(block)) return false;
            if (!ShowErrors) return false;

            List<IMyCubeGrid> gridGroup = MyAPIGateway.GridGroups.GetGroup(block.CubeGrid, GridLinkTypeEnum.Physical);
            foreach (var grid in gridGroup)
            {
                if (grid.IsStatic)
                {
                    IsStatic = true;
                    return IsStatic;
                }
            }

            return IsStatic;
        }

        public static bool IsGridUnderground(IMyTerminalBlock block)
        {
            IsUnderground = false;
            if (!IsJumpDriveBlock(block)) return false;
            if (!ShowErrors) return false;

            if (!MyVisualScriptLogicProvider.IsPlanetNearby(block.CubeGrid.GetPosition())) return IsUnderground;
            IsUnderground = MyVisualScriptLogicProvider.IsUnderGround(block.CubeGrid.GetPosition());
            return IsUnderground;
        }

        /*public static bool IsGridInvoxel(IMyTerminalBlock block)
        {
            if (inVoxelDelay != 0) return InVoxel;
            InVoxel = false;
            if (!IsJumpDriveBlock(block)) return false;
            if (!ShowErrors) return false;
            if (!ValidEnergy) return false;
            if (!ValidTokens) return false;
            if (IsBeingClaimed) return false;
            if (!InClaimRange) return false;

            List<IMySlimBlock> blockList = new List<IMySlimBlock>();
            List<MyVoxelBase> m_tmpVoxelList = new List<MyVoxelBase>();
            List<IMyCubeGrid> gridGroup = MyAPIGateway.GridGroups.GetGroup(block.CubeGrid, GridLinkTypeEnum.Physical);

            foreach(var grid in gridGroup)
            {
                blockList.Clear();
                grid.GetBlocks(blockList);

                foreach (var blocks in blockList)
                {
                    BoundingBoxD boundingBoxD;
                    blocks.GetWorldBoundingBox(out boundingBoxD, false);
                    m_tmpVoxelList.Clear();
                    MyGamePruningStructure.GetAllVoxelMapsInBox(ref boundingBoxD, m_tmpVoxelList);
                    float gridSize = blocks.CubeGrid.GridSize;
                    BoundingBoxD aabb = new BoundingBoxD((double)gridSize * (blocks.Min - (int)0.5), (double)gridSize * (blocks.Max + (int)0.5));
                    MatrixD worldMatrix = blocks.CubeGrid.WorldMatrix;
                    using (List<MyVoxelBase>.Enumerator enumerator = m_tmpVoxelList.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            if (enumerator.Current.IsAnyAabbCornerInside(ref worldMatrix, aabb))
                            {
                                InVoxel = true;
                                break;
                            }
                        }
                    }
                }
            }
            
            inVoxelDelay = 5;
            return InVoxel;
        }*/

        public static void InitClaim(IMyTerminalBlock block)
        {
            if (ClaimBlockId == 0) return;

            ClaimBlockSettings claimBlock;
            Session.Instance.claimBlocks.TryGetValue(ClaimBlockId, out claimBlock);
            if (claimBlock == null) return;
            if (claimBlock.IsClaiming || claimBlock.IsClaimed) return;

            Comms.InitClaimToServer(block.EntityId, ClaimBlockId, MyAPIGateway.Session.LocalHumanPlayer.IdentityId);
        }

        public static void InitSiege(IMyTerminalBlock block)
        {
            if (ClaimBlockId == 0) return;

            ClaimBlockSettings claimBlock;
            Session.Instance.claimBlocks.TryGetValue(ClaimBlockId, out claimBlock);
            if (claimBlock == null) return;
            if (claimBlock.IsClaimed)
                Comms.InitSiegeToServer(block.EntityId, ClaimBlockId, MyAPIGateway.Session.LocalHumanPlayer.IdentityId);
        }

        public static bool AllowJump(IMyTerminalBlock block)
        {
            IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.OwnerId);
            foreach (var item in Session.Instance.claimBlocks.Values)
            {
                if (!item.Enabled || !item.IsClaimed) continue;
                if (Vector3D.Distance(item.BlockPos, block.GetPosition()) > 10000) continue;

                if (blockFaction == null || blockFaction.Tag != item.ClaimedFaction) return false;
            }

            return true;
        }

        // SafeZone Block Controls

        public static bool AllowSwitchEnabled(IMyTerminalBlock block)
        {
            foreach (var item in Session.Instance.claimBlocks.Values)
            {
                if (!item.Enabled || !item.IsClaimed) continue;

                IMyFaction blockFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(block.OwnerId);
                if (blockFaction != null && blockFaction.Tag == item.ClaimedFaction) return true;
                if (Vector3D.Distance(block.GetPosition(), item.TerritoryConfig._territoryOptions._centerOnPlanet ? item.PlanetCenter : item.BlockPos) > item.TerritoryConfig._territoryRadius) return true;

                foreach (var item2 in item.GetZonesDelayRemove)
                {
                    if (item2.zoneId == block.EntityId) return true;
                }

                return false;
            }

            return true;
        }

        public static void RefreshControls(IMyTerminalBlock block, bool force = true)
        {

            if (MyAPIGateway.Gui.GetCurrentScreen == MyTerminalPageEnum.ControlPanel)
            {
                if (Session.Instance.controlRefreshDelay > 0 && force == false) return;
                var myCubeBlock = block as MyCubeBlock;

                if (myCubeBlock.IDModule != null)
                {

                    var share = myCubeBlock.IDModule.ShareMode;
                    var owner = myCubeBlock.IDModule.Owner;
                    myCubeBlock.ChangeOwner(owner, share == MyOwnershipShareModeEnum.None ? MyOwnershipShareModeEnum.All : MyOwnershipShareModeEnum.None);
                    myCubeBlock.ChangeOwner(owner, share);
                }
            }
        }

    }
}
