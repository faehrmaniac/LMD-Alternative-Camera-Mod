using System.Globalization;
using System.Text;
using UnityEngine;


namespace AlternativeCameraMod.Replay;

/// <summary>
/// Stores the state of an object at a single point in time.
///
/// Timestamp|BikePos|BikeRot|CamPos|CamRot
/// e.g. "0|0,0,0:0,0,0,0|0,0,0:0,0,0,0"
/// </summary>
internal struct Snapshot
{
   public const string SnapshotMarker = "#";
   private const string ElementSep = "|";
   private const string LocationSep = ":";
   private const string PropSep = ",";
   
   // private const string OverallFmt = SnapshotMarker + "{0}" + ElementSep + "{1}" + ElementSep + "{2}";

   private const string LocationFmt = "{0}" + PropSep + "{1}" + PropSep + "{2}" // pos
                                       + LocationSep + "{3}" + PropSep + "{4}" + PropSep + "{5}" + PropSep + "{6}"; // rot  

   public int SectionId;
   public float Timestamp;
   public List<Tuple<ReplayPart, Vector3, Quaternion>> Locations;


   public Snapshot(int sectionId, float timestamp, Vector3 camPosition, Quaternion camRotation, BikeAnimator bikeAnimator)
   {
      SectionId = sectionId;
      Timestamp = timestamp;
      Locations = new List<Tuple<ReplayPart, Vector3, Quaternion>>();
      Add(ReplayPart.Camera, camPosition, camRotation);
      foreach (var loc in bikeAnimator.GetLocations())
      {
         Locations.Add(loc);
      }
   }


   private void Add(ReplayPart part, Vector3 position, Quaternion rotation)
   {
      Locations.Add(new Tuple<ReplayPart, Vector3, Quaternion>(part, position, rotation));
   }


   private Snapshot(int sectionId, float timestamp, List<Tuple<ReplayPart, Vector3, Quaternion>> locations)
   {
      SectionId = sectionId;
      Timestamp = timestamp;
      Locations = locations;
   }


   public static Snapshot Parse(int trackSection, string snapshotData)
   {
      if (String.IsNullOrEmpty(snapshotData))
      {
         throw new InvalidOperationException("Can not parse empty snapshot data");
      }

      string[] parts = snapshotData.Split(ElementSep);
      
      var timestamp = ParseFloat(parts[0]);
      
      var locList = new List<Tuple<ReplayPart, Vector3, Quaternion>>();
      for (int i = 1; i < parts.Length; i++)
      {
         string[] subParts = parts[i].Split(LocationSep);
         string[] posParts = subParts[0].Split(PropSep);
         string[] rotParts = subParts[1].Split(PropSep);

         var position = new Vector3(
            ParseFloat(posParts[0]),
            ParseFloat(posParts[1]),
            ParseFloat(posParts[2]));
         var rotation = new Quaternion(
            ParseFloat(rotParts[0]),
            ParseFloat(rotParts[1]),
            ParseFloat(rotParts[2]),
            ParseFloat(rotParts[3]));

         locList.Add(new Tuple<ReplayPart, Vector3, Quaternion>((ReplayPart)(i - 1), position, rotation));
      }

      var snapshot = new Snapshot(trackSection, timestamp, locList);
      return snapshot;
   }


   public string Format()
   {
      StringBuilder sn = new StringBuilder();
      sn.Append(SnapshotMarker);
      sn.Append(FormatFloat(Timestamp));
      
      foreach (var location in Locations)
      {
         var pos = location.Item2;
         var rot = location.Item3;
         var locStr = String.Format(LocationFmt,
            FormatFloat(pos.x),
            FormatFloat(pos.y),
            FormatFloat(pos.z),
            FormatFloat(rot.x),
            FormatFloat(rot.y),
            FormatFloat(rot.z),
            FormatFloat(rot.w));
         sn.Append(ElementSep).Append(locStr);
      }
      
      return sn.ToString();
   }


   private static float ParseFloat(string bikePosPart)
   {
      return float.Parse(bikePosPart, CultureInfo.InvariantCulture);
   }


   private string FormatFloat(float val)
   {
      string fs = Math.Round(val, 5).ToString(CultureInfo.InvariantCulture);
      return fs;
   }


   /// <summary>
   /// Returns the Snapshot as a super compact JSON string.
   /// </summary>
   public override string ToString()
   {
      return Format();
   }
}


internal enum ReplayPart
{
   Camera,
   Bike,
   Bike1,
   Bike2,
   Bike3,
   Bike4,
   Rider1,
   Rider2,
   Rider3,
   BikeL1,
   BikeL2,
   BikeL3,
   BikeL4,
   RiderL1,
   RiderL2,
   RiderL3,
}