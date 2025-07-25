﻿using System.Collections.Generic;

using Nullinside.Api.Common.Twitch;

using TwitchStreamingTools.Models;

namespace TwitchStreamingTools;

/// <summary>
///   The contract for the configuration of the application.
/// </summary>
public interface IConfiguration {
  /// <summary>
  ///   The username of the user logged in through the <see cref="OAuth" /> token.
  /// </summary>
  string? TwitchUsername { get; set; }

  /// <summary>
  ///   The twitch OAuth token.
  /// </summary>
  TwitchAccessToken? OAuth { get; set; }

  /// <summary>
  ///   The twitch application configuration for getting OAuth tokens.
  /// </summary>
  TwitchAppConfig? TwitchAppConfig { get; set; }

  /// <summary>
  ///   The collection of twitch chats we should read from.
  /// </summary>
  IEnumerable<TwitchChatConfiguration>? TwitchChats { get; set; }

  /// <summary>
  ///   The collection of usernames to skip reading the messages of.
  /// </summary>
  IEnumerable<string>? TtsUsernamesToSkip { get; set; }

  /// <summary>
  ///   The collection of phonetic pronunciations of words.
  /// </summary>
  IDictionary<string, string>? TtsPhonetics { get; set; }

  /// <summary>
  ///   The arguments to pass to sound stretch to manipulate TTS audio.
  /// </summary>
  SoundStretchArgs? SoundStretchArgs { get; set; }

  /// <summary>
  ///   True if "username says message" should be used as the template for TTS messages, false to read just the message.
  /// </summary>
  bool SayUsernameWithMessage { get; set; }

  /// <summary>
  ///   Writes the configuration file to disk.
  /// </summary>
  /// <returns>True if successful, false otherwise.</returns>
  bool WriteConfiguration();
}