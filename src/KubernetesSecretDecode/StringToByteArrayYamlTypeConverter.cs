using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace KubernetesSecretDecode
{
    internal sealed class StringToByteArrayYamlTypeConverter : IYamlTypeConverter
    {
        public static StringToByteArrayYamlTypeConverter Instance = new StringToByteArrayYamlTypeConverter();

        private StringToByteArrayYamlTypeConverter()
        {
        }

        public bool Accepts(Type type)
        {
            return type == typeof(byte[]);
        }

        public object? ReadYaml(IParser parser, Type type)
        {
            var value = parser.Consume<Scalar>()
               .Value;
            return Convert.FromBase64String(value);
        }

        public void WriteYaml(
            IEmitter emitter,
            object? value,
            Type type)
        {
            throw new NotImplementedException();
        }
    }
}