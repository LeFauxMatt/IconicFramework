namespace LeFauxMods.IconicFramework.Utilities;

using LeFauxMods.Core.Services;

/// <summary>Static wrapper for logging service.</summary>
internal sealed class Log
{
    private static Log instance = null!;

    private readonly SimpleLogging simpleLogging;

    /// <summary>
    /// Initializes a new instance of the <see cref="Log" /> class.
    /// </summary>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    public Log(IMonitor monitor)
    {
        instance = this;
        this.simpleLogging = new SimpleLogging(monitor);
    }

    /// <summary>Logs an alert message to the console.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="hudType">The hud type to show.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    [StringFormatMethod("message")]
    public static void Alert(string message, int hudType = HUDMessage.error_type, params object?[]? args) => instance.simpleLogging.Alert(message, hudType, args);

    /// <summary>Logs a debug message to the console.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    [StringFormatMethod("message")]
    public static void Debug(string message, params object?[]? args) => instance.simpleLogging.Debug(message, args);

    /// <summary>Logs an error message to the console.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    [StringFormatMethod("message")]
    public static void Error(string message, params object?[]? args) => instance.simpleLogging.Error(message, args);

    /// <summary>Logs an info message to the console.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    [StringFormatMethod("message")]
    public static void Info(string message, params object?[]? args) => instance.simpleLogging.Info(message, args);

    /// <summary>Logs a trace message to the console.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    [StringFormatMethod("message")]
    public static void Trace(string message, params object?[]? args) => instance.simpleLogging.Trace(message, args);

    /// <summary>Logs a trace message to the console only once.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    [StringFormatMethod("message")]
    public static void TraceOnce(string message, params object?[]? args) => instance.simpleLogging.TraceOnce(message, args);

    /// <summary>Logs a warn message to the console.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    [StringFormatMethod("message")]
    public static void Warn(string message, params object?[]? args) => instance.simpleLogging.Warn(message, args);

    /// <summary>Logs a warn message to the console only once.</summary>
    /// <param name="message">The message to send.</param>
    /// <param name="args">The arguments to parse in a formatted string.</param>
    [StringFormatMethod("message")]
    public static void WarnOnce(string message, params object?[]? args) => instance.simpleLogging.WarnOnce(message, args);
}
