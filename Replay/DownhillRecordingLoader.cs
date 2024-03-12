using AlternativeCameraMod.Ini;


namespace AlternativeCameraMod.Replay;

internal class DownhillRecordingLoader
{
   public static DownhillRecording Load(string filePath)
   {
      using (var ini = IniFile.Open(filePath, true))
      {
         var revStr = ini["LMDReplay"]["FormatRev"];
         if (!Int32.TryParse(revStr, out var rev))
         {
            return null;
         }

         switch (rev)
         {
            default:
               return null;

            case 0:
               var dr = DownhillRecording.Load(filePath);
               return dr;

            // case 1:
            // upgrade from 1 -> then load
         }
      }
   }
}
