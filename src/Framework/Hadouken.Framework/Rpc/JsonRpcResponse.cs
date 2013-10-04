﻿using System;
using Hadouken.Framework.SemVer;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Hadouken.Framework.Rpc
{
    public class SemanticVersionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof (SemanticVersion);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            if (reader.TokenType != JsonToken.String)
                throw new Exception(
                    String.Format("Unexpected token or value when parsing version. Token: {0}, Value: {1}",
                        reader.TokenType, reader.Value));
            try
            {
                var semver = new SemanticVersion((string) reader.Value);
                return semver;
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Error parsing SemanticVersion string {0}", reader.Value), e);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else if (value is SemanticVersion)
            {
                writer.WriteValue(value.ToString());
            }
            else
            {
                throw new Exception("Expected SemanticVersion object value.");
            }
        }
    }

    public sealed class JsonRpcResponse
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings();

        static JsonRpcResponse()
        {
            SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            SerializerSettings.Converters.Add(new SemanticVersionConverter());
            SerializerSettings.Converters.Add(new StringEnumConverter());
            SerializerSettings.Converters.Add(new VersionConverter());
        }

        public JsonRpcResponse()
        {
            Protocol = "2.0";
        }

        [JsonProperty("id", Required = Required.Always)]
        public object Id { get; set; }

        [JsonProperty("jsonrpc", Required = Required.Always)]
        public string Protocol { get; set; }

        [JsonProperty("result", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public object Result { get; set; }

        [JsonProperty("error", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
        public RpcError Error { get; set; }

        public static bool TryParse(string json, out JsonRpcResponse response, out Exception exception)
        {
            response = null;
            exception = null;

            try
            {
                response = JsonConvert.DeserializeObject<JsonRpcResponse>(json);
                return true;
            }
            catch (Exception e)
            {
                exception = e;
                return false;
            }
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, SerializerSettings);
        }
    }

    public abstract class RpcError
    {
        protected RpcError(int errorCode, string message)
        {
            ErrorCode = errorCode;
            Message = message;
        }

        [JsonProperty("code")]
        public int ErrorCode { get; private set; }

        [JsonProperty("message")]
        public string Message { get; private set; }

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public object Data { get; set; }
    }

    public sealed class RpcParseError : RpcError
    {
        public RpcParseError()
            : base(-32700, "Invalid JSON was received by the server. An error occurred on the server while parsing the JSON text.") {}
    }

    public sealed class InvalidRequestError : RpcError
    {
        public InvalidRequestError()
            : base(-32600, "The JSON sent is not a valid Request object.") {}
    }

    public sealed class MethodNotFoundError : RpcError
    {
        public MethodNotFoundError()
            : base(-32601, "The method does not exist / is not available.") {}
    }

    public sealed class InvalidParamsError : RpcError
    {
        public InvalidParamsError()
            : base(-32602, "Invalid method parameter(s).") {}
    }

    public sealed class InternalRpcError : RpcError
    {
        public InternalRpcError(Exception exception = null)
            : base(-32603, "Internal JSON-RPC error.")
        {
            if (exception != null)
                Data = exception.ToString();
        }
    }
}