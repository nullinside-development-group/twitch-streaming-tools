using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;

using Nullinside.Api.Common.Twitch;

using ReactiveUI;

using TwitchLib.Client.Events;
using TwitchLib.Client.Interfaces;

using TwitchStreamingTools.Models;

namespace TwitchStreamingTools.ViewModels.Pages;

/// <summary>
///   Handles settings up and viewing chat.
/// </summary>
public class ChatViewModel : PageViewModelBase, IDisposable {
  /// <summary>
  ///   The list of chat names selected in the list.
  /// </summary>
  private ObservableCollection<string> _selectedTwitchChatNames = [];
  
  /// <summary>
  /// The twitch chat client.
  /// </summary>
  private ITwitchClientProxy _twitchClient;

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
    foreach (string channel in Configuration.Instance.TwitchChats ?? []) {
      _selectedTwitchChatNames.Add(channel);
      _twitchClient.AddMessageCallback(channel, OnChatMessage);
    }

    OnAddChat = ReactiveCommand.Create(() => {
      string? username = TwitchChatName?.Trim();
      if (string.IsNullOrWhiteSpace(username)) {
        return;
      }

      _selectedTwitchChatNames.Remove(username);
      _selectedTwitchChatNames.Add(username);
      _ = _twitchClient.AddMessageCallback(username, OnChatMessage);

      TwitchChatName = null;
      Configuration.Instance.TwitchChats = _selectedTwitchChatNames;
      Configuration.Instance.WriteConfiguration();
    });

    OnRemoveChat = ReactiveCommand.Create<string>(channel => {
      _selectedTwitchChatNames.Remove(channel);
      _twitchClient.RemoveMessageCallback(channel, OnChatMessage);
      Configuration.Instance.TwitchChats = _selectedTwitchChatNames;
      Configuration.Instance.WriteConfiguration();
    });
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
    OnAddChat.Dispose();
    OnRemoveChat.Dispose();
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
}