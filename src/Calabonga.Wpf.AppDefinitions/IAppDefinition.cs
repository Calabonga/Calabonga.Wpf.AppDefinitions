using Microsoft.Extensions.DependencyInjection;

namespace Calabonga.Wpf.AppDefinitions;

/// <summary>
/// Application Definition interface abstraction
/// </summary>
public interface IAppDefinition
{
    /// <summary>
    /// Configure services for current application
    /// </summary>
    /// <param name="services">instance of <see cref="IServiceCollection"/></param>
    void ConfigureServices(IServiceCollection services);

    /// <summary>
    /// Order index for including into pipeline for ConfigureServices(). Default value is 0 for, that's why order index can be undefined.
    /// </summary>
    int ServiceOrderIndex { get; }

    /// <summary>
    /// Enable or disable to register into pipeline for the current application Definition.
    /// </summary>
    /// <remarks>Default values is <c>True</c></remarks>
    bool Enabled { get; }

    /// <summary>
    /// Enables or disables export definition as a content for module that can be exported.
    /// </summary>
    /// /// <remarks>Default values is <c>False</c></remarks>
    bool Exported { get; }
}