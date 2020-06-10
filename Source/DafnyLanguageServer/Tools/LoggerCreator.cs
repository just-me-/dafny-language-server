using DafnyLanguageServer.Commons;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;

namespace DafnyLanguageServer.Tools
{
    /// <summary>
    /// A static class that provides a logger.
    /// The log output is a file and defined by the environmental language server config <c>LanguageServerConfig</c>.
    /// The minimum log level is as well defined by the config.
    /// </summary>
    public static class LoggerCreator
    {
        public static Logger GetLogger()
        {
            var loggerconfig = new LoggerConfiguration();

            switch (LanguageServerConfig.LogLevel)
            {
                case LogLevel.Trace:
                    loggerconfig.MinimumLevel.Verbose();
                    break;
                case LogLevel.Debug:
                    loggerconfig.MinimumLevel.Debug();
                    break;
                case LogLevel.Information:
                    loggerconfig.MinimumLevel.Information();
                    break;
                case LogLevel.Warning:
                    loggerconfig.MinimumLevel.Warning();
                    break;
                case LogLevel.Critical:
                    loggerconfig.MinimumLevel.Fatal();
                    break;

                default:
                    loggerconfig.MinimumLevel.Error();
                    break;
            }

            var log = loggerconfig
                .Enrich.FromLogContext()
                .WriteTo.File(LanguageServerConfig.LogFile, fileSizeLimitBytes: 1024 * 1024)
                .CreateLogger();

            return log;
        }
    }
}