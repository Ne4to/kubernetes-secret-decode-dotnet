# Kubernetes Secret Decode .Net edition

Inspired by https://github.com/ashleyschuett/kubernetes-secret-decode

Reasons for existing this project:
- Primary: Learning of [Writing kubectl plugins](https://kubernetes.io/docs/tasks/extend-kubectl/kubectl-plugins/#writing-kubectl-plugins)
- Secondary: Original kubernetes-secret-decode has no Windows compatible binaries in releases.

## Installation

```
dotnet tool install --global KubernetesSecretDecode
```

### Add PowerShell alias

Open `$PROFILE` in text editor and add following line

```powershell
function kksd([Parameter(ValueFromRemainingArguments = $true)]$params) { & kubectl ksddotnet get secret -oyaml $params }
```

## Using

```shell
# Full command example
kubectl ksddotnet get secret -oyaml <secret-name>

# Using kksd alias
kksd <secret-name>
```