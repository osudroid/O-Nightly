using NLog;
using NLog.Config;
using NLog.Filters;
using NLog.Targets;
using NLog.Targets.Wrappers;
using Rimu.Repository.Printer.Adapter;

namespace Rimu.Repository.Printer.Domain;

public class PrinterSetup: IPrinterSetup {
    private static volatile bool _initialized = false;
    private static readonly object LockObject = new ();

    public PrinterSetup() {
      
    }

    private static bool LoggerNameIsMicrosoft(string loggerName) 
        => loggerName.StartsWith("Microsoft.", StringComparison.Ordinal);
    
    public void Run() {
        if (IsInitialized()) {
            return;
        }
        
        lock (LockObject) {
            if (IsInitialized()) {
                return;
            }
            Initialize();
            _initialized = true;
        }
    }
    
    public bool IsInitialized() => _initialized;

    private void Initialize() {
          var loggingConfiguration = new LoggingConfiguration();
        
        
        var coloredConsoleTarget = new ColoredConsoleTarget {
            AutoFlush = true,
            UseDefaultRowHighlightingRules = true
        };
        
        var cassandraTarget = new PostgresTarget();

        var bufferingTargetWrapper = new BufferingTargetWrapper() {
            BufferSize = 2,
            WrappedTarget = cassandraTarget,
            Name = "Postgres",
            FlushTimeout = 10000,
            SlidingTimeout = true,
        };
        
        loggingConfiguration.AddTarget("console", coloredConsoleTarget);
        // loggingConfiguration.AddTarget("db", bufferingTargetWrapper);
        
        var filter = new WhenMethodFilter(static info => {
            var result = info.Level.Ordinal switch {
                // Trace;
                // Debug;
                // Info;
                0 or 1 or 2 => LoggerNameIsMicrosoft(info.LoggerName ?? "")
                    ? FilterResult.IgnoreFinal
                    : FilterResult.Log,

                // Warn;
                // Error;
                // Fatal;
                3 or 4 or 5 => FilterResult.Log,
                // Off;
                6 => FilterResult.IgnoreFinal,
                _ => FilterResult.IgnoreFinal
            };

            return result;
        });
        
        var ruleColoredConsoleTarget = new LoggingRule("*", LogLevel.Trace, LogLevel.Fatal, coloredConsoleTarget) { Filters = { filter } };
        // var ruleCassandraTarget = new LoggingRule("*", LogLevel.Trace, LogLevel.Fatal, cassandraTarget) { Filters = { filter } };
        // var ruleBufferingTargetWrapper = new LoggingRule("*", LogLevel.Trace, LogLevel.Fatal, bufferingTargetWrapper) { Filters = { filter } };
        
        loggingConfiguration.AddRule(ruleColoredConsoleTarget);
        // loggingConfiguration.AddRule(ruleCassandraTarget);
        // loggingConfiguration.AddRule(ruleBufferingTargetWrapper);
        
        var logger =  NLog.LogManager
            .Setup()
            .LoadConfiguration(loggingConfiguration)
            .GetCurrentClassLogger();
        
        return;
    }
}