using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Nullinside.Api.Common.Twitch;

using TwitchStreamingTools.Utilities;
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
    // collection.AddSingleton<IRepository, Repository>();
    // collection.AddTransient<BusinessService>();
    collection.AddSingleton<IAccountManager, AccountManager>();
    collection.AddSingleton<ITwitchClientProxy, TwitchClientProxy>(_ => TwitchClientProxy.Instance);

    collection.AddTransient<MainWindowViewModel>();
    collection.AddTransient<AccountViewModel>();
    collection.AddTransient<ChatViewModel>();
    collection.AddTransient<ITwitchApiProxy, TwitchApiWrapper>(TwitchApiWrapperFactory);
  }

  /// <summary>
  ///   A factory for generating twitch apis.
  /// </summary>
  /// <param name="_">not used.</param>
  /// <returns>A new instance of the <see cref="TwitchApiWrapper" /> class.</returns>
  private static TwitchApiWrapper TwitchApiWrapperFactory(IServiceProvider _) {
    Task<TwitchApiWrapper> task = TwitchApiWrapper.CreateApi();
    Task.WaitAll(task);
    return task.Result;
  }
}