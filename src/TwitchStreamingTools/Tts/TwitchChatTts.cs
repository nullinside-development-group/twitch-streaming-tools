using System;
using System.Collections.Concurrent;
using System.IO;
using System.Speech.Synthesis;
using System.Threading;

using NAudio.Wave;

using Nullinside.Api.Common.Twitch;

using TwitchLib.Client.Events;

using TwitchStreamingTools.Models;
using TwitchStreamingTools.Tts.TtsFilter;
using TwitchStreamingTools.Utilities;

namespace TwitchStreamingTools.Tts;

/// <summary>
///   A twitch chat text-to-speech client.
/// </summary>
public class TwitchChatTts : IDisposable, ITwitchChatTts {
  /// <summary>
  ///   The ordered list of chat messages to play.
  /// </summary>
  private readonly BlockingCollection<OnMessageReceivedArgs> _soundsToPlay;

  /// <summary>
  ///   The thread to play sounds on.
  /// </summary>
  private readonly Thread _soundThread;

  /// <summary>
  ///   Filters for modifying an incoming message before it is passed to text to speech.
  /// </summary>
  private readonly ITtsFilter[] _ttsFilters = { new LinkFilter(), new UsernameSkipFilter(), new UsernameRemoveCharactersFilter(), new PhoneticFilter(), new CommandFilter(), new EmojiDeduplicationFilter(), new WordSpamFilter() };

  /// <summary>
  ///   The lock for ensuring mutual exclusion on the <see cref="_ttsSoundOutput" /> object.
  /// </summary>
  private readonly Lock _ttsSoundOutputLock = new();

  /// <summary>
  ///   The lock for ensuring mutual exclusion on the <see cref="_ttsSoundOutputSignal" /> object.
  /// </summary>
  private readonly Lock _ttsSoundOutputSignalLock = new();

  /// <summary>
  ///   The twitch chat client used to gather chat messages.
  /// </summary>
  private readonly ITwitchClientProxy _twitchClient;

  /// <summary>
  ///   The number of messages to skip in the message queue.
  /// </summary>
  private int _messageToSkip;

  /// <summary>
  ///   The poison pill to kill the sound thread.
  /// </summary>
  private bool _poisonPill;

  /// <summary>
  ///   The text-to-speech sound player.
  /// </summary>
  private WaveOutEvent? _ttsSoundOutput;

  /// <summary>
  ///   The signal used to make sound output synchronous.
  /// </summary>
  private ManualResetEvent? _ttsSoundOutputSignal;

  /// <summary>
  ///   Initializes a new instance of the <see cref="TwitchChatTts" /> class.
  /// </summary>
  public TwitchChatTts(ITwitchClientProxy twitchClient, TwitchChatConfiguration? config) {
    _twitchClient = twitchClient;
    ChatConfig = config;
    _soundsToPlay = new BlockingCollection<OnMessageReceivedArgs>();
    _soundThread = new Thread(PlaySoundsThread) {
      Name = "TwitchChatTts Thread",
      IsBackground = true
    };
    _soundThread.Start();
  }

  /// <summary>
  ///   Releases unmanaged resources.
  /// </summary>
  public void Dispose() {
    if (null != ChatConfig?.TwitchChannel) {
      _twitchClient.RemoveMessageCallback(ChatConfig.TwitchChannel, Client_OnMessageReceived);
    }

    _poisonPill = true;
    _soundsToPlay.Add(new OnMessageReceivedArgs());

    lock (_ttsSoundOutputLock) {
      _ttsSoundOutputSignal?.Set();
      _ttsSoundOutputSignal?.Dispose();
      _ttsSoundOutputSignal = null;
    }

    lock (_ttsSoundOutputSignalLock) {
      _ttsSoundOutput?.Stop();
      _ttsSoundOutput?.Dispose();
      _ttsSoundOutput = null;
    }

    if (_soundThread.Join(5000)) {
      _soundThread.Interrupt();
    }
  }

  /// <summary>
  ///   The configuration for the twitch chat.
  /// </summary>
  public TwitchChatConfiguration? ChatConfig { get; }

  /// <summary>
  ///   The username being used to connect to twitch chat.
  /// </summary>
  public string? CurrentUsername { get; set; }

  /// <summary>
  ///   Connects to the chat to listen for messages to read in text to speech.
  /// </summary>
  public void Connect() {
    if (null == ChatConfig) {
      return;
    }

    if (ChatConfig.TwitchChannel != null) {
      _twitchClient.AddMessageCallback(ChatConfig.TwitchChannel, Client_OnMessageReceived);
    }
  }

  /// <summary>
  ///   Pauses the text to speech.
  /// </summary>
  public void Pause() {
    lock (_ttsSoundOutputLock) {
      _ttsSoundOutput?.Pause();
    }
  }

  /// <summary>
  ///   Continues the text to speech.
  /// </summary>
  public void Unpause() {
    lock (_ttsSoundOutputLock) {
      _ttsSoundOutput?.Play();
    }
  }

  /// <summary>
  ///   Skips the current TTS.
  /// </summary>
  public void SkipCurrentTts() {
    _ttsSoundOutput?.Stop();
  }

  /// <summary>
  ///   Skips all TTSes in the queue.
  /// </summary>
  public void SkipAllTts() {
    _messageToSkip = _soundsToPlay.Count;
    _ttsSoundOutput?.Stop();
  }

  /// <summary>
  ///   The main thread used to play sound asynchronously.
  /// </summary>
  private void PlaySoundsThread() {
    while (!_poisonPill) {
      try {
        // Loop through each chat message we've received from the twitch chat client.
        foreach (OnMessageReceivedArgs e in _soundsToPlay.GetConsumingEnumerable()) {
          if (_poisonPill) {
            return;
          }

          // If we are currently skipping messages, skip them and decrement the skipper.
          if (_messageToSkip > 0) {
            --_messageToSkip;
            Console.WriteLine($"Skipping: {e.ChatMessage.Username} says {e.ChatMessage.Message}");
            continue;
          }

          // Debug
          Console.WriteLine($"Running: {e.ChatMessage.Username} says {e.ChatMessage.Message}");

          // Go through the TTS filters which modify the chat message before it is passed to TTS.
          Tuple<string, string> convertedChatEvent = ConvertChatMessage(e);

          // If we don't have a chat message then the message was completely filtered out and we have nothing
          // to do here.
          if (string.IsNullOrWhiteSpace(convertedChatEvent.Item2.Trim())) {
            continue;
          }

          // todo: #51 make this a configuration option and a filter.
          string chatMessage = $"{convertedChatEvent.Item1} says {convertedChatEvent.Item2}";

          // Converts the text to an audio stream and plays it.
          InitializeAndPlayTts(chatMessage, e.ChatMessage.Username);
        }
      }
      catch (Exception ex) {
        Console.WriteLine($"Got expection playing message: {ex}");
      }
    }
  }

  /// <summary>
  ///   Converts the <paramref name="chatMessage" /> to an audio stream and plays it.
  /// </summary>
  /// <param name="sender">The twitch chat login of the user that sent the message.</param>
  /// <param name="chatMessage">The chat message to convert to TTS.</param>
  private void InitializeAndPlayTts(string sender, string chatMessage) {
    WaveFileReader reader;
    try {
      // Create a microsoft TTS object and a stream for outputting its audio file to.
      using var synth = new SpeechSynthesizer();
      using var stream = new MemoryStream();

      // Setup the microsoft TTS object according to the settings.
      synth.SetOutputToWaveStream(stream);
      synth.SelectVoice(ChatConfig?.TtsVoice);
      synth.Volume = (int)(ChatConfig?.TtsVolume ?? 50);
      synth.Speak(chatMessage);

      // Now that we filled the stream, seek to the beginning so we can play it.
      stream.Seek(0, SeekOrigin.Begin);
      reader = new WaveFileReader(stream);
    }
    catch (Exception ex) {
      Console.WriteLine($"Exception initializing a new microsoft speech object: {ex}");
      return;
    }

    // This is only meant to ensure we don't play TTS over a sound alert. It can still happen but this
    // fixes most of the issues with very little investment.
    while (GlobalSoundManager.Instance.CurrentlyPlayingSound) {
      Thread.Sleep(100);
    }

    try {
      // Make sure we lock the objects used on multiple threads and play the file.
      lock (_ttsSoundOutputLock)
      lock (_ttsSoundOutputSignalLock) {
        _ttsSoundOutput = new WaveOutEvent();
        _ttsSoundOutputSignal = new ManualResetEvent(false);

        _ttsSoundOutput.DeviceNumber = NAudioUtilities.GetOutputDeviceIndex(ChatConfig?.OutputDevice);
        _ttsSoundOutput.Volume = (ChatConfig?.TtsVolume ?? 50.0f) / 100.0f;

        _ttsSoundOutput.Init(reader);

        // Play is async so we will make it synchronous here so we don't have to deal with
        // queueing. We can improve this to remove the hack in the future.
        _ttsSoundOutput.PlaybackStopped += delegate {
          lock (_ttsSoundOutputSignalLock) {
            _ttsSoundOutputSignal?.Set();
          }
        };

        // Play it.
        _ttsSoundOutput.Play();
      }

      // Wait for the play to finish, we will get signaled.
      CurrentUsername = sender;
      ManualResetEvent? signal = _ttsSoundOutputSignal;
      signal?.WaitOne();
      CurrentUsername = null;
    }
    finally {
      // Finally dispose of everything safely in the lock.
      lock (_ttsSoundOutputLock)
      lock (_ttsSoundOutputSignalLock) {
        _ttsSoundOutput?.Dispose();
        _ttsSoundOutput = null;
        _ttsSoundOutputSignal?.Dispose();
        _ttsSoundOutputSignal = null;
      }
    }
  }

  /// <summary>
  ///   Passes the chat message through all defined <see cref="_ttsFilters" />s to convert its text for TTS.
  /// </summary>
  /// <param name="chatEvent">The twitch chat event.</param>
  /// <returns>A tuple where Item1 is the login of the sender and Item2 is the message the user sent.</returns>
  private Tuple<string, string> ConvertChatMessage(OnMessageReceivedArgs chatEvent) {
    var chatMessageInfo = new Tuple<string, string>(chatEvent.ChatMessage.DisplayName, chatEvent.ChatMessage.Message);
    foreach (ITtsFilter filter in _ttsFilters) {
      try {
        chatMessageInfo = filter.Filter(chatEvent, chatMessageInfo.Item1, chatMessageInfo.Item2);
      }
      catch (Exception ex) {
        // Do not let a single filter fail the loop for all filters.
        Console.WriteLine($"Got expection evaluating filters on message: {ex}");
      }
    }

    return chatMessageInfo;
  }

  /// <summary>
  ///   Event called when a message is received in twitch chat.
  /// </summary>
  /// <param name="e">The chat message information.</param>
  private void Client_OnMessageReceived(OnMessageReceivedArgs e) {
    Console.WriteLine($"Adding: {e.ChatMessage.Username} says {e.ChatMessage.Message}");
    try {
      _soundsToPlay.Add(e);
    }
    catch (Exception ex) {
      Console.WriteLine($"Failed to add: {e.ChatMessage.Username} says {e.ChatMessage.Message}\r\n{ex}");
    }
  }
}