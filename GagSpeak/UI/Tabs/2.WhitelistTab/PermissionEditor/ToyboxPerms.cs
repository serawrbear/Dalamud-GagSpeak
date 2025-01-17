using System.Numerics;
using ImGuiNET;
using GagSpeak.Utility;
using OtterGui;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game.Text.SeStringHandling;
using OtterGui.Classes;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface;
using GagSpeak.CharacterData;
using Dalamud.Interface.Utility;

namespace GagSpeak.UI.Tabs.WhitelistTab;
public partial class WhitelistPlayerPermissions {
    public int _vibratorIntensity = 1;
    public string _vibePatternName = "";
    public int _activeStoredPatternListIdx = 0;

#region DrawWardrobePerms
    public void DrawToyboxPerms(ref bool _viewMode) {
        // Big Name Header
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);

        using (var ToyBoxHeaderTable = ImRaii.Table("ToyBoxHeaderTable", 2)) {
            if (!ToyBoxHeaderTable) { return; }

            ImGui.TableSetupColumn("##ToyboxHeader", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("##ToyboxHeaderButton", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("ToyTogglemmmmm").X);
            ImGui.TableNextRow();
            ImGui.TableNextColumn();

            // if our hardcord condition is fullfilled, begin disable
            if(!_viewMode && _characterHandler.IsLeanLesserThanPartner(_characterHandler.activeListIdx) && _config.hardcoreMode) { ImGui.BeginDisabled(); }

            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 2*ImGuiHelpers.GlobalScale);
            ImGui.PushFont(_fontService.UidFont);
            var name = _viewMode ? $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name.Split(' ')[0]}'s" : "Your";
            ImGui.Text($"{name} Toybox Settings");
            ImGui.PopFont();
            if(!_characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowChangingToyState) { ImGui.BeginDisabled(); }

            ImGui.TableNextColumn();
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 5*ImGuiHelpers.GlobalScale);
            ImGui.Text($"Toy State:");
            ImGui.SameLine();
            var text = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._isToyActive ? "On" : "Off";
            if(ImGuiUtil.DrawDisabledButton($"{text}##ToggleToyActive", new Vector2(ImGui.GetContentRegionAvail().X, 20*ImGuiHelpers.GlobalScale),
            string.Empty, _viewMode && !_characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowChangingToyState)) {
                TogglePlayersIsToyActiveOption();
                _interactOrPermButtonEvent.Invoke(5);
            }
            if(!_characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowChangingToyState) { ImGui.EndDisabled(); }
        }
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 3*ImGuiHelpers.GlobalScale);
        // store their dynamic tier for edit purposes
        DynamicTier dynamicTier = _characterHandler.whitelistChars[_characterHandler.activeListIdx].GetDynamicTierClient();
        
        // draw out the table for our permissions
        using (var tableOverrideSettings = ImRaii.Table("ToyboxManagerTable", 4, ImGuiTableFlags.RowBg)) {
            if (!tableOverrideSettings) return;
            // Create the headers for the table
            var text = _viewMode ? "Setting" : $"Permission Setting for {_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name.Split(' ')[0]}";
            ImGui.TableSetupColumn($"{text}",  ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("State",     ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("State").X);
            ImGui.TableSetupColumn("Req. Tier", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Req.Tier").X);
            ImGui.AlignTextToFramePadding();
            ImGui.TableSetupColumn("Toggle",    ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Togglemo").X);
            ImGui.TableHeadersRow();
            ImGui.TableNextRow();
            // Restraint Set Locking option
            ImGuiUtil.DrawFrameColumn($"Locked Toybox UI?");
            ImGui.TableNextColumn();
            var toyboxUILock = _viewMode ? _characterHandler.whitelistChars[_characterHandler.activeListIdx]._lockToyboxUI : _characterHandler.playerChar._lockToyboxUI;
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((toyboxUILock ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("4");
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Toggle##UpdatePlayerToyIntensityButton", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, _viewMode && !(dynamicTier >= DynamicTier.Tier3))) {
                if(_viewMode) {
                    // the whitelisted players lock
                    TogglePlayerToyboxLockOption();
                    _interactOrPermButtonEvent.Invoke(5);
                } else {
                    // toggle your lock
                    _characterHandler.ToggleToyboxUILocking();
                }
            }
            if(!_viewMode && _characterHandler.playerChar._lockToyboxUI) { ImGui.BeginDisabled(); }
            // Lock Gag Storage on Gag Lock option
            ImGuiUtil.DrawFrameColumn($"Allow Change Toy State:");

            ImGui.TableNextColumn();
            var toyStatePerm = _viewMode ? _characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowChangingToyState 
                                         : _characterHandler.playerChar._allowChangingToyState[_characterHandler.activeListIdx];
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((toyStatePerm ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("1");
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleToyActiveState", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, _viewMode && !(dynamicTier >= DynamicTier.Tier1))) {
                if(_viewMode) {
                    // toggle the whitelisted players permission to allow changing toy state
                    TogglePlayerToggleChangeToyState();
                    _interactOrPermButtonEvent.Invoke(5);
                } else {
                    // toggles if this person can change your toy state
                    _characterHandler.ToggleChangeToyState(_characterHandler.activeListIdx);
                }
            }
            // Can Control Intensity
            ImGuiUtil.DrawFrameColumn($"Can Control Intensity:");
            ImGui.TableNextColumn();
            var toyIntensityPerm = _viewMode ? _characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowsIntensityControl 
                                            : _characterHandler.playerChar._allowIntensityControl[_characterHandler.activeListIdx];
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((toyIntensityPerm ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("0");
            ImGui.TableNextColumn();
            if(_viewMode) {
                ImGuiUtil.Center("ReadOnly");
            } else {
                if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleToyIntensityControl", new Vector2(ImGui.GetContentRegionAvail().X, 0),
                string.Empty, false)) {
                    // toggles if this person can change your toy state
                    _characterHandler.ToggleAllowIntensityControl(_characterHandler.activeListIdx);
                }
            }
            // Enable Restraint Sets option
            ImGuiUtil.DrawFrameColumn($"Can Execute Patterns:");
            ImGui.TableNextColumn();
            var patternExecuttionPerm = _viewMode ? _characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowsUsingPatterns 
                                            : _characterHandler.playerChar._allowUsingPatterns[_characterHandler.activeListIdx];
            using (var font = ImRaii.PushFont(UiBuilder.IconFont)) {
                ImGuiUtil.Center((patternExecuttionPerm ? FontAwesomeIcon.Check : FontAwesomeIcon.Times).ToIconString());
            }
            ImGui.TableNextColumn();
            ImGuiUtil.Center("0");
            ImGui.TableNextColumn();
            if(_viewMode) {
                ImGuiUtil.Center("ReadOnly");
            } else {
                if(ImGuiUtil.DrawDisabledButton("Toggle##ToggleAllowingPatternExecution", new Vector2(ImGui.GetContentRegionAvail().X, 0),
                string.Empty, false)) {
                    // toggles if this person can change your toy state
                    _characterHandler.ToggleAllowPatternExecution(_characterHandler.activeListIdx);
                }
            }
        }
        if(!_viewMode) { ImGui.BeginDisabled(); }
        // seperate the table and sliders
        using (var toyboxDisplayList = ImRaii.Table("ToyboxManagerDisplayTable", 3, ImGuiTableFlags.RowBg)) {
            if (!toyboxDisplayList) return;
            // Create the headers for the table
            var text = _viewMode ? "Setting" : $"Permission Setting for {_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name.Split(' ')[0]}";
            ImGui.TableSetupColumn("Setting",  ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Stored Patternmm").X);
            ImGui.TableSetupColumn("Adjuster",     ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Toggle",    ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("Togglemo").X);
            ImGui.TableNextRow();

            ImGuiUtil.DrawFrameColumn("Intensity Level: ");
            ImGui.TableNextColumn();
            int intensityResult = _vibratorIntensity;
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            // default to a range of 10, but otherwise, display the toy's active step size
            var maxSliderVal = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._activeToystepSize==0 ? 10 : _characterHandler.whitelistChars[_characterHandler.activeListIdx]._activeToystepSize;
            if(ImGui.SliderInt("##ToyIntensity", ref intensityResult, 0, maxSliderVal)) {
                _vibratorIntensity = intensityResult;
            }
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Update##UpdateToyIntensity", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, _viewMode && !(dynamicTier >= DynamicTier.Tier2))) {
                UpdatePlayerToyIntensity(_vibratorIntensity);
                _interactOrPermButtonEvent.Invoke(2);
            }

            // pattern executtion section
            ImGuiUtil.DrawFrameColumn("Pattern Name: ");
            ImGui.TableNextColumn();
            string patternResult = _vibePatternName;
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            ImGui.AlignTextToFramePadding();
            if (ImGui.InputTextWithHint("##ToyPatternName", "Pattern Name", ref patternResult, 50)) {
                _vibePatternName = patternResult;
            }
            // then go over and draw the execute button
            ImGui.TableNextColumn();
            if(ImGuiUtil.DrawDisabledButton("Execute##ExecuteToyPattern", new Vector2(ImGui.GetContentRegionAvail().X, 0),
            string.Empty, !(_characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowsUsingPatterns == true))) {
                ExecutePlayerToyPattern(_vibePatternName);
                _interactOrPermButtonEvent.Invoke(5);
            }

            // stored pattern list
            if(_viewMode) {
                ImGuiUtil.DrawFrameColumn("Stored Patterns: ");
                ImGui.TableNextColumn();
                // Create a combo box with the stored restraint data (had to convert to array because am dumb)
                string[] patternData = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._storedPatternNames.ToArray();
                int currentPatternIndex = _activeStoredPatternListIdx==0 ? 0 : _activeStoredPatternListIdx; // This should be the current selected index
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                if (ImGui.Combo("##storedPatternData", ref currentPatternIndex, patternData, patternData.Length)) {
                    // If an item is selected from the dropdown, update the restraint set name field
                    _vibePatternName = patternData[currentPatternIndex];
                    // update the index to display
                    _activeStoredPatternListIdx = currentPatternIndex;
                }
                // end the disabled state
                if(!_characterHandler.whitelistChars[_characterHandler.activeListIdx]._enableWardrobe || _viewMode==false) {
                    ImGui.EndDisabled();
                }
            }
        }
        if(!_viewMode) { ImGui.EndDisabled(); }
        if(!_viewMode && _characterHandler.playerChar._lockToyboxUI) { ImGui.EndDisabled(); }

        // if our hardcord condition is fullfilled, end disable
        if(!_viewMode && _characterHandler.IsLeanLesserThanPartner(_characterHandler.activeListIdx) && _config.hardcoreMode) { ImGui.EndDisabled(); }
        // pop the style
        ImGui.PopStyleVar();
    }
#endregion DrawWardrobePerms
#region ButtonHelpers
    public void TogglePlayersEnableToyboxOption() {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s Enable Toybox Option!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.SetWhitelistEnableToybox(_characterHandler.activeListIdx, !_characterHandler.whitelistChars[_characterHandler.activeListIdx]._enableToybox);
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxToggleEnableToyboxOption(playerPayload, targetPlayer));
    }

    public void TogglePlayerToggleChangeToyState() {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s Toy State!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.SetWhitelistAllowChangingToyState(_characterHandler.activeListIdx, !_characterHandler.whitelistChars[_characterHandler.activeListIdx]._allowChangingToyState);
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxToggleActiveToyboxOption(playerPayload, targetPlayer));
    }

    public void TogglePlayersIsToyActiveOption() {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s Toy Active Option!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.SetWhitelistToyIsActive(_characterHandler.activeListIdx, !_characterHandler.whitelistChars[_characterHandler.activeListIdx]._isToyActive);
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxToggleToyOnOff(playerPayload, targetPlayer));
    }

    public void UpdatePlayerToyIntensity(int newIntensityLevel) {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Updating  "+ 
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s Toy Intensity to {newIntensityLevel}!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.SetWhitelistIntensityLevel(_characterHandler.activeListIdx, (byte)newIntensityLevel);
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxUpdateActiveToyIntensity(playerPayload, targetPlayer, newIntensityLevel));
    }

    public void ExecutePlayerToyPattern(string patternName) {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Executing  "+ 
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s Toy Pattern [{patternName}]!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxExecuteStoredToyPattern(playerPayload, targetPlayer, patternName));
    }

    public void TogglePlayerToyboxLockOption() {
        // get the player payload    
        PlayerPayload playerPayload; // get player payload
        UIHelpers.GetPlayerPayload(_clientState, out playerPayload);
        if (!_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)) { return; }
        string targetPlayer = _characterHandler.whitelistChars[_characterHandler.activeListIdx]._name + "@" + _characterHandler.whitelistChars[_characterHandler.activeListIdx]._homeworld;
        // print to chat that you sent the request
        _chatGui.Print(
            new SeStringBuilder().AddItalicsOn().AddYellow($"[GagSpeak]").AddText($"Toggling  "+ 
            $"{_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}'s Toybox Lock Option!").AddItalicsOff().BuiltString);
        //update information to be the new toggled state and send message
        _characterHandler.SetWhitelistAllowToyboxLocking(_characterHandler.activeListIdx, !_characterHandler.whitelistChars[_characterHandler.activeListIdx]._lockToyboxUI);
        _chatManager.SendRealMessage(_messageEncoder.EncodeToyboxToggleLockToyboxUI(playerPayload, targetPlayer));
    }
#endregion ButtonHelpers
}