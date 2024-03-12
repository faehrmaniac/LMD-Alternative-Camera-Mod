using AlternativeCameraMod.Config;
using AlternativeCameraMod.Language;
using Il2CppSystem.Text;
using UnityEngine;


namespace AlternativeCameraMod;

internal class DevHelper
{
   private readonly State _state;
   private readonly InputHandler _input;
   private readonly CameraControl _camera;
   private readonly Hud _hud;
   private readonly LanguageConfig _lang;
   private readonly Configuration _cfg;


   public DevHelper(State state, InputHandler input, CameraControl camera, Hud hud, LanguageConfig lang, Configuration cfg)
   {
      _state = state;
      _input = input;
      _camera = camera;
      _hud = hud;
      _lang = lang;
      _cfg = cfg;

   }


   private string GetOutputFolder()
   {
      var debugOutputFolder = new FileInfo("UserData\\LMDdev").FullName;
      Directory.CreateDirectory(debugOutputFolder);
      return debugOutputFolder;
   }

   
   private string GetOutputFilePath(string fileName)
   {
      var filePath = Path.Combine(GetOutputFolder(), fileName);
      return filePath;
   }


   public void ProcessDevRequest()
   {
      if (_input.DevKey(12))
      {
         // write all config examples
         var langs = LanguageConfig.GetAvailableLanguages();
         foreach (var lang in langs)
         {
            var lc = LanguageConfig.Load(lang);
            var cfg = Configuration.CreateForLanguage(lc);
            cfg.Save();   
         }
      }
   }


   public void ProcessGameplayDevRequest()
   {
      if (_input.DevKey(11))
      {
         WriteAllGameObjectsToFile(true);
      }
      if (_input.DevKey(12))
      {
         WriteAllGameObjectsToFile();
      }
   }


   private void WriteAllGameObjectsToFile(bool activeOnly = false)
   {
      GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject>();
      StringBuilder sb = new StringBuilder(200000);
      for (var index = 0; index < gameObjects.Length; index++)
      {
         GameObject currentObject = gameObjects[index];
         if (!activeOnly || currentObject.active)
         {
            sb.AppendLine(currentObject.name);
         }
      }

      var outFile = activeOnly ? GetOutputFilePath("LmdGameObjects_active.txt") : GetOutputFilePath("LmdGameObjects_all.txt");
      File.WriteAllText(outFile, sb.ToString());
   }
}
