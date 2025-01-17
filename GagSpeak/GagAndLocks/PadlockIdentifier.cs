using System;
using ImGuiNET;
using System.Text.RegularExpressions;
using System.Linq;
using GagSpeak.CharacterData;

namespace GagSpeak.Gagsandlocks;
/// <summary>
/// This class is used to handle the the idenfitication of padlocks before and after they are equipped, seperate from the config padlocks yet linked all the same
/// </summary>
public class PadlockIdentifier
{
    public string       _inputPassword { get; set; }                    // This will hold the input password
    public string       _inputCombination { get; set; }                 // This will hold the input combination
    public string       _inputTimer { get; set; }                       // This will hold the input timer
    public string       _storedPassword { get; set; }                   // 20 character max string
    public string       _storedCombination { get; set; }                // This will be a string in the format of 0000
    public string       _storedTimer { get; set; }                      // This will be a string in the format of 00h00m00s
    public string       _mistressAssignerName { get; set; }             // This will be the name of the player who assigned the padlock
    public Padlocks  _padlockType { get; set; } = Padlocks.None;  // This will be the type of padlock we are using

    /// <summary>
    /// Initializes a new instance of the PadlockIdentifier class.
    /// </summary>
    public PadlockIdentifier() {
        // set default values for our strings
        if(_inputPassword == null) { _inputPassword = "";}
        if(_inputCombination == null) { _inputCombination = "";}
        if(_inputTimer == null) { _inputTimer = "";}
        if(_storedPassword == null) { _storedPassword = "";}
        if(_storedCombination == null) { _storedCombination = "";}
        if(_storedTimer == null) { _storedTimer = "";}
        if(_mistressAssignerName == null) { _mistressAssignerName = "";}
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WhitelistCharData"/> class.
    /// <list type="bullet">
    /// <item><c>padlockType</c><param name="padlockType"> - the type of padlock to set our _padlockType to.</param></item>
    /// </list> </summary>
    public void SetType(Padlocks padlockType) {
        _padlockType = padlockType;
    }
    
    /// <summary>
    /// used by command opperations to both set and validate our password at the same time
    /// <list type="bullet">
    /// <item><c>_config</c><param name="_config"> - The GagSpeak configuration.</param></item>
    /// <item><c>locktype</c><param name="locktype"> - The type of padlock to set our _padlockType to.</param></item>
    /// <item><c>password</c><param name="password"> - The first password, if any.</param></item>
    /// <item><c>secondPassword</c><param name="secondPassword"> - The second password, if any.</param></item>
    /// <item><c>assignerPlayerName</c><param name="assignerPlayerName"> - The name of the player who assigned the padlock.</param></item>
    /// <item><c>targetPlayerName</c><param name="targetPlayerName"> - The name of the player who is being assigned the padlock.</param></item>
    /// </list> </summary>
    /// <returns>True if the password is valid, false if not.</returns>
    public bool SetAndValidate(CharacterHandler characterHandler, string locktype, string password = "", string secondPassword = "",
    string assignerPlayerName = "", string targetPlayerName = "") {
        // determine our padlock type
        if (!Enum.TryParse(locktype, true, out Padlocks padlockType)) {
            return false;}// or throw an exception
        GagSpeak.Log.Debug($"[PadlockIdentifer]: Setting padlock type to {padlockType}");
        // see if it is valid based on the type it is.
        switch (_padlockType) {
            case Padlocks.None:
                return false;
            case Padlocks.MetalPadlock:
                // handle MetalPadlock case
                break;
            case Padlocks.CombinationPadlock:
                this._storedCombination = password;
                break;
            case Padlocks.PasswordPadlock:
                this._storedPassword = password;
                break;
            case Padlocks.FiveMinutesPadlock:
                break;
            case Padlocks.TimerPasswordPadlock:
                this._storedPassword = password;
                this._storedTimer = secondPassword;
                break;
            case Padlocks.MistressPadlock:
                // handle MistressPadlock case
                break;
            case Padlocks.MistressTimerPadlock:
                this._storedTimer = password;
                break;
        }
        // finally, return if the password for it is actually valid, through the validation function normally used for UI input
        return ValidatePadlockPasswords(false, characterHandler, assignerPlayerName, targetPlayerName, targetPlayerName);
    }

    /// <summary>
    /// used by both command opperations and UI opperations to both determine if the active lock requires displaying a password field, and if so which one.
    /// <list type="bullet">
    /// <item><c>padlockType</c><param name="padlockType"> - the type of padlock.</param></item>
    /// </list> </summary>
    /// <returns>True if the password field should be displayed, false if not.</returns>
    public bool DisplayPasswordField(Padlocks padlockType) {
        // update our padlock type
        _padlockType = padlockType;
        // determine if we need to display a password field for it
        switch (padlockType) {
            case Padlocks.CombinationPadlock:
                _inputCombination = DisplayInputField("##Combination_Input", "Enter 4 digit combination...", _inputCombination, 4);
                return true;
            case Padlocks.PasswordPadlock:
                _inputPassword = DisplayInputField("##Password_Input", "Enter password", _inputPassword, 20);
                return true;
            case Padlocks.TimerPasswordPadlock:
                _inputPassword = DisplayInputField("##Password_Input", "Enter password", _inputPassword, 20, 2 / 3f);
                ImGui.SameLine();
                _inputTimer = DisplayInputField("##Timer_Input", "Ex: 0h2m7s", _inputTimer, 12);
                return true;
            case Padlocks.MistressTimerPadlock:
                _inputTimer = DisplayInputField("##Timer_Input", "Ex: 0h2m7s", _inputTimer, 12);
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// called by the displaypassword field, and used to vidsally display the input password field to the UI
    /// <list type="bullet">
    /// <item><c>id</c><param name="id"> - the id of the input field.</param></item>
    /// <item><c>hint</c><param name="hint"> - the hint of the input field.</param></item>
    /// <item><c>value</c><param name="value"> - the value of the input field.</param></item>
    /// <item><c>maxLength</c><param name="maxLength"> - the max length of the input field.</param></item>
    /// <item><c>widthRatio</c><param name="widthRatio"> - the width ratio of the input field.</param></item>
    /// </list> </summary>
    /// <returns>The input field.</returns>
    private string DisplayInputField(string id, string hint, string value, uint maxLength, float widthRatio = 1f) {
        // set the result to the value
        string result = value;
        // set the width of the input field
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * widthRatio);
        // display the input field
        if (ImGui.InputTextWithHint(id, hint, ref result, maxLength, ImGuiInputTextFlags.None))
            return result;
        return value;
    }

    /// <summary>
    /// used by both command opperations and UI opperations to both determine if passed in password satisfies the conditions of our locked padlock
    /// <list type="bullet">
    /// <item><c>isUnlocking</c><param name="isUnlocking"> - if we are unlocking the padlock or not.</param></item>
    /// <item><c>_config</c><param name="_config"> - The GagSpeak configuration.</param></item>
    /// <item><c>assignerPlayerName</c><param name="assignerPlayerName"> - The name of the player who assigned the padlock.</param></item>
    /// <item><c>targetPlayerName</c><param name="targetPlayerName"> - The name of the player who is being targetted for the check.</param></item>
    /// </list> </summary>
    /// <returns>True if the password is valid, false if not.</returns>
    public bool ValidatePadlockPasswords(bool isUnlocking, CharacterHandler characterHandler, string assignerPlayerName = "", string targetPlayerName = "", string YourPlayerName = "") {
        // setup a return bool variable called ret
        bool ret = false;
        GagSpeak.Log.Debug($"[PadlockIdentifer]: Validating password");
        // determine if we need the password for the padlock type is valid, if the padlock contains one.
        switch (_padlockType) {
            case Padlocks.None:
                return false;
            case Padlocks.MetalPadlock:
                return true;
            case Padlocks.CombinationPadlock:
                ret = ValidateCombination();
                if(ret && !isUnlocking && _inputCombination != "") {_storedCombination = _inputCombination; _inputCombination = "";}
                return ret;
            case Padlocks.PasswordPadlock:
                ret = ValidatePassword();
                if(ret && !isUnlocking && _inputPassword != "") {
                    GagSpeak.Log.Debug($"[PadlockIdentifer]: Password Validated and set to stored data");
                    _storedPassword = _inputPassword;
                    _inputPassword = "";
                }
                return ret;
            case Padlocks.FiveMinutesPadlock:
                _storedTimer = "0h5m0s";
                return true;
            case Padlocks.TimerPasswordPadlock:
                ret = (ValidatePassword() && ValidateTimer());
                if(ret && !isUnlocking && _inputPassword != "" && _inputTimer != "") {
                    _storedPassword = _inputPassword;
                    _storedTimer = _inputTimer;
                    _inputPassword = "";
                    _inputTimer = "";}
                return ret;
            case Padlocks.MistressPadlock:
                ret = ValidateMistress(characterHandler, assignerPlayerName, targetPlayerName, YourPlayerName);
                if(ret && !isUnlocking) {
                    _mistressAssignerName = assignerPlayerName;
                }
                return ret;
            case Padlocks.MistressTimerPadlock:
                ret = (ValidateMistress(characterHandler, assignerPlayerName, targetPlayerName, YourPlayerName) && ValidateTimer());
                if(ret && !isUnlocking) { 
                    _mistressAssignerName = assignerPlayerName;
                }

                if(ret && !isUnlocking && _inputTimer != "") {
                    _storedTimer = _inputTimer;
                    _inputTimer = "";}
                return ret;
            default:
                return true;
        }
    }

    /// <summary>
    /// used to see if the password type has a valid match or valid password parameters
    /// </summary>
    /// <returns> true if less then or equal to 20 characters and has no spaces, false if not.</returns>
    private bool ValidatePassword() {
        // see if it meets the password requirements
        if(_inputPassword == "") {
            GagSpeak.Log.Debug($"[PadlockIdentifer]: ValidatingPassword from set&Validate [{_storedPassword}]");
            return !string.IsNullOrWhiteSpace(_storedPassword) && _storedPassword.Length <= 20 && !_storedPassword.Contains(" ");
        } else {   
            GagSpeak.Log.Debug($"[PadlockIdentifer]: ValidatingPassword from DisplayPasswordField [{_inputPassword}]");
            return !string.IsNullOrWhiteSpace(_inputPassword) && _inputPassword.Length <= 20 && !_inputPassword.Contains(" ");
        }
    }

    /// <summary>
    /// used to see if the combination type has a valid match or valid combination parameters
    /// </summary>
    /// <returns> true if less then or equal to 4 characters and is a number, false if not.</returns>
    private bool ValidateCombination() {
        // see if it meets the combination requirements
        if(_inputCombination == "") {
            GagSpeak.Log.Debug($"[PadlockIdentifer]: ValidatingCombination from set&Validate [{_storedCombination}]");
            return int.TryParse(_storedCombination, out _) && _storedCombination.Length == 4;
        } else {
            GagSpeak.Log.Debug($"[PadlockIdentifer]: ValidatingCombination from DisplayPasswordField [{_inputCombination}]");
            return int.TryParse(_inputCombination, out _) && _inputCombination.Length == 4;
        }
    }

    /// <summary>
    /// used to see if the timer type has a valid match or valid timer parameters
    /// </summary>
    /// <returns> true if in the format of XdXhXmXs, false if not.</returns>
    private bool ValidateTimer() {
        // see if it meets the timer requirements
        if (_inputTimer == "") {
            GagSpeak.Log.Debug($"[PadlockIdentifer]: ValidatingTimer from set&Validate [{_storedTimer}]");
            var match = Regex.Match(_storedTimer, @"^(?:(\d+)d)?(?:(\d+)h)?(?:(\d+)m)?(?:(\d+)s)?$");
            return match.Success;
        } else {
            GagSpeak.Log.Debug($"[PadlockIdentifer]: ValidatingTimer from DisplayPasswordField [{_inputTimer}]");
            var match = Regex.Match(_inputTimer, @"^(?:(\d+)d)?(?:(\d+)h)?(?:(\d+)m)?(?:(\d+)s)?$");
            return match.Success;
        }
    }

    /// <summary>
    /// used to see if the mistress type has a valid match or valid mistress parameters
    /// <list type="bullet">
    /// <item><c>_config</c><param name="_config"> - The GagSpeak configuration.</param></item>
    /// <item><c>assignerPlayerName</c><param name="assignerPlayerName"> - The name of the player who assigned the padlock.</param></item>
    /// <item><c>targetPlayerName</c><param name="targetPlayerName"> - The name of the player who is being targetted for the check.</param></item>
    /// </list> </summary>
    private bool ValidateMistress(CharacterHandler characterHandler, string assignerPlayerName, string targetPlayerName, string YourPlayerName) {
        GagSpeak.Log.Debug($"[PadlockIdentifer]: Your Name: {YourPlayerName}");
        GagSpeak.Log.Debug($"[PadlockIdentifer]: AssignedPlayerName: {assignerPlayerName}");
        GagSpeak.Log.Debug($"[PadlockIdentifer]: TargetPlayerName {targetPlayerName}");
        // if we are the assigner, then we can just return true.
        if(assignerPlayerName == null) {
            GagSpeak.Log.Debug($"[PadlockIdentifer]: Assigner name is null!"); return false;}
        
        // first see if the assigner is us. If it is us, we must be the mistress
        if(assignerPlayerName == YourPlayerName) {
            GagSpeak.Log.Debug($"[PadlockIdentifer]: You are the assigner");
            // if we reach this point, it means we are the one assigning it and are in dom mode.
            // next make sure the target we are using this on views us as mistress
            if(characterHandler.whitelistChars.Any(w => targetPlayerName.Contains(w._name)
            && (w._yourStatusToThem == RoleLean.Mistress || w._yourStatusToThem == RoleLean.Master || w._yourStatusToThem == RoleLean.Owner) 
            && w.IsRoleLeanSubmissive(w._theirStatusToYou)))
            {
                // if we reached this point our dynamic is OK for a mistress assigning a lock to a pet or slave
                GagSpeak.Log.Debug($"[PadlockIdentifer]: You are the Mistress locking the padlock to your submissive, {targetPlayerName}");
                return true;
            }
            else {
                GagSpeak.Log.Debug($"[PadlockIdentifer]: But you cannot be the mistress because you cant be the mistress of yourself silly!");
                return false;
            }
        }

        // if the target player is us, and we are not in dominant mode, then we are the submissive receieving the mistress padlock from our mistress 
        if(targetPlayerName == YourPlayerName) { // need to fix this later
            GagSpeak.Log.Debug($"[PadlockIdentifer]: You are the target");
            // at this point we know what relation we are, so now we must verify the relations
            if(characterHandler.whitelistChars.Any(w => assignerPlayerName.Contains(w._name)
            && w.IsRoleLeanSubmissive(w._yourStatusToThem) 
            &&(w._theirStatusToYou == RoleLean.Mistress || w._theirStatusToYou == RoleLean.Master || w._theirStatusToYou == RoleLean.Owner)))
            {
                // if we reached this point we know our dynamic is sucessful and we can accept it.
                GagSpeak.Log.Debug($"[PadlockIdentifer]: You are the submissive recieving the lock from your mistress, {assignerPlayerName}");
                return true;
            }
            else {
                GagSpeak.Log.Debug($"[PadlockIdentifer]: But you cannot be the mistress because you cant be the mistress of yourself silly!");
                return false;
            }
        }

        // if we reach here, then we failed all conditions, so return false.
        GagSpeak.Log.Debug($"[PadlockIdentifer]: {assignerPlayerName} is not your mistress!");
        return false;
    }

    /// <summary>
    /// used to see if the guessed password matches the padlocks password
    /// <list type="bullet">
    /// <item><c>_config</c><param name="_config"> - The GagSpeak configuration.</param></item>
    /// <item><c>assignerName</c><param name="assignerName"> - The name of the player who assigned the padlock.</param></item>
    /// <item><c>targetName</c><param name="targetName"> - The name of the player who is being targetted for the check.</param></item>
    /// <item><c>password</c><param name="password"> - The password to check.</param></item>
    /// </list> </summary>
    /// <returns>True if the password is valid, false if not.</returns> 
    public bool CheckPassword(CharacterHandler characterHandler, string assignerName = "", string targetName = "", string password = "", string YourPlayerName = "") {
        // create a bool to return
        bool isValid = false;
        GagSpeak.Log.Debug($"[PadlockIdentifer]: Checking password {password}");
        GagSpeak.Log.Debug($"[PadlockIdentifer]: Stored Password: {_storedPassword}");
        // determine if we need the password for the padlock type is valid, if the padlock contains one.
        switch (_padlockType) {
            case Padlocks.None:
                return false;
            case Padlocks.MetalPadlock:
                return true;
            case Padlocks.CombinationPadlock:
                if(password != "" && password != null) {
                    // we've passed in a password
                    isValid = _storedCombination == password;
                } else {
                    // compare our input field
                    isValid = _storedCombination == _inputCombination;
                }
                break;
            case Padlocks.FiveMinutesPadlock:
                isValid = true;
                break;
            case Padlocks.PasswordPadlock:
                if(password != "" && password != null) {
                    // we've passed in a password
                    isValid = _storedPassword == password;
                } else {
                    // compare our input field
                    isValid = _storedPassword == _inputPassword;
                }
                break;
            case Padlocks.TimerPasswordPadlock:
                if(password != "" && password != null) {
                    // we've passed in a password
                    isValid = _storedPassword == password;
                } else {
                    // compare our input field
                    isValid = _storedPassword == _inputPassword;
                }
                break;
            case Padlocks.MistressPadlock:
                isValid = ValidateMistress(characterHandler, assignerName, targetName, YourPlayerName);
                break;
            case Padlocks.MistressTimerPadlock:
                isValid = ValidateMistress(characterHandler, assignerName, targetName, YourPlayerName);
                break;
            default:
                return false;
        }
        // if we are valid, then clear our input fields
        if (!isValid) {
            _inputPassword = "";
            _inputCombination = "";
            _inputTimer = "";
        }
        return isValid;
    }

    /// <summary>
    /// Used to clear our the padlockidentifier fields while unlocking a password
    /// </summary>
    public void ClearPasswords() {
        _inputPassword = "";
        _inputCombination = "";
        _inputTimer = "";
        _storedPassword = "";
        _storedCombination = "";
        _storedTimer = "";
        _mistressAssignerName = "";
    }
    
    /// <summary>
    /// Used to update the information of our padlock identifer to the configuration which we save and store our player data on
    /// <list type="bullet">
    /// <item><c>layerIndex</c><param name="layerIndex"> - The layer index of the padlock.</param></item>
    /// <item><c>isUnlocking</c><param name="isUnlocking"> - if we are unlocking the padlock or not.</param></item>
    /// <item><c>_config</c><param name="_config"> - The GagSpeak configuration.</param></item>
    /// </list> </summary>
    public void UpdatePadlockInfo(int layerIndex, bool isUnlocking, CharacterHandler characterHandler) {
        Padlocks padlockType = _padlockType;
        if (isUnlocking) { _padlockType = Padlocks.None; GagSpeak.Log.Debug("[Padlock] Unlocking Padlock");}
        // timers are handled by the timer service so we dont need to worry about it.
        switch (padlockType) {
            case Padlocks.MetalPadlock:
                characterHandler.SetPlayerGagPadlock(layerIndex, _padlockType);
                break;
            case Padlocks.CombinationPadlock:
                characterHandler.SetPlayerGagPadlock(layerIndex, _padlockType);
                characterHandler.SetPlayerGagPadlockPassword(layerIndex, _storedCombination);
                break;
            case Padlocks.PasswordPadlock:
                characterHandler.SetPlayerGagPadlock(layerIndex, _padlockType);
                characterHandler.SetPlayerGagPadlockPassword(layerIndex, _storedPassword);
                break;
            case Padlocks.FiveMinutesPadlock:
                characterHandler.SetPlayerGagPadlock(layerIndex, _padlockType);
                break;
            case Padlocks.TimerPasswordPadlock:
                characterHandler.SetPlayerGagPadlock(layerIndex, _padlockType);
                characterHandler.SetPlayerGagPadlockPassword(layerIndex, _storedPassword);
                break;
            case Padlocks.MistressPadlock:
                // handle MistressPadlock case
                characterHandler.SetPlayerGagPadlock(layerIndex, _padlockType);
                characterHandler.SetPlayerGagPadlockAssigner(layerIndex, _mistressAssignerName);
                break;
            case Padlocks.MistressTimerPadlock:
                characterHandler.SetPlayerGagPadlock(layerIndex, _padlockType);
                characterHandler.SetPlayerGagPadlockAssigner(layerIndex, _mistressAssignerName);
                break;
            default:
                // No password field should be displayed
                break;
        }
    }

    /// <summary>
    /// Used to update the information of our padlock identifer to the whitelist which we save and whitelisted player data on
    /// <list type="bullet">
    /// <item><c>character</c><param name="character"> - The character to update.</param></item>
    /// <item><c>layer</c><param name="layer"> - The layer index of the padlock.</param></item>
    /// <item><c>isUnlocking</c><param name="isUnlocking"> - if we are unlocking the padlock or not.</param></item>
    /// <item><c>_config</c><param name="_config"> - The GagSpeak configuration.</param></item>
    /// </list> </summary>
    public void UpdateWhitelistPadlockInfo(int playerIdx, int layer, bool isUnlocking, CharacterHandler characterHandler) {
        Padlocks padlockType = _padlockType;
        if (isUnlocking) { _padlockType = Padlocks.None; GagSpeak.Log.Debug("[Whitelist Padlock] Unlocking Padlock");}
        // timers are handled by the timer service so we dont need to worry about it.
        switch (padlockType) {
            case Padlocks.MetalPadlock:
                characterHandler.SetWhitelistSelectedGagPadlocks(playerIdx, layer, _padlockType);
                break;
            case Padlocks.CombinationPadlock:
                characterHandler.SetWhitelistSelectedGagPadlocks(playerIdx, layer, _padlockType);
                characterHandler.SetWhitelistSelectedGagPadlockPassword(playerIdx, layer, _storedCombination);
                break;
            case Padlocks.PasswordPadlock:
                characterHandler.SetWhitelistSelectedGagPadlocks(playerIdx, layer, _padlockType);
                characterHandler.SetWhitelistSelectedGagPadlockPassword(playerIdx, layer, _storedPassword);
                break;
            case Padlocks.FiveMinutesPadlock:
                characterHandler.SetWhitelistSelectedGagPadlocks(playerIdx, layer, _padlockType);
                break;
            case Padlocks.TimerPasswordPadlock:
                characterHandler.SetWhitelistSelectedGagPadlocks(playerIdx, layer, _padlockType);
                characterHandler.SetWhitelistSelectedGagPadlockPassword(playerIdx, layer, _storedPassword);
                break;
            case Padlocks.MistressPadlock:
                // handle MistressPadlock case
                characterHandler.SetWhitelistSelectedGagPadlocks(playerIdx, layer, _padlockType);
                characterHandler.SetWhitelistSelectedGagPadlockAssigner(playerIdx, layer, _mistressAssignerName);
                break;
            case Padlocks.MistressTimerPadlock:
                characterHandler.SetWhitelistSelectedGagPadlocks(playerIdx, layer, _padlockType);
                characterHandler.SetWhitelistSelectedGagPadlockAssigner(playerIdx, layer, _mistressAssignerName);
                break;
            default:
                // No password field should be displayed
                break;
        }
    }
} 
