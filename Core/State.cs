using UnityEngine;


namespace AlternativeCameraMod;

internal class State
{
   private readonly Logger _logger;
   private readonly Dictionary<string, GameObject> _menuObjects = new();
   private bool _suspended;
   private Screen _lastScreenState;
   private Screen _currentScreen;
   private bool _isMenuOpen = true;
   private bool _isMenuLastOpen;
   private bool _menuWasOpenedWhileInPhotoMode;
   private int _fps;
   private bool _needCameraReset;
   private bool _initialized;
   private string _activeMapName;
   private LevelState _levelState;
   private bool _handleStuckBike;


   private static readonly string[] __menuScenes = {
      "Menu_Alps_01", "Menu_Autumn_01", "Menu_Canyon_01", "Menu_Rockies_01", "Menu_Island_01",
      "gameplay", "DontDestroyOnLoad", "HideAndDontSave"
   };

   private int _trackSectionId;


   enum LevelState
   {
      WaitForStart,
      Running,
      Finished
   }



   public State(Logger logger)
   {
      _logger = logger;
   }


   public bool Initialize()
   {
      if (_initialized) return true;
      _initialized = GatherMenuRelatedGameObjects();
      return _initialized;
   }


   public void TrackScreenState()
   {
      _fps = (int)(1 / Math.Max(Time.deltaTime, 0.001));

      _lastScreenState = _currentScreen;
      var wrapper = GameObject.Find("Wrapper");
      if (wrapper == null)
      {
         _currentScreen = Screen.LoadingScreen;
         return;
      }

      bool blackActive = false;
      bool splashActive = false;
      bool mainMenuActive = false;
      bool gameMenuActive = false;
      bool playActive = false;
      bool pauseActive = false;
      var uiMainParent = wrapper.GetComponent<Transform>();
      for (int i = 0; i < uiMainParent.childCount; i++)
      {
         var ch = uiMainParent.GetChild(i);
         var g = ch.gameObject;
         if (g.name.StartsWith("BlackBorder"))
         {
            blackActive = g.active;
         }
         else if (g.name.StartsWith("SplashScreen"))
         {
            splashActive = g.active;
         }
         else if (g.name.StartsWith("MainMenu"))
         {
            mainMenuActive = g.active;
         }
         else if (g.name.StartsWith("GameMenu"))
         {
            gameMenuActive = g.active;
         }
         else if (g.name.StartsWith("PlayScreen"))
         {
            playActive = g.active;
         }
         else if (g.name.StartsWith("PauseScreen"))
         {
            pauseActive = g.active;
         }
      }

      if (blackActive && !splashActive && !mainMenuActive) _currentScreen = Screen.LoadingScreen;
      else if (splashActive && !mainMenuActive) _currentScreen = Screen.SplashScreen;
      else if (mainMenuActive) _currentScreen = Screen.MainMenuScreen;
      else if (gameMenuActive) _currentScreen = Screen.GameMenuScreen;
      else if (pauseActive) _currentScreen = Screen.PauseScreen;
      else if (playActive)
      {
         if (CameraMode == CameraMode.PhotoCam)
         {
            _currentScreen = Screen.PhotoScreen;
         }
         else
         {
            _currentScreen = Screen.PlayScreen;
         }
      }
      else _currentScreen = Screen.None;
   }


   public void TrackPausedInPhotoMode()
   {
      _menuWasOpenedWhileInPhotoMode = true;
      _logger.LogDebug("Open menu in photo mode");
   }
   
   
   public bool IsPausedInPhotoMode
   {
      get { return _menuWasOpenedWhileInPhotoMode; }
   }


   public void TogglePhotoModeInstructions()
   {
      PhotoModeInstructionsVisible = !PhotoModeInstructionsVisible;
   }


   public bool ShouldReturnToPhotoModeFromPauseMenu()
   {
      if (_lastScreenState == Screen.PauseScreen 
          && _currentScreen == Screen.PlayScreen 
          && _menuWasOpenedWhileInPhotoMode)
      {
         _isMenuOpen = false;
         _menuWasOpenedWhileInPhotoMode = false;
         return true;
      }

      return false;
   }


   public void OnPhotoModeEnter()
   {
      _logger.LogDebug("Enter photomode: {0} / {1} / {2}", _lastScreenState, _currentScreen, _menuWasOpenedWhileInPhotoMode);
      LastScreenshotInfo = null;
   }


   public void OnPhotoModeExit()
   {
      _logger.LogDebug("Exit photomode: {0} / {1} / {2}", _lastScreenState, _currentScreen, _menuWasOpenedWhileInPhotoMode);
      PhotoModeInstructionsVisible = true; // next time show instruction again
      _needCameraReset = true;
      _handleStuckBike = true;
   }
   

   public void SuspendOperation()
   {
      _suspended = true;
   }
   
   
   public void ResumeOperation()
   {
      _suspended = false;
   }

   
   public void CheckMenuOpen()
   {
      _isMenuOpen = _menuObjects.Values.Any(g => g.active);
      _logger.LogDebug(_isMenuOpen && _isMenuOpen != _isMenuLastOpen, "Menu opened");
      if (!_needCameraReset)
      {
         _needCameraReset = _isMenuLastOpen && !_isMenuOpen;
      }
      _isMenuLastOpen = _isMenuOpen;
   }

   
   public bool Suspended
   {
      get { return _suspended; }
   }


   public Screen CurrentScreen
   {
      get { return _currentScreen; }
   }
   
   
   public Screen LastScreen
   {
      get { return _lastScreenState; }
   }


   public CameraMode CameraMode { get; set; }


   public bool IsMenuOpen
   {
      get { return _isMenuOpen; }
   }


   public bool IsMenuOpenChanged()
   {
      return _isMenuOpen && !_isMenuLastOpen;
   }


   public int Fps
   {
      get { return _fps; }
   }


   public bool NeedCameraReset
   {
      get { return _needCameraReset; }
   }
   

   public void ClearNeedCameraReset()
   {
      _needCameraReset = false;
   }


   public string ErrorMessage { get; set; }


   public bool PhotoModeInstructionsVisible { get; private set; } = true;


   public string LastScreenshotInfo { get; set; }


   public ReplayOperatingMode ReplayOperatingMode { get; set; }


   public string ActiveMapName
   {
      get { return _activeMapName; }
   }


   public ReplayPlaybackMode PlaybackMode { get; set; }


   public int TrackSectionId
   {
      get { return _trackSectionId; }
   }


   public bool HandleStuckBike
   {
      get { return _handleStuckBike; }
   }


   public bool IsMapLoaded
   {
      get { return ActiveMapName != null; }
   }


   public void ResetHandleStuckBike()
   {
      _handleStuckBike = false;
   }


   private bool GatherMenuRelatedGameObjects()
   {
      var wrapper = GameObject.Find("Wrapper");
      if (wrapper == null)
      {
         return false;
      }

      var uiMainParent = GameObject.Find("Wrapper").GetComponent<Transform>();
      for (int i = 0; i < uiMainParent.childCount; i++)
      {
         var ch = uiMainParent.GetChild(i);
         var g = ch.gameObject;
         if (IsMenuObject(g.name))
         {
            _menuObjects[g.name] = g;
            _logger.LogVerbose("Game Object: {0}", g.name);
         }
      }

      return true;
   }


   private bool IsMenuObject(string name)
   {
      if (name == null) return false;
      if (name.StartsWith("BlackBorder")) return false;
      if (name.StartsWith("SplashScreen")) return false;
      if (name.StartsWith("PlayScreen")) return false;
      return true;
   }


   public void OnSceneLoaded(string sceneName)
   {
      if (CheckGameScene(sceneName))
      {
         _logger.LogDebug("Scene {0} loaded", sceneName);
         _activeMapName = sceneName;
         _levelState = LevelState.WaitForStart;
         InstallTriggers();
      }
   }


   private void InstallTriggers()
   {
      var checkpoints = GetCheckpoints();
      // Add the TriggerReader to each checkpoint
      for (var i = 0; i < checkpoints.Count; i++)
      {
         GameObject currentObject = checkpoints[i];
         if (currentObject.active)
         {
            var triggerScript = currentObject.AddComponent<TriggerReader>();
            triggerScript.Configure(this, TriggerEvent.Checkpoint);
         }
      }

      // Find the finish line and add the trigger
      var finishLine = GetFinishLine();
      var finishTriggerReader = finishLine.AddComponent<TriggerReader>();
      finishTriggerReader.Configure(this, TriggerEvent.Finish);
   }


   public List<GameObject> GetCheckpoints()
   {
      var cp = FindObjectsWithPartial("GameObject_Checkpoint", true);
      return cp;
   }

   
   private List<GameObject> FindObjectsWithPartial(string objectName, bool activeOnly)
   {
      GameObject[] gameObjects = GameObject.FindObjectsOfType<GameObject>();
      string objName = objectName.ToLower();
      var list = new List<GameObject>();
      for (var index = 0; index < gameObjects.Length; index++)
      {
         GameObject currentObject = gameObjects[index];
         string gameObjectName = currentObject.name.ToLower();
         if (gameObjectName.Contains(objName))
         {
            if (!activeOnly || currentObject.active)
            {
               list.Add(gameObjects[index]);
            }
         }
      }
      return list;
   }


   public GameObject GetFinishLine()
   {
      return GameObject.Find("GameObject_FinishLine");
   }
   
   
   public bool CheckGameScene(string sceneName)
   {
      return Array.IndexOf(__menuScenes, sceneName) == -1;
   }


   public void OnBikeMoved()
   {
      OnTrigger(TriggerEvent.Start, null, null);
   }

   
   public event EventHandler<TriggerEventArgs> TriggerOccurred;


   public void OnTrigger(TriggerEvent triggerEvent, GameObject gameObject, Collider other)
   {
      switch (triggerEvent)
      {
         case TriggerEvent.Start:
            _levelState = LevelState.Running;
            break;
         case TriggerEvent.Checkpoint:
            _levelState = LevelState.Running;
            DetermineCheckpoint(gameObject.name);
            break;
         case TriggerEvent.Finish:
            _levelState = LevelState.Finished;
            break;
      }
      TriggerOccurred?.Invoke(this, new TriggerEventArgs(triggerEvent, gameObject, other));
   }


   private void DetermineCheckpoint(string checkpointName)
   {
      _trackSectionId = 0;
      var cp = checkpointName.Split('_');
      if (cp.Length > 2)
      {
         Int32.TryParse(cp[2], out _trackSectionId);
      }
   }
}