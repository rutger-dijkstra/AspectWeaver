using System;
using System.Collections.Generic;
using System.Text;

namespace AspectRetry.Util {
  static class Extensions {
    /// <summary>
    /// Map <c>null</c> to an empty enumerable.
    /// </summary>
    public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> @this) =>
       @this ?? new T[0];
  }
}
