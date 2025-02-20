namespace TwitchStreamingTools.ViewModels.Pages;

/// <summary>
///   A base class for all pages of the application that are navigable through the left nav of the application.
/// </summary>
public abstract class PageViewModelBase : ViewModelBase {
  /// <summary>
  ///   The style resource key name of the icon.
  /// </summary>
  public abstract string IconResourceKey { get; }
}