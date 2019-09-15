using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace AspectWeaver.Util
{
    public class JsonWrapper
    {
        public static JsonWrapper Create(object value) => value is null ? null : new JsonWrapper(value);

        static readonly JsonSerializerSettings _settings = new JsonSerializerSettings()
        {
            ContractResolver = new ExtendableContractResolver()
        };

        readonly Lazy<string> _valueAsString;
        readonly object _value;

        JsonWrapper(object value)
        {
            _value = value;
            _valueAsString = new Lazy<string>(() => Serialize());
        }

        private string Serialize()
        {
            if(_value is string stringValue) { return stringValue; }
            return JsonConvert.SerializeObject(_value,_settings);
        }

        public override string ToString() => _valueAsString.Value;
    }
}
