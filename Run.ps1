$root = $PSScriptRoot
$projectFolder = Join-Path $root "src\WebSite\Website.csproj"
dotnet run --project $projectFolder --launch-profile https
