using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using log4net;

using Nullinside.Api.Common.Twitch;

using TwitchStreamingTools.Models;
using TwitchStreamingTools.Tts;

namespace TwitchStreamingTools.Services;

/// <summary>
///   Connects twitch chats to the TTS player.
/// </summary>
public class TwitchTtsService {
  private const int LOOP_DURATION_MILLISECONDS = 500;
  private readonly IList<TwitchChatTts> _chats = new List<TwitchChatTts>();

  /// <summary>
  ///   The logger.
  /// </summary>
  private readonly ILog _logger = LogManager.GetLogger(typeof(TwitchTtsService));

  private readonly Thread _thread;
  private readonly ITwitchClientProxy _twitchClientProxy;

  /// <summary>
  ///   Initializes a new instance of the <see cref="TwitchTtsService" /> class.
  /// </summary>
  /// <param name="twitchClientProxy">The twitch chat client.</param>
  public TwitchTtsService(ITwitchClientProxy twitchClientProxy) {
    _twitchClientProxy = twitchClientProxy;
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
        List<string?>? shouldNotExist = _chats?
          .Select(c => c.ChatConfig?.TwitchChannel)
          .Except(Configuration.Instance.TwitchChats?.Select(c => c?.TwitchChannel) ?? [])
          .Where(i => !string.IsNullOrWhiteSpace(i))
          .ToList();

        if (null != shouldNotExist && shouldNotExist.Count > 0) {
          foreach (string? disconnect in shouldNotExist) {
            TwitchChatTts? chat = _chats?.FirstOrDefault(c => c.ChatConfig?.TwitchChannel == disconnect);
            if (null == chat) {
              continue;
            }

            chat.Dispose();
            _chats?.Remove(chat);
          }
        }

        List<string?>? missing = Configuration.Instance.TwitchChats?
          .Select(c => c.TwitchChannel)
          .Except(_chats?.Select(c => c.ChatConfig?.TwitchChannel) ?? [])
          .Where(i => !string.IsNullOrWhiteSpace(i))
          .ToList();

        if (null == missing || missing.Count == 0) {
          Thread.Sleep(LOOP_DURATION_MILLISECONDS);
          continue;
        }

        foreach (string? newChat in missing) {
          var tts = new TwitchChatTts(_twitchClientProxy, Configuration.Instance.TwitchChats?.FirstOrDefault(t => t.TwitchChannel == newChat));
          tts.Connect();
          _chats?.Add(tts);
        }
      }
      catch (Exception ex) {
        _logger.Error("Failed a TTS loop", ex);
      }
    } while (true);
  }
}