dotnet tool uninstall -g KubernetesSecretDecode
dotnet clean
dotnet pack
dotnet tool install --global --add-source ./nupkg KubernetesSecretDecode --version 0.1.0