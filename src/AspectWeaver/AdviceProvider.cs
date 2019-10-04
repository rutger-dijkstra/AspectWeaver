using System;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectWeaver {

  /// <summary>
  /// This is the base class for interceptors that are used by the <see cref="Weaver"/>
  /// to inject orthogonal concerns into method invocations. Implementations exist for implementing 
  /// logging. 
  /// 
  /// Create you own subclass to realise your own implementation of an orthogonal concern.
  /// One instance of your interceptor will be created for each method invocation and it will
  /// be disposed upon completion (whether successful or exceptional). 
  /// </summary>
  public abstract class AdviceProvider: IDisposable {

    /// <summary>
    /// This method will be called before the invocation of the intercepted method. 
    /// The inherited implementation does nothing. 
    /// </summary>
    /// <param name="args">The arguments passed to the method invocation.</param>
    public virtual void BeforeCall(object[] args) { }

    /// <summary>
    /// For synchronous methods without result (return type <see cref="void"/>), this
    /// method is called upon successful completion. For asynchronous methods without result (return type
    /// <see cref="Task"/>), this method called upon successful completion <em>of the task</em>.
    ///
    /// The inherited implementation does nothing. 
    /// </summary>
    public virtual void AfterCompletion() { }

    /// <summary>
    /// For synchronous methods returning a result (not a <see cref="Task"/> or subclass thereof), this
    /// method is called upon successful completion. For asynchronous method returning a result
    /// (return type <see cref="Task{TResult}"/>), this method is called upon successful completion <em>of the task</em>.
    ///
    /// The inherited implementation does nothing. 
    /// </summary>
    /// <param name="result">The result of the call or task respectively.</param>
    public virtual void AfterCompletion(object result) { }

    /// <summary>
    /// This method is called when the intercepted method or the resulting <see cref="Task"/> encounter an exception. 
    ///
    /// It is not possible to handle the exceptions in the method. At most, one can raisie another one, thereby
    /// replacing the original exception with one's own.
    /// 
    /// The inherited implementation does nothing. 
    /// </summary>
    /// <remarks>Exceptions raised from any of the
    /// other methods of this interceptor terminate the invocation without 
    /// calling <see cref="OnError(Exception)"/>.</remarks>
    public virtual void OnError(Exception e) { }

    /// <summary>
    /// Called once the entire method invocation is finished.
    /// </summary>
    public virtual void Dispose() { }
  }
}
