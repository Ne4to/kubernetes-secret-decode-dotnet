using System;

namespace KubernetesSecretDecode;

[Serializable]
public class DecoderException : Exception
{
  public DecoderException()
  {
  }

  public DecoderException(string message)
      : base(message)
  {
  }

  public DecoderException(string message, Exception inner)
      : base(message, inner)
  {
  }
}