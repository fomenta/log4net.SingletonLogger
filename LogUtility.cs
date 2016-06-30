using log4net;
using log4net.Config;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace ConsoleApp
{
    public static class LogUtility
    {
        #region Private
        // WARNING: Not needed. Seemed a bug on target computer being tested 
        // frames to skip when detecting actual method calling the logger method.
        //#if DEBUG
        //#else
        //        private const int SKIP_FRAMES = 1;
        //#endif
        private const int SKIP_FRAMES = 2;

        private static ConcurrentDictionary<string, ILog> loggers = new ConcurrentDictionary<string, ILog>();
        private enum LevelEnum { Fatal, Error, Warn, Info, Debug, Trace }
        #endregion

        #region Public Properties
        public static int CurrentProcessID { get; private set; }
        public static bool IsDebugEnabled { get; private set; }
        public static bool IsInfoEnabled { get; private set; }
        public static bool IsWarnEnabled { get; private set; }
        #endregion

        #region Extra
        public static string GetFullMessage(Exception ex, string extraMessage = null, int maxLength = 0)
        {
            var errorMessage = (ex.InnerException == null ? "" : ex.InnerException.ToString() + Environment.NewLine)
                                + ex.ToString();

            if (!string.IsNullOrEmpty(extraMessage)) { errorMessage = extraMessage + " " + errorMessage; }

            if (maxLength > 0)
            { errorMessage = errorMessage.Substring(0, Math.Min(errorMessage.Length, maxLength)); }

            return errorMessage;
        }
        #endregion

        #region Static Constructor
        static LogUtility()
        {
            /* read config in the following order:
             * log4net.config (if exists)
             * app.config     (if exists)
            */
            var configInfo = new FileInfo(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
            var log4netConfigInfo = new FileInfo(Path.Combine(configInfo.DirectoryName, "log4net.config"));

            if (log4netConfigInfo.Exists) { XmlConfigurator.ConfigureAndWatch(log4netConfigInfo); }
            else if (configInfo.Exists) { XmlConfigurator.ConfigureAndWatch(configInfo); }

            CurrentProcessID = Process.GetCurrentProcess().Id;

            var log = LogManager.GetLogger("Root");
            IsDebugEnabled = log.IsDebugEnabled;
            IsInfoEnabled = log.IsInfoEnabled;
            IsWarnEnabled = log.IsWarnEnabled;
        }
        #endregion

        #region Wrappers
        // method on which setting might force to skip logging message
        public static void Warn(Func<string> getMessage) { Log(LevelEnum.Warn, getMessage); }
        public static void Info(Func<string> getMessage) { Log(LevelEnum.Info, getMessage); }
        public static void Debug(Func<string> getMessage) { Log(LevelEnum.Debug, getMessage); }
        public static void Trace(Func<string> getMessage) { Log(LevelEnum.Trace, getMessage); }

        public static void Fatal(string message, params object[] args) { Log(LevelEnum.Fatal, () => string_Format(message, args)); }
        public static void Error(string message, params object[] args) { Log(LevelEnum.Error, () => string_Format(message, args)); }
        public static void Warn(string message, params object[] args) { Log(LevelEnum.Warn, () => string_Format(message, args)); }
        public static void Info(string message, params object[] args) { Log(LevelEnum.Info, () => string_Format(message, args)); }
        public static void Debug(string message, params object[] args) { Log(LevelEnum.Debug, () => string_Format(message, args)); }
        public static void Trace(string message, params object[] args) { Log(LevelEnum.Trace, () => string_Format(message, args)); }

        public static void Fatal(int stackFrameOffset, string message, params object[] args) { Log(LevelEnum.Fatal, () => string_Format(message, args), stackFrameOffset); }
        public static void Error(int stackFrameOffset, string message, params object[] args) { Log(LevelEnum.Error, () => string_Format(message, args), stackFrameOffset); }
        public static void Warn(int stackFrameOffset, string message, params object[] args) { Log(LevelEnum.Warn, () => string_Format(message, args), stackFrameOffset); }
        public static void Info(int stackFrameOffset, string message, params object[] args) { Log(LevelEnum.Info, () => string_Format(message, args), stackFrameOffset); }
        public static void Debug(int stackFrameOffset, string message, params object[] args) { Log(LevelEnum.Debug, () => string_Format(message, args), stackFrameOffset); }
        public static void Trace(int stackFrameOffset, string message, params object[] args) { Log(LevelEnum.Trace, () => string_Format(message, args), stackFrameOffset); }

        private static void Log(LevelEnum level, Func<string> getMessage, int stackFrameOffset = 0)
        {
            try
            {
                var stackTrace = new StackTrace(SKIP_FRAMES + stackFrameOffset, true);
                var method = stackTrace.GetFrame(0).GetMethod();

                var className = method.DeclaringType.Name;
                int extraFrames = 0;
                string extraText = "";
                while (className == "Logger")
                {
#if DEBUG
                    if (!Debugger.IsAttached) { Debugger.Launch(); }
#endif
                    extraFrames++;
                    try
                    {
                        stackTrace = new StackTrace(SKIP_FRAMES + extraFrames, true);
                        method = stackTrace.GetFrame(0).GetMethod();
                        className = method.DeclaringType.Name;
                    }
                    catch (Exception ex)
                    {
                        extraText = "ERROR: " + ex.Message;
                        break;
                    }
                }
                extraText += extraFrames == 0 ? "" : string.Format("[Extra Frames: {0}] ", extraFrames);

                var assemblyName = method.DeclaringType.Assembly.GetName().Name;
                var methodName = method.Name;

                var loggerName = string.Format("{0}.{1}.{2}.{3}.{4}", Environment.MachineName, assemblyName, CurrentProcessID, className, methodName);
                var log = GetLogger(loggerName);

                switch (level)
                {
                    case LevelEnum.Warn: if (!log.IsWarnEnabled) { return; } break;
                    case LevelEnum.Info: if (!log.IsInfoEnabled) { return; } break;
                    case LevelEnum.Debug: if (!log.IsDebugEnabled) { return; } break;
                }

                ThreadContext.Properties["MachineName"] = Environment.MachineName;
                ThreadContext.Properties["PID"] = CurrentProcessID.ToString();
                ThreadContext.Properties["ThreadID"] = Thread.CurrentThread.ManagedThreadId.ToString();
                ThreadContext.Properties["Class"] = className;
                ThreadContext.Properties["Method"] = methodName;

                var message = extraText + getMessage();
                
                // fix encoding with special character (á, ñ, etc)
                message = Encoding.Default.GetString(Encoding.UTF8.GetBytes(message));
                //message = Encoding.GetEncoding("Windows-1252").GetString(Encoding.UTF8.GetBytes(message));

                // add extra text
                message = extraText + message;

                //message = string.Format("[{0}.{1}] {2}", method.DeclaringType.Name, method.Name, message);
                LogInternal(log, level, message);
            }
            catch (Exception ex)
            {
                var loggerName = string.Format("{0}.{1}.{2}.{3}", "Logging", CurrentProcessID, "Logger", "Trace");
                var log = LogManager.GetLogger(loggerName);
                LogInternal(log, LevelEnum.Error, ex.ToString());
            }
        }

        private static void LogInternal(ILog log, LevelEnum level, string message)
        {
            switch (level)
            {
                case LevelEnum.Fatal: log.Fatal(message); break;
                case LevelEnum.Error: log.Error(message); break;
                case LevelEnum.Warn: log.Warn(message); break;
                case LevelEnum.Info: log.Info(message); break;
                case LevelEnum.Debug: log.Debug(message); break;
                case LevelEnum.Trace: log.Logger.Log(null, log4net.Core.Level.Trace, message, exception: null); break;
                default: throw new NotImplementedException();
            }
        }

        private static string string_Format(string message, object[] args)
        {
            if (string.IsNullOrEmpty(message)) { return message; }
            if (args == null || args.Length == 0)
            {
                message = message.Replace("{", "{{").Replace("}", "}}");
            }
            return string.Format(message, args);
        }

        /// <summary>
        /// internal function to determine how to display correctly special character on Log2Console
        /// </summary>
        /// <param name="log"></param>
        /// <param name="level"></param>
        private static void buildSampleMessages(ILog log, LevelEnum level)
        {
            var sb = new StringBuilder();
            const string SPECIAL_CHARS = "áéíóúÁÉÍÓÚ àèìòùÀÈÌÒÙ äëïöüÄËÏÖÜ ñÑ";
            // check how to fix encoding with special character (á, ñ, etc)
            var text = SPECIAL_CHARS;

            LogInternal(log, level, SPECIAL_CHARS);
            LogInternal(log, level, Encoding.Default.GetString(Encoding.UTF8.GetBytes("WINNER: " + SPECIAL_CHARS)));

            var encodings = new List<EncodingInfo>();
            foreach (var item in Encoding.GetEncodings()) { encodings.Add(item); }

            var list = new List<string>();

            foreach (var targetItem in encodings)
            {
                var target = targetItem.GetEncoding();
                foreach (var sourceItem in encodings)
                {
                    var source = sourceItem.GetEncoding();
                    var newText = target.GetString(source.GetBytes(text));
                    newText = string.Format("[{0}:{1} to {2}:{3}] {4}", sourceItem.CodePage, sourceItem.Name, targetItem.CodePage, targetItem.Name, newText);
                    LogInternal(log, level, newText);
                    list.Add(newText);
                }
            }

            var f = list.Count;

            LogInternal(log, level, SPECIAL_CHARS);
        }

        private static ILog GetLogger(string loggerName)
        {
            ILog log;

            if (loggers.ContainsKey(loggerName)) { log = loggers[loggerName]; }
            else
            {
                loggers.GetOrAdd(loggerName, log = LogManager.GetLogger(loggerName));
            }

            return log;



        }
        #endregion

        #region Publish Exceptions
        /// <suwwary>
        /// Publishes a given exception.
        /// <fsuwwary>
        /// <paraw nawe="ex"><fparaw>
        /// <returns><freturns>
        public static Exception PublishException(Exception ex, string extraMessage = null)
        {
            ex = ex.GetBaseException();
            Log(LevelEnum.Error, () => (extraMessage ?? "") + GetFullMessage(ex));
            return ex;
        }
        #endregion
    }
}
