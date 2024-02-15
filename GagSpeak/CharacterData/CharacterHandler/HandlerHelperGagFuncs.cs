using System;
using System.Collections.Generic;
using GagSpeak.Gagsandlocks;
using GagSpeak.Utility;
using GagSpeak.Events;

namespace GagSpeak.CharacterData;

public partial class CharacterHandler
{
    public void SetPlayerGagType(int index, string gagName, bool invokeGlamourEvent = true) {
        // see if we reset it to none, and if so, we should remove our glamoured gag item
        if(gagName == "None") {
            ResetPlayerGagTypeToNone(index, invokeGlamourEvent);
        }
        // otherwise, just change the gag type
        else {
            if(playerChar._selectedGagTypes[index] != gagName) {
                playerChar._selectedGagTypes[index] = gagName;
                _saveService.QueueSave(this);
            }
        }
    }

    private void ResetPlayerGagTypeToNone(int index, bool invokeGlamourEvent) {
        var gagTypeName = playerChar._selectedGagTypes[index];
        if(playerChar._selectedGagTypes[index] != "None") {
            playerChar._selectedGagTypes[index] = "None";
            _saveService.QueueSave(this);
            // if we want to also invoke the event, then invoke it
            if(invokeGlamourEvent) {
                _gagSpeakGlamourEvent.Invoke(UpdateType.GagUnEquipped, gagTypeName);
            }
        }
    }

    public void SetPlayerGagPadlock(int index, string padlockName) {
        Padlocks lockToAssign = Enum.TryParse(padlockName, out Padlocks padlockType) ? padlockType : Padlocks.None;
        if(playerChar._selectedGagPadlocks[index] != lockToAssign) {
            playerChar._selectedGagPadlocks[index] = lockToAssign;
            _saveService.QueueSave(this);
        }
    }

    public void SetPlayerGagPadlock(int index, Padlocks padlockName) {
        if(playerChar._selectedGagPadlocks[index] != padlockName) {
            playerChar._selectedGagPadlocks[index] = padlockName;
            _saveService.QueueSave(this);
        }
    }

    public void SetPlayerGagPadlockPassword(int index, string password) {
        if(playerChar._selectedGagPadlockPassword[index] != password) {
            playerChar._selectedGagPadlockPassword[index] = password;
            _saveService.QueueSave(this);
        }
    }

    public void SetPlayerGagPadlockTimer(int index, string endTimeOfTimerLock) {
        DateTimeOffset value = UIHelpers.GetEndTime(endTimeOfTimerLock);
        if(playerChar._selectedGagPadlockTimer[index] != value) {
            playerChar._selectedGagPadlockTimer[index] = value;
            _saveService.QueueSave(this);
        }
    }

    public void ResetPlayerGagTimers() {
        playerChar._selectedGagPadlockTimer = new List<DateTimeOffset> {
            DateTimeOffset.Now,
            DateTimeOffset.Now,
            DateTimeOffset.Now
        };
        _saveService.QueueSave(this);
    }

    public void SetPlayerGagPadlockAssigner(int index, string AssignerName) {
        if(playerChar._selectedGagPadlockAssigner[index] != AssignerName) {
            playerChar._selectedGagPadlockAssigner[index] = AssignerName;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistSelectedGagTypes(int index, int gagIndex, string gagName) {
        if(whitelistChars[index]._selectedGagTypes[gagIndex] != gagName) {
            whitelistChars[index]._selectedGagTypes[gagIndex] = gagName;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistSelectedGagPadlocks(int index, int padlockIndex, string padlockName) {
        Padlocks lockToAssign = Enum.TryParse(padlockName, out Padlocks padlockType) ? padlockType : Padlocks.None;
        if(whitelistChars[index]._selectedGagPadlocks[padlockIndex] != lockToAssign) {
            whitelistChars[index]._selectedGagPadlocks[padlockIndex] = lockToAssign;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistSelectedGagPadlocks(int index, int padlockIndex, Padlocks padlockName) {
        if(whitelistChars[index]._selectedGagPadlocks[padlockIndex] != padlockName) {
            whitelistChars[index]._selectedGagPadlocks[padlockIndex] = padlockName;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistSelectedGagPadlockPassword(int index, int passwordIndex, string password) {
        if(whitelistChars[index]._selectedGagPadlockPassword[passwordIndex] != password) {
            whitelistChars[index]._selectedGagPadlockPassword[passwordIndex] = password;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistSelectedGagPadlockTimer(int index, int timerIndex, string endTimeOfTimerLock) {
        DateTimeOffset value = UIHelpers.GetEndTime(endTimeOfTimerLock);
        if(whitelistChars[index]._selectedGagPadlockTimer[timerIndex] != value) {
            whitelistChars[index]._selectedGagPadlockTimer[timerIndex] = value;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistSelectedGagPadlockTimer(int index, int timerIndex, DateTimeOffset endTimeOfTimerLock) {
        if(whitelistChars[index]._selectedGagPadlockTimer[timerIndex] != endTimeOfTimerLock) {
            whitelistChars[index]._selectedGagPadlockTimer[timerIndex] = endTimeOfTimerLock;
            _saveService.QueueSave(this);
        }
    }

    public void SetWhitelistSelectedGagPadlockAssigner(int index, int assignerIndex, string AssignerName) {
        if(whitelistChars[index]._selectedGagPadlockAssigner[assignerIndex] != AssignerName) {
            whitelistChars[index]._selectedGagPadlockAssigner[assignerIndex] = AssignerName;
            _saveService.QueueSave(this);
        }
    }

    // for not spamming the save service during info request transfer, do it all in bulk
    public void UpdateWhitelistGagInfoPart1(string whitelistName, string[] GagTypes, string[] Padlocks, string[] Passwords, string[] Timers, string[] Assigners) {
        int Idx = -1;
        if(IsPlayerInWhitelist(whitelistName)){
            Idx = GetWhitelistIndex(whitelistName);
        }
        if(Idx != -1) {
            whitelistChars[Idx]._selectedGagTypes[0] = GagTypes[0];
            whitelistChars[Idx]._selectedGagPadlocks[0] = Enum.TryParse(Padlocks[0], out Padlocks padlockType) ? padlockType : Gagsandlocks.Padlocks.None;
            whitelistChars[Idx]._selectedGagPadlockPassword[0] = Passwords[0];
            whitelistChars[Idx]._selectedGagPadlockTimer[0] = string.IsNullOrEmpty(Timers[0]) ? DateTimeOffset.Now : UIHelpers.GetEndTime(Timers[0]);
            whitelistChars[Idx]._selectedGagPadlockAssigner[0] = Assigners[0];
        }
        _saveService.QueueSave(this);
    }

    public void UpdateWhitelistGagInfoPart2(string whitelistName, string[] GagTypes, string[] Padlocks, string[] Passwords, string[] Timers, string[] Assigners) {
        int Idx = -1;
        if(IsPlayerInWhitelist(whitelistName)){
            Idx = GetWhitelistIndex(whitelistName);
        }
        if(Idx != -1) {
            whitelistChars[Idx]._selectedGagTypes[1] = GagTypes[1];
            whitelistChars[Idx]._selectedGagTypes[2] = GagTypes[2];
            whitelistChars[Idx]._selectedGagPadlocks[1] = Enum.TryParse(Padlocks[1], out Padlocks padlockType2) ? padlockType2 : Gagsandlocks.Padlocks.None;
            whitelistChars[Idx]._selectedGagPadlocks[2] = Enum.TryParse(Padlocks[2], out Padlocks padlockType3) ? padlockType3 : Gagsandlocks.Padlocks.None;
            whitelistChars[Idx]._selectedGagPadlockPassword[1] = Passwords[1];
            whitelistChars[Idx]._selectedGagPadlockPassword[2] = Passwords[2];
            whitelistChars[Idx]._selectedGagPadlockTimer[1] = string.IsNullOrEmpty(Timers[1]) ? DateTimeOffset.Now : UIHelpers.GetEndTime(Timers[1]);
            whitelistChars[Idx]._selectedGagPadlockTimer[2] = string.IsNullOrEmpty(Timers[2]) ? DateTimeOffset.Now : UIHelpers.GetEndTime(Timers[2]);
            whitelistChars[Idx]._selectedGagPadlockAssigner[1] = Assigners[1];
            whitelistChars[Idx]._selectedGagPadlockAssigner[2] = Assigners[2]; 
        }
        _saveService.QueueSave(this);
    }
}