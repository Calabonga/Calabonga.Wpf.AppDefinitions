namespace Calabonga.Wpf.AppDefinitions;

/// <summary>
/// Exception fired when something wrong during search libraries with <see cref="IAppDefinition"/> or libraries can't be loaded.
/// </summary>
public class AppDefinitionsNotFoundException : InvalidOperationException
{
    public AppDefinitionsNotFoundException() : base() { }

    public AppDefinitionsNotFoundException(string message) : base(message) { }

    public AppDefinitionsNotFoundException(string message, Exception? exception) : base(message, exception) { }
}