using System.Text;


namespace AlternativeCameraMod.Replay;

internal class TrackSection
{
   public int Id { get; }
   public List<Snapshot> Snapshots { get; } = new();


   public TrackSection(int id)
   {
      Id = id;
   }


   public static TrackSection Parse(int trackSection, string sectionFrames, List<string> elems)
   {
      var rs = new TrackSection(trackSection);
      var snapshotStrings = sectionFrames.Split(Snapshot.SnapshotMarker, StringSplitOptions.RemoveEmptyEntries);
      foreach (var snapshotStr in snapshotStrings)
      {
         try
         {
            var snapshot = Snapshot.Parse(trackSection, snapshotStr, elems);
            rs.Snapshots.Add(snapshot);
         }
         catch
         {
            // ignore corrupt snapshots; with regular usage this should never happen
         }
      }

      return rs;
   }


   public string Format()
   {
      var firstFrame = Snapshots[0].Format();
      StringBuilder sb = new StringBuilder((int)(Snapshots.Count * (firstFrame.Length * 1.1)));
      sb.Append(firstFrame);
      for (int i = 1; i < Snapshots.Count; i++)
      {
         sb.Append(Snapshots[i].Format());
      }

      return sb.ToString();
   }
}
