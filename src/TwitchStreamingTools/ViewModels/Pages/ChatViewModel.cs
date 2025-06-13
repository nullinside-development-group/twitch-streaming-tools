using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Speech.Synthesis;
using System.Threading.Tasks;

using Nullinside.Api.Common.Twitch;

using ReactiveUI;

using TwitchLib.Client.Events;

using TwitchStreamingTools.Models;

namespace TwitchStreamingTools.ViewModels.Pages;

/// <summary>
///   Handles settings up and viewing chat.
/// </summary>
public class ChatViewModel : PageViewModelBase, IDisposable {
  /// <summary>
  ///   The twitch chat client.
  /// </summary>
  private readonly ITwitchClientProxy _twitchClient;

  /// <summary>
  ///   The list of chat names selected in the list.
  /// </summary>
  private ObservableCollection<string> _selectedTwitchChatNames = [];

  /// <summary>
  ///   The current position of the cursor for the text box showing our chat logs, increment to move down.
  /// </summary>
  private int _textBoxCursorPosition;

  /// <summary>
  ///   The current twitch chat.
  /// </summary>
  private string? _twitchChat = string.Empty;

  /// <summary>
  ///   The current twitch chat name entered by the user.
  /// </summary>
  private string? _twitchChatName;

  /// <summary>
  ///   Initializes a new instance of the <see cref="ChatViewModel" /> class.
  /// </summary>
  public ChatViewModel(ITwitchClientProxy twitchClient) {
    _twitchClient = twitchClient;

    OnAddChat = ReactiveCommand.Create(OnAddChatCommand);
    OnRemoveChat = ReactiveCommand.Create<string>(OnRemoveChatCommand);
  }

  /// <inheritdoc />
  public override string IconResourceKey { get; } = "ChatRegular";

  /// <summary>
  ///   Called when adding a chat.
  /// </summary>
  public ReactiveCommand<Unit, Unit> OnAddChat { get; }

  /// <summary>
  ///   Called when removing a chat.
  /// </summary>
  public ReactiveCommand<string, Unit> OnRemoveChat { get; }

  /// <summary>
  ///   The current twitch chat name entered by the user.
  /// </summary>
  public string? TwitchChatName {
    get => _twitchChatName;
    set => this.RaiseAndSetIfChanged(ref _twitchChatName, value);
  }

  /// <summary>
  ///   The current twitch chat.
  /// </summary>
  public string? TwitchChat {
    get => _twitchChat;
    set => this.RaiseAndSetIfChanged(ref _twitchChat, value);
  }

  /// <summary>
  ///   The list of chat names selected in the list.
  /// </summary>
  public ObservableCollection<string> ChatItems {
    get => _selectedTwitchChatNames;
    set => this.RaiseAndSetIfChanged(ref _selectedTwitchChatNames, value);
  }

  /// <summary>
  ///   The current position of the cursor for the text box showing our chat logs, increment to move down.
  /// </summary>
  public int TextBoxCursorPosition {
    get => _textBoxCursorPosition;
    set => this.RaiseAndSetIfChanged(ref _textBoxCursorPosition, value);
  }

  /// <inheritdoc />
  public void Dispose() {
    foreach (TwitchChatConfiguration channel in Configuration.Instance.TwitchChats ?? []) {
      if (string.IsNullOrWhiteSpace(channel.TwitchChannel)) {
        continue;
      }

      _twitchClient.RemoveMessageCallback(channel.TwitchChannel, OnChatMessage);
    }

    OnAddChat.Dispose();
    OnRemoveChat.Dispose();
  }

  /// <summary>
  ///   Handles adding a new chat to monitor.
  /// </summary>
  private void OnAddChatCommand() {
    // Ensure we have a username
    string? username = TwitchChatName?.Trim();
    if (string.IsNullOrWhiteSpace(username) || _selectedTwitchChatNames.Contains(username)) {
      return;
    }

    _selectedTwitchChatNames.Add(username);

    // Add the callback, we don't need to wait for it to return it can run in the background.
    _ = _twitchClient.AddMessageCallback(username, OnChatMessage);


    using var speech = new SpeechSynthesizer();
    TwitchChatName = null;
    Configuration.Instance.TwitchChats = (
      from user in _selectedTwitchChatNames
      select new TwitchChatConfiguration {
        TwitchChannel = user,
        OutputDevice = Configuration.GetDefaultAudioDevice(),
        TtsOn = true,
        TtsVoice = Configuration.GetDefaultTtsVoice(),
        TtsVolume = 100
      }).ToList();
    Configuration.Instance.WriteConfiguration();
  }

  private void OnRemoveChatCommand(string channel) {
    _selectedTwitchChatNames.Remove(channel);
    _twitchClient.RemoveMessageCallback(channel, OnChatMessage);
    using var speech = new SpeechSynthesizer();
    Configuration.Instance.TwitchChats = (
      from user in _selectedTwitchChatNames
      select new TwitchChatConfiguration {
        TwitchChannel = user,
        OutputDevice = Configuration.GetDefaultAudioDevice(),
        TtsOn = true,
        TtsVoice = Configuration.GetDefaultTtsVoice(),
        TtsVolume = 100
      }).ToList();

    Configuration.Instance.WriteConfiguration();
  }

  private void OnChatMessage(OnMessageReceivedArgs msg) {
    if (_selectedTwitchChatNames.Count > 1) {
      TwitchChat = (TwitchChat + $"\n({msg.ChatMessage.Channel}) {msg.ChatMessage.Username}: {msg.ChatMessage.Message}").Trim();
    }
    else {
      TwitchChat = (TwitchChat + $"\n{msg.ChatMessage.Username}: {msg.ChatMessage.Message}").Trim();
    }

    TextBoxCursorPosition = int.MaxValue;
  }

  /// <summary>
  ///   Handles registering for twitch chat messages while the UI is open.
  /// </summary>
  public override void OnLoaded() {
    base.OnLoaded();

    foreach (TwitchChatConfiguration channel in Configuration.Instance.TwitchChats ?? []) {
      if (string.IsNullOrWhiteSpace(channel.TwitchChannel)) {
        continue;
      }

      _selectedTwitchChatNames.Add(channel.TwitchChannel);
      Task.Run(() => _twitchClient.AddMessageCallback(channel.TwitchChannel, OnChatMessage));
    }
  }

  /// <summary>
  ///   Handles unregistering for twitch chat messages while the UI is closed.
  /// </summary>
  public override void OnUnloaded() {
    base.OnUnloaded();
    Dispose();
  }
}