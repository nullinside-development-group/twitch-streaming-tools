using System.Collections.Generic;

using Avalonia.Input.Platform;

namespace TwitchStreamingTools;

/// <summary>
///   Constants used throughout the application.
/// </summary>
public static class Constants {
#if DEBUG
  /// <summary>
  ///   The twitch app client id.
  /// </summary>
  public static string TWITCH_CLIENT_ID = "cvipqhi9y6ri8yhv0w8ryxokxh0ebd";

  /// <summary>
  ///   The domain that the api service is hosted at.
  /// </summary>
  public static string API_SITE_DOMAIN = "localhost:5036";
#else
  /// <summary>
  /// The twitch app client id.
  /// </summary>
  public static string TWITCH_CLIENT_ID = "gi1eu8xu9tl6vkjqz4tjqkdzfmcq5h";

  /// <summary>
  /// The domain that the api service is hosted at.
  /// </summary>
  public static string API_SITE_DOMAIN = "nullinside.com";
#endif

  /// <summary>
  ///   The twitch app redirect link.
  /// </summary>
  public static string TWITCH_CLIENT_REDIRECT = $"http://{API_SITE_DOMAIN}/api/v1/user/twitch-login/twitch-streaming-tools";

  /// <summary>
  ///   The reference to the clipboard API.
  /// </summary>
  /// <remarks>This is a hack because it's hard to get to.</remarks>
  public static IClipboard? CLIPBOARD;

  /// <summary>
  ///   The twitch permissions to request.
  /// </summary>
  public static readonly IEnumerable<string> TWITCH_SCOPES = new[] {
    "analytics:read:extensions",
    "analytics:read:games",
    "bits:read",
    "channel:edit:commercial",
    "channel:manage:broadcast",
    "channel:manage:extensions",
    "channel:manage:polls",
    "channel:manage:predictions",
    "channel:manage:redemptions",
    "channel:manage:schedule",
    "channel:manage:videos",
    "channel:read:editors",
    "channel:read:goals",
    "channel:read:hype_train",
    "channel:read:polls",
    "channel:read:predictions",
    "channel:read:redemptions",
    "channel:read:stream_key",
    "channel:read:subscriptions",
    "clips:edit",
    "moderation:read",
    "moderator:manage:automod",
    "user:edit",
    "user:edit:follows",
    "user:manage:blocked_users",
    "user:read:blocked_users",
    "user:read:broadcast",
    "user:read:email",
    "user:read:follows",
    "user:read:subscriptions",
    "channel_subscriptions",
    "channel_commercial",
    "channel_editor",
    "user_follows_edit",
    "channel_read",
    "user_read",
    "user_blocks_read",
    "user_blocks_edit",
    "channel:moderate",
    "chat:edit",
    "chat:read",
    "whispers:read",
    "whispers:edit"
  };
}