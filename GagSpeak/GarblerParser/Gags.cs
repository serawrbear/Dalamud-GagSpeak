using System;
using System.Collections.Generic;
// using System.Linq;
// using GagSpeak.Events;
// using System.Text.RegularExpressions;
// using FFXIVClientStructs.FFXIV.Component.GUI;
// using GagSpeak.Data;
// using GagSpeak.Services;
// using GagSpeak.Garbler.PhonemeData;

namespace GagSpeak.Data;
public class Gag
{
    private readonly    GagSpeakConfig                          _config;                            // The GagSpeak configuration
    public              string                                  _gagName { get; set; } = "";        // Name of the gag
    public              Dictionary<string, int>                 _muffleStrOnPhoneme { get; set; }   // dict of phonemes and their restriction strengths
    public              Dictionary<string, string>              _ipaSymbolSound { get; set; }       // Stores muffled sound for IPA symbol.

    public Gag(GagSpeakConfig config) {
        _config = config;
        // set up the restrictions
        _muffleStrOnPhoneme = new Dictionary<string, int>();
        // set up the ipa symbol sounds
        _ipaSymbolSound = new Dictionary<string, string>();

    }

    // Adding this stupid info here because it's being a butt and i dont want to put up with this things sorry ass right now.
    public void AddInfo(string name, Dictionary<string, int> muffleStrOnPhoneme, Dictionary<string,string> ipaSymbolSound)
    {
        _gagName = name;
        _muffleStrOnPhoneme = muffleStrOnPhoneme ?? throw new ArgumentNullException(nameof(muffleStrOnPhoneme));
        _ipaSymbolSound = ipaSymbolSound ?? throw new ArgumentNullException(nameof(ipaSymbolSound));
    }
}
