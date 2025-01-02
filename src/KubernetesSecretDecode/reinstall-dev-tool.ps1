$ErrorActionPreference = "Stop"

function Assert-ExitCode {
    if (-not $?) {
        throw 'Latest command failed'
    }
}
try {
    [xml]$XmlConfig = Get-Content 'KubernetesSecretDecode.csproj'

    $XmlElement = Select-Xml '/Project/PropertyGroup/VersionPrefix' $XmlConfig |
        Select-Object -ExpandProperty Node

    $VersionPrefix = $XmlElement.InnerText
    $VersionSuffix = "rc.$(Get-Date -Format 'yyyy-MM-dd-HHmm')"
    $PackageVersion = "$VersionPrefix-$VersionSuffix"

    dotnet tool uninstall -g KubernetesSecretDecode
    dotnet clean

    dotnet publish
    Assert-ExitCode

    dotnet pack --version-suffix $VersionSuffix
    Assert-ExitCode

    dotnet tool install --global --add-source ./nupkg KubernetesSecretDecode --version $PackageVersion
    Assert-ExitCode
}
catch {
    Write-Host 'Install global tool - FAILED!' -ForegroundColor Red
    throw
}
finally {
    Push-Location
}
