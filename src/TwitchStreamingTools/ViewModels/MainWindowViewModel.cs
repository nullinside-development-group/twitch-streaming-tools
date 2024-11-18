using System;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

using Avalonia.Controls;

using Nullinside.Api.Common;

using ReactiveUI;

using SiteMonitor.Models;

namespace SiteMonitor.ViewModels;

/// <summary>
///   The view model for the main UI.
/// </summary>
public class MainWindowViewModel : ViewModelBase {
  /// <summary>
  ///   Initializes a new instance of the <see cref="MainWindowViewModel" /> class.
  /// </summary>
  public MainWindowViewModel() {
  }
}