using System;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace AspectLogging {
  public interface IAspectLoggingConfiguration {
    bool? IncludeException { get; }
    LogLevel LogLevelBefore { get; }
    LogLevel LogLevelOnCompletion { get; }
    LogLevel LogLevelOnError { get; }
    bool IncludeInherited { get; }
  }
}
