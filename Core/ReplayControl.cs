// using AlternativeCameraMod.Config;
// using AlternativeCameraMod.Language;
// using AlternativeCameraMod.Replay;
// using UnityEngine;
//
//
// namespace AlternativeCameraMod;
//
// internal class ReplayControl
// {
//    private static readonly Logger Log = LogProvider.GetLogger<ReplayControl>();
//
//    private readonly State _state;
//    private readonly CameraControl _camera;
//    private readonly InputHandler _input;
//    private readonly Configuration _cfg;
//    private readonly LanguageConfig _lang;
//    
//    private float _timer;
//    private DownhillRecording _recording;
//    private DownhillReplay _replay;
//    private float _samplingThreshold;
//
//
//    public ReplayControl(State state, CameraControl camera, InputHandler input, Configuration cfg,
//       LanguageConfig lang)
//    {
//       _state = state;
//       _camera = camera;
//       _input = input;
//       _cfg = cfg;
//       _lang = lang;
//
//       _state.TriggerOccurred += OnLevelTrigger;
//    }
//
//
//    private void OnLevelTrigger(object? sender, TriggerEventArgs e)
//    {
//       switch (e.TriggerEvent)
//       {
//          case TriggerEvent.Start:
//             OnTrackStarted();
//             break;
//          case TriggerEvent.Finish:
//             OnTrackFinished();
//             break;
//          case TriggerEvent.Checkpoint:
//             OnTrackCheckpointReached(e.RelatedGameObject.name);
//             break;
//       }
//    }
//    
//
//    #region --- Recording ---
//
//    private void StartRecording()
//    {
//       _recording = new DownhillRecording(_state.ActiveMapName);
//       _recording.Record();
//       _timer = 0;
//       _samplingThreshold = 1f / _cfg.ReplayMode.RecordFrequency.Value;
//       _state.ReplayOperatingMode = ReplayOperatingMode.Recording;
//       Log.LogDebug("Recording started");
//       Log.LogDebug("Sampling Threshold: " + _samplingThreshold);
//    }
//
//
//    private void StopRecording()
//    {
//       _state.ReplayOperatingMode = ReplayOperatingMode.None;
//       _recording.Stop();
//       Log.LogDebug("Recording stopped, captured " + _recording.SnapshotCount + " snapshots");
//    }
//
//
//    private void RecordSnapshot()
//    {
//       System.Diagnostics.Debug.Assert(_recording != null);
//
//       _timer += Time.unscaledDeltaTime;
//       if (_timer >= _samplingThreshold)
//       {
//          _recording.Record(_state.TrackSectionId, _camera);
//          _timer = 0;
//       }
//       else
//       {
//          _recording.TrackTimestamp();
//       }
//    }
//
//
//    private void SaveToFile()
//    {
//       string replaySaveFolder = _cfg.ReplayMode.RecordingFolder.Value;
//       Directory.CreateDirectory(replaySaveFolder);
//       string replaySavePath = Path.Combine(replaySaveFolder, _recording.MapName + ".lmdr");
//       _recording.Save(replaySavePath);
//       Log.LogDebug($"Recording saved to '{replaySavePath}'; snapshots: {_recording.SnapshotCount}");
//    }
//
//    #endregion --- Recording ---
//
//    
//    #region --- Playback ---
//
//    public ReplayPlaybackMode PlaybackMode
//    {
//       get { return _state.PlaybackMode; }
//    }
//
//
//    private void StartPlayback(ReplayPlaybackMode playbackMode)
//    {
//       if (!IsLoaded)
//       {
//          LoadAndPlayback(playbackMode);
//          return;
//       }
//       
//       _replay = new DownhillReplay(_recording, _camera);
//       _replay.Play(playbackMode); // TODO section tracking not working yet _state.TrackSectionId); // play from checkpoint the player passed / is currently at
//
//       _state.ReplayOperatingMode = ReplayOperatingMode.Playback;
//       _state.PlaybackMode = playbackMode;
//
//       Log.LogDebug("Replay started. Playing " + _recording.SnapshotCount + " frames.");
//    }
//
//
//    private void StopPlayback()
//    {
//       _replay.Stop();
//       _state.ReplayOperatingMode = ReplayOperatingMode.None;
//       Log.LogDebug("Replay stopped");
//    }
//
//    
//    private void PlayNextSnapshot()
//    {
//       if (_replay.PrepareNext())
//       {
//          _replay.ApplyState();
//       }
//       else
//       {
//          StopPlayback();
//       }
//    }
//    
//
//    private DownhillRecording LoadFromFile(string fileName)
//    {
//       string replaySaveFolder = _cfg.ReplayMode.RecordingFolder.Value;
//       string filePath = Path.Combine(replaySaveFolder, fileName + ".lmdr");
//       var dr = DownhillRecordingLoader.Load(filePath);
//       return dr;
//    }
//
//
//    private bool IsLoaded
//    {
//       get { return _recording != null; }
//    }
//
//
//    #endregion --- Playback ---
//
//
//    #region --- Operation ---
//    
//
//    public void LoadAndPlayback(ReplayPlaybackMode playbackMode)
//    {
//       if (_state.ReplayOperatingMode == ReplayOperatingMode.Recording)
//       {
//          return;
//       }
//
//       if (_state.IsMapLoaded)
//       {
//          try
//          {
//             LoadRecording();
//          }
//          catch (Exception e)
//          {
//             Log.LogError(e.Message);
//             return;
//          }
//
//          if (_recording == null)
//          {
//             // TODO notify player?
//             return;
//          }
//
//          if (_state.ReplayOperatingMode == ReplayOperatingMode.None)
//          {
//             StartPlayback(playbackMode);
//          }
//          else
//          {
//             StopPlayback();
//             StartPlayback(playbackMode);
//          }
//       }
//    }
//
//
//    public void ToggleRecording()
//    {
//       if (_state.ReplayOperatingMode == ReplayOperatingMode.Playback)
//       {
//          return;
//       }
//
//       if (_state.ReplayOperatingMode == ReplayOperatingMode.None)
//       {
//          StartRecording();
//       }
//       // else if (_state.ReplayOperatingMode == ReplayOperatingMode.Recording)
//       // {
//       //    StopRecording();
//       // }
//    }
//
//    
//
//    public void LoadRecording()
//    {
//       if (_recording != null && _recording.IsRecording)
//       {
//          return;
//       }
//
//       _recording = LoadFromFile(_state.ActiveMapName);
//       if (_recording == null)
//       {
//          return;
//       }
//       Log.LogDebug($"Replay loaded: '{_recording.FilePath}'; Snapshots: {_recording.SnapshotCount}");
//    }
//
//
//    public void SaveRecording()
//    {
//       if (_recording == null)
//       {
//          return;
//       }
//
//       StopRecording();
//       SaveToFile();
//    }
//
//
//    public void TogglePlayback(ReplayPlaybackMode playbackMode)
//    {
//       if (_state.ReplayOperatingMode == ReplayOperatingMode.Recording)
//       {
//          return;
//       }
//       
//       if (_state.ReplayOperatingMode == ReplayOperatingMode.None)
//       {
//          StartPlayback(playbackMode);
//       }
//       // else if (_state.ReplayOperatingMode == ReplayOperatingMode.Playback)
//       // {
//       //    StopPlayback();
//       // }
//    }
//
//
//    public void Process()
//    {
//       if (_state.ReplayOperatingMode == ReplayOperatingMode.Recording)
//       {
//          RecordSnapshot();
//       }
//       if (_state.ReplayOperatingMode == ReplayOperatingMode.Playback)
//       {
//          PlayNextSnapshot();
//       }
//    }
//
//
//    private void OnTrackStarted()
//    {
//       if (_cfg.ReplayMode.AutoStart.Value && _state.ReplayOperatingMode != ReplayOperatingMode.Recording)
//       {
//          StartRecording();
//       }
//    }
//
//
//    private void OnTrackCheckpointReached(string checkpointName)
//    {
//    }
//
//
//    public void OnTrackFinished()
//    {
//       if (_state.ReplayOperatingMode == ReplayOperatingMode.Recording)
//       {
//          StopRecording();
//       }
//
//       if (_cfg.ReplayMode.AutoSave.Value)
//       {
//          SaveRecording();
//       }
//    }
//
//    #endregion --- Operation ---
//
//
//    public void StopCurrentOperation()
//    {
//       if (_state.ReplayOperatingMode == ReplayOperatingMode.Playback)
//       {
//          StopPlayback();
//       }
//       else if (_state.ReplayOperatingMode == ReplayOperatingMode.Recording)
//       {
//          StopRecording();
//       }
//    }
//
// }
