using System;
using System.Runtime.Versioning;

using Avalonia;
using Avalonia.ReactiveUI;

[assembly: SupportedOSPlatform("windows")]

namespace TwitchStreamingTools;

internal sealed class Program {
  // Initialization code. Don't use any Avalonia, third-party APIs or any
  // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
  // yet and stuff might break.

  /// <summary>
  ///   Main entrypoint of the application.
  /// </summary>
  /// <param name="args">The arguments passed to the application.</param>
  [STAThread]
  public static void Main(string[] args) {
    BuildAvaloniaApp()
      .StartWithClassicDesktopLifetime(args);
  }

  // Avalonia configuration, don't remove; also used by visual designer.

  /// <summary>
  ///   Builds the avalonia application.
  /// </summary>
  /// <returns>The application builder.</returns>
  public static AppBuilder BuildAvaloniaApp() {
    return AppBuilder.Configure<App>()
      .UsePlatformDetect()
      .WithInterFont()
      .LogToTrace()
      .UseReactiveUI();
  }
}