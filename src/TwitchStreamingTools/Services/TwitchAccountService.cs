﻿using System;
using System.Threading.Tasks;

using Avalonia.Threading;

using Nullinside.Api.Common.Twitch;

using TwitchStreamingTools.Models;
using TwitchStreamingTools.Utilities;

namespace TwitchStreamingTools.Services;

/// <summary>
///   Manages the credentials in the application, ensuring credentials are kept up to date.
/// </summary>
public class TwitchAccountService : ITwitchAccountService {
  /// <summary>
  ///   The application configuration.
  /// </summary>
  private readonly IConfiguration _configuration;

  /// <summary>
  ///   The timer used to check the twitch OAuth token against the API.
  /// </summary>
  private readonly DispatcherTimer _timer;

  /// <summary>
  ///   The twitch chat client.
  /// </summary>
  private readonly ITwitchClientProxy _twitchClient;

  /// <summary>
  ///   Initializes a new instance of the <see cref="TwitchAccountService" /> class.
  /// </summary>
  /// <param name="twitchClient">The twitch chat client.</param>
  /// <param name="configuration">The application configuration.</param>
  public TwitchAccountService(ITwitchClientProxy twitchClient, IConfiguration configuration) {
    _configuration = configuration;
    _twitchClient = twitchClient;
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
    var twitchApi = new TwitchApiWrapper {
      OAuth = new TwitchAccessToken {
        AccessToken = bearer,
        RefreshToken = refresh,
        ExpiresUtc = expires
      }
    };

    (string? id, string? username)? user = null;
    try {
      user = await twitchApi.GetUser();
    }
    catch {
      // Do nothing
    }

    _configuration.OAuth = new OAuthResponse {
      Bearer = bearer,
      Refresh = refresh,
      ExpiresUtc = expires
    };

    _configuration.TwitchUsername = user?.username;
    _configuration.WriteConfiguration();
    _twitchClient.TwitchOAuthToken = bearer;
    _twitchClient.TwitchUsername = user?.username;

    OnCredentialsChanged?.Invoke(null);
    await OnCheckCredentials();
  }

  /// <inheritdoc />
  public void DeleteCredentials() {
    _configuration.OAuth = null;
    _configuration.TwitchUsername = null;
    _twitchClient.TwitchOAuthToken = null;
    _twitchClient.TwitchUsername = null;
    CredentialsAreValid = false;
    TwitchUsername = null;
    _configuration.WriteConfiguration();

    OnCredentialsChanged?.Invoke(null);
    OnCredentialsStatusChanged?.Invoke(false);
  }

  /// <summary>
  ///   Checks the OAuth token against the API to verify its validity.
  /// </summary>
  private async Task OnCheckCredentials() {
    _timer.Stop();
    try {
      // Grab the value so we can check if the value changed
      bool previousValue = CredentialsAreValid;

      // Refresh the token
      await DoTokenRefreshIfNearExpiration();

      // Make sure the new token works
      var twitchApi = new TwitchApiWrapper();
      string? username = (await twitchApi.GetUser()).username;

      // Update the credentials
      CredentialsAreValid = !string.IsNullOrWhiteSpace(username);
      TwitchUsername = username;

      // Fire off the event if something changed
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

  /// <summary>
  ///   Checks the expiration of the OAuth token and refreshes if it's within 1 hour of the time.
  /// </summary>
  private async Task DoTokenRefreshIfNearExpiration() {
    var twitchApi = new TwitchApiWrapper();

    // Don't wait until the token is expired, refresh it ~1 hour before it expires 
    DateTime expiration = twitchApi.OAuth?.ExpiresUtc ?? DateTime.MaxValue;
    TimeSpan timeUntil = expiration - (DateTime.UtcNow + TimeSpan.FromHours(1));
    if (timeUntil.Ticks >= 0) {
      return;
    }

    if (null == twitchApi.OAuth || string.IsNullOrWhiteSpace(twitchApi.OAuth.AccessToken) ||
        string.IsNullOrWhiteSpace(twitchApi.OAuth.RefreshToken)) {
      return;
    }

    // Refresh the token
    await twitchApi.RefreshAccessToken();

    // Update the configuration
    _configuration.OAuth = new OAuthResponse {
      Bearer = twitchApi.OAuth.AccessToken,
      Refresh = twitchApi.OAuth.RefreshToken,
      ExpiresUtc = twitchApi.OAuth.ExpiresUtc ?? DateTime.MinValue
    };
    _configuration.WriteConfiguration();
  }
}