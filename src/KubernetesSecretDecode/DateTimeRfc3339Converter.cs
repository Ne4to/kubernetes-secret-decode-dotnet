using System;
using System.Xml;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace KubernetesSecretDecode
{
    public sealed class DateTimeRfc3339Converter : IYamlTypeConverter
    {
        public static readonly DateTimeRfc3339Converter Instance = new DateTimeRfc3339Converter();

        private DateTimeRfc3339Converter()
        {
        }

        /// <inheritdoc />
        public bool Accepts(Type type)
        {
            return type == typeof(DateTime);
        }

        /// <inheritdoc />
        public object ReadYaml(IParser parser, Type type)
        {
            string s = parser.Consume<Scalar>()
               .Value;
            return XmlConvert.ToDateTime(s, XmlDateTimeSerializationMode.Utc);
        }

        /// <inheritdoc />
        public void WriteYaml(
            IEmitter emitter,
            object? value,
            Type type)
        {
            DateTime dateTime = (DateTime) value;
            string str = XmlConvert.ToString(dateTime, XmlDateTimeSerializationMode.Utc);
            emitter.Emit(
                (ParsingEvent) new Scalar(
                    (string) null,
                    (string) null,
                    str,
                    ScalarStyle.Any,
                    true,
                    false));
        }
    }
}