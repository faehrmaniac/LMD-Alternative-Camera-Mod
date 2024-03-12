using Il2CppMegagon.Downhill.Players;
using Il2CppMegagon.Downhill.Vehicle.Controller;
using AlternativeCameraMod.Config;
using AlternativeCameraMod.Language;
using AlternativeCameraMod.Replay;
using UnityEngine;


namespace AlternativeCameraMod;

internal class ReplayControl
{
   private readonly State _state;
   private readonly CameraControl _camera;
   private readonly InputHandler _input;
   private readonly Configuration _cfg;
   private readonly LanguageConfig _lang;
   private readonly Logger _logger;

   private Transform? _bikeTransform;
   private GameObject? _bike;
   private GameObject? _replayBike;
   
   private float _timer;
   private DownhillRecording _recording;
   private DownhillReplay _replay;


   public ReplayControl(State state, CameraControl camera, InputHandler input, Configuration cfg,
      LanguageConfig lang, Logger logger)
   {
      _state = state;
      _camera = camera;
      _input = input;
      _cfg = cfg;
      _lang = lang;
      _logger = logger;

      _state.TriggerOccurred += OnLevelTrigger;
   }


   private void OnLevelTrigger(object? sender, TriggerEventArgs e)
   {
      switch (e.TriggerEvent)
      {
         case TriggerEvent.Start:
            OnTrackStarted();
            break;
         case TriggerEvent.Finish:
            OnTrackFinished();
            break;
         case TriggerEvent.Checkpoint:
            OnTrackCheckpointReached(e.RelatedGameObject.name);
            break;
      }
   }
   

   private void GatherGameObjects()
   {
      if (_bike == null)
      {
         _bike = GameObject.Find("Bike(Clone)");
      }

      if (_bikeTransform == null)
      {
         _bikeTransform = _bike.GetComponent<Transform>();
      }
   }

   
   #region --- Recording ---

   private void StartRecording()
   {
      GatherGameObjects();
      _recording = new DownhillRecording(_state.ActiveMapName);
      _recording.Record();
      _timer = 0;
      _state.ReplayOperatingMode = ReplayOperatingMode.Recording;
      _logger.LogDebug("Replay recording started.");
   }


   private void StopRecording()
   {
      _state.ReplayOperatingMode = ReplayOperatingMode.None;
      _recording.Stop();
      _logger.LogDebug("Replay recording stopped. Captured " + _recording.SnapshotCount + " snapshots");
   }


   private void RecordSnapshot()
   {
      System.Diagnostics.Debug.Assert(_recording != null);

      _timer += Time.unscaledDeltaTime;
      if (_timer >= 1 / _cfg.ReplayMode.RecordFrequency.Value)
      {
         _recording.Record(_state.TrackSectionId, _bikeTransform, _camera);
         _timer = 0;
      }
   }


   private void SaveToFile()
   {
      string replaySaveFolder = _cfg.ReplayMode.RecordingFolder.Value;
      Directory.CreateDirectory(replaySaveFolder);
      string replaySavePath = Path.Combine(replaySaveFolder, _recording.MapName + ".lmdr");
      _recording.Save(replaySavePath);
      _logger.LogDebug($"Recording saved to '{replaySavePath}'; snapshots: {_recording.SnapshotCount}");
   }

   #endregion --- Recording ---

   
   #region --- Playback ---

   public ReplayPlaybackMode PlaybackMode
   {
      get { return _state.PlaybackMode; }
   }


   private void StartPlayback(ReplayPlaybackMode playbackMode)
   {
      GatherGameObjects();
      
      if (!IsLoaded)
      {
         LoadAndPlayReplay(playbackMode);
         return;
      }
      
      CreateReplayBike();
      if (playbackMode == ReplayPlaybackMode.Real)
      {
         // hide player bike, replay does not work with it
         _bike.active = false;
      }
      
      _replay = new DownhillReplay(_recording, playbackMode, _replayBike.transform, _camera);
      _replay.Play(_state.TrackSectionId); // play from checkpoint the player passed / is currently at
      _state.ReplayOperatingMode = ReplayOperatingMode.Playback;
      _state.PlaybackMode = playbackMode;
      _logger.LogDebug("Replay started. Playing " + _recording.SnapshotCount + " frames.");
   }


   private void StopPlayback()
   {
      _replay.Stop();
      _state.ReplayOperatingMode = ReplayOperatingMode.None;
      DestroyReplayBike();
      _bike.active = true; // enable player bike
      _logger.LogDebug("Replay stopped");
   }


   private void CreateReplayBike()
   {
      _replayBike = GameObject.Instantiate(_bikeTransform.gameObject);
      GameObject.Destroy(_replayBike.GetComponent<BikeLocomotion>());
      // GameObject.Destroy(_replayBike.GetComponent<PlayerCameraTarget>());
      // GameObject.Destroy(_replayBike.GetComponent<Stamina>());
   }


   private void DestroyReplayBike()
   {
      GameObject.Destroy(_replayBike);
   }


   private void PlayNextSnapshot()
   {
      if (_replay.PrepareNext())
      {
         _replay.ApplyState();
      }
      else
      {
         StopPlayback();
      }
   }
  


   private DownhillRecording LoadFromFile(string fileName)
   {
      string replaySaveFolder = _cfg.ReplayMode.RecordingFolder.Value;
      string filePath = Path.Combine(replaySaveFolder, fileName + ".lmdr");
      var dr = DownhillRecordingLoader.Load(filePath);
      return dr;
   }


   private bool IsLoaded
   {
      get { return _recording != null; }
   }


   #endregion --- Playback ---


   #region --- Operation ---
   

   public void LoadAndPlayReplay(ReplayPlaybackMode playbackMode)
   {
      if (_state.ReplayOperatingMode == ReplayOperatingMode.Recording)
      {
         return;
      }

      GatherGameObjects();
      if (_state.IsMapLoaded)
      {
         try
         {
            _recording = LoadFromFile(_state.ActiveMapName);
            _logger.LogDebug($"Replay loaded: '{_recording.FilePath}'; Snapshots: {_recording.SnapshotCount}");
         }
         catch (Exception e)
         {
            _logger.LogError(e.Message);
            return;
         }

         if (_recording == null)
         {
            // TODO notify player?
            return;
         }

         if (_state.ReplayOperatingMode == ReplayOperatingMode.None)
         {
            StartPlayback(playbackMode);
         }
         else
         {
            StopPlayback();
            StartPlayback(playbackMode);
         }
      }
   }


   public void ToggleRecording()
   {
      if (_state.ReplayOperatingMode == ReplayOperatingMode.Playback)
      {
         return;
      }

      if (_state.ReplayOperatingMode == ReplayOperatingMode.None)
      {
         StartRecording();
      }
      // else if (_state.ReplayOperatingMode == ReplayOperatingMode.Recording)
      // {
      //    StopRecording();
      // }
   }

   
   public void SaveRecording()
   {
      if (_state.ReplayOperatingMode != ReplayOperatingMode.Recording)
      {
         return;
      }

      StopRecording();
      SaveToFile();
   }


   public void TogglePlayback(ReplayPlaybackMode playbackMode)
   {
      if (_state.ReplayOperatingMode == ReplayOperatingMode.Recording)
      {
         return;
      }
      
      if (_state.ReplayOperatingMode == ReplayOperatingMode.None)
      {
         StartPlayback(playbackMode);
      }
      // else if (_state.ReplayOperatingMode == ReplayOperatingMode.Playback)
      // {
      //    StopPlayback();
      // }
   }


   public void Process()
   {
      if (_state.ReplayOperatingMode == ReplayOperatingMode.Recording)
      {
         RecordSnapshot();
      }
      if (_state.ReplayOperatingMode == ReplayOperatingMode.Playback)
      {
         PlayNextSnapshot();
      }
   }


   private void OnTrackStarted()
   {
      if (_cfg.ReplayMode.AutoStart.Value && _state.ReplayOperatingMode != ReplayOperatingMode.Recording)
      {
         StartRecording();
      }
   }


   private void OnTrackCheckpointReached(string checkpointName)
   {
   }


   public void OnTrackFinished()
   {
      if ((_cfg.ReplayMode.AutoStop.Value || _cfg.ReplayMode.AutoSave.Value) && _state.ReplayOperatingMode == ReplayOperatingMode.Recording)
      {
         StopRecording();
      }
      if (_cfg.ReplayMode.AutoSave.Value)
      {
         SaveRecording();
      }
   }

   #endregion --- Operation ---


   public void StopCurrentOperation()
   {
      if (_state.ReplayOperatingMode == ReplayOperatingMode.Playback)
      {
         StopPlayback();
      }
      else if (_state.ReplayOperatingMode == ReplayOperatingMode.Recording)
      {
         StopRecording();
      }
   }
}
