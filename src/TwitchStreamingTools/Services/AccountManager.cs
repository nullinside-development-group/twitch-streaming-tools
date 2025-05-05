using System;
using System.Threading.Tasks;

using Avalonia.Threading;

using Nullinside.Api.Common.Twitch;

using TwitchStreamingTools.Models;

namespace TwitchStreamingTools.Services;

/// <summary>
///   Manages the credentials in the application.
/// </summary>
public class AccountManager : IAccountManager {
  /// <summary>
  ///   The timer used to check the twitch OAuth token against the API.
  /// </summary>
  private readonly DispatcherTimer _timer;

  /// <summary>
  ///   The twitch chat api.
  /// </summary>
  private readonly ITwitchApiProxy _twitchApi;

  /// <summary>
  ///   The twitch chat client.
  /// </summary>
  private readonly ITwitchClientProxy _twitchClient;

  /// <summary>
  ///   Initializes a new instance of the <see cref="AccountManager" /> class.
  /// </summary>
  /// <param name="twitchClient">The twitch chat client.</param>
  /// <param name="twitchApi">The twitch chat api.</param>
  public AccountManager(ITwitchClientProxy twitchClient, ITwitchApiProxy twitchApi) {
    _twitchClient = twitchClient;
    _twitchApi = twitchApi;
    _timer = new DispatcherTimer {
      Interval = TimeSpan.FromSeconds(5)
    };

    _timer.Tick += async (_, _) => await OnCheckCredentials();
    _ = OnCheckCredentials();
  }

  /// <inheritdoc />
  public string? TwitchUsername { get; set; }

  /// <inheritdoc />
  public bool CredentialsAreValid { get; set; }

  /// <inheritdoc />
  public Action<bool>? OnCredentialsStatusChanged { get; set; }

  /// <inheritdoc />
  public Action<TwitchAccessToken?>? OnCredentialsChanged { get; set; }

  /// <inheritdoc />
  public async Task UpdateCredentials(string bearer, string refresh, DateTime expires) {
    _twitchApi.OAuth = new TwitchAccessToken {
      AccessToken = bearer,
      RefreshToken = refresh,
      ExpiresUtc = expires
    };

    (string? id, string? username)? user = null;
    try {
      user = await _twitchApi.GetUser();
    }
    catch {
      // Do nothing
    }

    Configuration.Instance.OAuth = new OAuthResponse {
      Bearer = bearer,
      Refresh = refresh,
      ExpiresUtc = expires
    };

    Configuration.Instance.TwitchUsername = user?.username;
    Configuration.Instance.WriteConfiguration();
    _twitchClient.TwitchOAuthToken = bearer;
    _twitchClient.TwitchUsername = user?.username;

    OnCredentialsChanged?.Invoke(null);
    await OnCheckCredentials();
  }

  /// <inheritdoc />
  public void DeleteCredentials() {
    _twitchApi.OAuth = null;
    Configuration.Instance.OAuth = null;
    Configuration.Instance.TwitchUsername = null;
    _twitchClient.TwitchOAuthToken = null;
    _twitchClient.TwitchUsername = null;
    CredentialsAreValid = false;
    TwitchUsername = null;
    Configuration.Instance.WriteConfiguration();

    OnCredentialsChanged?.Invoke(null);
    OnCredentialsStatusChanged?.Invoke(false);
  }

  /// <summary>
  ///   Checks the OAuth token against the API to verify its validity.
  /// </summary>
  private async Task OnCheckCredentials() {
    _timer.Stop();
    try {
      bool previousValue = CredentialsAreValid;
      string? username = (await _twitchApi.GetUser()).username;
      CredentialsAreValid = !string.IsNullOrWhiteSpace(username);
      TwitchUsername = username;

      if (previousValue != CredentialsAreValid) {
        OnCredentialsStatusChanged?.Invoke(CredentialsAreValid);
      }
    }
    catch {
      // Do nothing
    }
    finally {
      _timer.Start();
    }
  }
}