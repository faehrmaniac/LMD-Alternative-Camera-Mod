using AlternativeCameraMod.Ini;
using UnityEngine;


namespace AlternativeCameraMod.Replay;

internal class DownhillRecording
{
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
      using (var ini = IniFile.Open(filePath, false))
      {
         dr.FormatRev = ini["LMDReplay"].GetValue("FormatRev", 0);
         dr.MapName = ini["LMDReplay"].GetValue("MapName", "unknown");

         foreach (var sectionKey in ini.GetSection("Sections").Keys)
         {
            var secNum = Int32.Parse(sectionKey);
            var secStr = ini["Sections"].GetValue(sectionKey, "");
            var seg = TrackSection.Parse(secNum, secStr);
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


   public void Record(int sectionId, Transform bike, CameraControl cam)
   {
      System.Diagnostics.Debug.Assert(_recording);

      _timePos += Time.unscaledDeltaTime;
      Snapshot snapshot = new Snapshot(
         sectionId,
         _timePos,
         bike.position,
         bike.rotation,
         cam.Position,
         cam.Rotation
      );

      while (snapshot.SectionId >= Sections.Count)
      {
         Sections.Add(new TrackSection(Sections.Count));
      }

      Sections[snapshot.SectionId].Snapshots.Add(snapshot);
   }
}
