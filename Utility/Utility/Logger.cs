using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;

namespace BimGen.PerpectoPlacerOne.Utility
{
    public class Logger
    {
        private static Logger _instance;
        private readonly NLog.Logger _logger;

        private Logger()
        {
            try
            {
                var assembly = Assembly.GetCallingAssembly();
                var name = assembly.GetName().Name;
                var directory = Path.GetDirectoryName(assembly.CodeBase.Replace(@"file:///", string.Empty));

                if (!Directory.Exists(directory))
                    throw new DirectoryNotFoundException(directory);

                var logDirectory = Path.Combine(directory, "Logs/");

                var config = new LoggingConfiguration();

                var allFileTarget = new FileTarget("allFileTarget")
                {
                    FileName = logDirectory + "Debug.log",
                    Layout = "${longdate} ${uppercase:${level}} ${message}",
                    Encoding = Encoding.Unicode,
                    ArchiveFileName = logDirectory + "Debug.${shortdate}.{#}.log",
                    ArchiveAboveSize = 16_777_216,
                    ArchiveEvery = FileArchivePeriod.Day,
                    ArchiveNumbering = ArchiveNumberingMode.Rolling,
                    MaxArchiveFiles = 20
                };
                var errorFileTarget = new FileTarget("errorFileTarget")
                {
                    FileName = logDirectory + "Error.log",
                    Layout = "${longdate} ${uppercase:${level}} ${message}",
                    Encoding = Encoding.Unicode,
                    ArchiveFileName = logDirectory + "Error.${shortdate}.{#}.log",
                    ArchiveAboveSize = 16_777_216,
                    ArchiveEvery = FileArchivePeriod.Day,
                    ArchiveNumbering = ArchiveNumberingMode.Rolling,
                    MaxArchiveFiles = 20
                };
                config.AddTarget(allFileTarget);
                config.AddTarget(errorFileTarget);

                config.AddRuleForAllLevels(allFileTarget);
                config.AddRule(LogLevel.Error, LogLevel.Fatal, errorFileTarget);

                LogManager.Configuration = config;
                _logger = LogManager.GetLogger($"{name}.Logger");
            }
            catch (Exception exception)
            {
                MessageBox.Show($"{exception.Message}\n{exception.StackTrace}");
            }
        }

        private static Logger Instance => _instance?._logger.Name == Assembly.GetCallingAssembly().GetName().Name
            ? _instance
            : _instance = new Logger();

        public static NLog.Logger Log => Instance._logger;

        public static void Debug(string message)
        {
            Log.Debug(message);
        }

        public static void Debug(Exception exception)
        {
            Log.Debug(exception);
        }

        public static void Error(string message)
        {
            Log.Error(message);
        }

        public static void Error(Exception exception)
        {
            Log.Error(exception);
        }

        public static void Info(string message)
        {
            Log.Info(message);
        }

        public static void Info(Exception exception)
        {
            Log.Info(exception);
        }

        public static void Warn(string message)
        {
            Log.Warn(message);
        }

        public static void Warn(Exception exception)
        {
            Log.Warn(exception);
        }

        public static void LogMethodStart(StackTrace stackTrace)
        {
            LogMethodStatus(stackTrace, "START");
        }

        public static void LogMethodDone(StackTrace stackTrace)
        {
            LogMethodStatus(stackTrace, "DONE");
        }

        public static void LogMethodCancel(StackTrace stackTrace)
        {
            LogMethodStatus(stackTrace, "CANCEL");
        }

        public static void LogMethodStatus(StackTrace stackTrace, string status)
        {
            if (stackTrace != null)
                _instance._logger.Info($"{stackTrace.GetMethodName()} - '{status}'");
        }
    }
}