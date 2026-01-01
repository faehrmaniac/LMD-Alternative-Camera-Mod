using System.Globalization;
using System.Text;
using UnityEngine;


namespace AlternativeCameraMod.Replay;

/// <summary>
/// Stores the state of an object at a single point in time.
///
/// Timestamp|CamPos|CamRot|AllBikeParts
/// e.g. "0|0,0,0:0,0,0,0|0,0,0:0,0,0,0 ... "
/// </summary>
internal struct Snapshot
{
   public const string SnapshotMarker = "#";
   private const string ElementSep = "|";
   private const string LocationSep = ":";
   private const string PropSep = ",";
   
   private const string LocationFmt = "{0}" + PropSep + "{1}" + PropSep + "{2}" // pos
                                      + LocationSep + "{3}" + PropSep + "{4}" + PropSep + "{5}" + PropSep + "{6}"; // rot

   public int SectionId;
   public float Timestamp;
   public Dictionary<string, Tuple<Vector3, Quaternion>> LocationMap;


   public Snapshot(int sectionId, float timestamp, Vector3 camPosition, Quaternion camRotation, BikeReanimator bikeReanimator)
   {
      SectionId = sectionId;
      Timestamp = timestamp;
      LocationMap = new Dictionary<string, Tuple<Vector3, Quaternion>>();
      Add("Camera", camPosition, camRotation);
      foreach (var loc in bikeReanimator.GetLocations())
      {
         LocationMap.Add(loc.Item1, new Tuple<Vector3, Quaternion>(loc.Item2, loc.Item3));
      }
   }


   private void Add(string name, Vector3 position, Quaternion rotation)
   {
      LocationMap.Add(name, new Tuple<Vector3, Quaternion>(position, rotation));
   }


   private Snapshot(int sectionId, float timestamp, Dictionary<string, Tuple<Vector3, Quaternion>> locationMap)
   {
      SectionId = sectionId;
      Timestamp = timestamp;
      LocationMap = locationMap;
   }


   public static Snapshot Parse(int trackSection, string snapshotData, List<string> elems)
   {
      if (String.IsNullOrEmpty(snapshotData))
      {
         throw new InvalidOperationException("Can not parse empty snapshot data");
      }

      string[] parts = snapshotData.Split(ElementSep);
      
      var timestamp = ParseFloat(parts[0]);
      
      var locMap = new Dictionary<string, Tuple<Vector3, Quaternion>>();
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

         string name;
         if (i == 1)
         {
            name = "Camera";
         }
         else
         {
            name = elems[i - 2];
         }
         locMap.Add(name, new Tuple<Vector3, Quaternion>(position, rotation));
      }

      var snapshot = new Snapshot(trackSection, timestamp, locMap);
      return snapshot;
   }


   public string Format()
   {
      StringBuilder sn = new StringBuilder();
      sn.Append(SnapshotMarker);
      sn.Append(FormatFloat(Timestamp));
      
      foreach (var location in LocationMap)
      {
         var pos = location.Value.Item1;
         var rot = location.Value.Item2;
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
      string fs = Math.Round(val, 3).ToString(CultureInfo.InvariantCulture);
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
