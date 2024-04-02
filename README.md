# equity-orderbook

## Purpose

This demo app is a example usage for the [`Connectifi.DesktopAgent.WPF`](https://www.nuget.org/packages/Connectifi.DesktopAgent.WPF) library.  

See `MainWindow.xaml.cs` for example usage of the `DesktopAgentWPF` class.

## Build Notes

This project's build should build as expected on .NET 7 or higher.  We are targeting .NET 8 (the current active LTS release) due to dependency of the Svg.Skia library used in this project.  

## App.Config

An `App.config` file is required with the following parameters:
- `connectifiDevServer` - Base Connectifi server (e.g. "https://dev.connectifi-interop.com/") configured for your agent usage

### Build Configurations

- To test locally with a relative 'agent-dotnet' build, use the Debug-TestLocal or Release-TestLocal configurations
- To test with the published NuGet build, use Debug or Release
