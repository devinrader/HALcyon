using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;
using Westwind.Utilities;

namespace HALcyon
{
    public class JsonResourceConverter : JsonConverter
    {        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            dynamic representation = new Expando();
            representation["_links"] = new Expando();

            if (value is IEnumerable)
            {
                representation["_links"][value.GetType().GenericTypeArguments[0].Name.ToLower()] = BuildRepresentationList(value);
            }
            else
            {
                representation = BuildRepresentation(value);
            }

            // use our own instance of JSON serialize so we 
            // don't serialize using our own converter
            JsonSerializer internalSerializer = new JsonSerializer();
            internalSerializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            internalSerializer.Serialize(writer, representation);
        }

        private string BuildUri(string template, object instance)
        {
            string href = template;

            MatchCollection matches = Regex.Matches(template, @"\{([^}]+)\}");

            foreach (Match match in matches)
            {
                var tokens = match.Groups[1].Value.Split('.');

                href = href.Replace(match.ToString(), LocatePropertyValue(tokens, instance).ToString());
            }

            return href;
        }

        private object LocatePropertyValue(string[] tokens, object instance)
        {
            for (int i = 0; i < tokens.Length; i++)
            {
                var prop = instance.GetType().GetProperty(tokens[i], BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance );

                if (prop == null)
                    throw new Exception(string.Format("Link template property matching the name '{0}' could not found on the object type '{1}'.", tokens[i], instance.GetType().ToString()));

                instance = prop.GetValue(instance);
            }

            return instance;
        }

        private IEnumerable BuildRepresentationList(object value)
        {
            List<object> links = new List<object>();
            var type = value.GetType();

            if (type.IsGenericType)
            {
                var genericType = type.GenericTypeArguments[0];
                var linkAttr = Attribute.GetCustomAttribute(genericType, typeof(LinkAttribute)) as LinkAttribute;

                if (linkAttr == null)
                {
                    throw new Exception(string.Format("Link template property could not found on the object type '{0}'.", genericType.ToString()));
                }

                string template = linkAttr.Template.ToLower();

                var enumerator = ((IEnumerable)value).GetEnumerator();
                while (enumerator.MoveNext())
                {
                    string href = BuildUri(template, enumerator.Current);
                    links.Add(new { href = href });
                }
            }
            return links;
        }

        private Expando BuildRepresentation(object value)
        {
            dynamic representation = new Expando();
            representation["_links"] = new Expando();

            var type = value.GetType();
            var props = type.GetProperties();

            foreach (var prop in props)
            {
                //if this property is marked as the Key, ignore it
                if (Attribute.IsDefined(prop, typeof(KeyAttribute)) || Attribute.IsDefined(prop, typeof(JsonIgnoreAttribute)))
                    continue;

                var propValue = prop.GetValue(value);

                if (typeof(ICollection).IsAssignableFrom(prop.PropertyType))
                {
                    if (propValue == null) {
                        representation[prop.Name.ToLower()] = null; //if the collection property is null, just write out a null
                    }
                    else {
                        representation["_links"][prop.Name.ToLower()] = BuildRepresentationList(propValue);
                    }
                    
                    continue;
                }

                var linkAttr = Attribute.GetCustomAttribute(prop, typeof(LinkAttribute)) as LinkAttribute;
                var embedAttr = Attribute.GetCustomAttribute(prop, typeof(EmbeddedAttribute)) as EmbeddedAttribute;

                if (propValue != null && linkAttr != null)
                {
                    string subHref = linkAttr.Template.ToLower();
                    string href = BuildUri(subHref, propValue);

                    representation["_links"][linkAttr.Name] = new { href = href };
                }
                else if (propValue != null && embedAttr != null)
                {
                    representation["_embedded"][embedAttr.Name] = BuildRepresentation(propValue);
                }
                else
                {
                    representation[prop.Name.ToLower()] = propValue;
                }
            }

            var selfAttr = Attribute.GetCustomAttribute(type, typeof(LinkAttribute)) as LinkAttribute;
            if (selfAttr != null)
            {
                string selfHref = selfAttr.Template.ToLower();
                string href = BuildUri(selfHref, value);

                representation["_links"][selfAttr.Name] = new { href = href };
            }
            return representation;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value;
        }

        public override bool CanConvert(Type objectType)
        {
            return true; //IsResource(objectType) && !IsResourceList(objectType);
        }

        //static bool IsResourceList(Type objectType)
        //{
        //    return typeof(IRepresentationList).IsAssignableFrom(objectType);
        //}

        //static bool IsResource(Type objectType)
        //{
        //    return typeof(Representation).IsAssignableFrom(objectType);
        //}
    }
}