using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using AspectWeaver.Util;

namespace AspectWeaver {
  /// <summary>
  /// Atribute to mark a string property or a parameter/return value in an interface as private
  /// in order to replace it in the <see cref="LoggingInterceptor"/> by '***'.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
  public class PrivateAttribute: Attribute, IJsonConverterProvider {
    class PrivateStringConverter: JsonConverter {
      public override bool CanConvert(Type objectType) => objectType == typeof(string);

      public override object ReadJson(
          JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer
      ) {
        if( reader.TokenType == JsonToken.Null ) { return null; }

        if( reader.TokenType == JsonToken.String ) { return (string)reader.Value; }

        throw new JsonSerializationException(
            $"Unexpected token of type {reader.TokenType} when expecting a string. " +
            $"Value: {reader.Value}"
        );
      }

      public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
        if( value == null ) {
          writer.WriteNull();
        } else if( value is string ) {
          writer.WriteValue("***");
        } else {
          throw new JsonSerializationException("Expected String object value");
        }
      }
    }

    JsonConverter IJsonConverterProvider.Converter { get; } = new PrivateStringConverter();
  }
}
