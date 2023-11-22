﻿﻿
using System;
using ImGuiNET;
using Dalamud.Interface.Windowing;
using System.Numerics;
using Dalamud.Interface.Internal;
using Dalamud.Interface;
using Dalamud.Plugin;
using System.IO;
using Dalamud.Interface.Utility.Raii;

using OtterGui;
using System.Collections.Generic;


namespace GagSpeak.UI.UserProfile;

#pragma warning disable IDE1006 // the warning that goes off whenever you use _ or __ or any other nonstandard naming convention
public class UserProfileWindow : Window, IDisposable
{
    private readonly IDalamudTextureWrap _dalamudTextureWrap;
    private readonly GagSpeakConfig _config;
    private readonly UiBuilder _uiBuilder;

    public int _profileIndexOfUserSelected { get; set;}
    public Vector2 mainWindowPosition { get; set; }

    public UserProfileWindow(GagSpeakConfig config, UiBuilder uiBuilder,
        DalamudPluginInterface pluginInterface): base(GetLabel()) {
    {
        // Set the readonlys
        _config = config;
        _uiBuilder = uiBuilder;
        var imagePath = Path.Combine(pluginInterface.AssemblyLocation.Directory?.FullName!, "ReallyHeavyGag.png");
        GagSpeak.Log.Debug($"[Profile Popout]: Loading image from {imagePath}");
        var IconImage = _uiBuilder.LoadImage(imagePath);
        GagSpeak.Log.Debug($"[Profile Popout]: Loaded image from {imagePath}");

        _dalamudTextureWrap = IconImage;
    }
        SizeConstraints = new WindowSizeConstraints() {
            MinimumSize = new Vector2(250, 325),     // Minimum size of the window
            MaximumSize = new Vector2(300, 375) // Maximum size of the window
        };
        // add flags that allow you to move, but not resize the window, also disable collapsible
        Flags = ImGuiWindowFlags.NoCollapse;
        _config = config;
    }

    public override void Draw() {
        // otherwise draw the content
        try
        {
            ImGui.Columns(2,"ProfileTop", false);
            // set column 0 to width of 62
            ImGui.SetColumnWidth(0, 62);
            ImGui.Image(_dalamudTextureWrap.ImGuiHandle, new Vector2(60, 60));
            ImGui.NextColumn();
            var whitelistPlayerData = _config.Whitelist[_profileIndexOfUserSelected];
            ImGui.TextColored(new Vector4(0.2f, 0.9f, 0.2f, 0.9f), $"Player: {whitelistPlayerData.name}");
            ImGui.TextColored(new Vector4(0.4f, 0.9f, 0.4f, 1.0f), $"World: {whitelistPlayerData.homeworld}");
            ImGui.TextColored(new Vector4(0.4f, 0.9f, 0.4f, 1.0f), $"Lean: {(whitelistPlayerData.isDomMode ? "Dominant" : "Submissive")}");
            ImGui.NextColumn(); ImGui.Columns(1); ImGui.Separator();
            if(whitelistPlayerData.relationshipStatus == "None" || whitelistPlayerData.relationshipStatus == "") {
                ImGui.Text($"Player Relation: "); ImGui.SameLine();
                ImGui.TextColored(new Vector4(0.2f, 0.9f, 0.4f, 1.0f), $"{whitelistPlayerData.relationshipStatus}");
            } else {
                ImGui.Text($"{whitelistPlayerData.name.Split(' ')[0]} Is Your "); ImGui.SameLine();
                ImGui.TextColored(new Vector4(0.9f, 0.2f, 0.4f, 1.0f), $"{whitelistPlayerData.relationshipStatus}");
                ImGui.Text($"Commited for: "); ImGui.SameLine();
                ImGui.TextColored(new Vector4(0.9f, 0.9f, 0.2f, 1.0f), $"{whitelistPlayerData.GetCommitmentDuration()}");
            }
            ImGui.Text($"Gag Strength: "); ImGui.SameLine();
            ImGui.TextColored(new Vector4(0.2f, 0.9f, 0.4f, 1.0f), $"{whitelistPlayerData.garbleLevel}");
            ImGui.Separator();
            using var style = ImRaii.PushStyle(ImGuiStyleVar.ButtonTextAlign, new Vector2(0, 0.5f)); 
            // create a table with 3 columns
            DrawGagTabs();
        }
        catch {
            ImGui.NewLine();
            ImGui.Text($"Error while fetching profile information");
            ImGui.NewLine();
        }
    }

    public void DrawGagTabs() {
        using var _ = ImRaii.PushId( "ProfileGagListingInfo" );
        using var tabBar = ImRaii.TabBar( "Tabs");
        if( !tabBar ) return;

        if( ImGui.BeginTabItem( "Layer One" ) ) {
            DrawLayerOneInfo();
            ImGui.EndTabItem();
        }
        if( ImGui.BeginTabItem( "Layer Two" ) ) {
            DrawLayerTwoInfo();
            ImGui.EndTabItem();
        }
        if( ImGui.BeginTabItem( "Layer Three" ) ) {
            DrawLayerThreeInfo();
            ImGui.EndTabItem();
        }
    }

    public void DrawLayerOneInfo() {
        using var child2 = ImRaii.Child( "LayerOneInfo" );
        using (var table2 = ImRaii.Table("RelationsManagerTable", 2, ImGuiTableFlags.RowBg)) {
            if (!table2)
                return;
            ImGui.TableSetupColumn("Info Piece", ImGuiTableColumnFlags.WidthFixed, ImGui.GetContentRegionAvail().X/3);
            ImGui.TableSetupColumn("Information", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableNextRow();
            ImGuiUtil.DrawFrameColumn("Gag Type: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
            ImGui.Text($"{_config.Whitelist[_profileIndexOfUserSelected].selectedGagTypes[0]}");
            ImGui.TableNextRow();
            ImGuiUtil.DrawFrameColumn("Padlock: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
            ImGui.Text($"{_config.Whitelist[_profileIndexOfUserSelected].selectedGagPadlocks[0]}");
            ImGui.TableNextRow();
            ImGuiUtil.DrawFrameColumn("Time Left: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
            ImGui.Text($"{_config.Whitelist[_profileIndexOfUserSelected].GetPadlockTimerDurationLeft(0)}");
            ImGui.TableNextRow();
            ImGuiUtil.DrawFrameColumn("Gag Assigner: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
            ImGui.Text($"{_config.Whitelist[_profileIndexOfUserSelected].selectedGagPadlocksAssigner[0]}");
        }
    }

    public void DrawLayerTwoInfo() {
        using var child3 = ImRaii.Child( "LayerTwoInfo" );
        using (var table3 = ImRaii.Table("RelationsManagerTable", 2, ImGuiTableFlags.RowBg)) {
            if (!table3)
                return;
            ImGui.TableSetupColumn("Info Piece", ImGuiTableColumnFlags.WidthFixed, ImGui.GetContentRegionAvail().X/3);
            ImGui.TableSetupColumn("Information", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableNextRow();
            ImGuiUtil.DrawFrameColumn("Gag Type: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
            ImGui.Text($"{_config.Whitelist[_profileIndexOfUserSelected].selectedGagTypes[1]}");
            ImGui.TableNextRow();
            ImGuiUtil.DrawFrameColumn("Padlock: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
            ImGui.Text($"{_config.Whitelist[_profileIndexOfUserSelected].selectedGagPadlocks[1]}");
            ImGui.TableNextRow();
            ImGuiUtil.DrawFrameColumn("Time Left: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
            ImGui.Text($"{_config.Whitelist[_profileIndexOfUserSelected].GetPadlockTimerDurationLeft(1)}");
            ImGui.TableNextRow();
            ImGuiUtil.DrawFrameColumn("Gag Assigner: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
            ImGui.Text($"{_config.Whitelist[_profileIndexOfUserSelected].selectedGagPadlocksAssigner[1]}");
        }
    }

    public void DrawLayerThreeInfo() {
        using var child4 = ImRaii.Child( "LayerThreeInfo" );
        using (var table4 = ImRaii.Table("RelationsManagerTable", 2, ImGuiTableFlags.RowBg)) {
            if (!table4)
                return;
            ImGui.TableSetupColumn("Info Piece", ImGuiTableColumnFlags.WidthFixed, ImGui.GetContentRegionAvail().X/3);
            ImGui.TableSetupColumn("Information", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableNextRow();
            ImGuiUtil.DrawFrameColumn("Gag Type: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
            ImGui.Text($"{_config.Whitelist[_profileIndexOfUserSelected].selectedGagTypes[2]}");
            ImGui.TableNextRow();
            ImGuiUtil.DrawFrameColumn("Padlock: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
            ImGui.Text($"{_config.Whitelist[_profileIndexOfUserSelected].selectedGagPadlocks[2]}");
            ImGui.TableNextRow();
            ImGuiUtil.DrawFrameColumn("Time Left: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
            ImGui.Text($"{_config.Whitelist[_profileIndexOfUserSelected].GetPadlockTimerDurationLeft(2)}");
            ImGui.TableNextRow();
            ImGuiUtil.DrawFrameColumn("Gag Assigner: "); ImGui.TableNextColumn(); // Next Row (Commitment Length)
            ImGui.Text($"{_config.Whitelist[_profileIndexOfUserSelected].selectedGagPadlocksAssigner[2]}");
        }
    }

    public void Dispose() {
        _dalamudTextureWrap.Dispose();
    }

    // basic string function to get the label of title for the window
    private static string GetLabel() => "GagSpeakProfileViewer###GagSpeakProfileViewer"; 
}

#pragma warning restore IDE1006