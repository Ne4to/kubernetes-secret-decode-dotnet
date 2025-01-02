using k8s;
using k8s.Models;

using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace KubernetesSecretDecode;

internal class Decoder
{
  private readonly string[] _args;

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
    return outputType switch
    {
      OutputType.Json => DeserializeSecretFromJson(kubectlOutput),
      OutputType.Yaml => DeserializeSecretFromYaml(kubectlOutput),
      _ => throw new ArgumentOutOfRangeException(
                  nameof(outputType),
                  outputType,
                  null),
    };
  }

  private static V1Secret DeserializeSecretFromJson(string kubectlOutput)
  {
    return KubernetesJson.Deserialize<V1Secret>(kubectlOutput);
  }

  private static V1Secret DeserializeSecretFromYaml(string kubectlOutput)
  {
    return KubernetesYaml.Deserialize<V1Secret>(kubectlOutput);
  }

  private static string SerializeSecret(V1Secret secret, OutputType outputType)
  {
    return outputType switch
    {
      OutputType.Json => SerializeSecretToJson(secret),
      OutputType.Yaml => SerializeSecretToYaml(secret),
      _ => throw new ArgumentOutOfRangeException(
                  nameof(outputType),
                  outputType,
                  null),
    };
  }

  private static string SerializeSecretToJson(V1Secret secret)
  {
    return KubernetesJson.Serialize(secret);
  }

  private static string SerializeSecretToYaml(V1Secret secret)
  {
    return KubernetesYaml.Serialize(secret);
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
        var bytes = new byte[Base64.GetMaxDecodedFromUtf8Length(kvp.Value.Length)];
        Base64.DecodeFromUtf8(value, bytes, out _, out var bytesWritten);
        secret.StringData[key] = Encoding.UTF8.GetString(bytes, 0, bytesWritten);
      }
    }

    secret.Data = null;
  }

  private string GetKubectlOutput()
  {
    var processStartInfo = new ProcessStartInfo("kubectl")
    {
      UseShellExecute = false,
      RedirectStandardOutput = true,
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