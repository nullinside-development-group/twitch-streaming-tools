using System;
using System.Timers;

using Avalonia.Input.Platform;

using Newtonsoft.Json;

namespace TwitchStreamingTools.Utilities;

/// <summary>
///   Polls the clipboard looking for a specific JSON message.
/// </summary>
public class ClipboardPoller<T> : IClipboardPoller<T> {
  /// <summary>
  ///   The callback to invoke when an OAuth token is found.
  /// </summary>
  private readonly Action<T> _callback;

  /// <summary>
  ///   The clipboard api.
  /// </summary>
  private readonly IClipboard _clipboard;

  /// <summary>
  ///   The timer that looks for the copied OAuth token on the clipboard.
  /// </summary>
  private readonly Timer _timer;

  /// <summary>
  ///   Initializes a new instance of the <see cref="ClipboardPoller{T}" /> class.
  /// </summary>
  /// <param name="clipboard">The clipboard api.</param>
  /// <param name="callback">The callback to invoke when an OAuth token is found.</param>
  public ClipboardPoller(IClipboard clipboard, Action<T> callback) {
    _clipboard = clipboard;
    _callback = callback;

    _timer = new Timer(100) {
      AutoReset = false
    };

    _timer.Elapsed += OnClipboardCheck;
    _timer.Start();
  }

  /// <inheritdoc />
  public void StopPolling() {
    _timer.Stop();
  }

  /// <summary>
  ///   Checks the clipboard for our token.
  /// </summary>
  /// <param name="sender">The invoker.</param>
  /// <param name="e">The event arguments.</param>
  private async void OnClipboardCheck(object? sender, ElapsedEventArgs e) {
    string? json = await _clipboard.GetTextAsync();
    if (string.IsNullOrWhiteSpace(json)) {
      _timer.Start();
      return;
    }

    try {
      var oauth = JsonConvert.DeserializeObject<T>(json);
      if (null == oauth) {
        return;
      }

      _callback(oauth);
      await _clipboard.SetTextAsync(null);
      return;
    }
    catch {
      // Do nothing, we expect this to happen when the string isn't valid.
    }

    _timer.Start();
  }
}