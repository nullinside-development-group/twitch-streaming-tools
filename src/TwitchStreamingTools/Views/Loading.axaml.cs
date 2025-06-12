using Avalonia;
using Avalonia.Controls;

namespace TwitchStreamingTools.Views;

/// <summary>
///   A loading icon.
/// </summary>
public partial class Loading : UserControl {
  /// <summary>
  ///   The width of the icon.
  /// </summary>
  public static readonly StyledProperty<int> WidthProperty =
    AvaloniaProperty.Register<Loading, int>(nameof(Width), 1);

  /// <summary>
  ///   Initializes a new instance of the <see cref="Loading" /> class.
  /// </summary>
  public Loading() {
    InitializeComponent();
  }

  /// <summary>
  ///   The width of the icon.
  /// </summary>
  public int Width {
    get => GetValue(WidthProperty);
    set => SetValue(WidthProperty, value);
  }
}