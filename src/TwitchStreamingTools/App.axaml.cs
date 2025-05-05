using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using Microsoft.Extensions.DependencyInjection;

using Nullinside.Api.Common.Twitch;

using TwitchStreamingTools.Models;
using TwitchStreamingTools.Services;
using TwitchStreamingTools.ViewModels;
using TwitchStreamingTools.Views;

namespace TwitchStreamingTools;

/// <summary>
///   Main entry point of the application.
/// </summary>
public class App : Application {
  /// <summary>
  ///   Initializes the GUI.
  /// </summary>
  public override void Initialize() {
    AvaloniaXamlLoader.Load(this);
  }

  /// <summary>
  ///   Launches the main application window.
  /// </summary>
  public override void OnFrameworkInitializationCompleted() {
    TwitchClientProxy.Instance.TwitchOAuthToken = Configuration.Instance.OAuth?.Bearer;
    TwitchClientProxy.Instance.TwitchUsername = Configuration.Instance.TwitchUsername;

    // Register all the services needed for the application to run
    var collection = new ServiceCollection();
    collection.AddCommonServices();

    // Creates a ServiceProvider containing services from the provided IServiceCollection
    ServiceProvider services = collection.BuildServiceProvider();
    var vm = services.GetRequiredService<MainWindowViewModel>();
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
      desktop.MainWindow = new MainWindow {
        DataContext = vm
      };
    }

    base.OnFrameworkInitializationCompleted();
  }

  private void ShowSettings_OnClick(object? sender, EventArgs e) {
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
      if (null == desktop.MainWindow) {
        return;
      }

      desktop.MainWindow.WindowState = WindowState.Normal;
    }
  }

  private void Close_OnClick(object? sender, EventArgs e) {
    Environment.Exit(0);
  }
}