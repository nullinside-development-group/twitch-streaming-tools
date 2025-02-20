using System.Collections.ObjectModel;
using System.Reactive;

using ReactiveUI;

namespace TwitchStreamingTools.ViewModels.Pages;

/// <summary>
///   Handles settings up and viewing chat.
/// </summary>
public class ChatViewModel : PageViewModelBase {
  /// <summary>
  ///   The list of chat names selected in the list.
  /// </summary>
  private ObservableCollection<string> _selectedTwitchChatNames = [];

  /// <summary>
  ///   The current twitch chat name entered by the user.
  /// </summary>
  private string? _twitchChatName;

  /// <summary>
  ///   Initializes a new instance of the <see cref="ChatViewModel" /> class.
  /// </summary>
  public ChatViewModel() {
    OnAddChat = ReactiveCommand.Create(() => {
      string? username = TwitchChatName?.Trim();
      if (string.IsNullOrWhiteSpace(username)) {
        return;
      }

      _selectedTwitchChatNames.Remove(username);
      _selectedTwitchChatNames.Add(username);
      TwitchChatName = null;
    });

    OnRemoveChat = ReactiveCommand.Create<string>(s => {
      _selectedTwitchChatNames.Remove(s);
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
  ///   The list of chat names selected in the list.
  /// </summary>
  public ObservableCollection<string> ChatItems {
    get => _selectedTwitchChatNames;
    set => this.RaiseAndSetIfChanged(ref _selectedTwitchChatNames, value);
  }
}