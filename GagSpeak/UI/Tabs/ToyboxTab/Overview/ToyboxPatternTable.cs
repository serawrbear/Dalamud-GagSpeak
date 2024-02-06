using System.Numerics;
using ImGuiNET;
using OtterGui.Raii;
using GagSpeak.CharacterData;
using System.Linq;
using GagSpeak.ToyboxandPuppeteer;

namespace GagSpeak.UI.Tabs.ToyboxTab;
public partial class ToyboxPatternTable {

    private readonly    CharacterHandler            _characterHandler;
    private             PatternHandler              _patternHandler;
    public ToyboxPatternTable(CharacterHandler characterHandler, PatternHandler patternHandler) {
        _characterHandler = characterHandler;
        _patternHandler = patternHandler;
    }

    public void Draw() {
        var _ = ImRaii.Group();
        var spacing = ImGui.GetStyle().ItemInnerSpacing with { Y = ImGui.GetStyle().ItemInnerSpacing.Y };
        ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        // draw out the table
        DrawPatternHeader();
        DrawPatternsTable();
    }

    private void DrawPatternHeader() // Draw our header
        => WindowHeader.Draw("Stored Patterns", 0, ImGui.GetColorU32(ImGuiCol.FrameBg), 0, 0);

    private void DrawPatternsTable() {
        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(ImGui.GetStyle().CellPadding.X * 0.2f, ImGui.GetStyle().CellPadding.Y)); // Modify the X padding
        try{
        using (var table = ImRaii.Table("UniqueAliasListCreator", 4, ImGuiTableFlags.RowBg, new Vector2(-1, -1))) {
        if (!table) { return; }
            // draw the header row
            ImGui.TableSetupColumn("##Delete", ImGuiTableColumnFlags.WidthFixed, ImGui.GetFrameHeight());
            ImGui.TableSetupColumn("Alias Text Input", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("ThisIsALongName").X);
            ImGui.TableSetupColumn("Use##IsEnabled", ImGuiTableColumnFlags.WidthFixed, ImGui.GetFrameHeight());
            ImGui.TableSetupColumn("Replacement text to execute instead", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableHeadersRow();
            // Replace this with your actual data
            foreach (var (aliasTrigger, idx) in _characterHandler.playerChar._triggerAliases.ElementAt(_characterHandler.activeListIdx)
                                                                                    ._aliasTriggers.Select((value, index) => (value, index)))
            {
                using var id = ImRaii.PushId(idx);
                DrawAssociatedModRow(aliasTrigger, idx);
            }
            DrawNewModRow();
        }
        } catch (System.Exception e) {
            GagSpeak.Log.Debug($"{e} Error drawing the pattern table");
        } finally {
            ImGui.PopStyleVar();
        }
    }

    private void DrawAssociatedModRow(AliasTrigger aliasTrigger, int idx) {
        // ImGui.TableNextColumn();
        // if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Trash.ToIconString(), new Vector2(ImGui.GetFrameHeight()),
        //         "Delete this alias from associations", false, true))
        //     _characterHandler.RemoveAliasEntry(idx); // Use the helper function to remove the alias entry

        // ImGui.TableNextColumn();
        // string aliasText = _tempAliasText ?? aliasTrigger._inputCommand;
        // ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        // if (ImGui.InputText($"##aliasText{idx}", ref aliasText, 50)) {
        //     _tempAliasText = aliasText; // Update the alias entry input
        // }
        // if(ImGui.IsItemDeactivatedAfterEdit()) {
        //     _characterHandler.UpdateAliasEntryInput(idx, aliasText);
        //     _tempAliasText = null;
        // }
        // ImGui.TableNextColumn();
        // bool isEnabled = aliasTrigger._enabled;
        // if (ImGui.Checkbox($"##isEnabled{idx}", ref isEnabled)) {
        //     _characterHandler.UpdateAliasEntryEnabled(idx, isEnabled); // Use the helper function to update the alias entry enabled status
        // }


        // ImGui.TableNextColumn();
        // string command = _tempAliasCommand ?? aliasTrigger._outputCommand;
        // ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        // if (ImGui.InputText($"##command{idx}", ref command, 200)) // If the input text is modified
        //     _tempAliasCommand = command; // Update the alias entry output
        // if(ImGui.IsItemDeactivatedAfterEdit()) {
        //     _characterHandler.UpdateAliasEntryOutput(idx, command);
        //     _tempAliasText = null;
        // }
    }

    private void DrawNewModRow() {
        // ImGui.TableNextColumn();
        // if (ImGuiUtil.DrawDisabledButton(FontAwesomeIcon.Plus.ToIconString(), new Vector2(ImGui.GetFrameHeight()), "", false, true)){
        //     _characterHandler.AddNewAliasEntry(_tempNewAlias); // Use the helper function to add the new alias entry
        //     _tempNewAlias = new AliasTrigger();
        // }
        // ImGui.TableNextColumn();
        // string newAliasText = _tempNewAlias._inputCommand;
        // ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        // if (ImGui.InputText("##newAliasText", ref newAliasText, 50)){
        //     _tempNewAlias._inputCommand = newAliasText; // Update the new alias entry input
        // }
        // ImGui.TableNextColumn();
        // bool newAliasEnabled = _tempNewAlias._enabled;
        // if (ImGui.Checkbox("##newAliasEnabled", ref newAliasEnabled)) {
        //     _tempNewAlias._enabled = newAliasEnabled; // Update the new alias entry enabled status
        // }
        // ImGui.TableNextColumn();
        // ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
        // string newAliasCommand = _tempNewAlias._outputCommand;
        // if (ImGui.InputText("##newAliasCommand", ref newAliasCommand, 200)) {
        //     _tempNewAlias._outputCommand = newAliasCommand; // Update the new alias entry output
        // }
    }
}