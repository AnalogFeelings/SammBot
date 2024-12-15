using Microsoft.Extensions.DependencyInjection;
using SammBot.Library.Models;
using SammBot.SamplePlugin.Services;

namespace SammBot.SamplePlugin;

/// <summary>
/// The entry point for the plugin.
/// </summary>
public class EntryPoint : IPlugin
{
    /// <inheritdoc/>
    public void Initialize(ServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<MyService>();
    }
}