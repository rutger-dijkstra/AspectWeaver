using Microsoft.Extensions.Logging;
using System;

namespace Test.Logging {
  class TestLoggerProvider: ILoggerProvider {

    class UnitTestLogger: ILogger {

      public UnitTestLogger(string categoryName, Action<TestLogEntry> appendToLog) {
        _categoryName = categoryName;
        _appendToLog = appendToLog;
      }

      private readonly string _categoryName;
      private readonly Action<TestLogEntry> _appendToLog;

      public IDisposable BeginScope<TState>(TState state) => null;
      public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;
      public void Log<TState>(
          LogLevel logLevel, EventId eventId, TState state,
          Exception exception, Func<TState, Exception, string> formatter
      ) {
        _appendToLog(new TestLogEntry {
          LogLevel = logLevel,
          EventId = eventId,
          Exception = exception,
          Message = formatter(state, exception)

        });
      }
    }

    private readonly Action<TestLogEntry> _appendToLog;

    public TestLoggerProvider(Action<TestLogEntry> appendToLog) {
      _appendToLog = appendToLog;
    }

    public ILogger CreateLogger(string categoryName) =>
        new UnitTestLogger(categoryName, _appendToLog);

    public void Dispose() { }
  }

  /// <summary>
  /// <see cref="ILoggerFactory"/> extension.
  /// </summary>
  public static class TestLoggerProviderExtensions {
    /// <summary>
    /// Add a test logger provider to an <see cref="ILoggerFactory"/>
    /// </summary>
    public static ILoggerFactory AddTestLogger(this ILoggerFactory factory, Action<TestLogEntry> appendToLog) {
      factory.AddProvider(new TestLoggerProvider(appendToLog));
      return factory;
    }
  }
}
