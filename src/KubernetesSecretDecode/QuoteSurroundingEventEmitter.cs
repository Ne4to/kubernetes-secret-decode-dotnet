using System.Text.RegularExpressions;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;

namespace KubernetesSecretDecode
{
    public class QuoteSurroundingEventEmitter : ChainedEventEmitter
    {
        private static readonly Regex NumberRegex = new Regex("^\\d+$", RegexOptions.Compiled);
        private static readonly Regex BoolRegex = new Regex("^(true|false)$", RegexOptions.Compiled);

        public QuoteSurroundingEventEmitter(IEventEmitter nextEmitter)
            : base(nextEmitter)
        {
        }

        public override void Emit(ScalarEventInfo eventInfo, IEmitter emitter)
        {
            if (eventInfo.Source.Value is string stringValue && !string.IsNullOrEmpty(stringValue))
            {
                if (NumberRegex.IsMatch(stringValue) || BoolRegex.IsMatch(stringValue))
                {
                    eventInfo.Style = ScalarStyle.DoubleQuoted;
                }
            }

            base.Emit(eventInfo, emitter);
        }
    }
}