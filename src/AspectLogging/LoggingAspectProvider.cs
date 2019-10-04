using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using AspectWeaver;
using AspectLogging.Util;
using System.Collections.Generic;

namespace AspectLogging {
  /// <summary>
  /// An <see cref="InvocationInterceptor"/> for use with an <see cref="Weaver"/> 
  /// that wrap logging around a method call.
  /// </summary>
  class LoggingAspectProvider: AdviceProvider {
    private static readonly Action<ILogger, LogLevel, string, object[], Exception> _called = LogMessageBuilder.Define<string, object[]>(
        new EventId(1, "Call"), "Calling {Method:l}({Arguments:l})."
    );

    private static readonly Action<ILogger, LogLevel, string, long, Exception> _methodCompleted = LogMessageBuilder.Define<string, long>(
        new EventId(2, "Completed"), "{Method:l} completed in duration {ElapsedMilliseconds}ms."
    );

    private static readonly Action<ILogger, LogLevel, string, object, long, Exception> _functionCompleted = LogMessageBuilder.Define<string, object, long>(
        new EventId(2, "Completed"), "{Method:l} returned {Result:l} in duration {ElapsedMilliseconds}ms."
    );

    private static readonly Action<ILogger, LogLevel, string, string, string, object[], Exception> _failed = LogMessageBuilder.Define<string, string, string, object[]>(
        new EventId(3, "Failure"), "Exception {ExceptionType:l}: {Message:l} from {Method:l}({Arguments:l})."
    );

    readonly ILogger _logger;
    readonly IAspectLoggingConfiguration _config;
    readonly MethodInfo _invokedMethod;
    object[] _args;
    IDisposable _scope;
    Stopwatch _timer;

    /// <summary>
    /// Constructs a <see cref="LoggingAspectProvider"/>.
    /// </summary>
    /// <param name="invokedMethod">The method that is being invoked.</param>
    /// <param name="logger">The loger to use.</param>
    /// <param name="valueWrapper">An optional function to apply to arguments an results before they are passes to the logger.</param>
    public LoggingAspectProvider(MethodInfo invokedMethod, ILogger logger, IAspectLoggingConfiguration config) {
      _logger = logger;
      _config = config;
      _invokedMethod = invokedMethod;
      _scope = _logger.BeginScope(new Dictionary<string, string>() {
        { "MethodName", invokedMethod.DeclaringType.Name + "." + invokedMethod.Name }
      });
      _timer = new Stopwatch();
    }

    public object Wrap(ParameterInfo info, object value) {
      if( value is null ) return null;
      var privatestringAttr = info.GetCustomAttribute<PrivateAttribute>();
      if( privatestringAttr is null ) { return JsonWrapper.Create(value); }
      return "***";
    }

    /// <inheritdoc />
    public override void BeforeCall(object[] args) {
      var parameters = _invokedMethod.GetParameters();
      _called(
        _logger, _config.LogLevelBefore, _invokedMethod.Name,
        _args = args.Select((o, i) => Wrap(parameters[i], o)).ToArray(), null
      );
      _timer.Start();
    }

    /// <inheritdoc />
    public override void AfterCompletion() {
      _timer.Stop();
      _methodCompleted(_logger, _config.LogLevelOnCompletion, _invokedMethod.Name, _timer.ElapsedMilliseconds, null);
    }

    /// <inheritdoc />
    public override void AfterCompletion(object result) {
      _timer.Stop();
      var returnParam = _invokedMethod.ReturnParameter;
      _functionCompleted(
        _logger, _config.LogLevelOnCompletion, _invokedMethod.Name,
        Wrap(returnParam, result), _timer.ElapsedMilliseconds, null
      );
    }

    static bool IsExceptionLevel(LogLevel level) =>
        LogLevel.Warning <= level;

    /// <inheritdoc />
    public override void OnError(Exception e) {
      var logLevelOnError = _config.LogLevelOnError;
      _failed(
        _logger, logLevelOnError, e.GetType().Name, e.Message, _invokedMethod.Name, _args,
        _config.IncludeException ?? IsExceptionLevel(logLevelOnError) ? e : null
      );
    }

    /// <inheritdoc />
    public override void Dispose() {
      _scope?.Dispose();
      _scope = null;
    }
  }
}
