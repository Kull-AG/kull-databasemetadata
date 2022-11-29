
Get-ChildItem "$PSScriptRoot/bin/Release" | ForEach-Object { Remove-Item $_.FullName -Force -Recurse }
dotnet build --configuration Release

Get-ChildItem "$PSScriptRoot/bin/Release" *.nupkg | ForEach-Object {
	dotnet nuget push $_.FullName --source https://api.nuget.org/v3/index.json --api-key $env:Nuget_API_KEY
}