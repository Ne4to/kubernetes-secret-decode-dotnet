using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using k8s.Models;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace KubernetesSecretDecode
{
    internal class Decoder
    {
        private readonly string[] _args;

        private static readonly JsonSerializerSettings SerializationSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            ContractResolver = new ReadOnlyJsonContractResolver(),
            Converters = new  List<JsonConverter>
            {
                new Iso8601TimeSpanConverter()
            }
        };

        private static readonly JsonSerializerSettings DeserializationSettings = new JsonSerializerSettings
        {
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            ContractResolver = new ReadOnlyJsonContractResolver(),
            Converters = new List<JsonConverter>
            {
                new Iso8601TimeSpanConverter()
            }
        };

        public Decoder(string[] args)
        {
            _args = args;
        }

        public string Decode()
        {
            var outputType = GetOutputType();
            if (outputType == null)
            {
                throw new DecoderException("please set -o flag to json or yaml");
            }

            var kubectlOutput = GetKubectlOutput();
            var secret = DeserializeSecret(kubectlOutput, outputType.Value);
            DecodeSecret(secret);
            return SerializeSecret(secret, outputType.Value);
        }

        private static V1Secret DeserializeSecret(string kubectlOutput, OutputType outputType)
        {
            switch (outputType)
            {
                case OutputType.Json:
                    return DeserializeSecretFromJson(kubectlOutput);

                case OutputType.Yaml:
                    return DeserializeSecretFromYaml(kubectlOutput);

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(outputType),
                        outputType,
                        null);
            }
        }

        private static V1Secret DeserializeSecretFromJson(string kubectlOutput)
        {
            return SafeJsonConvert.DeserializeObject<V1Secret>(kubectlOutput, DeserializationSettings);
        }

        private static V1Secret DeserializeSecretFromYaml(string kubectlOutput)
        {
            var deserializer = new DeserializerBuilder().WithNamingConvention(KubernetesNamingConvention.Instance)
               .WithTypeConverter(StringToByteArrayYamlTypeConverter.Instance)
               .WithTypeConverter(DateTimeRfc3339Converter.Instance)
               .Build();

            return deserializer.Deserialize<V1Secret>(kubectlOutput);
        }

        private static string SerializeSecret(V1Secret secret, OutputType outputType)
        {
            switch (outputType)
            {
                case OutputType.Json:
                    return SerializeSecretToJson(secret);

                case OutputType.Yaml:
                    return SerializeSecretToYaml(secret);

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(outputType),
                        outputType,
                        null);
            }
        }

        private static string SerializeSecretToJson(V1Secret secret)
        {
            return SafeJsonConvert.SerializeObject(secret, SerializationSettings);
        }

        private static string SerializeSecretToYaml(V1Secret secret)
        {
            var serializer = new SerializerBuilder()
               .WithNamingConvention(KubernetesNamingConvention.Instance)
               .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
               .WithTypeConverter(DateTimeRfc3339Converter.Instance)
               .WithEventEmitter(nextEmitter => new QuoteSurroundingEventEmitter(nextEmitter))
               .Build();

            return serializer.Serialize(secret);
        }

        private static void DecodeSecret(V1Secret secret)
        {
            secret.StringData ??= new Dictionary<string, string>();

            if (secret.Data != null)
            {
                foreach (var kvp in secret.Data)
                {
                    var key = kvp.Key;
                    var value = kvp.Value;
                    secret.StringData[key] = Encoding.UTF8.GetString(value);
                }
            }

            secret.Data = null;
        }

        private string GetKubectlOutput()
        {
            var processStartInfo = new ProcessStartInfo("kubectl")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            foreach (var arg in _args)
            {
                processStartInfo.ArgumentList.Add(arg);
            }

            var process = Process.Start(processStartInfo);
            var stdout = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return stdout;
        }

        private OutputType? GetOutputType()
        {
            for (var argIndex = 0; argIndex < _args.Length; argIndex++)
            {
                var arg = _args[argIndex];

                switch (arg)
                {
                    case "-oyaml":
                        return OutputType.Yaml;

                    case "-ojson":
                        return OutputType.Json;

                    case "-o" when argIndex != _args.Length - 1:
                    {
                        switch (_args[argIndex + 1])
                        {
                            case "yaml":
                                return OutputType.Yaml;

                            case "json":
                                return OutputType.Json;

                            default:
                                throw new DecoderException($"Output format '{_args[argIndex + 1]}' is not supported");
                        }
                    }
                }
            }

            return null;
        }
    }
}