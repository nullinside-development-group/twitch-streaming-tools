using System;
using System.Diagnostics;
using System.Reactive;

using ReactiveUI;

using TwitchStreamingTools.Models;
using TwitchStreamingTools.Services;
using TwitchStreamingTools.Utilities;

namespace TwitchStreamingTools.ViewModels.Pages;

/// <summary>
///   Handles binding your account to the application.
/// </summary>
public class AccountViewModel : PageViewModelBase, IDisposable {
  /// <summary>
  ///   Manages the account OAuth information.
  /// </summary>
  private readonly ITwitchAccountService _twitchAccountService;

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
  /// <param name="twitchAccountService">Manages the account OAuth information.</param>
  public AccountViewModel(ITwitchAccountService twitchAccountService) {
    _twitchAccountService = twitchAccountService;
    _twitchAccountService.OnCredentialsStatusChanged += OnCredentialsStatusChanged;
    OnLaunchBrowser = ReactiveCommand.Create(LaunchBrowser);
    OnLogout = ReactiveCommand.Create(ClearCredentials);

    // Set the initial state of the ui
    HasValidOAuthToken = _twitchAccountService.CredentialsAreValid;
    TwitchUsername = _twitchAccountService.TwitchUsername;
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
  ///   The application version number.
  /// </summary>
  public string? Version => Constants.APP_VERSION;

  /// <inheritdoc />
  public void Dispose() {
    _clipboardPoller?.Dispose();
    OnLaunchBrowser.Dispose();
    OnLogout.Dispose();
  }

  /// <summary>
  ///   Invoked when the status of the credentials changes from the <seealso cref="ITwitchAccountService" />.
  /// </summary>
  /// <param name="valid">True if the credentials are valid, false otherwise.</param>
  private void OnCredentialsStatusChanged(bool valid) {
    if (!valid) {
      HasValidOAuthToken = false;
      TwitchUsername = null;
      return;
    }

    HasValidOAuthToken = true;
    TwitchUsername = _twitchAccountService.TwitchUsername;
  }

  /// <summary>
  ///   Launches the computer's default browser to generate an OAuth token.
  /// </summary>
  private void LaunchBrowser() {
    if (null != _clipboardPoller) {
      _clipboardPoller.Dispose();
    }

    _clipboardPoller = new ClipboardPoller<OAuthResponse>(Constants.Clipboard!, OnOAuthOnClipboard);

    string url = $"https://id.twitch.tv/oauth2/authorize?client_id={Constants.TWITCH_CLIENT_ID}&" +
                 $"redirect_uri={Constants.TWITCH_CLIENT_REDIRECT}&" +
                 "response_type=code&" +
                 $"scope={string.Join("%20", Constants.TWITCH_SCOPES)}";
    Process.Start(new ProcessStartInfo("cmd", $"/c start {url.Replace("&", "^&")}") { CreateNoWindow = true });
  }

  /// <summary>
  ///   Invoked when the clipboard finds OAuth credential on it.
  /// </summary>
  /// <param name="obj">The response from twitch.</param>
  private void OnOAuthOnClipboard(OAuthResponse obj) {
    _twitchAccountService.UpdateCredentials(obj.Bearer, obj.Refresh, obj.ExpiresUtc).Wait();
  }

  /// <summary>
  ///   Clears the credentials out (logging out).
  /// </summary>
  private void ClearCredentials() {
    _twitchAccountService.DeleteCredentials();
  }
}