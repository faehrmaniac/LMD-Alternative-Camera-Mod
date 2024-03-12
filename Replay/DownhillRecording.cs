using AlternativeCameraMod.Ini;
using Il2CppMegagon.Downhill.Vehicle.Animation;
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


   public void Record(int sectionId, CameraControl cam, BikeAnimator bikeAnim)
   {
      System.Diagnostics.Debug.Assert(_recording);

      _timePos += Time.unscaledDeltaTime;
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
}


internal class BikeAnimator
{
   private readonly Transform _bike;
   private readonly Transform _bikePart1;
   private readonly Transform _bikePart2;
   private readonly Transform _bikePart3;
   private readonly Transform _bikePart4;
   private readonly Transform _riderPart1;
   private readonly Transform _riderPart2;
   private readonly Transform _riderPart3;


   public BikeAnimator(GameObject bike)
   {
      var ba = bike.GetComponent<BikeAnimation>();
      _bike = bike.transform;
      _bikePart1 = ba.m_bikeRootTransform.GetChild(0);
      _bikePart2 = ba.m_bikeRootTransform.GetChild(1);
      _bikePart3 = ba.m_bikeRootTransform.GetChild(2);
      _bikePart4 = ba.m_bikeRootTransform.GetChild(3);
      _riderPart1 = ba.m_riderRootTransform.GetChild(0);
      _riderPart2 = ba.m_riderRootTransform.GetChild(1);
      _riderPart3 = ba.m_riderRootTransform.GetChild(2);
   }


   public IEnumerable<Tuple<ReplayPart, Vector3, Quaternion>> GetLocations()
   {
      yield return new Tuple<ReplayPart, Vector3, Quaternion>(ReplayPart.Bike, _bike.position, _bike.rotation);
      yield return new Tuple<ReplayPart, Vector3, Quaternion>(ReplayPart.Bike1, _bikePart1.position, _bikePart1.rotation);
      yield return new Tuple<ReplayPart, Vector3, Quaternion>(ReplayPart.Bike2, _bikePart2.position, _bikePart2.rotation);
      yield return new Tuple<ReplayPart, Vector3, Quaternion>(ReplayPart.Bike3, _bikePart3.position, _bikePart3.rotation);
      yield return new Tuple<ReplayPart, Vector3, Quaternion>(ReplayPart.Bike4, _bikePart4.position, _bikePart4.rotation);
      yield return new Tuple<ReplayPart, Vector3, Quaternion>(ReplayPart.Rider1, _riderPart1.position, _riderPart1.rotation);
      yield return new Tuple<ReplayPart, Vector3, Quaternion>(ReplayPart.Rider2, _riderPart2.position, _riderPart2.rotation);
      yield return new Tuple<ReplayPart, Vector3, Quaternion>(ReplayPart.Rider3, _riderPart3.position, _riderPart3.rotation);

      yield return new Tuple<ReplayPart, Vector3, Quaternion>(ReplayPart.BikeL1, _bikePart1.localPosition, _bikePart1.localRotation);
      yield return new Tuple<ReplayPart, Vector3, Quaternion>(ReplayPart.BikeL2, _bikePart2.localPosition, _bikePart2.localRotation);
      yield return new Tuple<ReplayPart, Vector3, Quaternion>(ReplayPart.BikeL3, _bikePart3.localPosition, _bikePart3.localRotation);
      yield return new Tuple<ReplayPart, Vector3, Quaternion>(ReplayPart.BikeL4, _bikePart4.localPosition, _bikePart4.localRotation);
      yield return new Tuple<ReplayPart, Vector3, Quaternion>(ReplayPart.RiderL1, _riderPart1.localPosition, _riderPart1.localRotation);
      yield return new Tuple<ReplayPart, Vector3, Quaternion>(ReplayPart.RiderL2, _riderPart2.localPosition, _riderPart2.localRotation);
      yield return new Tuple<ReplayPart, Vector3, Quaternion>(ReplayPart.RiderL3, _riderPart3.localPosition, _riderPart3.localRotation);
   }


   public void ApplyState(Snapshot s1, Snapshot s2, float interpolationFactor)
   {
      RecreatePosition(_bike, ReplayPart.Bike, s1, s2, interpolationFactor);
      RecreatePosition(_bikePart1, ReplayPart.Bike1, s1, s2, interpolationFactor);
      RecreatePosition(_bikePart2, ReplayPart.Bike2, s1, s2, interpolationFactor);
      RecreatePosition(_bikePart3, ReplayPart.Bike3, s1, s2, interpolationFactor);
      RecreatePosition(_bikePart4, ReplayPart.Bike4, s1, s2, interpolationFactor);
      RecreatePosition(_riderPart1, ReplayPart.Rider1, s1, s2, interpolationFactor);
      RecreatePosition(_riderPart2, ReplayPart.Rider2, s1, s2, interpolationFactor);
      RecreatePosition(_riderPart3, ReplayPart.Rider3, s1, s2, interpolationFactor);
      
      RecreateLocalPosition(_bikePart1, ReplayPart.BikeL1, s1, s2, interpolationFactor);
      RecreateLocalPosition(_bikePart2, ReplayPart.BikeL2, s1, s2, interpolationFactor);
      RecreateLocalPosition(_bikePart3, ReplayPart.BikeL3, s1, s2, interpolationFactor);
      RecreateLocalPosition(_bikePart4, ReplayPart.BikeL4, s1, s2, interpolationFactor);
      RecreateLocalPosition(_riderPart1, ReplayPart.RiderL1, s1, s2, interpolationFactor);
      RecreateLocalPosition(_riderPart2, ReplayPart.RiderL2, s1, s2, interpolationFactor);
      RecreateLocalPosition(_riderPart3, ReplayPart.RiderL3, s1, s2, interpolationFactor);
   }


   private void RecreatePosition(Transform target, ReplayPart part, Snapshot s1, Snapshot s2, float interpolationFactor)
   {
      var pos1 = s1.Locations[(int)part].Item2;
      var rot1 = s1.Locations[(int)part].Item3;
      if (interpolationFactor < 0)
      {
         target.position = pos1;
         target.rotation = rot1;
      }
      else
      {
         var pos2 = s2.Locations[(int)part].Item2;
         var rot2 = s2.Locations[(int)part].Item3;
         target.position = Vector3.Lerp(pos1, pos2, interpolationFactor);
         target.rotation = Quaternion.Slerp(rot1, rot2, interpolationFactor);
      }
   }
   
   
   private void RecreateLocalPosition(Transform target, ReplayPart part, Snapshot s1, Snapshot s2, float interpolationFactor)
   {
      var pos1 = s1.Locations[(int)part].Item2;
      var rot1 = s1.Locations[(int)part].Item3;
      if (interpolationFactor < 0)
      {
         target.localPosition = pos1;
         target.localRotation = rot1;
      }
      else
      {
         var pos2 = s2.Locations[(int)part].Item2;
         var rot2 = s2.Locations[(int)part].Item3;
         target.localPosition = Vector3.Lerp(pos1, pos2, interpolationFactor);
         target.localRotation = Quaternion.Slerp(rot1, rot2, interpolationFactor);
      }
   }
}
