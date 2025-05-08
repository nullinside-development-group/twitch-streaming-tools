using System;
using System.Reflection;
using System.Threading.Tasks;

using Avalonia.Controls;

using Nullinside.Api.Common.Desktop;
#if !DEBUG
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using TwitchStreamingTools.ViewModels;
#else
using Avalonia;
#endif

namespace TwitchStreamingTools.Views;

/// <summary>
///   The main application window.
/// </summary>
public partial class MainWindow : Window {
  /// <summary>
  ///   Initializes a new instance of the <see cref="MainWindow" /> class.
  /// </summary>
  public MainWindow() {
    InitializeComponent();
    Constants.CLIPBOARD = Clipboard;
#if DEBUG
    this.AttachDevTools();
#endif
  }

  /// <summary>
  ///   The service provider for DI.
  /// </summary>
  public IServiceProvider? ServiceProvider { get; set; }

  /// <summary>
  ///   Checks for a new version number of the application.
  /// </summary>
  protected override void OnInitialized() {
    base.OnInitialized();

    Task.Factory.StartNew(async () => {
      GithubLatestReleaseJson? serverVersion =
        await GitHubUpdateManager.GetLatestVersion("nullinside-development-group", "twitch-streaming-tools");
      string? localVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
      if (null == serverVersion || string.IsNullOrWhiteSpace(serverVersion.name) ||
          string.IsNullOrWhiteSpace(localVersion)) {
        return;
      }

      localVersion = localVersion.Substring(0, localVersion.LastIndexOf('.'));
      if (string.IsNullOrWhiteSpace(localVersion)) {
        return;
      }

      if (serverVersion.name?.Equals(localVersion, StringComparison.InvariantCultureIgnoreCase) ?? true) {
        return;
      }

#if !DEBUG
      var vm = ServiceProvider?.GetRequiredService<NewVersionWindowViewModel>();
      if (null == vm) {
        return;
      }

      vm.LocalVersion = localVersion;
      Dispatcher.UIThread.Post(async () => {
        var versionWindow = new NewVersionWindow {
          DataContext = vm
        };

        await versionWindow.ShowDialog(this);
      });
#endif
    });
  }
}