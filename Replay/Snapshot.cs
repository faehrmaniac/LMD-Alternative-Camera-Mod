using System.Globalization;
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
   
   private const string OverallFmt = SnapshotMarker + "{0}" + ElementSep + "{1}" + ElementSep + "{2}";

   private const string BikeLocationFmt = "{0}" + PropSep + "{1}" + PropSep + "{2}" // pos
                                          + LocationSep + "{3}" + PropSep + "{4}" + PropSep + "{5}" + PropSep + "{6}"; // rot  

   private const string CamLocationFmt = "{0}" + PropSep + "{1}" + PropSep + "{2}" // pos
                                         + LocationSep + "{3}" + PropSep + "{4}" + PropSep + "{5}" + PropSep + "{6}"; // rot 

   public int SectionId;
   public float Timestamp;
   public Vector3 BikePosition;
   public Quaternion BikeRotation;
   public Vector3 CamPosition;
   public Quaternion CamRotation;


   public Snapshot(int sectionId, float timestamp, Vector3 bikePosition, Quaternion bikeRotation, Vector3 camPosition, Quaternion camRotation)
   {
      SectionId = sectionId;
      Timestamp = timestamp;
      BikePosition = bikePosition;
      BikeRotation = bikeRotation;
      CamPosition = camPosition;
      CamRotation = camRotation;
   }


   public static Snapshot Parse(int trackSection, string snapshotData)
   {
      if (String.IsNullOrEmpty(snapshotData))
      {
         throw new InvalidOperationException("Can not parse empty snapshot data");
      }

      string[] parts = snapshotData.Split(ElementSep);

      var timestamp = ParseFloat(parts[0]);

      string[] bikeParts = parts[1].Split(LocationSep);
      string[] bikePosParts = bikeParts[0].Split(PropSep);
      string[] bikeRotParts = bikeParts[1].Split(PropSep);

      var bikePosition = new Vector3(
         ParseFloat(bikePosParts[0]),
         ParseFloat(bikePosParts[1]),
         ParseFloat(bikePosParts[2]));
      var bikeRotation = new Quaternion(
         ParseFloat(bikeRotParts[0]),
         ParseFloat(bikeRotParts[1]),
         ParseFloat(bikeRotParts[2]),
         ParseFloat(bikeRotParts[3]));

      string[] camParts = parts[2].Split(LocationSep);
      string[] camPosParts = camParts[0].Split(PropSep);
      string[] camRotParts = camParts[1].Split(PropSep);

      var camPosition = new Vector3(
         ParseFloat(camPosParts[0]),
         ParseFloat(camPosParts[1]),
         ParseFloat(camPosParts[2]));
      var camRotation = new Quaternion(
         ParseFloat(camRotParts[0]),
         ParseFloat(camRotParts[1]),
         ParseFloat(camRotParts[2]),
         ParseFloat(camRotParts[3]));

      return new Snapshot(trackSection, timestamp, bikePosition, bikeRotation, camPosition, camRotation);
   }


   public string Format()
   {
      var bikeStr = String.Format(BikeLocationFmt,
         FormatFloat(BikePosition.x),
         FormatFloat(BikePosition.y),
         FormatFloat(BikePosition.z),
         FormatFloat(BikeRotation.x),
         FormatFloat(BikeRotation.y),
         FormatFloat(BikeRotation.z),
         FormatFloat(BikeRotation.w));

      var camStr = String.Format(CamLocationFmt,
         FormatFloat(CamPosition.x),
         FormatFloat(CamPosition.y),
         FormatFloat(CamPosition.z),
         FormatFloat(CamRotation.x),
         FormatFloat(CamRotation.y),
         FormatFloat(CamRotation.z),
         FormatFloat(CamRotation.w));

      var overall = String.Format(OverallFmt, FormatFloat(Timestamp), bikeStr, camStr);
      return overall;
   }


   private static float ParseFloat(string bikePosPart)
   {
      return float.Parse(bikePosPart, CultureInfo.InvariantCulture);
   }


   private string FormatFloat(float val)
   {
      string str = val.ToString(CultureInfo.InvariantCulture);
      string fs = str.Substring(0, Math.Min(str.Length, str.IndexOf('.') + 5));
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
