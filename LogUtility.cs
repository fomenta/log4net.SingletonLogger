﻿using log4net;
using log4net.Config;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ConsoleApp
{
    public static class LogUtility
    {
        #region Private
        // frames to skip when detecting actual method calling the logger method.
#if DEBUG
        private const int SKIP_FRAMES = 2;
#else
        private const int SKIP_FRAMES = 1;
#endif
        private static ConcurrentDictionary<string, ILog> loggers = new ConcurrentDictionary<string, ILog>();
        private enum LevelEnum { Fatal, Error, Warn, Info, Debug }
        #endregion

        #region Public Properties
        public static int CurrentProcessID { get; private set; }
        public static bool IsDebugEnabled { get; private set; }
        public static bool IsInfoEnabled { get; private set; }
        public static bool IsWarnEnabled { get; private set; }
        #endregion

        #region Static Constructor
        static LogUtility()
        {


            string configPath = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            if (File.Exists(configPath)) { XmlConfigurator.ConfigureAndWatch(new FileInfo(configPath)); }

            CurrentProcessID = Process.GetCurrentProcess().Id;

            var log = LogManager.GetLogger("Root");
            IsDebugEnabled = log.IsDebugEnabled;
            IsInfoEnabled = log.IsInfoEnabled;
            IsWarnEnabled = log.IsWarnEnabled;
        }
        #endregion

        #region Wrappers
        // method on which setting might force to skip logging message
        public static void Warn(Func<string> getMessage) { Trace(LevelEnum.Warn, getMessage); }
        public static void Info(Func<string> getMessage) { Trace(LevelEnum.Info, getMessage); }
        public static void Debug(Func<string> getMessage) { Trace(LevelEnum.Debug, getMessage); }

        public static void Fatal(string message, params object[] args) { Trace(LevelEnum.Fatal, () => string.Format(message, args)); }
        public static void Error(string message, params object[] args) { Trace(LevelEnum.Error, () => string.Format(message, args)); }
        public static void Warn(string message, params object[] args) { Trace(LevelEnum.Warn, () => string.Format(message, args)); }
        public static void Info(string message, params object[] args) { Trace(LevelEnum.Info, () => string.Format(message, args)); }
        public static void Debug(string message, params object[] args) { Trace(LevelEnum.Debug, () => string.Format(message, args)); }


        private static void Trace(LevelEnum level, Func<string> getMessage)
        {
            var stacktrace = new StackTrace(SKIP_FRAMES, true);
            var method = stacktrace.GetFrame(0).GetMethod();

            var assemblyName = method.DeclaringType.Assembly.GetName().Name;
            var className = method.DeclaringType.Name;
            var methodName = method.Name;

            var loggerName = string.Format("{0}.{1}.{2}.{3}", assemblyName, CurrentProcessID, className, methodName);
            var log = GetLogger(loggerName);

            switch (level)
            {
                case LevelEnum.Warn: if (!log.IsWarnEnabled) { return; } break;
                case LevelEnum.Info: if (!log.IsInfoEnabled) { return; } break;
                case LevelEnum.Debug: if (!log.IsDebugEnabled) { return; } break;
            }

            ThreadContext.Properties["PID"] = CurrentProcessID.ToString();
            ThreadContext.Properties["Class"] = className;
            ThreadContext.Properties["Method"] = methodName;

            var message = getMessage();
            // fix encoding with special character (á, ñ, etc)
            message = Encoding.Default.GetString(Encoding.UTF8.GetBytes(message));

            //message = string.Format("[{0}.{1}] {2}", method.DeclaringType.Name, method.Name, message);
            TraceInternal(log, level, message);
        }

        private static void TraceInternal(ILog log, LevelEnum level, string message)
        {
            switch (level)
            {
                case LevelEnum.Fatal: log.Fatal(message); break;
                case LevelEnum.Error: log.Error(message); break;
                case LevelEnum.Warn: log.Warn(message); break;
                case LevelEnum.Info: log.Info(message); break;
                case LevelEnum.Debug: log.Debug(message); break;
                default: throw new NotImplementedException();
            }
        }

        /// <summary>
        /// internal function to determine how to display correctly special character on Log2Console
        /// </summary>
        /// <param name="log"></param>
        /// <param name="level"></param>
        private static void buildSampleMessages(ILog log, LevelEnum level)
        {
            var sb = new StringBuilder();
            const string SPECIAL_CHARS = "áéíóúüñ ÁÉÍÓÚÜÑ";
            // check how to fix encoding with special character (á, ñ, etc)

            TraceInternal(log, level, SPECIAL_CHARS);
            TraceInternal(log, level, Encoding.Default.GetString(Encoding.UTF8.GetBytes("WINNER: " + SPECIAL_CHARS)));

            var encodings = new[] { Encoding.ASCII, Encoding.BigEndianUnicode, Encoding.Default, Encoding.Unicode, Encoding.UTF32, Encoding.UTF7, Encoding.UTF8 };

            foreach (var parent in encodings)
            {
                foreach (var child in encodings)
                {
                    TraceInternal(log, level,
                        string.Format("[{1} to {0}] {2}\r\n", parent.ToString(), child.ToString(), parent.GetString(child.GetBytes(SPECIAL_CHARS))));
                }
            }

            TraceInternal(log, level, SPECIAL_CHARS);
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
            Trace(LevelEnum.Error, () => (extraMessage ?? "") + ex.ToString() + (ex.InnerException == null ? "" : ex.InnerException.ToString()));
            return ex;
        }
        #endregion
    }
}
