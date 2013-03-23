using System;
using System.Collections;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace HALcyon
{
    public class JsonHalMediaTypeFormatter : JsonMediaTypeFormatter
    {
        readonly JsonResourceConverter resourceConverter = new JsonResourceConverter();

        public JsonHalMediaTypeFormatter()
        {
            this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/hal+json"));
            this.SerializerSettings.Converters.Add(resourceConverter);
        }

        public override bool CanReadType(Type type)
        {
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            return (!(typeof(IDictionary).IsAssignableFrom(type)));
        }
    }
}