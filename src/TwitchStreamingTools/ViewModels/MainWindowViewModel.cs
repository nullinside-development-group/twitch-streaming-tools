using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;

using DynamicData;

using Microsoft.Extensions.DependencyInjection;

using ReactiveUI;

using TwitchStreamingTools.ViewModels.Pages;

using MenuItem = TwitchStreamingTools.Models.MenuItem;

namespace TwitchStreamingTools.ViewModels;

/// <summary>
///   The view model for the main UI.
/// </summary>
public class MainWindowViewModel : ViewModelBase {
  /// <summary>
  ///   The dependency injection service provider.
  /// </summary>
  private readonly IServiceProvider _provider;

  /// <summary>
  ///   A flag indicating whether the menu is open.
  /// </summary>
  private bool _isMenuOpen;

  /// <summary>
  ///   True if the application is updating, false otherwise.
  /// </summary>
  private bool _isUpdating;

  /// <summary>
  ///   The open page.
  /// </summary>
  private ViewModelBase _page;

  /// <summary>
  ///   The currently selected page.
  /// </summary>
  private MenuItem _selectedMenuItem;

  /// <summary>
  ///   Initializes a new instance of the <see cref="MainWindowViewModel" /> class.
  /// </summary>
  /// <param name="provider">The dependency injection service provider.</param>
  public MainWindowViewModel(IServiceProvider provider) {
    _provider = provider;
    OnToggleMenu = ReactiveCommand.Create(() => IsMenuOpen = !IsMenuOpen);
    _isUpdating = Environment.GetCommandLineArgs().Contains("--update");

    // Dynamically setup the pages
    MenuItems = new ObservableCollection<MenuItem>();
    List<MenuItem>? pages = AppDomain.CurrentDomain.GetAssemblies()
      .SelectMany(a => a.GetTypes())
      .Where(t => (t.FullName?.StartsWith("TwitchStreamingTools.ViewModels.Pages") ?? false) &&
                  typeof(PageViewModelBase).IsAssignableFrom(t) && t is { IsAbstract: false, IsInterface: false })
      .Select(t => new MenuItem(t, (_provider.GetRequiredService(t) as PageViewModelBase)!.IconResourceKey))
      .ToList();
    MenuItems.AddRange(pages);
    _selectedMenuItem = pages.First(p => typeof(AccountViewModel).IsAssignableTo(p.ModelType));
    PropertyChanged += (_, e) => {
      if (nameof(SelectedMenuItem).Equals(e.PropertyName)) {
        OnSelectedMenuItemChanged();
      }
    };

    // Set the initial page
    _page = (_provider.GetRequiredService(typeof(AccountViewModel)) as AccountViewModel)!;
  }

  /// <summary>
  ///   The menu items.
  /// </summary>
  public ObservableCollection<MenuItem> MenuItems { get; set; }

  /// <summary>
  ///   A flag indicating whether the menu is open.
  /// </summary>
  public bool IsMenuOpen {
    get => _isMenuOpen;
    set => this.RaiseAndSetIfChanged(ref _isMenuOpen, value);
  }

  /// <summary>
  ///   Called when toggling the menu open and close.
  /// </summary>
  public ReactiveCommand<Unit, bool> OnToggleMenu { get; }

  /// <summary>
  ///   The open page.
  /// </summary>
  public ViewModelBase Page {
    get => _page;
    set => this.RaiseAndSetIfChanged(ref _page, value);
  }

  /// <summary>
  ///   The currently selected page.
  /// </summary>
  public MenuItem SelectedMenuItem {
    get => _selectedMenuItem;
    set => this.RaiseAndSetIfChanged(ref _selectedMenuItem, value);
  }

  /// <summary>
  ///   True if the application is updating, false otherwise.
  /// </summary>
  public bool IsUpdating {
    get => _isUpdating;
    set => this.RaiseAndSetIfChanged(ref _isUpdating, value);
  }

  /// <summary>
  ///   Links the <see cref="Page" /> showing on the screen with changes to the <see cref="SelectedMenuItem" />.
  /// </summary>
  private void OnSelectedMenuItemChanged() {
    var viewModel = _provider.GetService(SelectedMenuItem.ModelType) as ViewModelBase;
    if (null == viewModel) {
      return;
    }

    Page = viewModel;
  }
}