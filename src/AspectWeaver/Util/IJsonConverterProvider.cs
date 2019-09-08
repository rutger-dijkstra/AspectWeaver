using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace AspectWeaver.Util
{
    interface IJsonConverterProvider
    {
       JsonConverter Converter { get; } 
    }
}