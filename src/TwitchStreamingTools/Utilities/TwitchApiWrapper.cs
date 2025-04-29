using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Nullinside.Api.Common.Twitch;
using Nullinside.Api.Common.Twitch.Json;

using TwitchStreamingTools.Models;

namespace TwitchStreamingTools.Utilities;

/// <summary>
///   A wrapper for interacting with the Twitch Api.
/// </summary>
public class TwitchApiWrapper : TwitchApiProxy {
  /// <summary>
  ///   Lock to prevent the creation of more than one API at a time.
  /// </summary>
  private static readonly SemaphoreSlim _lock = new(1);

  /// <summary>
  ///   Initializes a new instance of the <see cref="TwitchApiWrapper" /> class.
  /// </summary>
  protected TwitchApiWrapper() : base(
    Configuration.Instance.OAuth?.Bearer ?? "",
    Configuration.Instance.OAuth?.Refresh ?? "",
    Configuration.Instance.OAuth?.ExpiresUtc ?? DateTime.MinValue,
    Configuration.Instance.TwitchAppConfig?.ClientId ?? Constants.TWITCH_CLIENT_ID,
    Configuration.Instance.TwitchAppConfig?.ClientSecret,
    Configuration.Instance.TwitchAppConfig?.ClientRedirect ?? Constants.TWITCH_CLIENT_REDIRECT) {
  }

  /// <summary>
  ///   Creates a new instance of the API.
  /// </summary>
  /// <returns>A new API instance.</returns>
  public static async Task<TwitchApiWrapper> CreateApi() {
    await _lock.WaitAsync();
    try {
      var api = new TwitchApiWrapper();
      DateTime expiration = api.OAuth?.ExpiresUtc ?? DateTime.MaxValue;
      TimeSpan timeUntil = expiration - (DateTime.UtcNow + TimeSpan.FromHours(1));
      if (timeUntil.Ticks < 0) {
        if (null != api.OAuth && !string.IsNullOrWhiteSpace(api.OAuth.AccessToken) &&
            !string.IsNullOrWhiteSpace(api.OAuth.RefreshToken)) {
          await api.RefreshAccessToken();
          (string? id, string? username) userInfo = await api.GetUser();
          Configuration.Instance.OAuth = new OAuthResponse {
            Bearer = api.OAuth.AccessToken,
            Refresh = api.OAuth.RefreshToken,
            ExpiresUtc = api.OAuth.ExpiresUtc ?? DateTime.MinValue
          };
        }
      }

      return api;
    }
    finally {
      _lock.Release();
    }
  }

  /// <summary>
  ///   Handles refreshing the twitch oauth token eithe through a local application or through the website.
  /// </summary>
  /// <param name="token">The refresh token.</param>
  /// <returns>The new OAuth token information if successful, null otherwise.</returns>
  public override async Task<TwitchAccessToken?> RefreshAccessToken(CancellationToken token = new()) {
    if (!string.IsNullOrWhiteSpace(TwitchAppConfig?.ClientSecret)) {
      await base.RefreshAccessToken(token);
    }

    using var client = new HttpClient();
    string url = $"{Constants.API_SITE_DOMAIN}/api/v1/user/twitch-login/twitch-streaming-tools";
    var request = new HttpRequestMessage(HttpMethod.Post, url);
    using HttpResponseMessage response =
      await client.PostAsJsonAsync(request.RequestUri, $"{{\"refreshToken\":\"{OAuth?.RefreshToken}\"}}");
    response.EnsureSuccessStatusCode();
    string responseBody = await response.Content.ReadAsStringAsync();
    var moderatedChannels = JsonConvert.DeserializeObject<TwitchModeratedChannelsResponse>(responseBody);
    if (null == moderatedChannels) {
      return null;
    }

    return null;
  }
}