# equity-orderbook

## Purpose

This demo app is a example usage for the [`Connectifi.DesktopAgent.WPF`](https://www.nuget.org/packages/Connectifi.DesktopAgent.WPF) library.  

See `MainWindow.xaml.cs` for example usage of the `DesktopAgentWPF` class.

## Build Notes

This project's build should build as expected on .NET 7 or higher.  We are targeting .NET 8 (the current active LTS release) due to dependency of the Svg.Skia library used in this project.  

## AppConfig

To configure the Connectifi settings, modify the 'connectifiHost' and 'connectifiAppId' strings in the `AppConfig` class. (In a production application, do this as an app config file, with environment variables, etc.)

### Build Configurations

- To test locally with a relative 'agent-dotnet' build, use the Debug-TestLocal or Release-TestLocal configurations
- To test with the published NuGet build, use Debug or Release
