using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Reactive;
using System.Threading;

using log4net;

using Newtonsoft.Json;

using Nullinside.Api.Common.Extensions;
using Nullinside.Api.Common.Twitch;

using ReactiveUI;

using TwitchStreamingTools.Services;

namespace TwitchStreamingTools.ViewModels.Pages;

/// <summary>
///   Handles binding your account to the application.
/// </summary>
public class AccountViewModel : PageViewModelBase, IDisposable {
  /// <summary>
  ///   The logger.
  /// </summary>
  private readonly ILog _logger = LogManager.GetLogger(typeof(AccountViewModel));

  /// <summary>
  ///   Manages the account OAuth information.
  /// </summary>
  private readonly ITwitchAccountService _twitchAccountService;

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
  private async void LaunchBrowser() {
    try {
      var token = CancellationToken.None;
      
      // Create an identifier for this credential request.
      var guid = Guid.NewGuid();
      
      // Create a web socket connection to the api which will provide us with the credentials from twitch.
      ClientWebSocket webSocket = new();
      await webSocket.ConnectAsync(new Uri($"ws://{Constants.DOMAIN}/api/v1/user/twitch-login/twitch-streaming-tools/ws"), token);
      await webSocket.SendTextAsync(guid.ToString(), token);

      // Launch the web browser to twitch to ask for account permissions. Twitch will be instructed to callback to our
      // api (redirect_uri) which will give us the credentials on the web socket above.
      string url = $"https://id.twitch.tv/oauth2/authorize?client_id={Constants.TWITCH_CLIENT_ID}&" +
                   $"redirect_uri={Constants.TWITCH_CLIENT_REDIRECT}&" +
                   "response_type=code&" +
                   $"scope={string.Join("%20", Constants.TWITCH_SCOPES)}&" +
                   $"state={guid}";
      Process.Start(new ProcessStartInfo("cmd", $"/c start {url.Replace("&", "^&")}") { CreateNoWindow = true });

      // Wait for the user to finish giving us permission on the website. Once they provide us access we will receive
      // a response on the web socket containing a JSON with our OAuth information.
      string json = await webSocket.ReceiveTextAsync(token);
      
      // Close the connection, both sides will be waiting to do this so we do it immediately.
      await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Completed Successfully!", token);
      
      // Update the oauth token in the twitch account service. 
      var oauthResp = JsonConvert.DeserializeObject<TwitchAccessToken>(json);
      if (null == oauthResp || null == oauthResp.AccessToken || null == oauthResp.RefreshToken || null == oauthResp.ExpiresUtc) {
        _logger.Error($"Failed to get a valid oauth token, got: {json}");
        return;
      }

      await _twitchAccountService.UpdateCredentials(oauthResp.AccessToken, oauthResp.RefreshToken, oauthResp.ExpiresUtc.Value);
    }
    catch (Exception ex) {
      _logger.Error("Failed to launch browser to login", ex);
    }
  }

  /// <summary>
  ///   Clears the credentials out (logging out).
  /// </summary>
  private void ClearCredentials() {
    _twitchAccountService.DeleteCredentials();
  }
}