using AlternativeCameraMod.Ini;
using UnityEngine;


namespace AlternativeCameraMod.Replay;

internal class DownhillRecording
{
   private readonly ReplayBike _bike;
   private float _lastTimestamp;
   private int _snapshotCount;
   public int FormatRev { get; private set; }
   public string MapName { get; private set; }
   public List<TrackSection> Sections { get; } = new();
   private float _timePos;
   private bool _recording;
   private string _filePath;


   private DownhillRecording()
   {
      _bike = ReplayBike.Player();
   }


   public DownhillRecording(string mapName)
   {
      MapName = mapName;
   }
   

   public int SnapshotCount
   {
      get { return _snapshotCount; }
   }


   public string FilePath
   {
      get { return _filePath; }
   }


   public float LastTimestamp
   {
      get { return _lastTimestamp; }
   }
   

   public bool IsRecording
   {
      get { return _recording; }
   }


   public void Save(string filePath)
   {
      System.Diagnostics.Debug.Assert(!_recording);

      using (var ini = IniFile.Open(filePath, true))
      {
         ini["LMDReplay"]["FormatRev"] = FormatRev.ToString();
         ini["LMDReplay"]["MapName"] = MapName;
         ini["LMDReplay"]["Elements"] = GetSnapshotElementCount(_bike.Reanimator).ToString();
         ini["LMDReplay"]["Snapshots"] = _snapshotCount.ToString();
         ini["LMDReplay"]["Duration"] = GetDuration().ToString();
         ini["LMDReplay"]["CheckpointsPassed"] = (Sections.Count - 1).ToString();

         foreach (var sec in Sections)
         {
            ini["Sections"][sec.Id.ToString()] = sec.Format();
         }

         ini.Save();
      }

      _filePath = filePath;
   }


   public static DownhillRecording Load(string filePath)
   {
      var dr = new DownhillRecording();
      var bike = ReplayBike.Player();
      using (var ini = IniFile.Open(filePath, false))
      {
         dr.FormatRev = ini["LMDReplay"].GetValue("FormatRev", 0);
         dr.MapName = ini["LMDReplay"].GetValue("MapName", "unknown");
         var elems = ini["LMDReplay"].GetValue("Elements", 0);

         // Check if format matches the state of the game (are there any future changes to expect?)
         if (elems != GetSnapshotElementCount(bike.Reanimator))
         {
            return null;
         }

         foreach (var sectionKey in ini.GetSection("Sections").Keys)
         {
            var secNum = Int32.Parse(sectionKey);
            var secStr = ini["Sections"].GetValue(sectionKey, "");
            var seg = TrackSection.Parse(secNum, secStr, bike.Reanimator);
            dr.Sections.Add(seg);
         }
      }

      dr._filePath = filePath;
      dr.Update();
      return dr;
   }


   public void Update()
   {
      _snapshotCount = Sections.Sum(x => x.Snapshots.Count);
      _lastTimestamp = Sections.Last().Snapshots.Last().Timestamp;
   }


   public void Record()
   {
      _recording = true;
      _timePos = 0;
   }


   public void Stop()
   {
      Update();
      _recording = false;
   }

   
   public void TrackTimestamp()
   {
      _timePos += Time.unscaledDeltaTime;
   }


   public void Record(int sectionId, CameraControl cam, BikeReanimator bikeAnim)
   {
      System.Diagnostics.Debug.Assert(_recording);

      TrackTimestamp();
      Snapshot snapshot = new Snapshot(
         sectionId,
         _timePos,
         cam.Position,
         cam.Rotation,
         bikeAnim);

      while (snapshot.SectionId >= Sections.Count)
      {
         Sections.Add(new TrackSection(Sections.Count));
      }

      Sections[snapshot.SectionId].Snapshots.Add(snapshot);
   }

   
   private float GetDuration()
   {
      return Sections.LastOrDefault()?.Snapshots.LastOrDefault().Timestamp ?? 0f;
   }


   private static int GetSnapshotElementCount(BikeReanimator reanimator)
   {
      return reanimator.GetLocationCount() + 1;
   }
}