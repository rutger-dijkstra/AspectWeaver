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

### Setting Up Logging and Retry
To add aspect behavior to an object, several extension methods are available. To use them, the object to be wrapped should have an interface
to be supplied to the `AspectWeaver` and it should be typed as this interface when supplying it as an argument.

To add logging behavior, use the `AddLoggingAspect` extension method from the `AspectLogging` namespace, supplying the object typed as its interface,
an `ILogger`, and an implementation of the `IAspectLoggingConfiguration` interface. You can control the behavior of the AspectLogger through the
configuration interface. If you use dependency injection through the `IServiceProvider` interface, an overload is supplied that accepts an object conforming to
the `IServiceProvider` interface instead of an `ILogger`.

```
(myObject as IMyInterface).AddLoggingAspect(logger, new MyAspectLoggingConfiguration());
```

To add retry behavior, use the `AddRetryAspect` extension method from the `AspectRetry` namespace, supplying an instance of the `IRetryStrategy` interface.
You can customize retry behavior through the `IRetryStrategy` interface.

```
(myObject as IMyInterface).AddRetryAspect(new MyRetryStrategy());
```

### Creating Your Own Aspect
TBD