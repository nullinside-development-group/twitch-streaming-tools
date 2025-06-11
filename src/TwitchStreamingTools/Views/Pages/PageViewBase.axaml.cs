using Avalonia.Controls;
using Avalonia.Interactivity;

using TwitchStreamingTools.ViewModels.Pages;

namespace TwitchStreamingTools.Views.Pages;

/// <summary>
///   The base class for the page view.
/// </summary>
public partial class PageViewBase : UserControl {
  /// <summary>
  ///   Initializes a new instance of the <see cref="PageViewBase" /> class.
  /// </summary>
  public PageViewBase() {
    InitializeComponent();
  }

  private void Control_OnLoaded(object? sender, RoutedEventArgs e) {
    var dataContext = (DataContext as PageViewModelBase)!;
    dataContext.OnLoaded();
  }

  private void Control_OnUnloaded(object? sender, RoutedEventArgs e) {
    var dataContext = (DataContext as PageViewModelBase)!;
    dataContext.OnUnloaded();
  }
}