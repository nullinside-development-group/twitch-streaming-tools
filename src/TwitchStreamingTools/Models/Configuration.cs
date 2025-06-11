using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

using Nullinside.Api.Common.Twitch;

namespace TwitchStreamingTools.Models;

/// <summary>
///   The configuration of the application.
/// </summary>
public class Configuration {
  private static readonly string CONFIG_LOCATION =
    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "nullinside",
      "twitch-streaming-tools", "config.json");

  private static Configuration? s_instance;

  /// <summary>
  ///   The singleton instance of the class.
  /// </summary>
  public static Configuration Instance {
    get {
      if (null == s_instance) {
        s_instance = ReadConfiguration() ?? new Configuration();
      }

      return s_instance;
    }
  }

  /// <summary>
  ///   The username of the user logged in through the <see cref="OAuth" /> token.
  /// </summary>
  public string? TwitchUsername { get; set; }

  /// <summary>
  ///   The twitch OAuth token.
  /// </summary>
  public OAuthResponse? OAuth { get; set; }

  /// <summary>
  ///   The twitch application configuration for getting OAuth tokens.
  /// </summary>
  public TwitchAppConfig? TwitchAppConfig { get; set; }

  /// <summary>
  ///   The collection of twitch chats we should read from.
  /// </summary>
  public IEnumerable<TwitchChatConfiguration>? TwitchChats { get; set; }

  /// <summary>
  ///   The collection of usernames to skip reading the messages of.
  /// </summary>
  public IEnumerable<string>? TtsUsernamesToSkip { get; set; }

  /// <summary>
  ///   The collection of phonetic pronunciations of words.
  /// </summary>
  public IDictionary<string, string>? TtsPhoneticUsernames { get; set; }

  /// <summary>
  ///   Reads the configuration from disk.
  /// </summary>
  /// <returns>The configuration if successful, null otherwise.</returns>
  private static Configuration? ReadConfiguration() {
    try {
      string json = File.ReadAllText(CONFIG_LOCATION);
      return JsonConvert.DeserializeObject<Configuration>(json);
    }
    catch { return null; }
  }

  /// <summary>
  ///   Writes the configuration file to disk.
  /// </summary>
  /// <returns>True if successful, false otherwise.</returns>
  public bool WriteConfiguration() {
    try {
      Directory.CreateDirectory(Path.GetDirectoryName(CONFIG_LOCATION)!);

      string json = JsonConvert.SerializeObject(this);
      File.WriteAllText(CONFIG_LOCATION, json);
      return true;
    }
    catch {
      return false;
    }
  }

  /// <summary>
  ///   Represents a single connection to a twitch chat by a single user.
  /// </summary>
  public class TwitchChatConfiguration {
    /// <summary>
    ///   Gets or sets the output device to send audio to.
    /// </summary>
    public string? OutputDevice { get; set; }

    /// <summary>
    ///   Gets or sets a value indicating whether text to speech is on.
    /// </summary>
    public bool TtsOn { get; set; }

    /// <summary>
    ///   Gets or sets the selected Microsoft Text to Speech voice.
    /// </summary>
    public string? TtsVoice { get; set; }

    /// <summary>
    ///   Gets or sets the volume of the text to speech voice.
    /// </summary>
    public uint TtsVolume { get; set; }

    /// <summary>
    ///   Gets or sets the twitch channel to read chat from.
    /// </summary>
    public string? TwitchChannel { get; set; }
  }
}