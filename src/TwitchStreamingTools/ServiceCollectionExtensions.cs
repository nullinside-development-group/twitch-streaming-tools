using System;

using Microsoft.Extensions.DependencyInjection;

using Nullinside.Api.Common.Twitch;

using TwitchStreamingTools.Services;
using TwitchStreamingTools.Utilities;
using TwitchStreamingTools.ViewModels;
using TwitchStreamingTools.ViewModels.Pages;
using TwitchStreamingTools.ViewModels.Pages.SettingsView;

namespace TwitchStreamingTools;

/// <summary>
///   A wrapper that contains the registered services.
/// </summary>
public static class ServiceCollectionExtensions {
  /// <summary>
  ///   Adds the services used throughout the application.
  /// </summary>
  /// <param name="collection">The services collection to initialize.</param>
  public static void AddCommonServices(this IServiceCollection collection) {
    // Regular stuff
    collection.AddSingleton<IConfiguration, Configuration>(_ => Configuration.Instance);
    collection.AddSingleton<ITwitchClientProxy, TwitchClientProxy>(_ => TwitchClientProxy.Instance);
    collection.AddSingleton<ITwitchChatLog, TwitchChatLog>();
    collection.AddSingleton<GlobalKeyPressService>();

    // Services
    collection.AddSingleton<ITwitchAccountService, TwitchAccountService>();
    collection.AddSingleton<ITwitchTtsService, TwitchTtsService>();

    // View models
    collection.AddTransient<MainWindowViewModel>();
    collection.AddTransient<AccountViewModel>();
    collection.AddTransient<ChatViewModel>();
    collection.AddTransient<NewVersionWindowViewModel>();
    collection.AddTransient<SettingsViewModel>();
    collection.AddTransient<TtsPhoneticWordsViewModel>();
    collection.AddTransient<TtsSkipUsernamesViewModel>();
  }

  /// <summary>
  ///   Buffers the services for the top level services that live in the dependency injection framework.
  /// </summary>
  /// <param name="provider">The service provider.</param>
  public static void StartupServices(this IServiceProvider provider) {
    provider.GetRequiredService<ITwitchAccountService>();
    provider.GetRequiredService<ITwitchTtsService>();
    provider.GetRequiredService<GlobalKeyPressService>();
  }
}