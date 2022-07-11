using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Utils;
using Sandbox.Game;
using VRageMath;
using VRage.Game.ModAPI;

namespace Faction_TerritoriesV2
{
    public static class Controls
    {
        public static bool _beaconControlsCreated;
        public static bool _jumpdriveControlsCreated;
        public static bool _jumpdriveActionsCreated;
        public static StringBuilder sb;

        public static IMyTerminalBlock currentBlock;

        public static void CreateBeaconControls(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (block as IMyBeacon == null) return;

            currentBlock = block;
            if (_beaconControlsCreated)
            {
                /*foreach (var control in controls)
                {
                    if (control.Id.Contains("PlayerToggleSpeed"))
                    {
                        var toggle = control as IMyTerminalControlCheckbox;
                        if (toggle == null) continue;
                        toggle.Title = MyStringId.GetOrCompute($"Add {Math.Round(ActionControls.GetProductionSpeed(currentBlock), 0)}% Production Speed:\n{(ActionControls.GetSpeedTokens(currentBlock))} Token(s)/{ActionControls.GetTimeToConsumeToken(currentBlock) / 60} minute(s)");
                        control.RedrawControl();
                    }

                    if (control.Id.Contains("PlayerToggleYield"))
                    {
                        var toggle = control as IMyTerminalControlCheckbox;
                        if (toggle == null) continue;
                        toggle.Title = MyStringId.GetOrCompute($"Add {Math.Round(ActionControls.GetProductionYield(currentBlock), 0)}% Production Yield:\n{(ActionControls.GetYieldTokens(currentBlock))} Token(s)/{ActionControls.GetTimeToConsumeToken(currentBlock) / 60} minute(s)");
                        control.RedrawControl();
                    }

                    if (control.Id.Contains("PlayerToggleEnergy"))
                    {
                        var toggle = control as IMyTerminalControlCheckbox;
                        if (toggle == null) continue;
                        toggle.Title = MyStringId.GetOrCompute($"Add {Math.Round(ActionControls.GetProductionEnergy(currentBlock), 0)}% Energy Efficiency:\n{(ActionControls.GetEnergyTokens(currentBlock))} Token(s)/{ActionControls.GetTimeToConsumeToken(currentBlock) / 60} minute(s)");
                        control.RedrawControl();
                    }

                    if (control.Id.Contains("Label"))
                    {
                        var label = control as IMyTerminalControlLabel;
                        if (label == null) continue;

                        string info = label.Label.ToString();

                        if (info.Contains("Consume"))
                        {
                            label.Label = MyStringId.GetOrCompute($"Consume {ActionControls.GetDelayCost(currentBlock)} tokens to extend\nsiege time by {ActionControls.GetSiegeDelayTime(currentBlock)} hours");
                            label.UpdateVisual();
                            control.RedrawControl();
                            continue;
                        }

                        if (info.Contains("Territory Center On:"))
                        {
                            label.Label = MyStringId.GetOrCompute($"Territory Center On: {ActionControls.GetPlanetName(currentBlock)}");
                            label.UpdateVisual();
                            control.RedrawControl();
                            continue;
                        }
                    }
                }*/

                return;
            }

            _beaconControlsCreated = true;

            foreach (var control in controls)
            {
                control.Visible = Block => ActionControls.HideVanilla(Block, control);
            }

            // Admin Controls
            var adminControls = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBeacon>("AdminControlsLabel");
            adminControls.Enabled = Block => true;
            adminControls.SupportsMultipleBlocks = false;
            adminControls.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
            adminControls.Label = MyStringId.GetOrCompute("=== Admin Only Visible Controls ===");
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(adminControls);
            controls.Add(adminControls);

            // Enable Switch
            var enableSwitch = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, IMyBeacon>("EnableSwitch");
            enableSwitch.Enabled = Block => ActionControls.IsClaimAndAdmin(Block);
            enableSwitch.SupportsMultipleBlocks = false;
            enableSwitch.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
            enableSwitch.Title = MyStringId.GetOrCompute("Enable Territory Switch");
            enableSwitch.OnText = MyStringId.GetOrCompute("On");
            enableSwitch.OffText = MyStringId.GetOrCompute("Off");
            enableSwitch.Getter = ActionControls.GetSwitchState;
            enableSwitch.Setter = ActionControls.SetSwitchState;
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(enableSwitch);
            controls.Add(enableSwitch);

            // Config Selection
            var config = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCombobox, IMyBeacon>("ConfigSelection");
            config.Enabled = Block => ActionControls.IsAdminAndEnabled(Block);
            config.SupportsMultipleBlocks = false;
            config.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
            config.Title = MyStringId.GetOrCompute("Select Territory Config");
            config.ComboBoxContent = ActionControls.GetTerritoryConfigs;
            config.Getter = Block => ActionControls.GetSelectedConfig(Block);
            config.Setter = ActionControls.SetSelectedConfig;
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(config);
            controls.Add(config);

            // Refresh Config
            var refreshConfig = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyBeacon>("RefreshConfig");
            refreshConfig.Enabled = Block => ActionControls.IsAdmin(Block);
            refreshConfig.SupportsMultipleBlocks = false;
            refreshConfig.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
            refreshConfig.Title = MyStringId.GetOrCompute("Refresh Config");
            refreshConfig.Tooltip = MyStringId.GetOrCompute("Refreshing the config if changes are made in the xml");
            refreshConfig.Action = Block => ActionControls.RefreshConfig(Block);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(refreshConfig);
            controls.Add(refreshConfig);

            // Faction List
            var factionList = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlListbox, IMyBeacon>("FactionList");
            factionList.Enabled = Block => ActionControls.IsAdmin(Block);
            factionList.SupportsMultipleBlocks = false;
            factionList.Visible = Block => ActionControls.IsAdmin(Block);
            factionList.Title = MyStringId.GetOrCompute("Select Faction To Assign Territory");
            factionList.ListContent = ActionControls.GetFactionList;
            factionList.VisibleRowsCount = 10;
            factionList.ItemSelected = ActionControls.SetSelectedFaction;
            factionList.Multiselect = false;
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(factionList);
            controls.Add(factionList);

            // Set Territory To Faction
            var setTerritory = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyBeacon>("SetTerritory");
            setTerritory.Enabled = Block => ActionControls.IsFactionSelected(Block);
            setTerritory.SupportsMultipleBlocks = false;
            setTerritory.Visible = Block => ActionControls.IsAdmin(Block);
            setTerritory.Title = MyStringId.GetOrCompute("Set Faction To Territory");
            setTerritory.Tooltip = MyStringId.GetOrCompute("Manually sets a faction to a territory");
            setTerritory.Action = Block => ActionControls.SetManualTerritory(Block);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(setTerritory);
            controls.Add(setTerritory);

            // Reset Claim Button
            var reset = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyBeacon>("Reset");
            reset.Enabled = Block => ActionControls.IsAdmin(Block);
            reset.SupportsMultipleBlocks = false;
            reset.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
            reset.Title = MyStringId.GetOrCompute("Reset Territory");
            reset.Tooltip = MyStringId.GetOrCompute("Resets the block data to unclaimed");
            reset.Action = Block => ActionControls.ResetTerritory(Block);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(reset);
            controls.Add(reset);

            // Discord Role Name
            /*var defaultName = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlTextbox, IMyBeacon>("DiscordName");
            defaultName.Enabled = Block => ActionControls.IsAdmin(Block);
            defaultName.SupportsMultipleBlocks = false;
            defaultName.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
            defaultName.Title = MyStringId.GetOrCompute("Discord Role Name");
            defaultName.Getter = Block => ActionControls.GetDefaultName(Block);
            defaultName.Setter = (Block, Builder) => ActionControls.SetDefaultName(Block, Builder);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(defaultName);
            controls.Add(defaultName);

            // Discord Id
            var discordId = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlTextbox, IMyBeacon>("DiscordId");
            discordId.Enabled = Block => ActionControls.IsAdmin(Block);
            discordId.SupportsMultipleBlocks = false;
            discordId.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
            discordId.Title = MyStringId.GetOrCompute("Discord Role Id");
            discordId.Getter = Block => ActionControls.GetDiscordId(Block);
            discordId.Setter = (Block, Builder) => ActionControls.SetDiscordId(Block, Builder);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(discordId);
            controls.Add(discordId);

            // Consumption Item
            var consumptionItem = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlTextbox, IMyBeacon>("ConsumptionItem");
            consumptionItem.Enabled = Block => ActionControls.IsAdminAndEnabled(Block);
            consumptionItem.SupportsMultipleBlocks = false;
            consumptionItem.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
            consumptionItem.Title = MyStringId.GetOrCompute("Consumption Item");
            consumptionItem.Getter = Block => ActionControls.GetConsumptionItem(Block);
            consumptionItem.Setter = (Block, Builder) => ActionControls.SetConsumptionItem(Block, Builder);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(consumptionItem);
            controls.Add(consumptionItem);

            // Consumption Item Valid Check
            var itemValid = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBeacon>("ValidItem");
            itemValid.Enabled = Block => true;
            itemValid.SupportsMultipleBlocks = false;
            itemValid.Visible = Block => ActionControls.IsConsumptionItemValid(Block);
            itemValid.Label = MyStringId.GetOrCompute("* That Item is NOT Valid *");
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(itemValid);
            controls.Add(itemValid);

            // Territory Maintain Cost
            var maintainCost = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("MaintainCost");
            maintainCost.Enabled = Block => ActionControls.IsAdminAndEnabled(Block);
            maintainCost.SupportsMultipleBlocks = false;
            maintainCost.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
            maintainCost.Title = MyStringId.GetOrCompute("Territory Maintain Cost");
            maintainCost.Tooltip = MyStringId.GetOrCompute("Admin only slider, sets the cost to maintain a territory");
            maintainCost.SetLimits((float)Math.Round(0d, 0), (float)Math.Round(10000d, 0));
            maintainCost.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetMaintainCost(Block), 0)} units");

            };
            maintainCost.Getter = Block => ActionControls.GetMaintainCost(Block);
            maintainCost.Setter = (Block, Value) => ActionControls.SetMaintainCost(Block, Value);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(maintainCost);
            controls.Add(maintainCost);

            // SafeZone Radius Slider
            var safezone = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("SafeZoneRadius");
            safezone.Enabled = Block => ActionControls.IsAdminAndEnabled(Block);
            safezone.SupportsMultipleBlocks = false;
            safezone.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
            safezone.Title = MyStringId.GetOrCompute("Safe Zone Radius");
            safezone.Tooltip = MyStringId.GetOrCompute("Admin only slider, sets the radius of the safe zone when territory is claimed");
            safezone.SetLimits((float)Math.Round(10d, 0), (float)Math.Round(5000d, 0));
            safezone.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetSafeZoneSlider(Block), 0)} m");

            };
            safezone.Getter = Block => ActionControls.GetSafeZoneSlider(Block);
            safezone.Setter = (Block, Value) => ActionControls.SetSafeZoneSlider(Block, Value);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(safezone);
            controls.Add(safezone);

            // Claim Radius Slider
            var claimArea = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("ClaimAreaRadius");
            claimArea.Enabled = Block => ActionControls.IsAdminAndEnabled(Block);
            claimArea.SupportsMultipleBlocks = false;
            claimArea.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
            claimArea.Title = MyStringId.GetOrCompute("Territory Area Radius");
            claimArea.Tooltip = MyStringId.GetOrCompute("Admin only slider, adjusts the territory area radius");
            claimArea.SetLimits((float)Math.Round(10d, 0), (float)Math.Round(500000d, 0));
            claimArea.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetClaimAreaSlider(Block), 0)} m");

            };
            claimArea.Getter = Block => ActionControls.GetClaimAreaSlider(Block);
            claimArea.Setter = (Block, Value) => ActionControls.SetClaimAreaSlider(Block, Value);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(claimArea);
            controls.Add(claimArea);

            // Combo UI Controls
            var combo = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCombobox, IMyBeacon>("ComboBox");
            combo.Enabled = Block => ActionControls.IsAdmin(Block);
            combo.SupportsMultipleBlocks = false;
            combo.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
            combo.Title = MyStringId.GetOrCompute("Select Control UI");
            combo.ComboBoxContent = ActionControls.GetControlsContent;
            combo.Getter = Block => ActionControls.GetSelectedControl(Block);
            combo.Setter = ActionControls.SetSelectedControl;
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(combo);
            controls.Add(combo);

            // =================== TERRITORY OPTIONS CONTROLS ========================

            // Territory Options Label
            var terriOptionsLabel = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBeacon>("TerritoryOptionLabel");
            terriOptionsLabel.Enabled = Block => true;
            terriOptionsLabel.SupportsMultipleBlocks = false;
            terriOptionsLabel.Visible = Block => ActionControls.IsTerritoryOptionsControls(Block);
            terriOptionsLabel.Label = MyStringId.GetOrCompute("--- Territory Option Configurations ---");
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(terriOptionsLabel);
            controls.Add(terriOptionsLabel);

            // Sep G
            var sepG = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyBeacon>("SepG");
            sepG.Enabled = Block => true;
            sepG.SupportsMultipleBlocks = false;
            sepG.Visible = Block => ActionControls.IsTerritoryOptionsControls(Block);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(sepG);
            controls.Add(sepG);

            // Center Territory To Planet
            var centerToggle = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("CenterToggle");
            centerToggle.Enabled = Block => ActionControls.IsAdminAndEnabled(Block);
            centerToggle.SupportsMultipleBlocks = false;
            centerToggle.Visible = Block => ActionControls.IsTerritoryOptionsControls(Block);
            centerToggle.Title = MyStringId.GetOrCompute("Center Territory To\nNearest Planet/Moon");
            centerToggle.Getter = Block => ActionControls.GetCenterToggle(Block);
            centerToggle.Setter = (Block, Builder) => ActionControls.SetCenterToggle(Block, Builder);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(centerToggle);
            controls.Add(centerToggle);

            // Planet Label
            var planetLabel = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBeacon>("PlanetLabel");
            planetLabel.Enabled = Block => true;
            planetLabel.SupportsMultipleBlocks = false;
            planetLabel.Visible = Block => ActionControls.IsCenterToPlanetEnabled(Block);
            planetLabel.Label = MyStringId.GetOrCompute($"Territory Center On: {ActionControls.GetPlanetName(currentBlock)}");
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(planetLabel);
            controls.Add(planetLabel);

            // Allow Allies SafeZone Choice
            var adminAllowAlliesSZ = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("AdminAllowAlliesSafeZone");
            adminAllowAlliesSZ.Enabled = Block => ActionControls.IsAdmin(Block);
            adminAllowAlliesSZ.SupportsMultipleBlocks = false;
            adminAllowAlliesSZ.Visible = Block => ActionControls.IsTerritoryOptionsControls(Block);
            adminAllowAlliesSZ.Title = MyStringId.GetOrCompute("Allow Claimers Safezone Allies");
            adminAllowAlliesSZ.Getter = Block => ActionControls.GetAdminSafeZoneAllies(Block);
            adminAllowAlliesSZ.Setter = (Block, Builder) => ActionControls.SetAdminSafeZoneAllies(Block, Builder);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(adminAllowAlliesSZ);
            controls.Add(adminAllowAlliesSZ);

            // Allow Allies In Territory Choice
            var adminAllowAlliesTerritory = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("AdminAllowAlliesTerritory");
            adminAllowAlliesTerritory.Enabled = Block => !ActionControls.IsAdminAllowAlliesEnabledSZ(Block);
            adminAllowAlliesTerritory.SupportsMultipleBlocks = false;
            adminAllowAlliesTerritory.Visible = Block => ActionControls.IsTerritoryOptionsControls(Block);
            adminAllowAlliesTerritory.Title = MyStringId.GetOrCompute("Allow Claimers Territory Allies");
            adminAllowAlliesTerritory.Getter = Block => ActionControls.GetAdminTerritoryAllies(Block);
            adminAllowAlliesTerritory.Setter = (Block, Builder) => ActionControls.SetAdminTerritoryAllies(Block, Builder);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(adminAllowAlliesTerritory);
            controls.Add(adminAllowAlliesTerritory);

            // Allow All Enemy Tools
            var allowTools = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("AdminAllowTools");
            allowTools.Enabled = Block => ActionControls.IsAdmin(Block);
            allowTools.SupportsMultipleBlocks = false;
            allowTools.Visible = Block => ActionControls.IsTerritoryOptionsControls(Block);
            allowTools.Title = MyStringId.GetOrCompute("Allow All Tools");
            allowTools.Getter = Block => ActionControls.GetAllowTools(Block);
            allowTools.Setter = (Block, Builder) => ActionControls.SetAllowTools(Block, Builder);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(allowTools);
            controls.Add(allowTools);

            // Allow Drilling
            var allowDrilling = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("AdminDrilling");
            allowDrilling.Enabled = Block => ActionControls.IsAllowToolsEnabled(Block);
            allowDrilling.SupportsMultipleBlocks = false;
            allowDrilling.Visible = Block => ActionControls.IsTerritoryOptionsControls(Block);
            allowDrilling.Title = MyStringId.GetOrCompute("Allow Drilling");
            allowDrilling.Getter = Block => ActionControls.GetAllowDrilling(Block);
            allowDrilling.Setter = (Block, Builder) => ActionControls.SetAllowDrilling(Block, Builder);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(allowDrilling);
            controls.Add(allowDrilling);

            // Allow Welding
            var allowWelding = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("AdminWelding");
            allowWelding.Enabled = Block => ActionControls.IsAllowToolsEnabled(Block);
            allowWelding.SupportsMultipleBlocks = false;
            allowWelding.Visible = Block => ActionControls.IsTerritoryOptionsControls(Block);
            allowWelding.Title = MyStringId.GetOrCompute("Allow Welding");
            allowWelding.Getter = Block => ActionControls.GetAllowWelding(Block);
            allowWelding.Setter = (Block, Builder) => ActionControls.SetAllowWelding(Block, Builder);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(allowWelding);
            controls.Add(allowWelding);

            // Allow Grinding
            var allowGrinding = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("AdminGrinding");
            allowGrinding.Enabled = Block => ActionControls.IsAllowToolsEnabled(Block);
            allowGrinding.SupportsMultipleBlocks = false;
            allowGrinding.Visible = Block => ActionControls.IsTerritoryOptionsControls(Block);
            allowGrinding.Title = MyStringId.GetOrCompute("Allow Grinding");
            allowGrinding.Getter = Block => ActionControls.GetAllowGrinding(Block);
            allowGrinding.Setter = (Block, Builder) => ActionControls.SetAllowGrinding(Block, Builder);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(allowGrinding);
            controls.Add(allowGrinding);

            // ======================= CLAIMING CONTROLS =============================

            // Claiming Label
            var claimLabel = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBeacon>("ClaimLabel");
            claimLabel.Enabled = Block => true;
            claimLabel.SupportsMultipleBlocks = false;
            claimLabel.Visible = Block => ActionControls.IsClaimingControls(Block);
            claimLabel.Label = MyStringId.GetOrCompute("--- Claiming Control Configurations ---");
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(claimLabel);
            controls.Add(claimLabel);

            // Sep C
            var sepC = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyBeacon>("SepC");
            sepC.Enabled = Block => true;
            sepC.SupportsMultipleBlocks = false;
            sepC.Visible = Block => ActionControls.IsClaimingControls(Block);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(sepC);
            controls.Add(sepC);

            // ToClaim Slider
            var toClaimTime = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("ToClaimTime");
            toClaimTime.Enabled = Block => ActionControls.IsAdmin(Block);
            toClaimTime.SupportsMultipleBlocks = false;
            toClaimTime.Visible = Block => ActionControls.IsClaimingControls(Block);
            toClaimTime.Title = MyStringId.GetOrCompute("Time To Claim");
            toClaimTime.Tooltip = MyStringId.GetOrCompute("Sets the time it takes to claim a territory");
            toClaimTime.SetLimits((float)Math.Round(1d, 0), (float)Math.Round(3600d, 0));
            toClaimTime.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetToClaimTime(Block), 0)} second(s)");

            };
            toClaimTime.Getter = Block => ActionControls.GetToClaimTime(Block);
            toClaimTime.Setter = (Block, Value) => ActionControls.SetToClaimTime(Block, Value);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(toClaimTime);
            controls.Add(toClaimTime);

            // Tokens ToClaim Slider
            var tokensToClaim = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("TokensToClaim");
            tokensToClaim.Enabled = Block => ActionControls.IsAdmin(Block);
            tokensToClaim.SupportsMultipleBlocks = false;
            tokensToClaim.Visible = Block => ActionControls.IsClaimingControls(Block);
            tokensToClaim.Title = MyStringId.GetOrCompute("Tokens To Claim");
            tokensToClaim.Tooltip = MyStringId.GetOrCompute("Sets the amount of tokens required to claim territory");
            tokensToClaim.SetLimits((float)Math.Round(0d, 0), (float)Math.Round(10000d, 0));
            tokensToClaim.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetTokensToClaim(Block), 0)} token(s)");

            };
            tokensToClaim.Getter = Block => ActionControls.GetTokensToClaim(Block);
            tokensToClaim.Setter = (Block, Value) => ActionControls.SetTokensToClaim(Block, Value);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(tokensToClaim);
            controls.Add(tokensToClaim);

            // Consume Token Timer Slider
            var consumeTokenTimer = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("ConsumeTokenTimer");
            consumeTokenTimer.Enabled = Block => ActionControls.IsAdmin(Block);
            consumeTokenTimer.SupportsMultipleBlocks = false;
            consumeTokenTimer.Visible = Block => ActionControls.IsClaimingControls(Block);
            consumeTokenTimer.Title = MyStringId.GetOrCompute("Time To Consume\n    Token");
            consumeTokenTimer.Tooltip = MyStringId.GetOrCompute("Sets the amount of time before a token is consumed");
            consumeTokenTimer.SetLimits((float)Math.Round(1d, 0), (float)Math.Round(3600d, 0));
            consumeTokenTimer.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetTimeToConsumeToken(Block), 0)} second(s)");

            };
            consumeTokenTimer.Getter = Block => ActionControls.GetTimeToConsumeToken(Block);
            consumeTokenTimer.Setter = (Block, Value) => ActionControls.SetTimeToConsumeToken(Block, Value);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(consumeTokenTimer);
            controls.Add(consumeTokenTimer);

            // Update Token Timer
            var updateTokenTimer = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyBeacon>("UpdateTokenTimer");
            updateTokenTimer.Enabled = Block => ActionControls.IsAdmin(Block);
            updateTokenTimer.SupportsMultipleBlocks = false;
            updateTokenTimer.Visible = Block => ActionControls.IsClaimingControls(Block);
            updateTokenTimer.Title = MyStringId.GetOrCompute("Reset Token Timer");
            updateTokenTimer.Tooltip = MyStringId.GetOrCompute("Resets elapsed time to the defined amount");
            updateTokenTimer.Action = Block => ActionControls.SetTokenTimer(Block);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(updateTokenTimer);
            controls.Add(updateTokenTimer);

            // ToClaim Distance Slider
            var claimDistance = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("ClaimDistance");
            claimDistance.Enabled = Block => ActionControls.IsAdmin(Block);
            claimDistance.SupportsMultipleBlocks = false;
            claimDistance.Visible = Block => ActionControls.IsClaimingControls(Block);
            claimDistance.Title = MyStringId.GetOrCompute("To Claim Distance");
            claimDistance.Tooltip = MyStringId.GetOrCompute("Sets the distance required to claim in meters");
            claimDistance.SetLimits((float)Math.Round(10d, 0), (float)Math.Round(5000d, 0));
            claimDistance.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetDistanceToClaim(Block), 0)} m");

            };
            claimDistance.Getter = Block => ActionControls.GetDistanceToClaim(Block);
            claimDistance.Setter = (Block, Value) => ActionControls.SetDistanceToClaim(Block, Value);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(claimDistance);
            controls.Add(claimDistance);

            // GPS Update Slider
            var gpsUpdate = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("GpsUpdate");
            gpsUpdate.Enabled = Block => ActionControls.IsAdmin(Block);
            gpsUpdate.SupportsMultipleBlocks = false;
            gpsUpdate.Visible = Block => ActionControls.IsClaimingControls(Block);
            gpsUpdate.Title = MyStringId.GetOrCompute("Gps Update");
            gpsUpdate.Tooltip = MyStringId.GetOrCompute("Sets how often to update enemey gps markers (in seconds)");
            gpsUpdate.SetLimits((float)Math.Round(0d, 0), (float)Math.Round(30d, 0));
            gpsUpdate.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetGpsUpdate(Block), 0)} seconds(s)");

            };
            gpsUpdate.Getter = Block => ActionControls.GetGpsUpdate(Block);
            gpsUpdate.Setter = (Block, Value) => ActionControls.SetGpsUpdate(Block, Value);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(gpsUpdate);
            controls.Add(gpsUpdate);

            // ====================== SIEGING CONTROLS ===============================

            // Sieging Label
            var siegeLabel = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBeacon>("SiegeLabel");
            siegeLabel.Enabled = Block => true;
            siegeLabel.SupportsMultipleBlocks = false;
            siegeLabel.Visible = Block => ActionControls.IsSiegeControls(Block);
            siegeLabel.Label = MyStringId.GetOrCompute("--- Sieging Control Configurations ---");
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(siegeLabel);
            controls.Add(siegeLabel);

            // Sep D
            var sepD = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyBeacon>("SepD");
            sepD.Enabled = Block => true;
            sepD.SupportsMultipleBlocks = false;
            sepD.Visible = Block => ActionControls.IsSiegeControls(Block);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(sepD);
            controls.Add(sepD);

            // ToSiege Time Slider
            var toSiegeTime = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("ToSiegeTime");
            toSiegeTime.Enabled = Block => ActionControls.IsAdmin(Block);
            toSiegeTime.SupportsMultipleBlocks = false;
            toSiegeTime.Visible = Block => ActionControls.IsSiegeControls(Block);
            toSiegeTime.Title = MyStringId.GetOrCompute("Time To Init Siege");
            toSiegeTime.Tooltip = MyStringId.GetOrCompute("Sets the time it takes to init a siege in seconds");
            toSiegeTime.SetLimits((float)Math.Round(1d, 0), (float)Math.Round(3600d, 0));
            toSiegeTime.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetToSiegeTime(Block), 0)} second(s)");

            };
            toSiegeTime.Getter = Block => ActionControls.GetToSiegeTime(Block);
            toSiegeTime.Setter = (Block, Value) => ActionControls.SetToSiegeTime(Block, Value);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(toSiegeTime);
            controls.Add(toSiegeTime);

            // Tokens ToSiege Slider
            var tokensToSiege = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("TokensToSiege");
            tokensToSiege.Enabled = Block => ActionControls.IsAdmin(Block);
            tokensToSiege.SupportsMultipleBlocks = false;
            tokensToSiege.Visible = Block => ActionControls.IsSiegeControls(Block);
            tokensToSiege.Title = MyStringId.GetOrCompute("Tokens To Init Siege");
            tokensToSiege.Tooltip = MyStringId.GetOrCompute("Sets the amount of tokens required to init a siege");
            tokensToSiege.SetLimits((float)Math.Round(0d, 0), (float)Math.Round(10000d, 0));
            tokensToSiege.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetTokensToSiege(Block), 0)} token(s)");

            };
            tokensToSiege.Getter = Block => ActionControls.GetTokensToSiege(Block);
            tokensToSiege.Setter = (Block, Value) => ActionControls.SetTokensToSiege(Block, Value);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(tokensToSiege);
            controls.Add(tokensToSiege);

            // Final Siege Slider
            var finalSiege = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("FinalSiege");
            finalSiege.Enabled = Block => ActionControls.IsAdmin(Block);
            finalSiege.SupportsMultipleBlocks = false;
            finalSiege.Visible = Block => ActionControls.IsSiegeControls(Block);
            finalSiege.Title = MyStringId.GetOrCompute("Time To Final Siege");
            finalSiege.Tooltip = MyStringId.GetOrCompute("Sets the time it takes to final siege in seconds");
            finalSiege.SetLimits((float)Math.Round(1d, 0), (float)Math.Round(3600d, 0));
            finalSiege.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetToSiegeTimeFinal(Block), 0)} second(s)");

            };
            finalSiege.Getter = Block => ActionControls.GetToSiegeTimeFinal(Block);
            finalSiege.Setter = (Block, Value) => ActionControls.SetToSiegeTimeFinal(Block, Value);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(finalSiege);
            controls.Add(finalSiege);

            // Final Siege Tokens
            var finalSiegeTokens = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("FinalSiegeTokens");
            finalSiegeTokens.Enabled = Block => ActionControls.IsAdmin(Block);
            finalSiegeTokens.SupportsMultipleBlocks = false;
            finalSiegeTokens.Visible = Block => ActionControls.IsSiegeControls(Block);
            finalSiegeTokens.Title = MyStringId.GetOrCompute("Tokens To Final Siege");
            finalSiegeTokens.Tooltip = MyStringId.GetOrCompute("Sets the amount of tokens required to final siege");
            finalSiegeTokens.SetLimits((float)Math.Round(0d, 0), (float)Math.Round(10000d, 0));
            finalSiegeTokens.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetTokensToSiegeFinal(Block), 0)} token(s)");

            };
            finalSiegeTokens.Getter = Block => ActionControls.GetTokensToSiegeFinal(Block);
            finalSiegeTokens.Setter = (Block, Value) => ActionControls.SetTokensToSiegeFinal(Block, Value);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(finalSiegeTokens);
            controls.Add(finalSiegeTokens);

            // ToSiege Distance Slider
            var siegeDistance = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("SiegeDistance");
            siegeDistance.Enabled = Block => ActionControls.IsAdmin(Block);
            siegeDistance.SupportsMultipleBlocks = false;
            siegeDistance.Visible = Block => ActionControls.IsSiegeControls(Block);
            siegeDistance.Title = MyStringId.GetOrCompute("To Siege Distance");
            siegeDistance.Tooltip = MyStringId.GetOrCompute("Sets the distance required to siege in meters");
            siegeDistance.SetLimits((float)Math.Round(10d, 0), (float)Math.Round(5000d, 0));
            siegeDistance.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetDistanceToSiege(Block), 0)} m");

            };
            siegeDistance.Getter = Block => ActionControls.GetDistanceToSiege(Block);
            siegeDistance.Setter = (Block, Value) => ActionControls.SetDistanceToSiege(Block, Value);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(siegeDistance);
            controls.Add(siegeDistance);

            // NotifactionFreq
            var notifactionFreq = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("NotificationFreq");
            notifactionFreq.Enabled = Block => ActionControls.IsAdmin(Block);
            notifactionFreq.SupportsMultipleBlocks = false;
            notifactionFreq.Visible = Block => ActionControls.IsSiegeControls(Block);
            notifactionFreq.Title = MyStringId.GetOrCompute("Siege Warning\nNotification Frequency");
            notifactionFreq.Tooltip = MyStringId.GetOrCompute("Sets how often chat/discord warns everyone that the territory is being sieged (in minutes)");
            notifactionFreq.SetLimits((float)Math.Round(5d, 0), (float)Math.Round(30d, 0));
            notifactionFreq.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetSiegeNotificationFreq(Block), 0)} minute(s)");

            };
            notifactionFreq.Getter = Block => ActionControls.GetSiegeNotificationFreq(Block);
            notifactionFreq.Setter = (Block, Value) => ActionControls.SetSiegeNotificationFreq(Block, Value);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(notifactionFreq);
            controls.Add(notifactionFreq);

            // Territory Deactivation Time Slider
            var deactivationTime = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("DeactivationTime");
            deactivationTime.Enabled = Block => ActionControls.IsAdmin(Block);
            deactivationTime.SupportsMultipleBlocks = false;
            deactivationTime.Visible = Block => ActionControls.IsSiegeControls(Block);
            deactivationTime.Title = MyStringId.GetOrCompute("Siege Countdown\nTimer");
            deactivationTime.Tooltip = MyStringId.GetOrCompute("Sets the time it takes after a successful siege before final siege can start (in minutes)");
            deactivationTime.SetLimits((float)Math.Round(0d, 0), (float)Math.Round(1440d, 0));
            deactivationTime.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetDeactivationTime(Block), 0)} minute(s)");

            };
            deactivationTime.Getter = Block => ActionControls.GetDeactivationTime(Block);
            deactivationTime.Setter = (Block, Value) => ActionControls.SetDeactivationTime(Block, Value);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(deactivationTime);
            controls.Add(deactivationTime);

            // TimeFrame to Final Siege
            var siegeGap = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("SiegeGap");
            siegeGap.Enabled = Block => ActionControls.IsAdmin(Block);
            siegeGap.SupportsMultipleBlocks = false;
            siegeGap.Visible = Block => ActionControls.IsSiegeControls(Block);
            siegeGap.Title = MyStringId.GetOrCompute("Siege Gap");
            siegeGap.Tooltip = MyStringId.GetOrCompute("Timeframe given to start final siege (minutes)");
            siegeGap.SetLimits((float)Math.Round(0d, 0), (float)Math.Round(60d, 0));
            siegeGap.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetSiegeGapTime(Block), 0)} minute(s)");

            };
            siegeGap.Getter = Block => ActionControls.GetSiegeGapTime(Block);
            siegeGap.Setter = (Block, Value) => ActionControls.SetSiegeGapTime(Block, Value);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(siegeGap);
            controls.Add(siegeGap);

            // Siege Failure Cooldown
            var siegeCooldown = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("SiegeCooldown");
            siegeCooldown.Enabled = Block => ActionControls.IsAdmin(Block);
            siegeCooldown.SupportsMultipleBlocks = false;
            siegeCooldown.Visible = Block => ActionControls.IsSiegeControls(Block);
            siegeCooldown.Title = MyStringId.GetOrCompute("Siege Fail Cooldown");
            siegeCooldown.Tooltip = MyStringId.GetOrCompute("Sets the time to cooldown after a failed final siege before it can be sieged again (minutes)");
            siegeCooldown.SetLimits((float)Math.Round(0d, 0), (float)Math.Round(7200d, 0));
            siegeCooldown.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetSiegeCooldownTime(Block), 0)} minute(s)");

            };
            siegeCooldown.Getter = Block => ActionControls.GetSiegeCooldownTime(Block);
            siegeCooldown.Setter = (Block, Value) => ActionControls.SetSiegeCooldownTime(Block, Value);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(siegeCooldown);
            controls.Add(siegeCooldown);

            // Cooldown
            var cooldown = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("Cooldown");
            cooldown.Enabled = Block => ActionControls.IsAdmin(Block);
            cooldown.SupportsMultipleBlocks = false;
            cooldown.Visible = Block => ActionControls.IsSiegeControls(Block);
            cooldown.Title = MyStringId.GetOrCompute("Cooldown Timer");
            cooldown.Tooltip = MyStringId.GetOrCompute("Sets the time to cooldown after a successful final siege before territory can be claimed again (minutes)");
            cooldown.SetLimits((float)Math.Round(0d, 0), (float)Math.Round(1440d, 0));
            cooldown.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetCooldownTime(Block), 0)} minute(s)");

            };
            cooldown.Getter = Block => ActionControls.GetCooldownTime(Block);
            cooldown.Setter = (Block, Value) => ActionControls.SetCooldownTime(Block, Value);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(cooldown);
            controls.Add(cooldown);

            // Tokens To Delay Siege
            var tokenDelay = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("TokenDelay");
            tokenDelay.Enabled = Block => ActionControls.IsAdmin(Block);
            tokenDelay.SupportsMultipleBlocks = false;
            tokenDelay.Visible = Block => ActionControls.IsSiegeControls(Block);
            tokenDelay.Title = MyStringId.GetOrCompute("Tokens To Siege Delay");
            tokenDelay.Tooltip = MyStringId.GetOrCompute("Sets the cost of tokens to delay siege time");
            tokenDelay.SetLimits((float)Math.Round(0d, 0), (float)Math.Round(10000d, 0));
            tokenDelay.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetTokenSiegeDelay(Block), 0)} token(s)");

            };
            tokenDelay.Getter = Block => ActionControls.GetTokenSiegeDelay(Block);
            tokenDelay.Setter = (Block, Value) => ActionControls.SetTokenSiegeDelay(Block, Value);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(tokenDelay);
            controls.Add(tokenDelay);

            // # Times Siege Can Be Delayed
            var siegeDelayedCount = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("SiegeDelayCount");
            siegeDelayedCount.Enabled = Block => ActionControls.IsAdmin(Block);
            siegeDelayedCount.SupportsMultipleBlocks = false;
            siegeDelayedCount.Visible = Block => ActionControls.IsSiegeControls(Block);
            siegeDelayedCount.Title = MyStringId.GetOrCompute("Siege Delay Count");
            siegeDelayedCount.Tooltip = MyStringId.GetOrCompute("Sets how many times a siege can be delayed");
            siegeDelayedCount.SetLimits((float)Math.Round(0d, 0), (float)Math.Round(10d, 0));
            siegeDelayedCount.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetSiegeDelayCount(Block), 0)}");

            };
            siegeDelayedCount.Getter = Block => ActionControls.GetSiegeDelayCount(Block);
            siegeDelayedCount.Setter = (Block, Value) => ActionControls.SetSiegeDelayCount(Block, Value);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(siegeDelayedCount);
            controls.Add(siegeDelayedCount);

            // Siege Delay Time
            var siegeDelayedTime = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("SiegeDelayTime");
            siegeDelayedTime.Enabled = Block => ActionControls.IsAdmin(Block);
            siegeDelayedTime.SupportsMultipleBlocks = false;
            siegeDelayedTime.Visible = Block => ActionControls.IsSiegeControls(Block);
            siegeDelayedTime.Title = MyStringId.GetOrCompute("Siege Delay Time");
            siegeDelayedTime.Tooltip = MyStringId.GetOrCompute("Sets how long (in hours) to delay a siege");
            siegeDelayedTime.SetLimits((float)Math.Round(0d, 0), (float)Math.Round(24d, 0));
            siegeDelayedTime.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{ActionControls.GetSiegeDelayTime(Block)} hour(s)");

            };
            siegeDelayedTime.Getter = Block => ActionControls.GetSiegeDelayTime(Block);
            siegeDelayedTime.Setter = (Block, Value) => ActionControls.SetSiegeDelayTime(Block, Value);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(siegeDelayedTime);
            controls.Add(siegeDelayedTime);

            // ===================== MISC CONTROLS ==============================

            // Misc Label
            var miscLabel = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBeacon>("MiscLabel");
            miscLabel.Enabled = Block => true;
            miscLabel.SupportsMultipleBlocks = false;
            miscLabel.Visible = Block => ActionControls.IsMiscControls(Block);
            miscLabel.Label = MyStringId.GetOrCompute("--- Misc ---");
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(miscLabel);
            controls.Add(miscLabel);

            

            // ===================== PERKS CONTROLS ============================

            // Perks Label
            var perksLabel = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBeacon>("PerksLabel");
            perksLabel.Enabled = Block => true;
            perksLabel.SupportsMultipleBlocks = false;
            perksLabel.Visible = Block => ActionControls.IsPerkControls(Block);
            perksLabel.Label = MyStringId.GetOrCompute("--- Perks ---");
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(perksLabel);
            controls.Add(perksLabel);

            // Sep E
            var sepE = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyBeacon>("SepE");
            sepE.Enabled = Block => true;
            sepE.SupportsMultipleBlocks = false;
            sepE.Visible = Block => ActionControls.IsPerkControls(Block);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(sepE);
            controls.Add(sepE);

            // Combo Perk Types
            var comboPerkType = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCombobox, IMyBeacon>("ComboPerkType");
            comboPerkType.Enabled = Block => ActionControls.IsAdmin(Block);
            comboPerkType.SupportsMultipleBlocks = false;
            comboPerkType.Visible = Block => ActionControls.IsPerkControls(Block);
            comboPerkType.Title = MyStringId.GetOrCompute("Select Perk Type");
            comboPerkType.ComboBoxContent = ActionControls.GetPerkTypeContent;
            comboPerkType.Getter = Block => ActionControls.GetPerkType(Block);
            comboPerkType.Setter = ActionControls.SetPerkType;
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(comboPerkType);
            controls.Add(comboPerkType);

            // Production Toggle
            var productionToggle = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("ProductionToggle");
            productionToggle.Enabled = Block => ActionControls.IsAdmin(Block);
            productionToggle.SupportsMultipleBlocks = false;
            productionToggle.Visible = Block => ActionControls.IsPerkType(Block, PerkTypeList.Production);
            productionToggle.Title = MyStringId.GetOrCompute("Enable Production Additive Perk");
            productionToggle.Getter = Block => ActionControls.GetProductionEnabled(Block);
            productionToggle.Setter = (Block, Builder) => ActionControls.SetProductionEnabled(Block, Builder);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(productionToggle);
            controls.Add(productionToggle);

            // Production Speed
            var productionSpeed = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("ProductionSpeed");
            productionSpeed.Enabled = Block => ActionControls.IsAdminProductionEnabled(Block);
            productionSpeed.SupportsMultipleBlocks = false;
            productionSpeed.Visible = Block => ActionControls.IsAdminProductionEnabled(Block);
            productionSpeed.Title = MyStringId.GetOrCompute("Production Speed\nAdditive");
            productionSpeed.Tooltip = MyStringId.GetOrCompute("Adds additional production speed to refineries and assembliers (inside territory) to claimed faction");
            productionSpeed.SetLimits((float)Math.Round(0d, 2), (float)Math.Round(500d, 2));
            productionSpeed.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetProductionSpeed(Block), 0)} %");

            };
            productionSpeed.Getter = Block => ActionControls.GetProductionSpeed(Block);
            productionSpeed.Setter = (Block, Value) => ActionControls.SetProductionSpeed(Block, Value);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(productionSpeed);
            controls.Add(productionSpeed);

            // Production Yield
            var productionYield = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("ProductionYield");
            productionYield.Enabled = Block => ActionControls.IsAdminProductionEnabled(Block);
            productionYield.SupportsMultipleBlocks = false;
            productionYield.Visible = Block => ActionControls.IsAdminProductionEnabled(Block);
            productionYield.Title = MyStringId.GetOrCompute("Production Yield\nAdditive");
            productionYield.Tooltip = MyStringId.GetOrCompute("Adds additional production yield to refineries (inside territory) to claimed faction");
            productionYield.SetLimits((float)Math.Round(0d, 2), (float)Math.Round(500d, 2));
            productionYield.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetProductionYield(Block), 0)} %");

            };
            productionYield.Getter = Block => ActionControls.GetProductionYield(Block);
            productionYield.Setter = (Block, Value) => ActionControls.SetProductionYield(Block, Value);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(productionYield);
            controls.Add(productionYield);

            // Production Energy
            var productionEnergy = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyBeacon>("ProductionEnergy");
            productionEnergy.Enabled = Block => ActionControls.IsAdminProductionEnabled(Block);
            productionEnergy.SupportsMultipleBlocks = false;
            productionEnergy.Visible = Block => ActionControls.IsAdminProductionEnabled(Block);
            productionEnergy.Title = MyStringId.GetOrCompute("Production Energy Efficiency\nAdditive");
            productionEnergy.Tooltip = MyStringId.GetOrCompute("Adds additional production energy efficiency to refineries and assembliers (inside territory) to claimed faction");
            productionEnergy.SetLimits((float)Math.Round(0d, 2), (float)Math.Round(500d, 2));
            productionEnergy.Writer = (Block, Builder) =>
            {

                Builder.Clear();
                Builder.Append($"{Math.Round(ActionControls.GetProductionEnergy(Block), 0)} %");

            };
            productionEnergy.Getter = Block => ActionControls.GetProductionEnergy(Block);
            productionEnergy.Setter = (Block, Value) => ActionControls.SetProductionEnergy(Block, Value);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(productionEnergy);
            controls.Add(productionEnergy);

            // Apply Production Values
            var setProduction = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyBeacon>("SetProduction");
            setProduction.Enabled = Block => ActionControls.IsAdminProductionEnabled(Block);
            setProduction.SupportsMultipleBlocks = false;
            setProduction.Visible = Block => ActionControls.IsAdminProductionEnabled(Block);
            setProduction.Title = MyStringId.GetOrCompute("Update Production Values");
            setProduction.Tooltip = MyStringId.GetOrCompute("Sets the values to all production blocks for claim faction (inside territory)");
            setProduction.Action = Block => ActionControls.SetProduction(Block);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(setProduction);
            controls.Add(setProduction);

            // Allow Standalone Production
            var standaloneToggle = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("StandAloneToggle");
            standaloneToggle.Enabled = Block => ActionControls.CheckPlayerControls(Block);
            standaloneToggle.SupportsMultipleBlocks = false;
            standaloneToggle.Visible = Block => ActionControls.IsAdminProductionEnabled(Block);
            standaloneToggle.Title = MyStringId.GetOrCompute("Enable StandAlone");
            standaloneToggle.Tooltip = MyStringId.GetOrCompute("Enables the production perk to the values defined with no cost (no player interaction to controls)");
            standaloneToggle.Getter = Block => ActionControls.GetStandAloneEnabled(Block);
            standaloneToggle.Setter = (Block, Builder) => ActionControls.SetStandAloneEnabled(Block, Builder);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(standaloneToggle);
            controls.Add(standaloneToggle);

            // Allow Player Control Speed
            var playerSpeed = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("PlayerControlSpeed");
            playerSpeed.Enabled = Block => ActionControls.CheckStandAloneEnabled(Block);
            playerSpeed.SupportsMultipleBlocks = false;
            playerSpeed.Visible = Block => ActionControls.IsAdminProductionEnabled(Block);
            playerSpeed.Title = MyStringId.GetOrCompute("Enable Player Speed Control");
            playerSpeed.Tooltip = MyStringId.GetOrCompute("Allows the player choose if they want to add the production speed perk");
            playerSpeed.Getter = Block => ActionControls.GetPlayerControlSpeed(Block);
            playerSpeed.Setter = (Block, Builder) => ActionControls.SetPlayerControlSpeed(Block, Builder);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(playerSpeed);
            controls.Add(playerSpeed);

            // Speed Control Tokens
            var speedTokens = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlTextbox, IMyBeacon>("SpeedTokens");
            speedTokens.Enabled = Block => ActionControls.CheckStandAloneEnabled(Block);
            speedTokens.SupportsMultipleBlocks = false;
            speedTokens.Visible = Block => ActionControls.GetPlayerControlSpeed(Block);
            speedTokens.Title = MyStringId.GetOrCompute("Speed Token Cost");
            speedTokens.Getter = Block => ActionControls.GetSpeedTokens(Block);
            speedTokens.Setter = (Block, Builder) => ActionControls.SetSpeedTokens(Block, Builder);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(speedTokens);
            controls.Add(speedTokens);

            // Allow Player Control Yield
            var playerYield = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("PlayerControlYield");
            playerYield.Enabled = Block => ActionControls.CheckStandAloneEnabled(Block);
            playerYield.SupportsMultipleBlocks = false;
            playerYield.Visible = Block => ActionControls.IsAdminProductionEnabled(Block);
            playerYield.Title = MyStringId.GetOrCompute("Enable Player Yield Control   ");
            playerYield.Tooltip = MyStringId.GetOrCompute("Allows the player choose if they want to add the production yield perk");
            playerYield.Getter = Block => ActionControls.GetPlayerControlYield(Block);
            playerYield.Setter = (Block, Builder) => ActionControls.SetPlayerControlYield(Block, Builder);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(playerYield);
            controls.Add(playerYield);

            // Yield Control Tokens
            var yieldTokens = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlTextbox, IMyBeacon>("YieldTokens");
            yieldTokens.Enabled = Block => ActionControls.CheckStandAloneEnabled(Block);
            yieldTokens.SupportsMultipleBlocks = false;
            yieldTokens.Visible = Block => ActionControls.GetPlayerControlYield(Block);
            yieldTokens.Title = MyStringId.GetOrCompute("Yield Token Cost");
            yieldTokens.Getter = Block => ActionControls.GetYieldTokens(Block);
            yieldTokens.Setter = (Block, Builder) => ActionControls.SetYieldTokens(Block, Builder);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(yieldTokens);
            controls.Add(yieldTokens);

            // Allow Player Control Energy
            var playerEnergy = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("PlayerControlEnergy");
            playerEnergy.Enabled = Block => ActionControls.CheckStandAloneEnabled(Block);
            playerEnergy.SupportsMultipleBlocks = false;
            playerEnergy.Visible = Block => ActionControls.IsAdminProductionEnabled(Block);
            playerEnergy.Title = MyStringId.GetOrCompute("Enable Player Energy Control");
            playerEnergy.Tooltip = MyStringId.GetOrCompute("Allows the player choose if they want to add the production energy efficiency perk");
            playerEnergy.Getter = Block => ActionControls.GetPlayerControlEnergy(Block);
            playerEnergy.Setter = (Block, Builder) => ActionControls.SetPlayerControlEnergy(Block, Builder);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(playerEnergy);
            controls.Add(playerEnergy);

            // Energy Control Tokens
            var energyTokens = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlTextbox, IMyBeacon>("EnergyTokens");
            energyTokens.Enabled = Block => ActionControls.CheckStandAloneEnabled(Block);
            energyTokens.SupportsMultipleBlocks = false;
            energyTokens.Visible = Block => ActionControls.GetPlayerControlEnergy(Block);
            energyTokens.Title = MyStringId.GetOrCompute("Energy Token Cost");
            energyTokens.Getter = Block => ActionControls.GetEnergyTokens(Block);
            energyTokens.Setter = (Block, Builder) => ActionControls.SetEnergyTokens(Block, Builder);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(energyTokens);
            controls.Add(energyTokens);

            // Sep A
            var sepA = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyBeacon>("SepA");
            sepA.Enabled = Block => true;
            sepA.SupportsMultipleBlocks = false;
            sepA.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(sepA);
            controls.Add(sepA);

            // Sep B
            var sepB = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyBeacon>("SepB");
            sepB.Enabled = Block => true;
            sepB.SupportsMultipleBlocks = false;
            sepB.Visible = Block => ActionControls.IsClaimAndAdmin(Block);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(sepB);
            controls.Add(sepB);*/

            // =========================== NON-ADMIN CONTROLS ==============================

            // Player Controls
            var playerControls = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBeacon>("PlayerControlsLabel");
            playerControls.Enabled = Block => true;
            playerControls.SupportsMultipleBlocks = false;
            playerControls.Visible = Block => ActionControls.IsClaimBlock(Block);
            playerControls.Label = MyStringId.GetOrCompute("=== Player Controls ===");
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(playerControls);
            controls.Add(playerControls);

            // Delay Siege Label
            /*var delaySiegeLabel = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBeacon>("DelaySiegeLabel");
            delaySiegeLabel.Enabled = Block => true;
            delaySiegeLabel.SupportsMultipleBlocks = false;
            delaySiegeLabel.Visible = Block => ActionControls.IsClaimBlock(Block);
            delaySiegeLabel.Label = MyStringId.GetOrCompute($"Consume {ActionControls.GetDelayCost(currentBlock)} tokens to extend\nsiege time by {ActionControls.GetSiegeDelayTime(currentBlock)} hours.\n*Enabled only when siege time\nhas more than\n1hr left.*");
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(delaySiegeLabel);
            controls.Add(delaySiegeLabel);

            // Delay Siege Button
            var delaySiege = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyBeacon>("DelaySiege");
            delaySiege.Enabled = Block => ActionControls.CheckIsSieged(Block);
            delaySiege.SupportsMultipleBlocks = false;
            delaySiege.Visible = Block => ActionControls.IsClaimBlock(Block);
            delaySiege.Title = MyStringId.GetOrCompute("Delay Siege Time");
            delaySiege.Tooltip = MyStringId.GetOrCompute("Delays the siege time for the cost of tokens, only active if the siege timer is > 1hr");
            delaySiege.Action = Block => ActionControls.DelaySiege(Block);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(delaySiege);*/

            // Player Allow SafeZone Allies
            var playerAllowAlliesSZ = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("PlayerAllowAlliesSZ");
            playerAllowAlliesSZ.Enabled = Block => ActionControls.IsClaimedAndFaction(Block);
            playerAllowAlliesSZ.SupportsMultipleBlocks = false;
            playerAllowAlliesSZ.Visible = Block => ActionControls.IsClaimBlock(Block);
            playerAllowAlliesSZ.Title = MyStringId.GetOrCompute($"Allow SafeZone Allies");
            playerAllowAlliesSZ.Tooltip = MyStringId.GetOrCompute("If enabled, will allow your allies inside the safe zone");
            playerAllowAlliesSZ.Getter = Block => ActionControls.GetPlayerAllowAlliesSZ(Block);
            playerAllowAlliesSZ.Setter = (Block, Builder) => ActionControls.SetPlayerAllowAlliesSZ(Block, Builder);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(playerAllowAlliesSZ);
            controls.Add(playerAllowAlliesSZ);

            // Player Allow Territory Allies
            var playerAllowAlliesTerritory = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("PlayerAllowAlliesTerritory");
            playerAllowAlliesTerritory.Enabled = Block => ActionControls.IsClaimedAndFaction(Block);
            playerAllowAlliesTerritory.SupportsMultipleBlocks = false;
            playerAllowAlliesTerritory.Visible = Block => ActionControls.IsClaimBlock(Block);
            playerAllowAlliesTerritory.Title = MyStringId.GetOrCompute($"Allow Territory Allies");
            playerAllowAlliesTerritory.Tooltip = MyStringId.GetOrCompute("If enabled, will allow your allies inside the territory to use tools/not be alerted");
            playerAllowAlliesTerritory.Getter = Block => ActionControls.GetPlayerAllowAlliesTerritory(Block);
            playerAllowAlliesTerritory.Setter = (Block, Builder) => ActionControls.SetPlayerAllowAlliesTerritory(Block, Builder);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(playerAllowAlliesTerritory);
            controls.Add(playerAllowAlliesTerritory);

            // SafeZone Switch
            var szSwitch = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, IMyBeacon>("SafeZoneSwitch");
            szSwitch.Enabled = Block => ActionControls.IsOffAndSieging(Block);
            szSwitch.SupportsMultipleBlocks = false;
            szSwitch.Visible = Block => ActionControls.IsClaimBlock(Block);
            szSwitch.Title = MyStringId.GetOrCompute("SafeZone Switch");
            szSwitch.Tooltip = MyStringId.GetOrCompute("If disabled, tokens will still be consumed!");
            szSwitch.OnText = MyStringId.GetOrCompute("On");
            szSwitch.OffText = MyStringId.GetOrCompute("Off");
            szSwitch.Getter = ActionControls.IsSafeZoneValid;
            szSwitch.Setter = ActionControls.SetSafeZoneState;
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(szSwitch);
            controls.Add(szSwitch);

            // Installation Selection List
            var installationList = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlListbox, IMyBeacon>("InstallationList");
            installationList.Enabled = Block => true;
            installationList.SupportsMultipleBlocks = false;
            installationList.Visible = Block => ActionControls.IsClaimBlock(Block);
            installationList.Title = MyStringId.GetOrCompute("Installation Selection");
            installationList.ListContent = ActionControls.GetInstallationList;
            installationList.VisibleRowsCount = 6;
            installationList.ItemSelected = ActionControls.SetSelectedInstallation;
            installationList.Multiselect = false;
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(installationList);
            controls.Add(installationList);

            // Buy Installation Button
            var buyInstallation = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyBeacon>("BuyInstallation");
            buyInstallation.Enabled = Block => ActionControls.CanBuyInstallation(Block);
            buyInstallation.SupportsMultipleBlocks = false;
            buyInstallation.Visible = Block => ActionControls.IsClaimBlock(Block);
            buyInstallation.Title = MyStringId.GetOrCompute("Buy Installation");
            //buyInstallation.Tooltip = MyStringId.GetOrCompute("Installation that extends the safe zone");
            buyInstallation.Action = Block => ActionControls.BuyInstallation(Block);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(buyInstallation);
            controls.Add(buyInstallation);

            // SafeZone Installation Label
            /*var szLabel = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBeacon>("SZInstallation_Label");
            szLabel.Enabled = Block => true;
            szLabel.SupportsMultipleBlocks = false;
            szLabel.Visible = Block => ActionControls.IsClaimBlock(Block);
            szLabel.Label = MyStringId.GetOrCompute($"SafeZone Installation Cost: {ActionControls.GetInstallationCost(currentBlock, InstallationType.SafeZone)} Token(s)");
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(szLabel);
            controls.Add(szLabel);

            // Buy SafeZone Installation Button
            var szInstallation = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyBeacon>("SZInstallation");
            szInstallation.Enabled = Block => ActionControls.CanBuyInstallation(Block, InstallationType.SafeZone);
            szInstallation.SupportsMultipleBlocks = false;
            szInstallation.Visible = Block => ActionControls.IsClaimBlock(Block);
            szInstallation.Title = MyStringId.GetOrCompute("Buy Safe Zone\n   Installation");
            szInstallation.Tooltip = MyStringId.GetOrCompute("Installation that extends the safe zone");
            szInstallation.Action = Block => ActionControls.BuyInstallation(Block, InstallationType.SafeZone);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(szInstallation);
            controls.Add(szInstallation);

            // Radar Installation Label
            var radarLabel = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBeacon>("RadarInstallation_Label");
            radarLabel.Enabled = Block => true;
            radarLabel.SupportsMultipleBlocks = false;
            radarLabel.Visible = Block => ActionControls.IsClaimBlock(Block);
            radarLabel.Label = MyStringId.GetOrCompute($"Radar Installation Cost: {ActionControls.GetInstallationCost(currentBlock, InstallationType.Radar)} Token(s)");
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(radarLabel);
            controls.Add(radarLabel);

            // Buy Radar Installation button
            var radarInstallation = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyBeacon>("RadarInstallation");
            radarInstallation.Enabled = Block => ActionControls.CanBuyInstallation(Block, InstallationType.Radar);
            radarInstallation.SupportsMultipleBlocks = false;
            radarInstallation.Visible = Block => ActionControls.IsClaimBlock(Block);
            radarInstallation.Title = MyStringId.GetOrCompute(" Buy Radar\nInstallation");
            radarInstallation.Tooltip = MyStringId.GetOrCompute("Installation that adds gps locations on enemies");
            radarInstallation.Action = Block => ActionControls.BuyInstallation(Block, InstallationType.Radar);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(radarInstallation);
            controls.Add(radarInstallation);

            // Production Installation Label
            var productionLabel = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBeacon>("ProductionInstallation_Label");
            productionLabel.Enabled = Block => true;
            productionLabel.SupportsMultipleBlocks = false;
            productionLabel.Visible = Block => ActionControls.IsClaimBlock(Block);
            productionLabel.Label = MyStringId.GetOrCompute($"Production Installation Cost: {ActionControls.GetInstallationCost(currentBlock, InstallationType.Production)} Token(s)");
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(productionLabel);
            controls.Add(productionLabel);

            // Buy Production Installation button
            var productionInstallation = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyBeacon>("ProductionInstallation");
            productionInstallation.Enabled = Block => ActionControls.CanBuyInstallation(Block, InstallationType.Production);
            productionInstallation.SupportsMultipleBlocks = false;
            productionInstallation.Visible = Block => ActionControls.IsClaimBlock(Block);
            productionInstallation.Title = MyStringId.GetOrCompute("Buy Production\n  Installation");
            productionInstallation.Tooltip = MyStringId.GetOrCompute("Installation that increases production speed, yield, power");
            productionInstallation.Action = Block => ActionControls.BuyInstallation(Block, InstallationType.Production);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(productionInstallation);
            controls.Add(productionInstallation);

            // Drone Installation Label
            var droneLabel = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBeacon>("DroneInstallation_Label");
            droneLabel.Enabled = Block => true;
            droneLabel.SupportsMultipleBlocks = false;
            droneLabel.Visible = Block => ActionControls.IsClaimBlock(Block);
            droneLabel.Label = MyStringId.GetOrCompute($"Drone Installation Cost: {ActionControls.GetInstallationCost(currentBlock, InstallationType.Drone)} Token(s)");
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(droneLabel);
            controls.Add(droneLabel);

            // Buy Drone Installation button
            var droneInstallation = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyBeacon>("DroneInstallation");
            droneInstallation.Enabled = Block => ActionControls.CanBuyInstallation(Block, InstallationType.Drone);
            droneInstallation.SupportsMultipleBlocks = false;
            droneInstallation.Visible = Block => ActionControls.IsClaimBlock(Block);
            droneInstallation.Title = MyStringId.GetOrCompute(" Buy Drone\nInstallation");
            droneInstallation.Tooltip = MyStringId.GetOrCompute("Installation that spawns AI drones to help defend your territory");
            droneInstallation.Action = Block => ActionControls.BuyInstallation(Block, InstallationType.Drone);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(droneInstallation);
            controls.Add(droneInstallation);*/

            // Claimed Territory Name
            /*var territoryName = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlTextbox, IMyBeacon>("TerritoryName");
            territoryName.Enabled = Block => ActionControls.IsClaimedAndFaction(Block);
            territoryName.SupportsMultipleBlocks = false;
            territoryName.Visible = Block => ActionControls.IsClaimBlock(Block);
            territoryName.Title = MyStringId.GetOrCompute("Custom Territory Name");
            territoryName.Getter = Block => ActionControls.GetClaimedName(Block);
            territoryName.Setter = (Block, Builder) => ActionControls.SetClaimedName(Block, Builder);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(territoryName);
            controls.Add(territoryName);*/

            // Set Custom Territory Name Button
            /*var setName = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyBeacon>("SetTerritoryName");
            setName.Enabled = Block => ActionControls.IsClaimedAndFaction(Block);
            setName.SupportsMultipleBlocks = false;
            setName.Visible = Block => ActionControls.IsClaimBlock(Block);
            setName.Title = MyStringId.GetOrCompute("Set Custom Name");
            setName.Tooltip = MyStringId.GetOrCompute("Sets the custom territory name");
            setName.Action = Block => ActionControls.TriggerCustomName(Block);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(setName);
            controls.Add(setName);



            // Combo Perk Types Player
            var comboPerkTypePlayer = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCombobox, IMyBeacon>("ComboPerkTypePlayer");
            comboPerkTypePlayer.Enabled = Block => ActionControls.IsClaimedAndFaction(Block);
            comboPerkTypePlayer.SupportsMultipleBlocks = false;
            comboPerkTypePlayer.Visible = Block => ActionControls.IsClaimBlock(Block);
            comboPerkTypePlayer.Title = MyStringId.GetOrCompute("Select Allowed Perk Type To Enable");
            comboPerkTypePlayer.Tooltip = MyStringId.GetOrCompute("Only available to the faction that claims this territory");
            comboPerkTypePlayer.ComboBoxContent = ActionControls.GetPerkTypeContentPlayer;
            comboPerkTypePlayer.Getter = Block => ActionControls.GetPerkTypePlayer(Block);
            comboPerkTypePlayer.Setter = ActionControls.SetPerkTypePlayer;
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(comboPerkTypePlayer);
            controls.Add(comboPerkTypePlayer);

            // Player Controls Perks
            var playerControlsPerks = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyBeacon>("PlayerControlsPerksLabel");
            playerControlsPerks.Enabled = Block => true;
            playerControlsPerks.SupportsMultipleBlocks = false;
            playerControlsPerks.Visible = Block => ActionControls.IsPlayerPerkType(Block, PlayerPerks.Production);
            playerControlsPerks.Label = MyStringId.GetOrCompute("--- Player Controllable Perks ---");
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(playerControlsPerks);
            controls.Add(playerControlsPerks);

            // Sep F
            var sepF = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyBeacon>("SepF");
            sepF.Enabled = Block => true;
            sepF.SupportsMultipleBlocks = false;
            sepF.Visible = Block => ActionControls.IsClaimBlock(Block);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(sepF);
            controls.Add(sepF);

            // Enable Player Speed Perk
            var playerToggleSpeed = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("PlayerToggleSpeed");
            playerToggleSpeed.Enabled = Block => ActionControls.IsClaimedAndFaction(Block);
            playerToggleSpeed.SupportsMultipleBlocks = false;
            playerToggleSpeed.Visible = Block => ActionControls.IsPlayerProductionTypeAllowed(Block, "Speed");
            playerToggleSpeed.Title = MyStringId.GetOrCompute($"Add {Math.Round(ActionControls.GetProductionSpeed(currentBlock), 0)}% Production Speed:\n{(ActionControls.GetSpeedTokens(currentBlock))} Token(s)/{ActionControls.GetTimeToConsumeToken(currentBlock) / 60} minute(s)");
            playerToggleSpeed.Tooltip = MyStringId.GetOrCompute("If enabled, will add speed perk to all production inside the territory of the claimed faction and will add to token cost");
            playerToggleSpeed.Getter = Block => ActionControls.GetPlayerToggleSpeed(Block);
            playerToggleSpeed.Setter = (Block, Builder) => ActionControls.SetPlayerToggleSpeed(Block, Builder);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(playerToggleSpeed);
            controls.Add(playerToggleSpeed);

            // Enable Player Yield Perk
            var playerToggleYield = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("PlayerToggleYield");
            playerToggleYield.Enabled = Block => ActionControls.IsClaimedAndFaction(Block);
            playerToggleYield.SupportsMultipleBlocks = false;
            playerToggleYield.Visible = Block => ActionControls.IsPlayerProductionTypeAllowed(Block, "Yield");
            playerToggleYield.Title = MyStringId.GetOrCompute($"Add {Math.Round(ActionControls.GetProductionYield(currentBlock), 0)}% Production Yield:\n{(ActionControls.GetYieldTokens(currentBlock))} Token(s)/{ActionControls.GetTimeToConsumeToken(currentBlock) / 60} minute(s)");
            playerToggleYield.Tooltip = MyStringId.GetOrCompute("If enabled, will add yield perk to all production inside the territory of the claimed faction and will add to token cost");
            playerToggleYield.Getter = Block => ActionControls.GetPlayerToggleYield(Block);
            playerToggleYield.Setter = (Block, Builder) => ActionControls.SetPlayerToggleYield(Block, Builder);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(playerToggleYield);
            controls.Add(playerToggleYield);

            // Enable Player Energy Perk
            var playerToggleEnergy = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("PlayerToggleEnergy");
            playerToggleEnergy.Enabled = Block => ActionControls.IsClaimedAndFaction(Block);
            playerToggleEnergy.SupportsMultipleBlocks = false;
            playerToggleEnergy.Visible = Block => ActionControls.IsPlayerProductionTypeAllowed(Block, "Energy");
            playerToggleEnergy.Title = MyStringId.GetOrCompute($"Add {Math.Round(ActionControls.GetProductionEnergy(currentBlock), 0)}% Energy Efficiency:\n{(ActionControls.GetEnergyTokens(currentBlock))} Token(s)/{ActionControls.GetTimeToConsumeToken(currentBlock) / 60} minute(s)");
            playerToggleEnergy.Tooltip = MyStringId.GetOrCompute("If enabled, will add energy perk to all production inside the territory of the claimed faction and will add to token cost");
            playerToggleEnergy.Getter = Block => ActionControls.GetPlayerToggleEnergy(Block);
            playerToggleEnergy.Setter = (Block, Builder) => ActionControls.SetPlayerToggleEnergy(Block, Builder);
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(playerToggleEnergy);
            controls.Add(playerToggleEnergy);*/

        }

        public static void CreateJumpdriveControls(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (block as IMyJumpDrive == null) return;
            if (_jumpdriveControlsCreated)
            {
                sb = new StringBuilder();
                //text = "";
                IMyFaction faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(MyAPIGateway.Session.LocalHumanPlayer.IdentityId);
                if (faction == null) return;

                foreach (var item in Session.Instance.claimBlocks.Values)
                {
                    if (!item.Enabled) continue;
                    if (Vector3D.Distance(item.BlockPos, block.GetPosition()) < item.TerritoryConfig._territoryRadius)
                    {
                        sb.Append($"\n[Territory Info]:");
                        sb.Append($"\n[Territory Name]: {item.UnclaimName}");
                        var idx = item.TerritoryConfig._token.IndexOf('/') + 1;
                        sb.Append($"\n[Token]:\n{item.TerritoryConfig._token.Substring(idx)}");

                        if (item.IsCooling)
                        {
                            sb.Append($"\n[Cooldown Time Left]: {TimeSpan.FromSeconds(item.Timer)}");
                            break;
                        }

                        if (item.IsSiegeCooling && item.ClaimedFaction != faction.Tag)
                        {
                            sb.Append($"\n[Siege Cooldown Left]: {TimeSpan.FromSeconds(item.SiegeTimer)}");
                            break;
                        }

                        if (!item.IsClaimed)
                        {
                            sb.Append($"\n[Cost To Claim]: {item.TerritoryConfig._claimingConfig._tokensToClaim} tokens");
                            sb.Append($"\n[Total Time To Claim]: {TimeSpan.FromSeconds(item.TerritoryConfig._claimingConfig._claimingTime)}");
                            sb.Append($"\n[To Claim Distance]: {item.TerritoryConfig._claimingConfig._distanceToClaim}m");
                            break;
                        }

                        if (!item.IsSieged && faction.Tag != item.ClaimedFaction)
                        {
                            sb.Append($"\n[Cost To Siege]: {item.TerritoryConfig._siegingConfig._tokensToSiege} tokens");
                            sb.Append($"\n[Total Time To Siege]: {TimeSpan.FromSeconds(Utils.GetTotalSiegeTime(item))}");
                            sb.Append($"\n[To Siege Distance]: {item.TerritoryConfig._siegingConfig._distanceToSiege}m");
                            break;
                        }

                        /*if (item.IsSieged && !item.ReadyToSiege && item.SiegedBy == faction.Tag && !item.IsSiegingFinal)
                        {
                            sb.Append($"\n[Ready To Final Siege In]:\n{TimeSpan.FromSeconds(item.SiegeTimer)}");
                            break;
                        }

                        if (item.ReadyToSiege && item.SiegedBy == faction.Tag && !item.IsSiegingFinal)
                        {
                            sb.Append($"\n[Time Left To Start Final Siege]:\n{TimeSpan.FromSeconds(item.SiegeTimer)}");
                            sb.Append($"\n[Cost To Final Siege]: {item.TokensSiegeFinal} tokens");
                            sb.Append($"\n[Total Time To Final Siege]: {TimeSpan.FromSeconds(item.SiegeFinalTimer)}");
                            sb.Append($"\n[To Final Siege Distance]: {item.DistanceToSiege}m");
                            break;
                        }*/

                        break;
                    }
                }

                block.RefreshCustomInfo();
                return;
            }

            _jumpdriveControlsCreated = true;

            // Jumpdrive Control Seperate A
            var sepA = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyJumpDrive>("SepAJD");
            sepA.Enabled = Block => true;
            sepA.SupportsMultipleBlocks = false;
            sepA.Visible = Block => ActionControls.IsJumpDriveBlock(Block);
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(sepA);
            controls.Add(sepA);

            // Not in claim range error
            var claimrangeError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("ClaimRangeError");
            claimrangeError.Enabled = Block => true;
            claimrangeError.SupportsMultipleBlocks = false;
            claimrangeError.Visible = Block => ActionControls.IsInClaimOrSiegeRange(Block);
            claimrangeError.Label = MyStringId.GetOrCompute("Not Within Range of Claim Block");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(claimrangeError);
            controls.Add(claimrangeError);

            // Not in faction error
            var factionError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("FactionError");
            factionError.Enabled = Block => true;
            factionError.SupportsMultipleBlocks = false;
            factionError.Visible = Block => ActionControls.CheckForFaction(Block);
            factionError.Label = MyStringId.GetOrCompute("Must be in a faction");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(factionError);
            controls.Add(factionError);

            // Player not in range error
            var playerError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("PlayerError");
            playerError.Enabled = Block => true;
            playerError.SupportsMultipleBlocks = false;
            playerError.Visible = Block => ActionControls.CheckForPlayer(Block);
            playerError.Label = MyStringId.GetOrCompute("Player not within range of claim block");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(playerError);
            controls.Add(playerError);

            // Static Grid error
            var staticError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("StaticError");
            staticError.Enabled = Block => true;
            staticError.SupportsMultipleBlocks = false;
            staticError.Visible = Block => ActionControls.IsGridStatic(Block);
            staticError.Label = MyStringId.GetOrCompute("Grid must be a ship");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(staticError);
            controls.Add(staticError);

            // Underground error
            var undergroundError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("UndergroundError");
            undergroundError.Enabled = Block => true;
            undergroundError.SupportsMultipleBlocks = false;
            undergroundError.Visible = Block => ActionControls.IsGridUnderground(Block);
            undergroundError.Label = MyStringId.GetOrCompute("Grid must be above ground");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(undergroundError);
            controls.Add(undergroundError);

            // IsClaiming error
            var claimingError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("ClaimingError");
            claimingError.Enabled = Block => true;
            claimingError.SupportsMultipleBlocks = false;
            claimingError.Visible = Block => ActionControls.IsClaiming(Block);
            claimingError.Label = MyStringId.GetOrCompute("Territory is currently being claimed");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(claimingError);
            controls.Add(claimingError);

            // IsSieging error
            var siegingError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("SiegingError");
            siegingError.Enabled = Block => true;
            siegingError.SupportsMultipleBlocks = false;
            siegingError.Visible = Block => ActionControls.IsSieging(Block);
            siegingError.Label = MyStringId.GetOrCompute("Territory is currently being sieged");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(siegingError);
            controls.Add(siegingError);

            /*// Already Sieged Error
            var siegedError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("SiegedError");
            siegedError.Enabled = Block => true;
            siegedError.SupportsMultipleBlocks = false;
            siegedError.Visible = Block => ActionControls.IsSieged(Block);
            siegedError.Label = MyStringId.GetOrCompute("Territory is cooling to be claimed");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(siegedError);
            controls.Add(siegedError);*/

            /*// Ready to final siege error
            var finalSiegeError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("FinalSiegedError");
            finalSiegeError.Enabled = Block => true;
            finalSiegeError.SupportsMultipleBlocks = false;
            finalSiegeError.Visible = Block => ActionControls.ReadyToFinalSiege(Block);
            finalSiegeError.Label = MyStringId.GetOrCompute("Territory not ready to final siege");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(finalSiegeError);
            controls.Add(finalSiegeError);*/

            // Enemy neaby error
            var enemyError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("EnemyError");
            enemyError.Enabled = Block => true;
            enemyError.SupportsMultipleBlocks = false;
            enemyError.Visible = Block => ActionControls.IsEnemyNearby(Block);
            enemyError.Label = MyStringId.GetOrCompute("Enemy grid is nearby");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(enemyError);
            controls.Add(enemyError);

            // Tokens error
            var tokenError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("TokenError");
            tokenError.Enabled = Block => true;
            tokenError.SupportsMultipleBlocks = false;
            tokenError.Visible = Block => ActionControls.CheckForClaimTokens(Block);
            tokenError.Label = MyStringId.GetOrCompute("Not enough tokens in inventory");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(tokenError);
            controls.Add(tokenError);

            // Not enough energy error
            var energyError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("EnergyError");
            energyError.Enabled = Block => true;
            energyError.SupportsMultipleBlocks = false;
            energyError.Visible = Block => ActionControls.CheckForEnergy(Block);
            energyError.Label = MyStringId.GetOrCompute("Jumpdrive is not fully charged");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(energyError);
            controls.Add(energyError);

            // Cooling Error
            var isCoolingError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("CoolingError");
            isCoolingError.Enabled = Block => true;
            isCoolingError.SupportsMultipleBlocks = false;
            isCoolingError.Visible = Block => ActionControls.IsCooling(Block);
            isCoolingError.Label = MyStringId.GetOrCompute("Wait for cooldown to claim");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(isCoolingError);
            controls.Add(isCoolingError);

            // Siege Cooling Error
            var siegeCoolingError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("SiegeCoolingError");
            siegeCoolingError.Enabled = Block => true;
            siegeCoolingError.SupportsMultipleBlocks = false;
            siegeCoolingError.Visible = Block => ActionControls.IsSiegeCooling(Block);
            siegeCoolingError.Label = MyStringId.GetOrCompute("Wait for siege cooldown to siege");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(siegeCoolingError);
            controls.Add(siegeCoolingError);

            // InVoxel Error
            /*var inVoxelError = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyJumpDrive>("InvoxelError");
            inVoxelError.Enabled = Block => true;
            inVoxelError.SupportsMultipleBlocks = false;
            inVoxelError.Visible = Block => ActionControls.IsGridInvoxel(Block);
            inVoxelError.Label = MyStringId.GetOrCompute("Grid cannot be anchored to voxels");
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(inVoxelError);
            controls.Add(inVoxelError);*/

            // Claim Button
            var claimButton = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyJumpDrive>("ClaimButton");
            claimButton.Enabled = Block => ActionControls.AllowClaimEnable(Block);
            claimButton.SupportsMultipleBlocks = false;
            claimButton.Visible = Block => ActionControls.IsNearClaim(Block);
            claimButton.Title = MyStringId.GetOrCompute("Claim Territory");
            //claimButton.Tooltip = MyStringId.GetOrCompute("Sets the claim area radius.");
            claimButton.Action = Block => ActionControls.InitClaim(Block);
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(claimButton);
            controls.Add(claimButton);

            // Siege Button
            var siegeButton = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyJumpDrive>("SiegeButton");
            siegeButton.Enabled = Block => ActionControls.AllowSiegeEnable(Block);
            siegeButton.SupportsMultipleBlocks = false;
            siegeButton.Visible = Block => ActionControls.IsNearClaimed(Block);
            siegeButton.Title = MyStringId.GetOrCompute("Init Siege");
            //claimButton.Tooltip = MyStringId.GetOrCompute("Sets the claim area radius.");
            siegeButton.Action = Block => ActionControls.InitSiege(Block);
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(siegeButton);

            /*// Final Siege Button
            var finalSiegeButton = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyJumpDrive>("FinalSiegeButton");
            finalSiegeButton.Enabled = Block => ActionControls.AllowFinalSiegeEnable(Block);
            finalSiegeButton.SupportsMultipleBlocks = false;
            finalSiegeButton.Visible = Block => ActionControls.IsNearClaimed(Block);
            finalSiegeButton.Title = MyStringId.GetOrCompute("Final Siege");
            //claimButton.Tooltip = MyStringId.GetOrCompute("Sets the claim area radius.");
            finalSiegeButton.Action = Block => ActionControls.InitSiege(Block);
            MyAPIGateway.TerminalControls.AddControl<IMyJumpDrive>(finalSiegeButton);*/

        }

        public static void CreateJumpdriveActions(IMyTerminalBlock block, List<IMyTerminalAction> actions)
        {
            if (block as IMyJumpDrive == null) return;
            if (_jumpdriveActionsCreated) return;

            _jumpdriveActionsCreated = true;

            foreach (var action in actions)
            {
                if (action.Id == "Jump")
                    action.Enabled = Block => ActionControls.AllowJump(Block);

                return;
            }
        }
    }
}
