using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;

namespace AspectLogging.Util
{
    public class ExtendableContractResolver: DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var jsonProperty = base.CreateProperty(member, memberSerialization);
            var converterProvider = member.GetCustomAttributes()
                .OfType<IJsonConverterProvider>().FirstOrDefault();
            if( converterProvider is null ) { return jsonProperty; }
            jsonProperty.Converter = converterProvider.Converter;
            return jsonProperty;
        }
    }
}