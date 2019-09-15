using System;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace AspectWeaver {
  public interface ILoggingAspectConfiguration {
    bool? IncludeException { get; }
    LogLevel LogLevelBefore { get; }
    LogLevel LogLevelOnCompletion { get; }
    LogLevel LogLevelOnError { get; }
    bool IncludeInherited { get; }
  }
}
