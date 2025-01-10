@echo off
dotnet publish ../src/akka.App/akka.App.csproj --os linux --arch x64 -c Release -p:PublishProfile=DefaultContainer