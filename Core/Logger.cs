using MelonLoader;


namespace AlternativeCameraMod;

internal static class LogProvider
{
   public static MelonLogger.Instance BaseLogger { get; } = new MelonLogger.Instance("AltCamMod");
   public static LogLevel LogLevel { get; set; } =
#if DEBUG
      LogLevel.Debug;
#else
      LogLevel.Info;
#endif


   public static Logger GetLogger<T>()
   {
      var logger = new Logger(typeof(T));
      logger.Level = LogLevel;
#if DEBUG
      logger.Level = LogLevel.Debug;
#endif
      return logger;
   }
}


internal class Logger
{
   private readonly Type _type;


   public Logger(Type type)
   {
      _type = type;
   }


   private MelonLogger.Instance BaseLogger
   {
      get { return LogProvider.BaseLogger; }
   }


   public LogLevel Level { get; set; }


   private string FormatMsg(string msg)
   {
      return String.Format("[{0}] {1}", _type.Name, msg);
   }


   public void LogVerbose(string msg, params object[] args)
   {
      if (Level < LogLevel.Verbose) return;
      BaseLogger.Msg(System.ConsoleColor.DarkMagenta, FormatMsg(msg), args);
   }


   public void LogVerbose(bool condition, string msg, params object[] args)
   {
      if (Level < LogLevel.Verbose || !condition) return;
      BaseLogger.Msg(System.ConsoleColor.DarkMagenta, FormatMsg(msg), args);
   }


   public void LogDebug(string msg, params object[] args)
   {
      if (Level < LogLevel.Debug) return;
      BaseLogger.Msg(System.ConsoleColor.Magenta, FormatMsg(msg), args);
   }


   public void LogDebug(bool condition, string msg, params object[] args)
   {
      if (Level < LogLevel.Debug || !condition) return;
      BaseLogger.Msg(System.ConsoleColor.Magenta, FormatMsg(msg), args);
   }


   public void LogInfo(string msg, params object[] args)
   {
      if (Level < LogLevel.Info) return;
      BaseLogger.Msg(FormatMsg(msg), args);
   }


   public void LogInfo(bool condition, string msg, params object[] args)
   {
      if (Level < LogLevel.Info || !condition) return;
      BaseLogger.Msg(FormatMsg(msg), args);
   }


   public void LogWarning(string msg, params object[] args)
   {
      if (Level < LogLevel.Warning) return;
      BaseLogger.Warning(FormatMsg(msg), args);
   }


   public void LogWarning(bool condition, string msg, params object[] args)
   {
      if (Level < LogLevel.Warning || !condition) return;
      BaseLogger.Warning(FormatMsg(msg), args);
   }


   public void LogError(string msg, params object[] args)
   {
      if (Level < LogLevel.Error) return;
      BaseLogger.Error(FormatMsg(msg), args);
   }


   public void LogError(bool condition, string msg, params object[] args)
   {
      if (Level < LogLevel.Error || !condition) return;
      BaseLogger.Error(FormatMsg(msg), args);
   }


   public void Log(LogLevel level, string msg, params object[] args)
   {
      if (Level < level) return;
      switch (level)
      {
         case LogLevel.Debug:
            LogDebug(msg, args);
            break;
         case LogLevel.Info:
            LogInfo(msg, args);
            break;
         case LogLevel.Warning:
            LogWarning(msg, args);
            break;
         case LogLevel.Error:
            LogError(msg, args);
            break;
      }
   }
}
