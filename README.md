# Kubernetes Secret Decode .Net edition

![Push NuGet package](https://github.com/Ne4to/kubernetes-secret-decode-dotnet/workflows/Push%20NuGet%20package/badge.svg)

Inspired by https://github.com/ashleyschuett/kubernetes-secret-decode

Reasons for existing this project:
- Primary: Learning of [Writing kubectl plugins](https://kubernetes.io/docs/tasks/extend-kubectl/kubectl-plugins/#writing-kubectl-plugins)
- Secondary: Original kubernetes-secret-decode has no Windows compatible binaries in releases.

## Installation
[![NuGet Badge](https://buildstats.info/nuget/kubernetessecretdecode?includePreReleases=true&dWidth=0)](https://www.nuget.org/packages/KubernetesSecretDecode/)
```
dotnet tool install --global KubernetesSecretDecode <version>
```

### Add PowerShell alias

Open `$PROFILE` in text editor and add following line

```powershell
function kksd() { & kubectl ksddotnet get secret -oyaml $args }
```

## Using

```shell
# Full command example
kubectl ksddotnet get secret -oyaml <secret-name>

# Using kksd alias
kksd <secret-name>
```