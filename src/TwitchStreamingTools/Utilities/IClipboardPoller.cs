namespace TwitchStreamingTools.Utilities;

/// <summary>
///   Handles polling the clipboard for a specific json message.
/// </summary>
public interface IClipboardPoller<T> {
  /// <summary>
  ///   Stops polling the clipboard.
  /// </summary>
  public void StopPolling();
}