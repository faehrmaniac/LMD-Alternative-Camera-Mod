using AlternativeCameraMod.Language;
using MelonLoader;


namespace AlternativeCameraMod.Config;

internal class ReplayModeSettings : ModSettingsCategory
{
   private MelonPreferences_Entry<int> _recordFrequency;
   private MelonPreferences_Entry<bool> _autoStart;
   private MelonPreferences_Entry<bool> _autoStop;
   private MelonPreferences_Entry<bool> _autoSave;
   private MelonPreferences_Entry<string> _recordingFolder;
   private MelonPreferences_Entry<string> _recordingFilenameFormat;


   public ReplayModeSettings(string filePath, LanguageConfig lng) 
      : base("Common", filePath, lng)
   {
      _recordFrequency = CreateEntry("RecordingFrequency", 30);
      _autoStart = CreateEntry("StartRecordingOnLevelLoad", false);
      _autoStop = CreateEntry("StopRecordingOnTrackFinish", false);
      _autoSave = CreateEntry("AutoSaveOnTrackFinish", false);
      _recordingFolder = CreateEntry("RecordingFolder",
         Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "Lonely Mountains - Downhill"),
         "Output folder where screenshots are saved, folder is created if not exists");
      _recordingFilenameFormat = CreateEntry("RecordingFilenameFormat",
         "LMD_replay_{cnt2}_{w}x{h}_{d}_{t}.png",
         "Format used to create filenames to save replay files; use extensions .png or .jpg\n" +
         "Placeholders:\n" +
         "{w}=screen width\n" +
         "{h}=screen height\n" +
         "{d1}=date as yyyy-MM-dd\n" +
         "{t1]=time as HH-mm-ss\n" +
         "{d2}=date as yyyyMMdd\n" +
         "{t2]=time as HHmmss\n" +
         "{cnt2}=2-digit counter, {cnt3}=3-digit counter, up to 4, 5");
   }


   public MelonPreferences_Entry<int> RecordFrequency
   {
      get { return _recordFrequency; }
   }


   public MelonPreferences_Entry<bool> AutoStart
   {
      get { return _autoStart; }
   }


   public MelonPreferences_Entry<bool> AutoStop
   {
      get { return _autoStop; }
   }


   public MelonPreferences_Entry<bool> AutoSave
   {
      get { return _autoSave; }
   }


   public MelonPreferences_Entry<string> RecordingFolder
   {
      get { return _recordingFolder; }
   }


   public MelonPreferences_Entry<string> RecordingFilenameFormat
   {
      get { return _recordingFilenameFormat; }
   }
}
