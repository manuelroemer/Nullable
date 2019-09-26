Param(
    [string] $artifactPath = "./artifacts",
    [string] $srcPath = "./src"
)

$nuspecPath = Join-Path $srcPath 'Nullable.nuspec'

function Clean() {
    Get-ChildItem $srcPath -Include *.pp -Recurse | Remove-Item
}

# Clean before, so that there are no left-over files.
Clean

# Ensure that the project builds.
dotnet restore $srcPath --ignore-failed-sources
dotnet build $srcPath --ignore-failed-sources

if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed with errors."
}

# Rename the existing .cs files to .cs.pp.
# Doing this ensures that the files aren't listed in the solution explorer when the package
# is consumed via NuGet. With .cs files, that happens.
Get-ChildItem $srcPath -Include *.cs -Recurse | % { Copy-Item $_.FullName -Destination "$($_.FullName).pp" }

# Package the .pp files. The configuration is done in the .nuspec.
nuget pack $nuspecPath -OutputDirectory $artifactPath

Clean
