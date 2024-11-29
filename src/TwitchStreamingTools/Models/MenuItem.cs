using System;

namespace TwitchStreamingTools.Models;

/// <summary>
/// Represents a menu item on the left navigation of the main UI.
/// </summary>
public class MenuItem {
  /// <summary>
  /// Initializes a new instance of the <see cref="MenuItem"/> class.
  /// </summary>
  /// <param name="type">The view model to generate the page from.</param>
  public MenuItem(Type type) {
    ModelType = type;
    Label = type.Name.Replace("ViewModel", "");
  }

  /// <summary>
  /// The display name on the screen.
  /// </summary>
  public string Label { get; set; }

  /// <summary>
  /// The view model.
  /// </summary>
  public Type ModelType { get; set; }
}