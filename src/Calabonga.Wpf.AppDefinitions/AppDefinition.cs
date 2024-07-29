using Microsoft.Extensions.DependencyInjection;

namespace Calabonga.Wpf.AppDefinitions;

/// <summary>
/// Base implementation for <see cref="IAppDefinition"/>
/// </summary>
public abstract class AppDefinition : IAppDefinition
{
    /// <summary>
    /// Configure services for current application
    /// </summary>
    /// <param name="services">instance of <see cref="IServiceCollection"/></param>
    public abstract void ConfigureServices(IServiceCollection services);

    /// <summary>
    /// Order index for including into pipeline. Default value is 0 for, that's why order index can be undefined.
    /// </summary>
    public virtual int ServiceOrderIndex => 0;

    /// <summary>
    /// Order index for including into pipeline for ConfigureApplication() . Default value is 0 for, that's why order index can be undefined.
    /// </summary>
    public virtual int ApplicationOrderIndex => 0;

    /// <summary>
    /// Enable or disable to register into pipeline for the current application Definition.
    /// </summary>
    /// <remarks>Default values is <c>True</c></remarks>
    public virtual bool Enabled { get; protected set; } = true;

    /// <summary>
    /// Enables or disables export definition as a content for module that can be exported.
    /// </summary>
    /// /// <remarks>Default values is <c>False</c></remarks>
    public virtual bool Exported { get; protected set; }
}