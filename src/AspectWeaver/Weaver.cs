using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using AspectWeaver.Util;

namespace AspectWeaver {
  /// <summary>
  /// Weaves the <see cref="Advice"/> obtained from an <see cref="AdviceProvider"/> into
  /// method calls of an interface implementation.
  /// </summary>
  public class Weaver {

    /// <summary>
    /// Creates an implementation of the specified interface <typeparamref name="T"/> from an
    /// implementation <paramref name="inner"/> with the logic of an <see cref="AdviceProvider"/>
    /// woven around each method call.
    /// </summary>
    /// <typeparam name="T">The interface type to decorate.</typeparam>
    /// <param name="inner">An implementation of interface <typeparamref name="T"/>.</param>
    /// <param name="interceptorFactory">A factory method for <see cref="AdviceProvider"/>s that implement 
    /// an orthogonal concern.</param>
    public static T Create<T>(T inner, Func<MethodInfo, AdviceProvider> interceptorFactory) where T : class {
      _ = interceptorFactory.NotNull(nameof(interceptorFactory));
      return Wrap(inner, innerInvoker => new AdviceWeavingInvoker(innerInvoker, interceptorFactory));
    }

    /// <summary>
    /// Creates an implementation of the specified interface <typeparamref name="T"/> from an
    /// implementation <paramref name="inner"/> with the logic provided by the result of <paramref name="invocationWrapper"/>
    /// wrapped around each method call.
    /// </summary>
    /// <typeparam name="T">The interface type to decorate.</typeparam>
    /// <param name="inner">An implementation of interface <typeparamref name="T"/>.</param>
    /// <param name="invocationWrapper">A factory method for a <see cref="IMethodInvoker"/> on the basis of an inner <see cref="IMethodInvoker"/>.</param>
    public static T Wrap<T>(T inner, Func<IMethodInvoker, IMethodInvoker> invocationWrapper) where T : class {
      _ = inner.NotNull();
      var proxy = DispatchProxy.Create<T, DiscriminatingDispatchProxy>();
      ((DiscriminatingDispatchProxy)(object)proxy).MethodInvoker = 
        invocationWrapper(inner is DiscriminatingDispatchProxy p ? p.MethodInvoker : new ReflectionInvoker(inner));
      return proxy;
    }

    static MethodInfo _createT = typeof(Weaver)
        .GetMethods(BindingFlags.Static | BindingFlags.Public)
        .Single(m => m.IsGenericMethodDefinition && m.Name == "Create");

    static ConcurrentDictionary<Type, Func<object, Func<MethodInfo, AdviceProvider>, object>> _creatorsByType =
      new ConcurrentDictionary<Type, Func<object, Func<MethodInfo, AdviceProvider>, object>>();

    /// <summary>
    /// Creates an implementation of the specified interface <paramref name="interfaceType"/> from an
    /// implementation <paramref name="inner"/> with the logic of an <see cref="AdviceProvider"/>
    /// woven around each method call.
    /// </summary>
    /// <param name="interfaceType">The interface type to decorate.</param>
    /// <param name="inner">An implementation of interface <paramref name="interfaceType"/>.</param>
    /// <param name="interceptorFactory">A factory method for <see cref="AdviceProvider"/>s that implement 
    /// an orthogonal concern.</param>
    public static object Create(Type interfaceType, object inner, Func<MethodInfo, AdviceProvider> interceptorFactory) {
      if( !interfaceType.IsInterface ) {
        throw new InvalidOperationException($"Type {interfaceType.Name} is not an interface type.");
      }
      return _creatorsByType.GetOrAdd(interfaceType, t => {
        var objectParam = Expression.Parameter(typeof(object), "inner");
        var factoryParam = Expression.Parameter(typeof(Func<MethodInfo, AdviceProvider>), "interceptorFactory");
        return Expression.Lambda<Func<object, Func<MethodInfo, AdviceProvider>, object>>(
            Expression.Call(
                _createT.MakeGenericMethod(interfaceType),
                Expression.Convert(objectParam, interfaceType),
                factoryParam
            ),
            objectParam, factoryParam
        ).Compile();
      })(inner, interceptorFactory);
    }
  }
}

