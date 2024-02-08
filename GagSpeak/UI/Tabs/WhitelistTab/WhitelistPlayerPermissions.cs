using System.Numerics;
using ImGuiNET;
using OtterGui.Raii;
using GagSpeak.Events;
using Dalamud.Plugin.Services;
using GagSpeak.ChatMessages.MessageTransfer;
using GagSpeak.ChatMessages;
using GagSpeak.CharacterData;
using Dalamud.Interface;
using System;
using GagSpeak.Services;
using Dalamud.Interface.Utility;
using GagSpeak.UI.Equipment;
using GagSpeak.UI.Tabs.GeneralTab;

namespace GagSpeak.UI.Tabs.WhitelistTab;
public partial class WhitelistPlayerPermissions {
    private readonly    InteractOrPermButtonEvent   _interactOrPermButtonEvent;
    private readonly    GagSpeakConfig              _config;
    private readonly    CharacterHandler            _characterHandler;
    private readonly    GagService                  _gagService;
    private readonly    GagListingsDrawer           _gagListingsDrawer;
    private readonly    FontService                 _fontService;
    private readonly    IClientState                _clientState;
    private readonly    IChatGui                    _chatGui;
    private readonly    IDataManager                _dataManager;
    private readonly    MessageEncoder              _messageEncoder;
    private readonly    ChatManager                 _chatManager;
    private readonly    UserProfileWindow           _userProfileWindow;
    private             int                         _currentIconIndex = 0;
    public              TabType                     SelectedSubTab;
    public WhitelistPlayerPermissions(InteractOrPermButtonEvent interactOrPermButtonEvent, GagSpeakConfig config,
    CharacterHandler characterHandler, IClientState clientState, IChatGui chatGui, IDataManager dataManager,
    MessageEncoder messageEncoder, ChatManager chatManager, UserProfileWindow userProfileWindow, 
    FontService fontService, GagService gagService, GagListingsDrawer gagListingsDrawer) {
        _interactOrPermButtonEvent = interactOrPermButtonEvent;
        _config = config;
        _characterHandler = characterHandler;
        _clientState = clientState;
        _chatGui = chatGui;
        _dataManager = dataManager;
        _messageEncoder = messageEncoder;
        _chatManager = chatManager;
        _userProfileWindow = userProfileWindow;
        _fontService = fontService;
        _gagService = gagService;
        _gagListingsDrawer = gagListingsDrawer;

        _gagLabel = "None";
        _lockLabel = "None";
        _layer = 0;
        // draw out our gagtype filter combo listings
        _gagTypeFilterCombo = new GagTypeFilterCombo[] {
            new GagTypeFilterCombo(_gagService, _config),
            new GagTypeFilterCombo(_gagService, _config),
            new GagTypeFilterCombo(_gagService, _config)
        };
        // draw out our gagpadlock filter combo listings
        _gagLockFilterCombo = new GagLockFilterCombo[] {
            new GagLockFilterCombo(_config),
            new GagLockFilterCombo(_config),
            new GagLockFilterCombo(_config)
        };
    }

    /// <summary> This function is used to draw the content of the tab. </summary>
    public void Draw(Action<bool> setInteractions, ref bool _enableInteractions) {
        // Lets start by drawing the child.
        using (_ = ImRaii.Group()) {
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        DrawPermissionsHeader(setInteractions, _enableInteractions);
        // make content disabled
        if(!_enableInteractions) { ImGui.BeginDisabled(); }
        DrawPlayerPermissions();
        DrawPlayerPermissionsButtons();
        if(!_enableInteractions) { ImGui.EndDisabled(); }
        }
    }

    // draw the header
    private void DrawPermissionsHeader(Action<bool> setInteractions, bool _enableInteractions) {
        WindowHeader.Draw($"Status & Interactions for {_characterHandler.whitelistChars[_characterHandler.activeListIdx]._name}",
        0, ImGui.GetColorU32(ImGuiCol.FrameBg), 1, 0, InteractionsButton(setInteractions, _enableInteractions), ViewModeButton());
    }

    private void DrawPlayerPermissions() {
        using var _ = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
        using var child = ImRaii.Child("##WhitelistPlayerPermissions", new Vector2(ImGui.GetContentRegionAvail().X, -ImGui.GetFrameHeight()), true, ImGuiWindowFlags.NoScrollbar);
        if (!child)
            return;
        // draw ourcontent
        DrawOverview(); // Draw the overview of the permissions
        var xPosition = ImGui.GetCursorPosX();
        var yPosition = ImGui.GetCursorPosY();
        ImGui.SetCursorPos(new Vector2(xPosition, yPosition + 5*ImGuiHelpers.GlobalScale)); 
        DrawPermissionTabs(); // draws the tabs for the sub component permissions
        DrawBody(); // draws out the body for each tab of permissions
        
    }

    private void DrawPlayerPermissionsButtons() {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, Vector2.Zero)
            .Push(ImGuiStyleVar.FrameRounding, 0);
        var buttonWidth = new Vector2(ImGui.GetContentRegionAvail().X * 0.25f, 0);
        var buttonWidth2 = new Vector2(ImGui.GetContentRegionAvail().X * 0.5f, 0);
        // add a button to display it
        if (ImGui.Button("Mini-Profile", buttonWidth)) {
            // Get the currently selected user
            var selectedUser = _characterHandler.whitelistChars[_characterHandler.activeListIdx];
            // Check if the UserProfileWindow is already open
            _userProfileWindow.Toggle();
        }

        // draw the relationship removal
        ImGui.SameLine();
        if(_characterHandler.whitelistChars[_characterHandler.activeListIdx]._yourStatusToThem == RoleLean.None) {
            ImGui.BeginDisabled();
            if (ImGui.Button("Remove Relation With Player##RemoveOne", buttonWidth2)) {
                GagSpeak.Log.Debug("[Whitelist]: Sending Request to remove relation to player");
                RequestRelationRemovalToPlayer();
                // send a request to remove your relationship, or just send a message that does remove it, removing it from both ends.
            }
            ImGui.EndDisabled();
        } else {
            if (ImGui.Button("Remove Relation With Player##RemoveTwo", buttonWidth2)) {
                GagSpeak.Log.Debug("[Whitelist]: Sending Request to remove relation to player");
                RequestRelationRemovalToPlayer();
                // send a request to remove your relationship, or just send a message that does remove it, removing it from both ends.
            }
        } 

        // for requesting info
        ImGui.SameLine();
        if (ImGui.Button("Request Info", buttonWidth)) {
            // send a message to the player requesting their current info
            GagSpeak.Log.Debug("[Whitelist]: Sending Request for Player Info");
            InfoSendAndRequestHelpers.RequestInfoFromPlayer(_characterHandler.activeListIdx,
            _characterHandler, _chatManager, _messageEncoder, _clientState, _chatGui);
            // Start a 5-second cooldown timer
            _interactOrPermButtonEvent.Invoke();
        }
    }

#region Header Button Stuff
    private WindowHeader.Button InteractionsButton(Action<bool> setInteractions, bool _enableInteractions)
        => !_characterHandler.IsIndexWithinBounds(_characterHandler.activeListIdx)
            ? WindowHeader.Button.Invisible
            : _enableInteractions
                ? new WindowHeader.Button {
                    Description = "Disable interactions.",
                    Icon = FontAwesomeIcon.LockOpen,
                    OnClick = () => setInteractions(false),
                    Visible = true,
                    Disabled = false,
                }
                : new WindowHeader.Button {
                    Description = "Enable interactions.",
                    Icon = FontAwesomeIcon.Lock,
                    OnClick = () => setInteractions(true),
                    Visible = true,
                    Disabled = false,
                };

    // get our view mode buttons list
    private readonly FontAwesomeIcon[] _viewModeIcons = {
        FontAwesomeIcon.PersonArrowDownToLine,
        FontAwesomeIcon.PersonArrowUpFromLine,
        FontAwesomeIcon.PeopleArrows,
    };
    private WindowHeader.Button ViewModeButton()
        => new() {
            Description = "Changes the state of what permissions are visable to you.",
        Icon = _viewModeIcons[_currentIconIndex],
            OnClick     = () => {
            _currentIconIndex = (_currentIconIndex + 1) % _viewModeIcons.Length;
        },
            Disabled    = false,
            Visible     = true,
            TextColor   = ColorId.DominantButton.Value(),
            BorderColor = ColorId.DominantButton.Value(),
        };
#endregion Header Button Stuff

    public void DrawPermissionTabs() {
        using var _ = ImRaii.PushId( "WhitelistPermissionEditTabList" );
        using var tabBar = ImRaii.TabBar( "PermissionEditorTabBar" );
        if( !tabBar ) return;

        if (ImGui.BeginTabItem("Overview")) {
            SelectedSubTab = TabType.ConfigSettings;
            ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("Gags")) {
            SelectedSubTab = TabType.General;
            ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("Warbrobe")) {
            SelectedSubTab = TabType.Wardrobe;
            ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("Puppeteer")) {
            SelectedSubTab = TabType.Puppeteer;
            ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("Toybox")) {
            SelectedSubTab = TabType.Toybox;
            ImGui.EndTabItem();
        }
    }

    public void DrawBody() {
        // determine which permissions we will draw out
        switch (SelectedSubTab) {
            case TabType.ConfigSettings:
                DrawOverviewPerms();
                break;
            case TabType.General:
                DrawGagInteractions();
                break;
            case TabType.Wardrobe:
                DrawWardrobePerms();
                break;
            case TabType.Puppeteer:
                DrawPuppeteerPerms();
                break;
            case TabType.Toybox:
                DrawToyboxPerms();
                break;
        }
    }
}