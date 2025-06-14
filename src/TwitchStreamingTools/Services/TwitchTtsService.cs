using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using log4net;

using Nullinside.Api.Common.Twitch;

using TwitchStreamingTools.Tts;
using TwitchStreamingTools.Utilities;

namespace TwitchStreamingTools.Services;

/// <summary>
///   Connects twitch chats to the TTS player whenever the configuration is updated.
/// </summary>
public class TwitchTtsService : ITwitchTtsService {
  /// <summary>
  ///   The length of time to wait between polls with nothing to do.
  /// </summary>
  private const int POLL_MILLISECONDS = 500;

  /// <summary>
  ///   The fully configured objects responsible for reading TTS chats.
  /// </summary>
  private readonly IList<TwitchChatTts> _chats = new List<TwitchChatTts>();

  /// <summary>
  ///   The application configuration.
  /// </summary>
  private readonly IConfiguration _configuration;

  /// <summary>
  ///   The logger.
  /// </summary>
  private readonly ILog _logger = LogManager.GetLogger(typeof(TwitchTtsService));

  /// <summary>
  ///   The thread that polls for updates to the configuration file.
  /// </summary>
  private readonly Thread _thread;

  /// <summary>
  ///   The twitch chat log.
  /// </summary>
  private readonly ITwitchChatLog _twitchChatLog;

  /// <summary>
  ///   The twitch chat client to forward chat messages from.
  /// </summary>
  private readonly ITwitchClientProxy _twitchClientProxy;

  /// <summary>
  ///   Initializes a new instance of the <see cref="TwitchTtsService" /> class.
  /// </summary>
  /// <param name="twitchClientProxy">The twitch chat client.</param>
  /// <param name="configuration">The application configuration.</param>
  /// <param name="twitchChatLog">The twitch chat log.</param>
  public TwitchTtsService(ITwitchClientProxy twitchClientProxy, IConfiguration configuration, ITwitchChatLog twitchChatLog) {
    _configuration = configuration;
    _twitchClientProxy = twitchClientProxy;
    _twitchChatLog = twitchChatLog;
    _thread = new Thread(Main) {
      IsBackground = true
    };

    _thread.Start();
  }

  /// <summary>
  ///   The main loop for this service.
  /// </summary>
  private void Main() {
    do {
      try {
        // Any chat we're currently connected to that isn't in the configuration file should be disconnected from.
        DisconnectChatsNotInConfig();

        // Any chat we're not currently connected to we should be.
        ConnectChatsInConfig();

        // Wait for a bit before checking again.
        Thread.Sleep(POLL_MILLISECONDS);
      }
      catch (Exception ex) {
        _logger.Error("Failed a TTS loop", ex);
      }
    } while (true);
  }

  /// <summary>
  ///   Connects to any configuration found in the config file that we are not currently connected to.
  /// </summary>
  private void ConnectChatsInConfig() {
    List<string?>? missing = _configuration.TwitchChats?
      .Select(c => c.TwitchChannel)
      .Except(_chats?.Select(c => c.ChatConfig?.TwitchChannel) ?? [])
      .Where(i => !string.IsNullOrWhiteSpace(i))
      .ToList();

    if (null == missing || missing.Count == 0) {
      return;
    }

    foreach (string? newChat in missing) {
      var tts = new TwitchChatTts(_configuration, _twitchClientProxy, _configuration.TwitchChats?.FirstOrDefault(t => t.TwitchChannel == newChat), _twitchChatLog);
      tts.Connect();
      _chats?.Add(tts);
    }
  }

  /// <summary>
  ///   Disconnects the twitch chats no longer in the configuration.
  /// </summary>
  private void DisconnectChatsNotInConfig() {
    List<string?>? chatsNotInConfig = _chats?
      .Select(c => c.ChatConfig?.TwitchChannel)
      .Except(_configuration.TwitchChats?.Select(c => c?.TwitchChannel) ?? [])
      .Where(i => !string.IsNullOrWhiteSpace(i))
      .ToList();

    if (null == chatsNotInConfig || chatsNotInConfig.Count <= 0) {
      return;
    }

    foreach (string? disconnect in chatsNotInConfig) {
      TwitchChatTts? chat = _chats?.FirstOrDefault(c => c.ChatConfig?.TwitchChannel == disconnect);
      if (null == chat) {
        continue;
      }

      chat.Dispose();
      _chats?.Remove(chat);
    }
  }
}