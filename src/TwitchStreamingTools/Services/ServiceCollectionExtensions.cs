using Microsoft.Extensions.DependencyInjection;

using Nullinside.Api.Common.Twitch;

using TwitchStreamingTools.ViewModels;
using TwitchStreamingTools.ViewModels.Pages;

namespace TwitchStreamingTools.Services;

/// <summary>
///   A wrapper that contains the registered services.
/// </summary>
public static class ServiceCollectionExtensions {
  /// <summary>
  ///   Adds the services used throughout the application.
  /// </summary>
  /// <param name="collection">The services collection to initialize.</param>
  public static void AddCommonServices(this IServiceCollection collection) {
    collection.AddSingleton<IAccountManager, AccountManager>();
    collection.AddSingleton<ITwitchClientProxy, TwitchClientProxy>(_ => TwitchClientProxy.Instance);

    collection.AddTransient<MainWindowViewModel>();
    collection.AddTransient<AccountViewModel>();
    collection.AddTransient<ChatViewModel>();
    collection.AddTransient<NewVersionWindowViewModel>();
  }
}