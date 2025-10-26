# Set Working Directory
Split-Path $MyInvocation.MyCommand.Path | Push-Location
[Environment]::CurrentDirectory = $PWD

Remove-Item "$env:RELOADEDIIMODS/P5RPC.Fnaf2/*" -Force -Recurse
dotnet publish "./P5RPC.Fnaf2.csproj" -c Release -o "$env:RELOADEDIIMODS/P5RPC.Fnaf2" /p:OutputPath="./bin/Release" /p:ReloadedILLink="true"

# Restore Working Directory
Pop-Location