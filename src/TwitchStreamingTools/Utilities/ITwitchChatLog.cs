using System;
using System.Collections.Generic;

using TwitchStreamingTools.Models;

namespace TwitchStreamingTools.Utilities;

/// <summary>
///   The contract for a logger that keeps old twitch messages.
/// </summary>
public interface ITwitchChatLog {
  /// <summary>
  ///   The maximum number of messages to keep in the log, anything larger than this number will be deleted starting with
  ///   the oldest messages.
  /// </summary>
  public int MaximumMessageCount { get; set; }

  /// <summary>
  ///   The maximum message age before the message should be deleted, anything older than this will be deleted starting
  ///   with the oldest messages.
  /// </summary>
  public TimeSpan MaximumMessageAge { get; set; }

  /// <summary>
  ///   Retrieves all messages.
  /// </summary>
  /// <param name="channel">If set, filter messages to those sent in a channel, otherwise all channels.</param>
  /// <returns>All known twitch messages.</returns>
  public IEnumerable<TwitchChatMessage> GetMessages(string? channel = null);

  /// <summary>
  ///   Adds a twitch chat log.
  /// </summary>
  /// <param name="message">The chat messages.</param>
  public void AddMessage(TwitchChatMessage message);
}