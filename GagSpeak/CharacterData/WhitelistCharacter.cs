using System;
using GagSpeak.Gagsandlocks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GagSpeak.CharacterData;

/// <summary> A class to hold the data for the whitelist character </summary>
public class WhitelistedCharacterInfo : CharacterInfoBase
{
    public string           _name { get; set; }                            // get the character name
    public string           _homeworld { get; set; }                        // get the characters world (dont know how to get this for now)
    public RoleLean         _yourStatusToThem { get; set; }                 // who you are to them
    public RoleLean         _theirStatusToYou { get; set; }                 // who they are to you
    public RoleLean         _pendingRelationRequestFromYou { get; set; }    // displays the current dyanmic request sent by you to this player
    public RoleLean         _pendingRelationRequestFromPlayer { get; set; } // displays the current dynamic request from this player to you
    public DateTimeOffset   _timeOfCommitment { get; set; }                 // how long has your commitment lasted?
    public bool             _grantExtendedLockTimes { get; set; } = false;  // [TIER 2] if whitelisted user allows you to use extended lock times
    public string           _theirTriggerPhrase { get; set; } = "";         // [TIER 0] this whitelisted user's trigger phrase
    public bool             _allowsSitRequests { get; set; } = false;       // [TIER 1] if they allow you to use sit requests
    public bool             _allowsMotionRequests { get; set; } = false;    // [TIER 2] if they allow you to use motion requests
    public bool             _allowsAllCommands { get; set; } = false;       // [TIER 4] If they allow you to use all commands on them
    public bool             _allowsChangingToyState { get; set; } = false;  // [TIER 1] if the whitelisted player is allowed to change the toy state
    public bool             _allowsUsingPatterns { get; set; } = false;     // [TIER 0] This should appear as a single var in whitelist, and a list of that var in player info to match whitelist
    ////////////////////////////////////////////////// PROTECTED FIELDS ////////////////////////////////////////////////////
    
    public WhitelistedCharacterInfo() : this("None", "None") { }
    public WhitelistedCharacterInfo(string name, string homeworld) {
        _name = name;
        _homeworld = homeworld;
        _yourStatusToThem = RoleLean.None;
        _theirStatusToYou = RoleLean.None;
        _pendingRelationRequestFromPlayer = RoleLean.None;
        _pendingRelationRequestFromYou = RoleLean.None;
        _timeOfCommitment = DateTimeOffset.Now;
    }
#region General Interactions
    public bool IsRoleLeanDominant(RoleLean roleLean) {
        if(roleLean == RoleLean.Mistress || roleLean == RoleLean.Master || roleLean == RoleLean.Owner) {
            return true;
        }
        return false;
    }

    public bool IsRoleLeanSubmissive(RoleLean roleLean) {
        if(roleLean == RoleLean.Pet || roleLean == RoleLean.Slave || roleLean == RoleLean.AbsoluteSlave) {
            return true;
        }
        return false;
    }
    /// <summary> Sets the time of commitment </summary>
    public void Set_timeOfCommitment() {
        _timeOfCommitment = DateTimeOffset.Now;
    }

    /// <summary> gets the time of commitment </summary>
    /// <returns>The time of commitment.</returns>
    public string GetCommitmentDuration() {
        if (_timeOfCommitment == default(DateTimeOffset))
            return ""; // Display nothing if commitment time is not set
        TimeSpan duration = DateTimeOffset.Now - _timeOfCommitment; // Get the duration
        int days = duration.Days % 30;
        // Display the duration in the desired format
        return $"{days}d, {duration.Hours}h, {duration.Minutes}m, {duration.Seconds}s";
    }

    /// <summary> gets the duration left on a timed padlock type specified by the index </summary>
    /// <returns>The duration left on the padlock.</returns>
    public string GetPadlockTimerDurationLeft(int index) {
        TimeSpan duration = _selectedGagPadlockTimer[index] - DateTimeOffset.Now; // Get the duration
        if (duration < TimeSpan.Zero) {
            // check if the padlock type was a type with a timer, and if so, set the other stuff to none
            if (_selectedGagPadlocks[index] == Padlocks.FiveMinutesPadlock
            ||  _selectedGagPadlocks[index] == Padlocks.MistressTimerPadlock
            ||  _selectedGagPadlocks[index] == Padlocks.TimerPasswordPadlock)
            {
                // set the padlock type to none
                _selectedGagPadlocks[index] = Padlocks.None;
                _selectedGagPadlockPassword[index] = "";
                _selectedGagPadlockAssigner[index] = "";
            }
            return "";
        }
        // Display the duration in the desired format
        return $"{duration.Hours}h, {duration.Minutes}m, {duration.Seconds}s";
    }
#endregion General Interactions
#region State Fetching / Setting
    /// <summary> get the spesified tier of a current dynamic </summary>
    public DynamicTier GetDynamicTier() {
        // If a two way dyanamic is note yet established, then our tier is 0.
        if (_yourStatusToThem == RoleLean.None || _theirStatusToYou == RoleLean.None) {
                return DynamicTier.Tier0;
        }
        // TIER 1 == dynamic of PET/SLAVE/ABSOLUTE-SLAVE with MISTRESS|MASTER/OWNER.
        if (_yourStatusToThem == RoleLean.Mistress || _yourStatusToThem == RoleLean.Master || _yourStatusToThem == RoleLean.Owner) {
            if(_theirStatusToYou == RoleLean.Submissive || _theirStatusToYou == RoleLean.Pet
            || _theirStatusToYou == RoleLean.Slave || _theirStatusToYou == RoleLean.AbsoluteSlave) {
                return DynamicTier.Tier1;
        }}
        // TIER 2 == dynamic of SLAVE/ABSOLUTE-SLAVE with MISTRESS|MASTER/OWNER.
        else if (_yourStatusToThem == RoleLean.Mistress || _yourStatusToThem == RoleLean.Master || _yourStatusToThem == RoleLean.Owner) {
            if(_theirStatusToYou == RoleLean.Slave || _theirStatusToYou == RoleLean.AbsoluteSlave) {
                return DynamicTier.Tier2;
        }}
        // TIER 3 == dynamic of SLAVE/ABSOLUTE-SLAVE with OWNER.
        else if (_yourStatusToThem == RoleLean.Owner) {
            if(_theirStatusToYou == RoleLean.Slave || _theirStatusToYou == RoleLean.AbsoluteSlave) {
                return DynamicTier.Tier3;
        }}
        // TIER 4 == dynamic of ABSOLUTE-SLAVE with OWNER.
        else if (_yourStatusToThem == RoleLean.Owner) {
            if(_theirStatusToYou == RoleLean.AbsoluteSlave) {
                return DynamicTier.Tier4;
        }}
        // we should never make it here, but if we do, set the dynamic to 0 anyways
        return DynamicTier.Tier0;
    }
#endregion State Fetching / Setting

#region Serialization and Deserialization
    public override JObject Serialize() {
        try{
            JObject derivedSerialized = new JObject() {
                ["Name"] = _name,
                ["Homeworld"] = _homeworld,
                ["YourStatusToThem"] = _yourStatusToThem.ToString(),
                ["TheirStatusToYou"] = _theirStatusToYou.ToString(),
                ["PendingRequestFromYou"] = _pendingRelationRequestFromYou.ToString(),
                ["PendingRequestFromPlayer"] = _pendingRelationRequestFromPlayer.ToString(),
                ["TimeOfCommitment"] = JsonConvert.SerializeObject(_timeOfCommitment),
                ["ExtendedLockTimes"] = _grantExtendedLockTimes,
                ["TriggerPhrase"] = _theirTriggerPhrase,
                ["AllowsSitRequests"] = _allowsSitRequests,
                ["AllowsMotionRequests"] = _allowsMotionRequests,
                ["AllowsAllCommands"] = _allowsAllCommands,
                ["AllowsChangingToyState"] = _allowsChangingToyState,
                ["AllowsUsingPatterns"] = _allowsUsingPatterns,
            };
            // merge with the base serialization
            JObject baseSerialized = base.Serialize();
            derivedSerialized.Merge(baseSerialized);
            // return it
            return derivedSerialized;
        } catch (Exception e) {
            Console.WriteLine($"[WhitelistedCharacterInfo] Error in Serialize: {e}");
            return new JObject();
        }
    }

    public override void Deserialize(JObject jsonObject) {
        base.Deserialize(jsonObject);
        _name = jsonObject["Name"]?.Value<string>() ?? "None";
        _homeworld = jsonObject["Homeworld"]?.Value<string>() ?? "None";
        _yourStatusToThem = Enum.TryParse(jsonObject["YourStatusToThem"]?.Value<string>(), out RoleLean statusToThem) ? statusToThem : RoleLean.None;
        _theirStatusToYou = Enum.TryParse(jsonObject["TheirStatusToYou"]?.Value<string>(), out RoleLean statusToYou) ? statusToYou : RoleLean.None;
        _pendingRelationRequestFromYou = Enum.TryParse(jsonObject["PendingRequestFromYou"]?.Value<string>(), out RoleLean requestFromYou) ? requestFromYou : RoleLean.None;
        _pendingRelationRequestFromPlayer = Enum.TryParse(jsonObject["PendingRequestFromPlayer"]?.Value<string>(), out RoleLean requestFromPlayer) ? requestFromPlayer : RoleLean.None;
        _timeOfCommitment = JsonConvert.DeserializeObject<DateTimeOffset>(jsonObject["TimeOfCommitment"]?.Value<string>() ?? "");
        _grantExtendedLockTimes = jsonObject["ExtendedLockTimes"]?.Value<bool>() ?? false;
        _theirTriggerPhrase = jsonObject["TheirTriggerPhrase"]?.Value<string>() ?? "";
        _allowsSitRequests = jsonObject["AllowsSitRequests"]?.Value<bool>() ?? false;
        _allowsMotionRequests = jsonObject["AllowsMotionRequests"]?.Value<bool>() ?? false;
        _allowsAllCommands = jsonObject["AllowsAllCommands"]?.Value<bool>() ?? false;
        _allowsChangingToyState = jsonObject["AllowsChangingToyState"]?.Value<bool>() ?? false;
        _allowsUsingPatterns = jsonObject["AllowsUsingPatterns"]?.Value<bool>() ?? false;
    }

#endregion Serialization and Deserialization
}

