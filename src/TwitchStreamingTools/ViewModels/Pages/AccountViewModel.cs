using System;
using System.Diagnostics;
using System.Reactive;

using Avalonia.Threading;

using Nullinside.Api.Common.Twitch;

using ReactiveUI;

using TwitchStreamingTools.Models;
using TwitchStreamingTools.Utilities;

namespace TwitchStreamingTools.ViewModels.Pages;

/// <summary>
///   Handles binding your account to the application.
/// </summary>
public class AccountViewModel : PageViewModelBase, IDisposable {
  /// <summary>
  ///   The timer used to check the twitch OAuth token against the API.
  /// </summary>
  private readonly DispatcherTimer _timer;

  /// <summary>
  ///   Polls the clipboard waiting for an oAuth configuration.
  /// </summary>
  private IDisposable? _clipboardPoller;

  /// <summary>
  ///   True if we have a valid OAuth token, false otherwise.
  /// </summary>
  private bool _hasValidOAuthToken;

  /// <summary>
  ///   The authenticated user's twitch username.
  /// </summary>
  private string? _twitchUsername;

  /// <summary>
  ///   Initializes a new instance of the <see cref="AccountViewModel" /> class.
  /// </summary>
  public AccountViewModel() {
    OnLaunchBrowser = ReactiveCommand.Create(LaunchBrowser);
    OnLogout = ReactiveCommand.Create(ClearCredentials);
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

  /// <inheritdoc />
  public void Dispose() {
    _timer.Stop();
    _clipboardPoller?.Dispose();
    OnLaunchBrowser.Dispose();
    OnLogout.Dispose();
  }

  /// <summary>
  ///   Checks the current twitch OAuth token against the twitch API to ensure it's valid.
  /// </summary>
  private async void OnCheckApiStatus() {
    _timer.Stop();
    try {
      if (null == Configuration.Instance.OAuth?.Bearer) {
        ClearCredentials();
        return;
      }

      var api = await TwitchApiWrapper.CreateApi();
      TwitchUsername = (await api.GetUser()).username;
      HasValidOAuthToken = !string.IsNullOrWhiteSpace(TwitchUsername);

      if (HasValidOAuthToken) {
        if (!string.Equals(api.OAuth?.AccessToken, Configuration.Instance.OAuth.Bearer) ||
            !string.Equals(TwitchUsername, Configuration.Instance.TwitchUsername)) {
          SetCredentials(TwitchUsername, new OAuthResponse {
            Bearer = api.OAuth!.AccessToken!,
            Refresh = api.OAuth.RefreshToken ?? string.Empty,
            ExpiresUtc = api.OAuth.ExpiresUtc ?? DateTime.MinValue
          });
        }
      }
      else {
        ClearCredentials();
      }
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
    if (null != _clipboardPoller) {
      _clipboardPoller.Dispose();
    }

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
      SetCredentials(null, oauth);
      OnCheckApiStatus();
    }
    catch {
      // do nothing
    }
  }

  private void ClearCredentials() {
    TwitchUsername = null;
    HasValidOAuthToken = false;
    TwitchClientProxy.Instance.Dispose();

    if (null != Configuration.Instance.OAuth) {
      Configuration.Instance.OAuth = null;
      Configuration.Instance.TwitchUsername = null;
      Configuration.Instance.WriteConfiguration();
    }
  }

  private void SetCredentials(string? username, OAuthResponse oauth) {
    Configuration.Instance.OAuth = oauth;
    Configuration.Instance.TwitchUsername = username;
    Configuration.Instance.WriteConfiguration();
    TwitchClientProxy.Instance.TwitchUsername = username;
    TwitchClientProxy.Instance.TwitchOAuthToken = oauth.Bearer;
  }
}