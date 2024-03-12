using UnityEngine;


namespace AlternativeCameraMod.Replay;

internal class DownhillReplay
{
   private readonly DownhillRecording _recording;
   private readonly ReplayPlaybackMode _playbackMode;
   private readonly CameraControl _camera;
   private readonly BikeAnimator _bike;
   private float _timePos;
   private int _index1;
   private int _index2;
   private int _sectionId;
   private List<Snapshot>? _playList;
   private int _playCount;
   private bool _playing;


   public DownhillReplay(DownhillRecording recording, ReplayPlaybackMode playbackMode, CameraControl camera,
      BikeAnimator bike)
   {
      _recording = recording;
      _playbackMode = playbackMode;
      _bike = bike;
      _camera = camera;
   }


   public void Play(int startAtSectionId = 0)
   {
      _sectionId = startAtSectionId;
      _timePos = _recording.Sections[_sectionId].Snapshots[0].Timestamp;
      _index1 = 0;
      _index2 = 0;

      _playList = new();
      foreach (var section in _recording.Sections)
      {
         _playList.AddRange(section.Snapshots);
      }

      _playCount = _playList.Count;
      _playing = true;
   }


   public void Stop()
   {
      _index1 = 0;
      _index2 = 0;
      _playing = false;
   }


   public bool IsPlaying
   {
      get { return _playing; }
   }


   private bool CanPlayNext()
   {
      return _timePos <= _recording.LastTimestamp;
   }


   public int CurrentTrackSection
   {
      get { return _sectionId; }
   }


   public bool PrepareNext()
   {
      if (!CanPlayNext())
      {
         return false;
      }

      _timePos += Time.unscaledDeltaTime;
      UpdatePlaybackPosition();
      return true;
   }


   private void UpdatePlaybackPosition()
   {
      for (int i = 0; i < _playCount - 2; i++)
      {
         var frame = _playList[i];

#warning is this compare really working as intended?
         if (frame.Timestamp == _timePos)
         {
            _index1 = i;
            _index2 = i;
            return;
         }

         var frame1 = _playList[i + 1];
         if (frame.Timestamp < _timePos & _timePos < frame1.Timestamp)
         {
            _index1 = i;
            _index2 = i + 1;
            return;
         }
      }

      _index1 = _playCount - 1;
      _index2 = _playCount - 1;
   }


   public void ApplyState()
   {
      Snapshot s1, s2;
      float interpolationFactor;
      if (_index1 == _index2)
      {
         s1 = _playList[_index1];
         s2 = s1;
         interpolationFactor = 0; //unused
      }
      else
      {
         s1 = _playList[_index1];
         s2 = _playList[_index2];
         interpolationFactor = (_timePos - s1.Timestamp) / (s2.Timestamp - s1.Timestamp);
      }

      _sectionId = s1.SectionId;

      _bike.ApplyState(s1, s2, interpolationFactor);
      
      if (_playbackMode == ReplayPlaybackMode.Real)
      {
         // when real playback, the camera must follow
         // otherwise the player runs with the ghost

         RecreatePosition(ReplayPart.Camera, s1, s2, interpolationFactor);
      }
   }


   private void RecreatePosition(ReplayPart part, Snapshot s1, Snapshot s2, float interpolationFactor)
   {
      var pos1 = s1.Locations[(int)part].Item2;
      var rot1 = s1.Locations[(int)part].Item3;
      if (interpolationFactor < 0)
      {
         _camera.Position = pos1;
         _camera.Rotation = rot1;
      }
      else
      {
         var pos2 = s2.Locations[(int)ReplayPart.Camera].Item2;
         var rot2 = s2.Locations[(int)ReplayPart.Camera].Item3;
         _camera.Position = Vector3.Lerp(pos1, pos2, interpolationFactor);
         _camera.Rotation = Quaternion.Slerp(rot1, rot2, interpolationFactor);
      }
   }
}
