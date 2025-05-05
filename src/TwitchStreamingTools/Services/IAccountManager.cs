using System;
using System.Threading.Tasks;

using Nullinside.Api.Common.Twitch;

namespace TwitchStreamingTools.Services;

/// <summary>
///   The contract for management credentials in the application.
/// </summary>
public interface IAccountManager {
  /// <summary>
  ///   The current OAuth's twitch username.
  /// </summary>
  string? TwitchUsername { get; set; }

  /// <summary>
  ///   A flag indicating whether the credentials are currently valid.
  /// </summary>
  bool CredentialsAreValid { get; set; }

  /// <summary>
  ///   An event indicating that the current status of the credentials has changed.
  /// </summary>
  Action<bool>? OnCredentialsStatusChanged { get; set; }

  /// <summary>
  ///   An event indicating that the credentials have changed.
  /// </summary>
  Action<TwitchAccessToken?>? OnCredentialsChanged { get; set; }

  /// <summary>
  ///   Updates the credentials.
  /// </summary>
  /// <param name="bearer">The bearer token.</param>
  /// <param name="refresh">The refresh token.</param>
  /// <param name="expires">The <seealso cref="DateTime" /> when the credentials expire.</param>
  Task UpdateCredentials(string bearer, string refresh, DateTime expires);

  /// <summary>
  ///   Clears out the credentials.
  /// </summary>
  void DeleteCredentials();
}