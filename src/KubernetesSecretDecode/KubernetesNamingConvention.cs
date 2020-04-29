using k8s.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace KubernetesSecretDecode
{
    internal sealed class KubernetesNamingConvention : INamingConvention
    {
        public static readonly KubernetesNamingConvention Instance = new KubernetesNamingConvention();

        private KubernetesNamingConvention()
        {
        }

        public string Apply(string value)
        {
            if (value == nameof(V1ObjectMeta.NamespaceProperty))
            {
                return "namespace";
            }

            return CamelCaseNamingConvention.Instance.Apply(value);
        }
    }
}