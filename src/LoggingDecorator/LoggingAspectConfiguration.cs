using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using AspectWeaver.Util;

namespace AspectWeaver {
  /// <summary>
  /// Configuration for the <see cref="LoggingInterceptor"/>
  /// </summary>
  public class LoggingAspectConfiguration: ILoggingAspectConfiguration {
    /// <summary>
    /// The <see cref="LogLevel"/> to use when nothing else is specified.
    /// </summary>
    public static LogLevel DefaultLevel { get; } = LogLevel.Information;

    /// <summary>
    /// Create an instance
    /// </summary>
    /// <param name="logLevelBefore">The <see cref="LogLevel"/> to use in <see cref="LoggingInterceptor.BeforeCall(object[])"/>. 
    /// Defaults to <see cref="LoggingAspectConfiguration.DefaultLevel"/>.</param>
    /// <param name="logLevelOnCompletion">The <see cref="LogLevel"/> to use in <see cref="LoggingInterceptor.AfterCompletion"/> and <see cref="LoggingInterceptor.AfterCompletion(object)"/>. 
    /// Defaults to <see cref="LoggingAspectConfiguration.DefaultLevel"/>.</param>
    /// <param name="logLevelOnError">The <see cref="LogLevel"/> to use in <see cref="LoggingInterceptor.OnError(Exception)"/>. 
    /// Defaults to <see cref="LoggingAspectConfiguration.DefaultLevel"/>.</param>
    /// <param name="includeExceptions">Whether to include the exception in the OnError logging. The default is to
    /// include the exception if <see cref="LoggingAspectConfiguration.LogLevelOnError"/> is one of <see cref="LogLevel.Warning"/>, <see cref="LogLevel.Error"/>,
    /// or <see cref="LogLevel.Critical"/>, and the exclude the exception otherwise.</param>
    /// <param name="valueWrapper">Function to wrap a custom <see cref="object.ToString()"/> implementation around values. Defaults to <see cref="JsonWrapper.Create(object)"/></param>
    public LoggingAspectConfiguration(
        LogLevel? logLevelBefore = null,
        LogLevel? logLevelOnCompletion = null,
        LogLevel? logLevelOnError = null,
        bool? includeExceptions = null,
        Func<object, object> valueWrapper = null,
        bool includeInherited = false
     ) {
      LogLevelBefore = logLevelBefore ?? DefaultLevel;
      LogLevelOnCompletion = logLevelOnCompletion ?? DefaultLevel;
      LogLevelOnError = logLevelOnError ?? DefaultLevel;
      IncludeException = includeExceptions;
      ValueWrapper = valueWrapper ?? JsonWrapper.Create;
      IncludeInherited = includeInherited;
    }

    /// <summary>
    /// The <see cref="LogLevel"/> to use in <see cref="LoggingInterceptor.BeforeCall(object[])"/>
    /// </summary>
    public LogLevel LogLevelBefore { get; } = DefaultLevel;

    /// <summary>
    /// The <see cref="LogLevel"/> to use in <see cref="LoggingInterceptor.AfterCompletion"/> and <see cref="LoggingInterceptor.AfterCompletion(object)"/>
    /// </summary>
    public LogLevel LogLevelOnCompletion { get; } = DefaultLevel;

    /// <summary>
    /// The <see cref="LogLevel"/> to use in <see cref="LoggingInterceptor.OnError(Exception)"/>
    /// </summary>
    public LogLevel LogLevelOnError { get; } = DefaultLevel;

    /// <summary>
    /// Whether to include the exception in the OnError logging.
    /// </summary>
    public bool? IncludeException { get; }

    /// <summary>
    /// A function to wrap a custom  <see cref="object.ToString()"/> implementation around values.
    /// </summary>
    public Func<object, object> ValueWrapper { get; }

    /// <summary>
    /// Whether to log invocations of methods from inherited interfaces.
    /// </summary>
    public bool IncludeInherited { get; }
  }
}
