# equity-orderbook

Demo of .NET WPF app

## Purpose

This demo is an example of usage of the Connectifi.DesktopAgent.WPF library.  See `MainWindow.xaml.cs` for example usage of the `DesktopAgentWPF` class.

## Build Notes

This code should work as expected on .NET 7 or higher.  We are targeting .NET 8 (the active LTS release) due to dependency of the Svg.Skia library used in this project.  

### Build Configurations

- To test locally with a relative 'agent-dotnet' build, use the Debug-TestLocal or Release-TestLocal configurations
- To test with the published NuGet build, use Debug or Release