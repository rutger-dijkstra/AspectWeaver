# AspectWeaver
AspectWeaver is a .NET library for inserting ("weaving") orthogonal concerns, or aspects, into
your code at run time. It is fully `Task`-aware and runs completion and/or error logic on
completion of the `Task` rather than the function that creates the `Task`.

This repository comprises three NuGet packages:
- `AspectWeaver`: the base package that contains the classes for supporting run time aspect insertion. It is
  referenced by the other two packages. It is the only package required to write your own aspects.
- `AspectRetry`: a package for wrapping methods with retry logic. Retry parameters can be customized.
- `AspectLogging`: a package for wrapping methods with logging invocations.

### Installing via NuGet
Depending on which aspects you wish to add to your code, you would run:
```
Install-Package AspectRetry
```
and/or
```
Install-Package AspectLogging
```
If you only want to create your own aspects, it suffices to install `AspectWeaver`:
```
Install-Package AspectWeaver
```

### Setting up Logging and Retry
To add aspect behavior to an object, several extension methods are available. To use them, the object to be wrapped should have an interface
to be supplied to the `AspectWeaver` and it should be typed as this interface when supplying it as an argument.

To add logging behavior, use the `AddLoggingAspect` extension method from the `AspectLogging` namespace, supplying the object typed as its interface,
an `ILogger`, and, optionally, an implementation of the `IAspectLoggingConfiguration` interface. You can control the behavior of the AspectLogger through the
configuration interface. If you use dependency injection through the `IServiceProvider` interface, an overload is supplied that accepts an object conforming to
the `IServiceProvider` interface instead of an `ILogger`.

```
// default logging
IMyInterface wrapped = (myObject as IMyInterface).AddLoggingAspect(logger);
// customized logging
IMyInterface wrapped = (myObject as IMyInterface).AddLoggingAspect(logger, new LoggingAspectConfiguration(...));
```

To add retry behavior, use the `AddRetryAspect` extension method from the `AspectRetry` namespace, supplying an instance of the `IRetryStrategy` interface.
You can customize retry behavior through the `IRetryStrategy` interface.

```
IMyInterface wrapped = (myObject as IMyInterface).AddRetryAspect(new MyRetryStrategy());
```

Any calls on the returned wrapped object will be fed through either the retry aspect or the logging aspect.

Note that it is also possible to wrap an object with both retry and logging. The recommended way of doing that
is by wrapping the `Logging` aspect around the `Retry` aspect, logging once per (possibly retried) method invocation.
When stacking them the other way around, you will also see any retried method invocations in your logging.

### Customizing Logging
By default, the logging aspect logs everything as an informational message. The parameters passed to a method invocation are
logged, as well as the duration of the invocation. You can customize the logging behavior through parameters passed into the
`LoggingAspectConfiguration` constructor.

If you do no want certain method parameters or return values to be logged as plain text, you can decorate these parameters
and return values with the `PrivateAttribute` attribute. This causes them to be logged as `***` rather than the actual value.

```
[return: Private] string GenerateAuthenticationToken([Private] string username, [Private] string password) { ... }
```

In case you do not wish certain properties of complex return values or parameters to be logged, the `PrivateAttribute` can also
be placed on properties of the return type:
```
class Token {
  string UserId { get; }
  [Private] string Password { get; }
}
```

### Customizing Retry
TBD

### Creating your own Aspect
There are two ways to create your own aspect implementation. The first option is to create your own subclass
of `AdviceProvider` and supplying a factory function returning your `AdviceProvider` subclass to the static
`Weaver.Create` method that takes this factory function as an argument.

The `AdviceProvider` class allows you to insert custom logic in keys places around the actual method call:
- Before the actuall call
- On completion of the call, or, in case of an asynchronous method, on completion of the `Task` that is returned by the call
- On an exception raised by the call, or, in case of an asynchronous method, on an exception raised during the `Task` returned by the call

The logging aspect as provided by `AspectLogging` uses this method of surrounding method calls with logging statements.

The other option provided by the `AspectWeaver` library of adding an aspect is by providing an implemention of the
`IMethodInvoker` interface. This interface allows you to customize the way methods are called and requires you
to supply implementations of the following ways of invoking a method:
- `InvokeAction`: synchronous method that returns `void`
- `InvokeFunc<S>`: Synchronous method that returns a result of type `S`
- `InvokeActionAsync`: asynchronous method that returns a `Task`
- `InvokeFuncAsync<S>`: asynchronous method that returns a `Task<S>`

This interface allows you to have control over method invocations and is used by the `AspectRetry` library to provide
retry logic around method invocations.


