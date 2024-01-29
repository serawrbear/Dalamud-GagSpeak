using System;
using System.Collections.Generic;
using Penumbra.GameData.Enums;
using GagSpeak.Data;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using System.Linq;
using OtterGui.Classes;
using GagSpeak.Services;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using Penumbra.GameData.Structs;

namespace GagSpeak.Wardrobe;

public class RestraintSetManager : ISavable
{
    public List<RestraintSet> _restraintSets = []; // stores the restraint sets

    [JsonIgnore]
    private readonly SaveService _saveService;

    public RestraintSetManager(SaveService saveService) {
        _saveService = saveService;
        
        // load the information from our storage file
        Load();
        // correctly account for any non-updated timed locked sets
        foreach (var set in _restraintSets) {
            if (set._locked && set._lockedTimer < DateTimeOffset.Now) {
                set._locked = false;
            }
        }
    }
    
    #region Manager Methods

    public void AddNewRestraintSet() {
        var newSet = new RestraintSet();
        string baseName = newSet._name;
        int copyNumber = 1;
        while (_restraintSets.Any(set => set._name == newSet._name)) {
            newSet.ChangeSetName(baseName + $"(copy{copyNumber})");
            copyNumber++;
        }
        _restraintSets.Add(newSet);
        Save();
    }

    /// <summary> Deletes a restraint set spesified by index if it exists. </summary>
    public void DeleteRestraintSet(int index) {
        // delete a restraint set spesified by index if it exists
        if (index >= 0 && index < _restraintSets.Count) {
            _restraintSets.RemoveAt(index);
            Save();
        }
    }


    /// <summary> Renames a restraint set spesified by index if it exists. </summary>
    public void ChangeRestraintSetName(int restraintSetIdx, string newName) {
        // Check if a set with the same name already exists
        if (_restraintSets.Any(set => _restraintSets[restraintSetIdx]._name == newName)) {
            // If it does, append "copy" to the new set name
            newName += "(copy)";
        }
        // append the new name       
        _restraintSets[restraintSetIdx].ChangeSetName(newName);
        Save(); // update our json after updating the manager 
        // (will remove old set but transfer all info to newly serialized one)
    }

    /// <summary> Changes the description of a restraint set spesified by index if it exists. </summary>
    public void ChangeRestraintSetDescription(int restraintSetIdx, string newDescription) {
        _restraintSets[restraintSetIdx].ChangeSetDescription(newDescription);
        Save();
    }

    /// <summary> Sets the IsEnabled for a restraint set spesified by index if it exists. </summary>
    public void ChangeRestraintSetEnabled(int restraintSetIdx, bool isEnabled) {
        bool anyOtherEnabled = false;
        for (int i = 0; i < _restraintSets.Count; i++) {
            if (i != restraintSetIdx) {
                _restraintSets[i]._enabled = false;
                anyOtherEnabled = anyOtherEnabled || _restraintSets[i]._enabled;
            }
        }
        // make sure its the only one left enabled
        _restraintSets[restraintSetIdx]._enabled = isEnabled && anyOtherEnabled;
        Save();
    }

    /// <summary> Toggles the enabled state of a restraint set spesified by index if it exists. </summary>
    public void ToggleRestraintSetEnabled(int restraintSetIdx) {
        if (_restraintSets[restraintSetIdx]._enabled) {
            // If the restraint set is currently enabled, disable it
            _restraintSets[restraintSetIdx].SetIsEnabled(false);
        } else {
            // If the restraint set is currently disabled, disable all other restraint sets and enable it
            for (int i = 0; i < _restraintSets.Count; i++) {
                if (i != restraintSetIdx) {
                    _restraintSets[i].SetIsEnabled(false);
                }
            }
            _restraintSets[restraintSetIdx].SetIsEnabled(true);
        }
        Save();
    }

    public void LockRestraintSet(int restraintSetIdx, string wasLockedBy = "") {
        // if the set is not enabled, then you cant lock it
        if (!_restraintSets[restraintSetIdx]._enabled) {
            GagSpeak.Log.Debug($"[RestraintSetManager] Cannot lock a disabled set!");
            return;
        }
        _restraintSets[restraintSetIdx].SetIsLocked(true, wasLockedBy);
        Save();
    }

    public bool TryUnlockRestraintSet(int restraintSetIdx, string UnlockerName = "") {
        // if the set is not locked, then you cant unlock it
        if (!_restraintSets[restraintSetIdx]._locked) {
            GagSpeak.Log.Debug($"[RestraintSetManager] Cannot unlock an unlocked set!");
            return false;
        }
        // if the set is not enabled, then you cant unlock it
        if (!_restraintSets[restraintSetIdx]._enabled) {
            GagSpeak.Log.Debug($"[RestraintSetManager] Cannot unlock a disabled set!");
            return false;
        }
        // if the set is locked by someone else, then you cant unlock it
        if (_restraintSets[restraintSetIdx]._wasLockedBy != UnlockerName
        && _restraintSets[restraintSetIdx]._wasLockedBy != "self"
        && _restraintSets[restraintSetIdx]._wasLockedBy != "")
        {
            GagSpeak.Log.Debug($"[RestraintSetManager] Cannot unlock a set locked by someone else!");
            return false;
        }
        _restraintSets[restraintSetIdx].SetIsLocked(false);
        Save();
        return true;
    }

    public void ChangeRestraintSetNewLockEndTime(int restraintSetIdx, DateTimeOffset newEndTime) {
        _restraintSets[restraintSetIdx].DeclareNewEndTimeForSet(newEndTime);
        Save();
    }

    public void ChangeSetDrawDataIsEnabled(int restraintSetIdx, EquipSlot DrawDataSlot, bool isEnabled) {
        _restraintSets[restraintSetIdx]._drawData[DrawDataSlot].SetDrawDataIsEnabled(isEnabled);
        Save();
    }

    public void ChangeSetDrawDataWasEquippedBy(int restraintSetIdx, EquipSlot DrawDataSlot, string wasEquippedBy) {
        _restraintSets[restraintSetIdx]._drawData[DrawDataSlot].SetDrawDataEquippedBy(wasEquippedBy);
        Save();
    }

    public void ChangeSetDrawDataIsLocked(int restraintSetIdx, EquipSlot DrawDataSlot, bool isLocked) {
        _restraintSets[restraintSetIdx]._drawData[DrawDataSlot].SetDrawDataLocked(isLocked);
        Save();
    }

    public void ChangeSetDrawDataSlot(int restraintSetIdx, EquipSlot DrawDataSlot, EquipSlot slot) {
        _restraintSets[restraintSetIdx]._drawData[DrawDataSlot].SetDrawDataSlot(slot);
        Save();
    }

    public void ChangeSetDrawDataGameItem(int restraintSetIdx, EquipSlot DrawDataSlot, EquipItem gameItem) {
        _restraintSets[restraintSetIdx]._drawData[DrawDataSlot].SetDrawDataGameItem(gameItem);
        Save();
    }

    public void ChangeSetDrawDataGameStain(int restraintSetIdx, EquipSlot DrawDataSlot, StainId gameStain) {
        _restraintSets[restraintSetIdx]._drawData[DrawDataSlot].SetDrawDataGameStain(gameStain);
        Save();
    }

    public void ResetSetDrawDataGameItem(int restraintSetIdx, EquipSlot DrawDataSlot) {
        _restraintSets[restraintSetIdx]._drawData[DrawDataSlot].ResetDrawDataGameItem();
        Save();
    }

    public void ResetSetDrawDataGameStain(int restraintSetIdx, EquipSlot DrawDataSlot) {
        _restraintSets[restraintSetIdx]._drawData[DrawDataSlot].ResetDrawDataGameStain();
        Save();
    }

    public void ResetEverythingDueToSafeword() {
        foreach (var set in _restraintSets) {
            set._enabled = false;
            set._locked = false;
            set._wasLockedBy = "";
            set._lockedTimer = DateTimeOffset.Now; 
        }
        GagSpeak.Log.Debug($"[RestraintSetManager] Reset all restraint sets due to safeword!");
        Save();
    }

    #endregion Manager Methods

    #region Json ISavable & Loads
    public string ToFilename(FilenameService filenameService)
        => filenameService.RestraintSetsFile;

    public void Save(StreamWriter writer)
    {
        using var j = new JsonTextWriter(writer);
        j.Formatting = Formatting.Indented;
        Serialize().WriteTo(j);
    }

    private void Save()
        => _saveService.DelaySave(this);

    public JObject Serialize() {
        // create a new array for serialization
        var array = new JArray();
        // for each of our restraint sets, serialize them and add them to the array
        foreach (var set in _restraintSets)
            array.Add(set.Serialize());
        // return the new object under the label "RestraintSets"
        return new JObject() {
            ["RestraintSets"] = array,
        };
    }

    public void Load() {
        #pragma warning disable CS8604, CS8602 // Possible null reference argument.
        var file = _saveService.FileNames.RestraintSetsFile;
        _restraintSets.Clear();
        if (!File.Exists(file)) {
            return;
        }
        try {
            var text = File.ReadAllText(file);
            var jsonObject = JObject.Parse(text);
            var restraintSetsArray = jsonObject["RestraintSets"]?.Value<JArray>();
            foreach (var item in restraintSetsArray) {
                var restraintSet = new RestraintSet();
                restraintSet.Deserialize(item.Value<JObject>());
                _restraintSets.Add(restraintSet);
            }
        } catch (Exception ex) {
            GagSpeak.Log.Error($"Failure to load automated designs: Error during parsing. {ex}");
        } finally {
            GagSpeak.Log.Debug($"[GagStorageManager] RestraintSets.json loaded! Loaded {_restraintSets.Count} restraint sets.");
        }
        #pragma warning restore CS8604, CS8602 // Possible null reference argument.
    }
}
#endregion Json ISavable & Loads