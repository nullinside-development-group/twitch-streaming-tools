using System;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace TwitchStreamingTools.Views;

/// <summary>
///   The window menu bar at the top of the UI
/// </summary>
/// <remarks>From: https://github.com/FrankenApps/Avalonia-CustomTitleBarTemplate.git</remarks>
public partial class WindowsTitleBar : UserControl {
  /// <summary>
  ///   A flag indicating whether the system bar should be seemless with other content or have its own vertical space.
  /// </summary>
  public static readonly StyledProperty<bool> IsSeamlessProperty =
    AvaloniaProperty.Register<WindowsTitleBar, bool>(nameof(IsSeamless));

  private readonly Button closeButton;
  private readonly NativeMenuBar defaultMenuBar;
  private readonly Button maximizeButton;
  private readonly Path maximizeIcon;
  private readonly ToolTip maximizeToolTip;
  private readonly Button minimizeButton;
  private readonly NativeMenuBar seamlessMenuBar;
  private readonly TextBlock systemChromeTitle;

  private readonly DockPanel titleBar;
  private readonly DockPanel titleBarBackground;
  private readonly Image windowIcon;

  /// <summary>
  ///   Initializes a new instance of the <see cref="WindowsTitleBar" /> class.
  /// </summary>
  public WindowsTitleBar() {
    InitializeComponent();
    minimizeButton = this.FindControl<Button>("MinimizeButton")!;
    maximizeButton = this.FindControl<Button>("MaximizeButton")!;
    maximizeIcon = this.FindControl<Path>("MaximizeIcon")!;
    maximizeToolTip = this.FindControl<ToolTip>("MaximizeToolTip")!;
    closeButton = this.FindControl<Button>("CloseButton")!;
    windowIcon = this.FindControl<Image>("WindowIcon")!;

    minimizeButton.Click += MinimizeWindow!;
    maximizeButton.Click += MaximizeWindow!;
    closeButton.Click += CloseWindow!;
    windowIcon.DoubleTapped += CloseWindow!;

    titleBar = this.FindControl<DockPanel>("TitleBar")!;
    titleBarBackground = this.FindControl<DockPanel>("TitleBarBackground")!;
    systemChromeTitle = this.FindControl<TextBlock>("SystemChromeTitle")!;
    seamlessMenuBar = this.FindControl<NativeMenuBar>("SeamlessMenuBar")!;
    defaultMenuBar = this.FindControl<NativeMenuBar>("DefaultMenuBar")!;

    SubscribeToWindowState();
  }

  /// <summary>
  ///   A flag indicating whether the system bar should be seemless with other content or have its own vertical space.
  /// </summary>
  public bool IsSeamless {
    get => GetValue(IsSeamlessProperty);
    set {
      SetValue(IsSeamlessProperty, value);
      if (titleBarBackground != null &&
          systemChromeTitle != null &&
          seamlessMenuBar != null &&
          defaultMenuBar != null) {
        titleBarBackground.IsVisible = IsSeamless ? false : true;
        systemChromeTitle.IsVisible = IsSeamless ? false : true;
        seamlessMenuBar.IsVisible = IsSeamless ? true : false;
        defaultMenuBar.IsVisible = IsSeamless ? false : true;

        if (IsSeamless == false) {
          titleBar.Resources["SystemControlForegroundBaseHighBrush"] =
            new SolidColorBrush { Color = new Color(255, 0, 0, 0) };
        }
      }
    }
  }

  private void CloseWindow(object sender, RoutedEventArgs e) {
    var hostWindow = (Window)VisualRoot!;
    hostWindow.Close();
  }

  private void MaximizeWindow(object sender, RoutedEventArgs e) {
    var hostWindow = (Window)VisualRoot!;

    if (hostWindow.WindowState == WindowState.Normal) {
      hostWindow.WindowState = WindowState.Maximized;
    }
    else {
      hostWindow.WindowState = WindowState.Normal;
    }
  }

  private void MinimizeWindow(object sender, RoutedEventArgs e) {
    var hostWindow = (Window)VisualRoot!;
    hostWindow.WindowState = WindowState.Minimized;
  }

  private async void SubscribeToWindowState() {
    var hostWindow = VisualRoot as Window;

    while (hostWindow == null) {
      hostWindow = VisualRoot as Window;
      await Task.Delay(50);
    }

    hostWindow.GetObservable(Window.WindowStateProperty).Subscribe(s => {
      if (s != WindowState.Maximized) {
        maximizeIcon.Data = Geometry.Parse("M2048 2048v-2048h-2048v2048h2048zM1843 1843h-1638v-1638h1638v1638z");
        hostWindow.Padding = new Thickness(0, 0, 0, 0);
        maximizeToolTip.Content = "Maximize";
      }

      if (s == WindowState.Maximized) {
        maximizeIcon.Data =
          Geometry.Parse(
            "M2048 1638h-410v410h-1638v-1638h410v-410h1638v1638zm-614-1024h-1229v1229h1229v-1229zm409-409h-1229v205h1024v1024h205v-1229z");
        hostWindow.Padding = new Thickness(7, 7, 7, 7);
        maximizeToolTip.Content = "Restore Down";

        // This should be a more universal approach in both cases, but I found it to be less reliable, when for example double-clicking the title bar.
        /*hostWindow.Padding = new Thickness(
                hostWindow.OffScreenMargin.Left,
                hostWindow.OffScreenMargin.Top,
                hostWindow.OffScreenMargin.Right,
                hostWindow.OffScreenMargin.Bottom);*/
      }
    });
  }

  private void InitializeComponent() {
    AvaloniaXamlLoader.Load(this);
  }
}