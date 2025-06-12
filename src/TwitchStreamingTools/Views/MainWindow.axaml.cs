#if !DEBUG
using Microsoft.Extensions.DependencyInjection;
#else
using Avalonia;
#endif
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Threading;

using Nullinside.Api.Common.Desktop;

using TwitchStreamingTools.ViewModels;

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
    Constants.Clipboard = Clipboard;
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

    string[] args = Environment.GetCommandLineArgs();
    if (args.Contains("--update")) {
      _ = GitHubUpdateManager.PerformUpdateAndRestart("nullinside-development-group", "twitch-streaming-tools", args[2], "windows-x64.zip").ContinueWith(t => {
        Dispatcher.UIThread.Invoke(() => {
          var fuck = DataContext as MainWindowViewModel;
          if (null == fuck) {
            return;
          }

          fuck.Error = t.Exception?.Message ?? "No error message was provided.";
        });
      });
      return;
    }

    if (args.Contains("--justUpdated")) {
      _ = GitHubUpdateManager.CleanupUpdate();
    }

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
// Had to add this because code clean up tools were removing the "redundant" return statement.
// which was causing the check to always be ignored.
#if !DEBUG
        return;
#endif
      }

#if !DEBUG
      var vm = ServiceProvider?.GetRequiredService<NewVersionWindowViewModel>();
      if (null == vm) {
        return;
      }

      vm.LocalVersion = localVersion;
      Dispatcher.UIThread.Post(async void () => {
        try
        {
          var versionWindow = new NewVersionWindow {
            DataContext = vm
          };

          await versionWindow.ShowDialog(this);
        }
        catch
        {
          // do nothing, don't crash
        }
      });
#endif
    });
  }
}