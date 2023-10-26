﻿using System;
using System.ComponentModel;
using Dalamud.Configuration;
using Dalamud.Game.Text; // Interacting with game chat, XIVChatType, ext.
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game.UI; // For enabling lists
// using Dalamud.Plugin;

// Sets up the configuration controls for the GagSpeak Plugin
namespace GagSpeak
{
    // Plugin Configuration Class used for dalamud
    public class GagSpeakConfig {
        // Gets or sets a boolean value to indicate of plugin is a fresh install
        public bool FreshInstall { get; set; } = true;

        // Gets or sets a boolean value to indicate of plugin is enabled
        public bool Enabled { get; set; } = true;

        // Gets or sets the channels that the plugin will translate
        public List<XivChatType> Channels { get; set; } = new() {XivChatType.Say};

        // stores the bool to see if the friendsonly button is checked
        public bool friendsOnly { get; set; } = false;

        // stores the bool to see if the partyonly button is checked
        public bool partyOnly { get; set; } = false;

        // Gets or sets the "garble level" of the plugin (determined how muffled translation is)
        public int GarbleLevel { get; set; } = 0;

        // Gets or sets the current process intervals for the history
        public int ProcessTranslationInterval { get; set; } = 300000;

        // Gets or sets max number of translations stored in history
        public int TranslationHistoryMax { get; set; } = 30;


        // create an enumeration for all the gag types
        // A majority of these likely wont be implemented, but its nice to have.
        public enum GagPadlocks {
            MetalPadlock,
            CombinationPadlock,
            PasswordPadlock,
            FiveMinutesPadlock,
            TimerPasswordPadlock,
            MistressPadlock,
            MistressTimerPadlock,
        }
        
        // create an dictionary for all the gag types and their strengths
        public Dictionary<string, int> GagTypes {get; set; } = new() {
            { "Ball Gag", 5 },
            { "Ball Gag Mask", 5 },
            { "Bamboo Gag", 4 },
            { "Bit Gag", 2 },
            { "Bone Gag", 2 },
            { "Chloroform Cloth", 1 },
            { "Chopstick Gag", 4 },
            { "Cloth Gag", 1 },
            { "Cloth Stuffing", 2 },
            { "Crop", 2 },
            { "Cup Holder Gag", 3 },
            { "Custom Latex Hood", 4 },
            { "Deepthroat Penis Gag", 6 },
            { "Dental Gag", 2 },
            { "Dildo Gag", 5 },
            { "Dog Hood", 4 },
            { "Duct Tape", 4 },
            { "Duster Gag", 3 },
            { "Exposed Dog Muzzle", 4 },
            { "Funnel Gag", 5 },
            { "Fur Scarf", 2 },
            { "Futuristic Ball Gag", 6 },
            { "Futuristic Harness Panel Gag", 7 },
            { "Futuristic Panel Gag", 5 },
            { "Gas Mask", 3 },
            { "Harness Ball Gag", 5 },
            { "Harness Ball Gag XL", 6 },
            { "Harness OTN Plug Gag", 8 },
            { "Harness Pacifier", 2 },
            { "Harness Panel Gag", 3 },
            { "Hook Gag Mask", 3 },
            { "Inflatable Hood", 5 },
            { "Large Dildo", 4 },
            { "Latex Ball Muzzle Gag", 5 },
            { "Latex Posture Collar Gag", 4 },
            { "Latex Respirator", 1 },
            { "Leather Corset Collar Gag", 4 },
            { "Leather Hood", 4 },
            { "Muzzle Gag", 4 },
            { "Panty Stuffing", 2 },
            { "Plastic Wrap", 2 },
            { "Plug Gag", 5 },
            { "Polished Steel Hood", 6 },
            { "Pony Hood", 4 },
            { "Prison Lockdown Gag", 4 },
            { "Pump Gag lv1", 2 },
            { "Pump Gag lv2", 3 },
            { "Pump Gag lv3", 5 },
            { "Pump Gag lv4", 7 },
            { "Ribbons", 2 },
            { "Ring Gag", 3 },
            { "Rope Gag", 2 },
            { "Rubber Carrot Gag", 5 },
            { "Scarf", 1 },
            { "Sensory Deprivation Hood", 6 },
            { "Silicon Bit Gag", 4 },
            { "Slime", 6 },
            { "Smooth Latex Mask", 5 },
            { "Sock Stuffing", 2 },
            { "Spider Gag", 3 },
            { "Steel Muzzle Gag", 4 },
            { "Stitched Muzzle Gag", 3 },
            { "Stitches", 6 },
            { "Tentacle", 5 },
            { "Web Gag", 2 },
            { "XL Bone Gag", 4 },
        };
    }

    // class for pluginconfig by dalamud
    [Serializable]
    public class PluginConfig : GagSpeakConfig, IPluginConfiguration {
        // inherited from the dalamud documentation
        public int Version { get; set; } = 0;
    }





}