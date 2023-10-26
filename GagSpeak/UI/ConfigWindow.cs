﻿using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using ImGuiScene;
using OtterGui.Raii;
﻿using Dalamud.Game.Text;
using Dalamud.Plugin;
using System.Diagnostics;
using Num = System.Numerics;
using System.Collections.Generic;

namespace GagSpeak
{

    // General ImGUI structure:
    // ref = what trigger is flipped
    // IsItemHovered = what displayts when hovered over
    // SetToolTip = text to display on hover

    public unsafe partial class GagSpeak : IDalamudPlugin {
        // Create a function for our config UI window
        private void GagSpeakConfigUI() {
            // If our _config is enabled, we should show the window!
            if(_config)
            {
                // Set window min and max size
                ImGui.SetNextWindowSizeConstraints(new Num.Vector2(600, 850), new Num.Vector2(1920, 1080));
                // Declare the name of the GUI component
                ImGui.Begin("Chat Bubbles Config", ref _config);
                // First, declare a space for people to type in their safeword
                ImGui.InputText("Safeword", ref _safeword, 128);
                if (ImGui.IsItemHovered()) {
                    ImGui.SetTooltip("This Safeword let's you override gags lock restrictions, but wont be able to gag again for awhile if you do.");
                }
                // Below this, put a horizontal line.
                ImGui.NewLine();
                ImGui.Separator();

                // In this line, include 3 checkboxes. One for FriendOnly, one for PartyOnly, one for WhitelistOnly
                ImGui.Checkbox("Only Friends", ref _friendsOnly);
                if (ImGui.IsItemHovered()) {
                    ImGui.SetTooltip("Will not process /gag commands unless they are recieved from a player on your friend list.");
				}
                ImGui.SameLine(); // This just ensures it happens on the same line
				ImGui.Checkbox("Only Party Members", ref _partyOnly);
                if (ImGui.IsItemHovered()) {
                    ImGui.SetTooltip("Will not process /gag commands unless they are recieved from a player in your party.");
                }
                ImGui.SameLine();
				ImGui.Checkbox("Only Whitelist", ref _whitelistOnly);
                if (ImGui.IsItemHovered()) {
                    ImGui.SetTooltip("Will not process /gag commands unless they are recieved from a player in your plugins whitelist.");
                }

                // Below this is a debug option. When checked, display notable info.
                ImGui.Checkbox("Debug Logging", ref _debug);
                if (ImGui.IsItemHovered()) {
                    ImGui.SetTooltip( "Enable logging for debug purposes.");
                }
                // Show Debug Menu when Debug logging is enabled
                if (_debug) {
                    ImGui.Text("DEBUG INFORMATION:");
                    try
                    {
                        // Eventually, display the following:
                        // Layer 1 Gag Type, Is Locked?, Lock Type (if Owner Locked, display owner name), Gag muffle Level,
                        // Layer 2 Gag Type, Is Locked?, Lock Type (if Owner Locked, display owner name), Gag muffle Level,
                        // Layer 3 Gag Type, Is Locked?, Lock Type (if Owner Locked, display owner name), Gag muffle Level,
                        // Total Garble Level
                        // Gag Capacity (should max at 3)
                        // Safeword
                        // Safeword Cooldown Timer
                        ImGui.Text($"Sample Debug Message");
                    }
                    catch (Exception e)
                    {
                        ImGui.Text($"Error while fetching config in debug: {e}");
                    }
                }
                // Below this, put a horizontal line.
                ImGui.NewLine();
                ImGui.Separator();

                // At the top of this section, write out in a larger font than the rest in bold of your gags have been locked or not.
                // In this section, display the full list of gags, and allow the user to select which ones they want to have on.
                // The system should only allow a maximum of 3 to be selected at once.


                // ImGui.Text($"Layer 1 GagType:");
                // if (ImGui.IsItemHovered()) {
                //     ImGui.SetTooltip("Select the first gag type to use.");
                // }
                // var selectedGagLayer1Type = this.Configuration.GagTypes;
                // if(ImGui.BeginCombo("Select Gag Type##GagTypes", $"{selectedGagLayer1Type}")) {
                //     // For each type of gag in the gagTypes list
                //     foreach(KeyValuePair<string, int> entry in selectedGagLayer1Type)
                //     {
                //         if(ImGui.Selectable($"{entry.Key}", entry.Key == ))
                //             this.Configuration.OpenBrioBehavior = openBrioBehavior;
                //         // do something with entry.Value or entry.Key
                //         }
                //         ImGui.EndCombo();
                //     }
                //     ImGui.PopItemWidth();
                // }
                using var combo = ImRaii.Combo( "Label" );
                if( !combo ) return;

                ImGui.SetNextItemWidth(200f);
                if( ImGui.InputText( "Search", ref SearchText, 255 ) ) {
                    SearchedItems = string.isNullOrEmpty(SearchText) ? 
                        AllItems : AllItems.Where( x => x.ToLowerCase().Contains(SearchText.ToLowerCase()));
                }

                using var child = ImRaii.Child( "Child", new Vector2(ImGui.GetWindowContentRegionAvail().X, 200), true);
                foreach( var item in SearchedItems ) {
                if( ImGui.Selectable( item ) ) {
                    // can close the combo here
                }
                }


                // Below this, put a horizontal line.
                ImGui.NewLine();
                ImGui.Separator();

                // in this section, display the checkboxes for all of the different chat types, 
                // allowing the user to select only the chats they want their garbles messages to process through.

                var i = 0;
                ImGui.Text("Enabled channels:");
                if (ImGui.IsItemHovered()) {
                    ImGui.SetTooltip("Which chat channels to show bubbles for.");
                }

                ImGui.Columns(4);
                foreach (var e in (XivChatType[]) Enum.GetValues(typeof(XivChatType))) {
                    // If the chat type is a valid chat type
                    if (_yesno[i]) {
                        // See if it is already enabled by default
                        var enabled = _channels.Contains(e);
                        // If a checkbox exists (it always will)...
                        if (ImGui.Checkbox($"{e}", ref enabled)) {
                            // See If checkbox is clicked, If not, add to list of enabled channels, otherwise, remove it.
                            if (enabled) _channels.Add(e);
                            else _channels.Remove(e);
                        }
                        ImGui.NextColumn();
                    }
                    i++;
                }

                // Set the columns back to 1 now and space over to next section
                ImGui.Columns(1);
                ImGui.NewLine();
                ImGui.Separator();
                ImGui.NewLine();

                // Below this, have a button to save and close the config. Next to it, have a button to link to my Ko-Fi
                // If the save & close button is clicked
                if (ImGui.Button("Save and Close Config")) {
                    // Save the config, and toggle _config, forcing it to close
                    SaveConfig();
                    _config = false;
                }

                // In that same line...
                ImGui.SameLine();
                // Configure the style for our next button
                ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | 0x005E5BFF);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | 0x005E5BFF);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | 0x005E5BFF);
                ImGui.Text(" ");
                ImGui.SameLine();

                // And now have that button be for the Ko-Fi Link
                if (ImGui.Button("Tip Cordy for her hard work!")) {
                    ImGui.SetTooltip( "Only if you want to though!");
                    Process.Start(new ProcessStartInfo {FileName = "https://ko-fi.com/cordeliamist", UseShellExecute = true});
                }

                ImGui.PopStyleColor(3);
                ImGui.End();
            }
        }
    }
}