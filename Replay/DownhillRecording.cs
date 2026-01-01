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
   private HashSet<string> _elements;


   private DownhillRecording()
   {
      _bike = ReplayBike.Player();
   }


   public DownhillRecording(string mapName) : this()
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


   public HashSet<string> SnapshotElements
   {
      get
      {
         if (_elements == null)
         {
            _elements = new HashSet<string>(_bike.Reanimator.GetObjectNames());
         }
         return _elements;
      }
      private set { _elements = value; }
   }


   public void Save(string filePath)
   {
      System.Diagnostics.Debug.Assert(!_recording);

      using (var ini = IniFile.Open(filePath, true))
      {
         ini["LMDReplay"]["FormatRev"] = FormatRev.ToString();
         ini["LMDReplay"]["MapName"] = MapName;
         ini["LMDReplay"]["Snapshots"] = _snapshotCount.ToString();
         ini["LMDReplay"]["Duration"] = GetDuration().ToString();
         ini["LMDReplay"]["CheckpointsPassed"] = (Sections.Count - 1).ToString();
         ini["LMDReplay"]["ElementCount"] = GetSnapshotElementCount(_bike.Reanimator).ToString();
         ini["LMDReplay"]["ElementOrder"] = String.Join('|', _bike.Reanimator.GetObjectNames());

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
         var elemCnt = ini["LMDReplay"].GetValue("ElementCount", 0);

         // Check if format matches the state of the game (are there any future changes to expect?)
         if (elemCnt != GetSnapshotElementCount(bike.Reanimator))
         {
            // maybe some additional stuff is not used or some stuff is missing,
            // e.g. when recording was done with a rider with hair and is loaded
            // when a rider without hair is active, the hair data is not used.
            // Other way round when recording is done with a rider without hair
            // and loaded for a rider with hair, the hair movement data is missing
            // and the hair will not be animated correctly
         }

         var elems = ini["LMDReplay"].GetValue("ElementOrder", "");
         var elemsInOrder = elems.Split('|').ToList();
         dr.SnapshotElements = new HashSet<string>();
         dr.SnapshotElements.UnionWith(elemsInOrder);

         foreach (var sectionKey in ini.GetSection("Sections").Keys)
         {
            var secNum = Int32.Parse(sectionKey);
            var secStr = ini["Sections"].GetValue(sectionKey, "");
            var seg = TrackSection.Parse(secNum, secStr, elemsInOrder);
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


   public void Record(int sectionId, CameraControl cam)
   {
      System.Diagnostics.Debug.Assert(_recording);

      TrackTimestamp();
      Snapshot snapshot = new Snapshot(
         sectionId,
         _timePos,
         cam.Position,
         cam.Rotation,
         _bike.Reanimator);

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