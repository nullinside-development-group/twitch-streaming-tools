using System;

using Newtonsoft.Json;

namespace TwitchStreamingTools.Models;

/// <summary>
///   A JSON representation of an OAuth token.
/// </summary>
public class OAuthResponse {
  /// <summary>
  ///   The OAuth token.
  /// </summary>
  [JsonProperty("bearer")]
  public string Bearer { get; set; } = null!;

  /// <summary>
  ///   The refresh token.
  /// </summary>
  [JsonProperty("refresh")]
  public string Refresh { get; set; } = null!;

  /// <summary>
  ///   The UTC time to refresh the token.
  /// </summary>
  [JsonProperty("expiresUtc")]
  public DateTime ExpiresUtc { get; set; }
}