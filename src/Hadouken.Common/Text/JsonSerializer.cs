﻿using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Hadouken.Common.Text
{
    public class JsonSerializer : IJsonSerializer
    {
        private readonly Newtonsoft.Json.JsonSerializer _serializer;

        public JsonSerializer()
        {
            _serializer = new Newtonsoft.Json.JsonSerializer();
            _serializer.Converters.Add(new StringEnumConverter());
            _serializer.Formatting = Formatting.Indented;
        }

        public object DeserializeObject(string json, Type type)
        {
            using (var reader = new StringReader(json))
            {
                return _serializer.Deserialize(reader, type);                
            }
        }

        public T DeserializeObject<T>(string json)
        {
            using (var reader = new StringReader(json))
            using (var jsonReader = new JsonTextReader(reader))
            {
                return _serializer.Deserialize<T>(jsonReader);
            }
        }

        public string SerializeObject(object obj)
        {
            var sb = new StringBuilder();

            using (var writer = new StringWriter(sb))
            {
                _serializer.Serialize(writer, obj);
            }

            return sb.ToString();
        }
    }
}