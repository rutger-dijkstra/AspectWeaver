using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace AspectLogging.Util {
  interface IJsonConverterProvider {
    JsonConverter Converter { get; }
  }
}
