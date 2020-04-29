dotnet tool uninstall -g KubernetesSecretDecode
dotnet pack
dotnet tool install --global --add-source ./nupkg KubernetesSecretDecode