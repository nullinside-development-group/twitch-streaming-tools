using System;

namespace TwitchStreamingTools.Models;

public class MenuItem {
  public MenuItem(Type type) {
    ModelType = type;
    Label = type.Name.Replace("ViewModel", "");
  }

  public string Label { get; set; }
  public Type ModelType { get; set; }
}