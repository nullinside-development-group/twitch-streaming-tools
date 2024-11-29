using System;
using System.Diagnostics;
using System.Reactive;

using Avalonia.Threading;

using ReactiveUI;

using TwitchStreamingTools.Models;
using TwitchStreamingTools.Utilities;

namespace TwitchStreamingTools.ViewModels.Pages;

/// <summary>
///   Handles binding your account to the application.
/// </summary>
public class AccountViewModel : PageViewModelBase {
  /// <summary>
  ///   Polls the clipboard waiting for an oAuth configuration.
  /// </summary>
  private IClipboardPoller<OAuthResponse>? _clipboardPoller;

  /// <summary>
  ///   True if we have a valid OAuth token, false otherwise.
  /// </summary>
  private bool _hasValidOAuthToken;

  /// <summary>
  ///   The timer used to check the twitch OAuth token against the API.
  /// </summary>
  private readonly DispatcherTimer _timer;

  /// <summary>
  ///   The authenticated user's twitch username.
  /// </summary>
  private string? _twitchUsername;

  /// <summary>
  ///   Initializes a new instance of the <see cref="AccountViewModel" /> class.
  /// </summary>
  public AccountViewModel() {
    OnLaunchBrowser = ReactiveCommand.Create(LaunchBrowser);
    OnLogout = ReactiveCommand.Create(Logout);
    _timer = new DispatcherTimer {
      Interval = TimeSpan.FromSeconds(5)
    };

    OnCheckApiStatus();
    _timer.Tick += (_, _) => OnCheckApiStatus();
    _timer.Start();
  }

  /// <inheritdoc />
  public override string IconResourceKey { get; } = "InprivateAccountRegular";

  /// <summary>
  ///   Called when toggling the menu open and close.
  /// </summary>
  public ReactiveCommand<Unit, Unit> OnLaunchBrowser { get; }

  /// <summary>
  ///   Called when logging out the current user.
  /// </summary>
  public ReactiveCommand<Unit, Unit> OnLogout { get; }

  /// <summary>
  ///   True if we have a valid OAuth token, false otherwise.
  /// </summary>
  public bool HasValidOAuthToken {
    get => _hasValidOAuthToken;
    set => this.RaiseAndSetIfChanged(ref _hasValidOAuthToken, value);
  }

  /// <summary>
  ///   The authenticated user's twitch username.
  /// </summary>
  public string? TwitchUsername {
    get => _twitchUsername;
    set => this.RaiseAndSetIfChanged(ref _twitchUsername, value);
  }

  /// <summary>
  ///   Handles logging out of the application.
  /// </summary>
  private void Logout() {
    try {
      Configuration.Instance.OAuth = null;
      Configuration.Instance.WriteConfiguration();
      OnCheckApiStatus();
    }
    catch {
      // do nothing
    }
  }

  /// <summary>
  ///   Checks the current twitch OAuth token against the twitch API to ensure it's valid.
  /// </summary>
  private async void OnCheckApiStatus() {
    _timer.Stop();
    try {
      if (null == Configuration.Instance.OAuth?.Bearer) {
        TwitchUsername = null;
        HasValidOAuthToken = false;
        return;
      }

      var api = await TwitchApiWrapper.CreateApi();
      TwitchUsername = (await api.GetUser()).username;
      HasValidOAuthToken = !string.IsNullOrWhiteSpace(TwitchUsername);
    }
    catch {
      TwitchUsername = null;
      HasValidOAuthToken = false;
    }
    finally {
      _timer.Start();
    }
  }

  /// <summary>
  ///   Launches the computer's default browser to generate an OAuth token.
  /// </summary>
  private void LaunchBrowser() {
    Configuration.Instance.OAuth = null;
    Configuration.Instance.WriteConfiguration();

    _clipboardPoller = new ClipboardPoller<OAuthResponse>(Constants.CLIPBOARD!, OnOAuthReceived);

    string url = $"https://id.twitch.tv/oauth2/authorize?client_id={Constants.TWITCH_CLIENT_ID}&" +
                 $"redirect_uri={Constants.TWITCH_CLIENT_REDIRECT}&" +
                 "response_type=code&" +
                 $"scope={string.Join("%20", Constants.TWITCH_SCOPES)}";
    Process.Start(new ProcessStartInfo("cmd", $"/c start {url.Replace("&", "^&")}") { CreateNoWindow = true });
  }

  /// <summary>
  ///   Called when an OAuth token is found.
  /// </summary>
  /// <param name="oauth">The new OAuth token.</param>
  private void OnOAuthReceived(OAuthResponse oauth) {
    try {
      Configuration.Instance.OAuth = oauth;
      Configuration.Instance.WriteConfiguration();
      OnCheckApiStatus();
    }
    catch {
      // do nothing
    }
  }
}