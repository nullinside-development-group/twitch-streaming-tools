using System;
using System.Runtime.InteropServices;
using System.Threading;

using log4net;

using PInvoke;

using TwitchStreamingTools.Models;

namespace TwitchStreamingTools.Services;

/// <summary>
///   Detects when keys are pressed anywhere on the OS.
/// </summary>
public class GlobalKeyPressService {
  /// <summary>
  ///   The logger.
  /// </summary>
  private static readonly ILog LOGGER = LogManager.GetLogger(typeof(GlobalKeyPressService));

  /// <summary>
  ///   The pointer to the hook we added.
  /// </summary>
  private static User32.SafeHookHandle? s_hook;

  /// <summary>
  ///   The thread to execute on.
  /// </summary>
  private readonly Thread _thread;

  /// <summary>
  ///   Initializes a new instance of the <see cref="GlobalKeyPressService" /> class.
  /// </summary>
  public GlobalKeyPressService() {
    _thread = new Thread(Main) {
      IsBackground = true
    };

    _thread.Start();
  }

  /// <summary>
  ///   Gets or sets the callbacks to invoke when a keystroke is pressed.
  /// </summary>
  public static Action<string>? OnKeystroke { get; set; }

  /// <summary>
  ///   The main loop which registers for keystrokes on the system and flushes the message buffer.
  /// </summary>
  private void Main() {
    // Set a hook.
    s_hook = User32.SetWindowsHookEx(User32.WindowsHookType.WH_KEYBOARD_LL, KeystrokeCallback, IntPtr.Zero, 0);

    unsafe {
      // Buffer the messages.
      User32.MSG message;
      while (0 != User32.GetMessage(&message, IntPtr.Zero, 0, 0)) { }
    }
  }

  /// <summary>
  ///   The event invoked when a key is pressed.
  /// </summary>
  /// <param name="nCode">The code.</param>
  /// <param name="wParam">The <seealso cref="KeyboardMessage" />.</param>
  /// <param name="lParam">The <seealso cref="KeyboardLowLevelHookStruct" />.</param>
  /// <returns>The next hook that should be called.</returns>
  private static int KeystrokeCallback(int nCode, IntPtr wParam, IntPtr lParam) {
    var keyboardEvent = Marshal.PtrToStructure<KeyboardLowLevelHookStruct>(lParam);
    var whatHappened = (KeyboardMessage)wParam;

    if (whatHappened == KeyboardMessage.KEY_DOWN) {
      var key = (Keys)keyboardEvent.vkCode;
      LOGGER.Debug($"Key pressed: {key}");
      OnKeystroke?.Invoke(key.ToString());
    }

    return User32.CallNextHookEx(s_hook?.DangerousGetHandle() ?? 0, nCode, wParam, lParam);
  }
}