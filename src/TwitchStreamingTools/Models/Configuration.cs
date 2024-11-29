﻿using System;
using System.IO;

using Newtonsoft.Json;

using Nullinside.Api.Common.Twitch;

namespace TwitchStreamingTools.Models;

/// <summary>
///   The configuration of the application.
/// </summary>
public class Configuration {
  private static readonly string s_configLocation =
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
  ///   The twitch OAuth token.
  /// </summary>
  public OAuthResponse? OAuth { get; set; }

  /// <summary>
  ///   The twitch application configuration for getting OAuth tokens.
  /// </summary>
  public TwitchAppConfig? TwitchAppConfig { get; set; }

  private static Configuration? ReadConfiguration() {
    try {
      string json = File.ReadAllText(s_configLocation);
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
      Directory.CreateDirectory(Path.GetDirectoryName(s_configLocation)!);

      string json = JsonConvert.SerializeObject(this);
      File.WriteAllText(s_configLocation, json);
      return true;
    }
    catch {
      return false;
    }
  }
}