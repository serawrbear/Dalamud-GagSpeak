using System;
using System.Collections.Generic;
using GagSpeak.Events;
using GagSpeak.Gagsandlocks;
using Newtonsoft.Json.Linq;

namespace GagSpeak.CharacterData;

/// <summary>
/// General settings that are both visable to client and whitelisted players, and modifyable, given correct access.
/// <para> Access is granted by the following (Note for all these conditions, the one with edit permissions is the dominant): </para>
/// <list type="bullet">
///     <item>[ TIER 0 ] A readonly field, for displaying information </item>
///     <item>[ TIER 1 ] Modifiable by a dynamic of PET/SLAVE/ABSOLUTE-SLAVE -- MISTRESS|MASTER/OWNER. </item>
///     <item>[ TIER 2 ] Modifiable by a dynamic of SLAVE/ABSOLUTE-SLAVE -- MISTRESS|MASTER/OWNER. </item>
///     <item>[ TIER 3 ] Modifiable by a dynamic of SLAVE/ABSOLUTE-SLAVE -- OWNER. </item>
///     <item>[ TIER 4 ] Modifiable by a dynamic of ABSOLUTE-SLAVE -- OWNER. </item>
/// </list>
/// </summary>
public class CharacterInfoBase
{
    //////////////////////////////////////// GENERAL STATUS VISIBILITY  ///////////////////////////////////////
    public  bool                _safewordUsed { get; set; } = false;            // [TIER 0] Has the safeword been used?
    /////////////////////////////////////////// CHAT GARBLER STATES  //////////////////////////////////////////
    public  bool                _directChatGarblerActive { get; set; } = false; // [TIER 4] Is direct chat garbler enabled?
    public  bool                _directChatGarblerLocked { get; set; } = false; // [TIER 3] Is live chat garbler enabled?
    ////////////////////////////////////////// GAG AND LOCK VARIABLES //////////////////////////////////////////
    public  WatchList<string>   _selectedGagTypes { get; set; }                 // [TIER 0] What gag types are selected?
    public  WatchList<Padlocks> _selectedGagPadlocks { get; set; }              // [TIER 0] which padlocks are equipped currently?
    public  List<string>        _selectedGagPadlockPassword { get; set; }       // [TIER 0] password lock on padlocks, if any
    public  List<DateTimeOffset>_selectedGagPadlockTimer { get; set; }          // [TIER 0] stores time when the padlock will be unlocked.
    public  List<string>        _selectedGagPadlockAssigner { get; set; }       // [TIER 0] name of who assigned the padlocks
    ///////////////////////////////////////// WARDROBE COMPONENT SETTINGS  //////////////////////////////////////
    public  bool                _enableWardrobe { get; set; } = false;          // [TIER 0] enables / disables all wardrobe functionality
    public  bool                _lockGagStorageOnGagLock { get; set; } = false; // [TIER 1] locks storage ui when gag is locked
    ///////////////////////////////////////////// PUPPETEER SETTINGS  ////////////////////////////////////////////
    public  bool                _allowPuppeteer { get; set; } = false;          // [TIER 0] lets the player set if puppeteer will function
    /////////////////////////////////////////// TOYBOX MODULE SETTINGS ////////////////////////////////////////
    public  bool                _enableToybox { get; set; } = false;            // [TIER 4] if granting them access to toybox
    public  bool                _isToyActive { get; set; } = false;             // [TIER 2] if the toybox is currently active
    public  int                 _intensityLevel { get; set; } = 0;              // [TIER 2] the guage from 0 to _activeToystepInterval     
    public  bool                _lockToyboxUI { get; set; } = false;            // [TIER 3] if granting them access to lock toybox        
    ///////////////////////////////////////// FUTURE MODULES CAN GO HERE ////////////////////////////////////////
    public CharacterInfoBase() {
        _selectedGagTypes = new WatchList<string> { "None", "None", "None" };
        _selectedGagPadlocks = new WatchList<Padlocks> { Padlocks.None, Padlocks.None, Padlocks.None };
        _selectedGagPadlockPassword = new List<string> { "", "", "" };
        _selectedGagPadlockTimer = new List<DateTimeOffset> { DateTimeOffset.Now, DateTimeOffset.Now, DateTimeOffset.Now };
        _selectedGagPadlockAssigner = new List<string> { "", "", "" };
    }
#region Json Saving and Loading
    public virtual JObject Serialize() {
        return new JObject()
        {
            ["SafewordUsed"] = _safewordUsed,
            ["DirectChatGarblerActive"] = _directChatGarblerActive,
            ["DirectChatGarblerLocked"] = _directChatGarblerLocked,
            ["SelectedGagTypes"] = new JArray(_selectedGagTypes),
            ["SelectedGagPadlocks"] = new JArray(_selectedGagPadlocks),
            ["SelectedGagPadlockPassword"] = new JArray(_selectedGagPadlockPassword),
            ["SelectedGagPadlockTimer"] = new JArray(_selectedGagPadlockTimer),
            ["SelectedGagPadlockAssigner"] = new JArray(_selectedGagPadlockAssigner),
            ["EnableWardrobe"] = _enableWardrobe,
            ["LockGagStorageOnGagLock"] = _lockGagStorageOnGagLock,
            ["AllowPuppeteer"] = _allowPuppeteer,
            ["EnableToybox"] = _enableToybox,
            ["IsToyActive"] = _isToyActive,
            ["IntensityLevel"] = _intensityLevel,
            ["AllowToyboxLocking"] = _lockToyboxUI
        };
    }

    public virtual void Deserialize(JObject jsonObject) {
        _safewordUsed = jsonObject["SafewordUsed"]?.Value<bool>() ?? false;
        _directChatGarblerActive = jsonObject["DirectChatGarblerActive"]?.Value<bool>() ?? false;
        _directChatGarblerLocked = jsonObject["DirectChatGarblerLocked"]?.Value<bool>() ?? false;
        _selectedGagTypes = jsonObject["SelectedGagTypes"]?.ToObject<WatchList<string>>() ?? new WatchList<string> { "None", "None", "None" };
        _selectedGagPadlocks = jsonObject["SelectedGagPadlocks"]?.ToObject<WatchList<Padlocks>>() ?? new WatchList<Padlocks> { Padlocks.None, Padlocks.None, Padlocks.None };
        _selectedGagPadlockPassword = jsonObject["SelectedGagPadlockPassword"]?.ToObject<List<string>>() ?? new List<string> { "", "", "" };
        _selectedGagPadlockTimer = jsonObject["SelectedGagPadlockTimer"]?.ToObject<List<DateTimeOffset>>() ?? new List<DateTimeOffset> { DateTimeOffset.Now, DateTimeOffset.Now, DateTimeOffset.Now };
        _selectedGagPadlockAssigner = jsonObject["SelectedGagPadlockAssigner"]?.ToObject<List<string>>() ?? new List<string> { "", "", "" };
        _enableWardrobe = jsonObject["EnableWardrobe"]?.Value<bool>() ?? false;
        _lockGagStorageOnGagLock = jsonObject["LockGagStorageOnGagLock"]?.Value<bool>() ?? false;
        _allowPuppeteer = jsonObject["AllowPuppeteer"]?.Value<bool>() ?? false;
        _enableToybox = jsonObject["EnableToybox"]?.Value<bool>() ?? false;
        _isToyActive = jsonObject["IsToyActive"]?.Value<bool>() ?? false;
        _intensityLevel = jsonObject["IntensityLevel"]?.Value<byte>() ?? 0;
        _lockToyboxUI = jsonObject["AllowToyboxLocking"]?.Value<bool>() ?? false;
    }
#endregion Json Saving and Loading
}