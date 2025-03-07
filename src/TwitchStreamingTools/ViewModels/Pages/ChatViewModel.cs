using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Channels;

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
  ///   The current twitch chat name entered by the user.
  /// </summary>
  private string? _twitchChatName;
  
  /// <summary>
  ///   The current twitch chat.
  /// </summary>
  private string _twitchChat = String.Empty;

  /// <summary>
  ///   Initializes a new instance of the <see cref="ChatViewModel" /> class.
  /// </summary>
  public ChatViewModel() {
    foreach (string channel in Configuration.Instance.TwitchChats ?? []) {
      _selectedTwitchChatNames.Add(channel);
    }

    OnAddChat = ReactiveCommand.Create(() => {
      string? username = TwitchChatName?.Trim();
      if (string.IsNullOrWhiteSpace(username)) {
        return;
      }

      _selectedTwitchChatNames.Remove(username);
      _selectedTwitchChatNames.Add(username);
      TwitchClientProxy.Instance.AddMessageCallback(username, OnChatMessage).Wait();
      
      TwitchChatName = null;
      Configuration.Instance.TwitchChats = _selectedTwitchChatNames;
      Configuration.Instance.WriteConfiguration();
    });

    OnRemoveChat = ReactiveCommand.Create<string>(s => {
      _selectedTwitchChatNames.Remove(s);
      // todo: disconnect from chat
      Configuration.Instance.TwitchChats = _selectedTwitchChatNames;
      Configuration.Instance.WriteConfiguration();
    });
    
    TwitchClientProxy.Instance.AddInstanceCallback(OnNewTwitchClient);
    OnNewTwitchClient(TwitchClientProxy.Instance);
  }

  private void OnNewTwitchClient(TwitchClientProxy chatClient) {
    foreach (var channel in _selectedTwitchChatNames) {
      chatClient.AddMessageCallback(channel, OnChatMessage);
    }
  }

  private void OnChatMessage(OnMessageReceivedArgs msg) {
    if (_selectedTwitchChatNames.Count > 1) {
      TwitchChat = (TwitchChat + $"\n({msg.ChatMessage.Channel}) {msg.ChatMessage.Username}: {msg.ChatMessage.Message}").Trim();
    }
    else {
      TwitchChat = (TwitchChat + $"\n{msg.ChatMessage.Username}: {msg.ChatMessage.Message}").Trim();
    }
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

  /// <inheritdoc />
  public void Dispose() {
    OnAddChat.Dispose();
    OnRemoveChat.Dispose();
  }
}