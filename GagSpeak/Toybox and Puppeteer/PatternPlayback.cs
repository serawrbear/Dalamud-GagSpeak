using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Common.Math;
using GagSpeak.Services;
using GagSpeak.ToyboxandPuppeteer;
using ImGuiNET;
using ImPlotNET;

namespace GagSpeak.UI.Tabs.ToyboxTab;
public class PatternPlayback : IDisposable
{
    private PatternData _tempStoredPattern;
    private TimerRecorder _timerRecorder;
    public Stopwatch _recordingStopwatch;
    private readonly PlugService    _plugService;
    private List<byte> storedRecordedPositions = new List<byte>(); // the stored pattern data to playback
    private double[] currentPos = new double[2];  // The plotted points position on the wavelength graph
    private bool _isPlaybackActive;  // Whether the playback is active
    private int _patternIdx;  // The index of the pattern being played back


    public PatternPlayback(PlugService plugService) {
        _plugService = plugService;
        _isPlaybackActive = false;
        _playbackIndex = 0;
        _patternIdx = -1;
        _tempStoredPattern = new PatternData(); // empty pattern
        // Create a new stopwatch
        _recordingStopwatch = new Stopwatch();
        // create a timer for realtime feedback display. This data is disposed of automatically after 300 entries (15s of data)
        _timerRecorder = new TimerRecorder(20, ReadVibePosFromBuffer);
    }

    public void Dispose() {
        _timerRecorder.Dispose();
        _recordingStopwatch.Stop();
        _recordingStopwatch.Reset();
    }

public void Draw() {
    using var style = ImRaii.PushStyle(ImGuiStyleVar.WindowPadding, new Vector2(0, 0)).Push(ImGuiStyleVar.CellPadding, new Vector2(0, 0));
    using var child = ImRaii.Child("##PatternPlaybackChild", new Vector2(ImGui.GetContentRegionAvail().X, -1), true, ImGuiWindowFlags.NoScrollbar);
    if (!child) { return;}
    try{
        // Draw the waveform
        float[] xs;  // x-values
        float[] ys;  // y-values
        // if we are playing back
        if (_isPlaybackActive) {
            int start = Math.Max(0, _playbackIndex - 150);
            int count = Math.Min(150, _playbackIndex - start + 1);
            int buffer = 150 - count; // The number of extra values to display at the end


            xs = Enumerable.Range(-buffer, count + buffer).Select(i => (float)i).ToArray();
            ys = storedRecordedPositions.Skip(storedRecordedPositions.Count - buffer).Take(buffer)
                .Concat(storedRecordedPositions.Skip(start).Take(count))
                .Select(pos => (float)pos).ToArray();

            // Transform the x-values so that the latest position appears at x=0
            for (int i = 0; i < xs.Length; i++) {
                xs[i] -= _playbackIndex;
            }
        } else {
            xs = new float[0];
            ys = new float[0];
        }
        float latestX = xs.Length > 0 ? xs[xs.Length - 1] : 0; // The latest x-value
        // Transform the x-values so that the latest position appears at x=0
        for (int i = 0; i < xs.Length; i++) {
            xs[i] -= latestX;
        }

        // get the xpos so we can draw it back a bit to span the whole width
        var xPos = ImGui.GetCursorPosX();
        var yPos = ImGui.GetCursorPosY();
        ImGui.SetCursorPos(new Vector2(xPos - ImGuiHelpers.GlobalScale * 10, yPos - ImGuiHelpers.GlobalScale * 10));
        var width = ImGui.GetContentRegionAvail().X + ImGuiHelpers.GlobalScale * 10;
        // set up the color map for our plots.
        ImPlot.PushStyleColor(ImPlotCol.Line, lushPinkLine);
        ImPlot.PushStyleColor(ImPlotCol.PlotBg, lovenseScrollingBG);
        // draw the waveform
        ImPlot.SetNextAxesLimits(- 150, 0, -5, 110, ImPlotCond.Always);
        if(ImPlot.BeginPlot("##Waveform", new System.Numerics.Vector2(width, 100), ImPlotFlags.NoBoxSelect | ImPlotFlags.NoMenus
        | ImPlotFlags.NoLegend | ImPlotFlags.NoFrame)) {
            ImPlot.SetupAxes("X Label", "Y Label", 
                ImPlotAxisFlags.NoGridLines | ImPlotAxisFlags.NoLabel | ImPlotAxisFlags.NoTickLabels | ImPlotAxisFlags.NoTickMarks | ImPlotAxisFlags.NoHighlight,
                ImPlotAxisFlags.NoGridLines | ImPlotAxisFlags.NoLabel | ImPlotAxisFlags.NoTickLabels | ImPlotAxisFlags.NoTickMarks);
            if (xs.Length > 0 || ys.Length > 0) {
                ImPlot.PlotLine("Recorded Positions", ref xs[0], ref ys[0], xs.Length);
            }
            ImPlot.EndPlot();
        }
        ImPlot.PopStyleColor(2);
    } catch (Exception e) {
        GagSpeak.Log.Error($"{e} Error drawing the toybox workshop subtab");
    }
}
#region Helper Fuctions
    // When active, the circle will not fall back to the 0 coordinate on the Y axis of the plot, and remain where it is
    public void StartPlayback(PatternData pattern, int IdxToPlay) {
        GagSpeak.Log.Debug($"Starting playback of pattern {pattern._name}");
        // set the playback index to the start
        _playbackIndex = 0;
        // set the stored pattern index we are using to playback here
        // (shoudld point to same place in memopry according to c# logic)
        _tempStoredPattern = pattern;
        // set the index in PatternHandler that it is stored to
        _patternIdx = IdxToPlay;
        // set the data to active and store the pattern data
        storedRecordedPositions = _tempStoredPattern._patternData;
        _tempStoredPattern._isActive = true;
        _isPlaybackActive = true;
        _recordingStopwatch.Start();
        _timerRecorder.Start();
    }

    public void StopPlayback() {
        GagSpeak.Log.Debug($"Stopping playback of pattern {_tempStoredPattern._name}");
        // clear the local variables
        _isPlaybackActive = false;
        _tempStoredPattern._isActive = false;
        _patternIdx = -1;
        _playbackIndex = 0;
        // clear the temp stored reference data, replacing it with a blank one
        _tempStoredPattern = new PatternData();
        // reset the timers
        _timerRecorder.Stop();
        _recordingStopwatch.Stop();
        _recordingStopwatch.Reset();
        _ = _plugService.ToyboxVibrateAsync(0, 10);
    }

    private int _playbackIndex;  // The current index of the playback
    private void ReadVibePosFromBuffer(object? sender, ElapsedEventArgs e) {
        // If we're playing back the stored positions
        if (_isPlaybackActive) {
            // If we've reached the end of the stored positions, stop playback
            if (_playbackIndex >= storedRecordedPositions.Count) {
                // first see if our current pattern is set to loop, if it is, then restart the playback
                if (_tempStoredPattern._loop) {
                    _playbackIndex = 0;
                    _recordingStopwatch.Restart();
                    return;
                } else {
                    StopPlayback();
                    return;
                }
            }
            //GagSpeak.Log.Debug($"Playing back position {_playbackIndex} with data {storedRecordedPositions[_playbackIndex]}");
            // Convert the current stored position to a float and store it in currentPos
            currentPos[1] = storedRecordedPositions[_playbackIndex];
            _playbackIndex++;

            // Send the vibration command to the device
            if(_plugService.HasConnectedDevice() && _plugService.IsClientConnected() && _plugService.anyDeviceConnected) {
                _ = _plugService.ToyboxVibrateAsync(storedRecordedPositions[_playbackIndex], 10);
            }
        }
    }
#endregion Helper Fuctions

    public Vector4 lushPinkLine = new Vector4(.806f, .102f, .407f, 1);
    public Vector4 lushPinkButton = new Vector4(1, .051f, .462f, 1);
    public Vector4 lovenseScrollingBG = new Vector4(0.042f, 0.042f, 0.042f, 0.930f);
    public Vector4 lovenseDragButtonBG = new Vector4(0.110f, 0.110f, 0.110f, 0.930f);
    public Vector4 lovenseDragButtonBGAlt = new Vector4(0.1f, 0.1f, 0.1f, 0.930f);
    public Vector4 ButtonDrag = new Vector4(0.097f, 0.097f, 0.097f, 0.930f);
    public Vector4 SideButton = new Vector4(0.451f, 0.451f, 0.451f, 1);
    public Vector4 SideButtonBG = new Vector4(0.451f, 0.451f, 0.451f, .25f);

}