namespace Calabonga.Wpf.AppDefinitions;

public class AppDefinitionsNotFoundException : InvalidOperationException
{
    public AppDefinitionsNotFoundException() : base() { }

    public AppDefinitionsNotFoundException(string message) : base(message) { }

    public AppDefinitionsNotFoundException(string message, Exception? exception) : base(message, exception) { }
}