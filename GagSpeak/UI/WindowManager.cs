using System;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Game.Text.SeStringHandling;
using OtterGui.Classes;
using GagSpeak.UI.Tabs.HelpPageTab;

namespace GagSpeak.UI;
/// <summary> This class is used to handle the window manager. </summary>
public class GagSpeakWindowManager : IDisposable
{
    private readonly WindowSystem               _windowSystem = new("GagSpeak");
    private readonly UiBuilder                  _uiBuilder;
    private readonly MainWindow                 _ui;
    private readonly IChatGui                   _chatGui;

    /// <summary> Initializes a new instance of the <see cref="GagSpeakWindowManager"/> class.</summary>
    public GagSpeakWindowManager(UiBuilder uiBuilder, MainWindow ui, GagSpeakConfig config, IChatGui chatGui,
    DebugWindow uiDebug, GagSpeakChangelog changelog, UserProfileWindow userProfile, TutorialWindow tutorialWindow,
    SavePatternWindow savePatternWindow) {
        // set the main ui window
        _uiBuilder       = uiBuilder;
        _ui              = ui;
        _chatGui         = chatGui;
        _windowSystem.AddWindow(ui);
        _windowSystem.AddWindow(uiDebug);
        _windowSystem.AddWindow(userProfile);
        _windowSystem.AddWindow(tutorialWindow);
        _windowSystem.AddWindow(savePatternWindow);
        _windowSystem.AddWindow(changelog.Changelog);

        _uiBuilder.Draw                  += _windowSystem.Draw;     // for drawing the UI stuff
        _uiBuilder.OpenConfigUi          += _ui.Toggle;             // for toggling the UI stuff
        
        _ui.Toggle();

        //handle a fresh install
        if (config.FreshInstall){
            // They are new, so print some nice messages
            _chatGui.Print(new SeStringBuilder().AddText("Thank you for installing ").AddBlue("GagSpeak!").BuiltString);
            _chatGui.Print(new SeStringBuilder().AddYellow("Instructions: ").AddText("You can use ").AddBlue("/gagspeak help ")
                .AddText("to see main functions, ").AddBlue("/gag ").AddText("to view gagging commands, and ").AddBlue("/gsm ")
                .AddText("to chat in gagspeak.").BuiltString);
            config.FreshInstall = false;
            config.Save();
            _ui.Toggle();
        }
    }

    /// <summary> This function is used to dispose of the window manager. </summary>
    public void Dispose() {
        _uiBuilder.Draw         -= _windowSystem.Draw;
        _uiBuilder.OpenConfigUi -= _ui.Toggle;
    }
}
