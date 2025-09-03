using System;
using System.Collections.Generic;

namespace Beep.Nugget.Engine.Interfaces
{
    /// <summary>
    /// Provides context and services to nugget plugins during initialization and operation.
    /// </summary>
    public interface INuggetPluginContext
    {
        /// <summary>
        /// Gets the service provider for dependency injection.
        /// </summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the configuration settings for the plugin.
        /// </summary>
        IDictionary<string, object> Configuration { get; }

        /// <summary>
        /// Gets the logger instance for the plugin.
        /// </summary>
        INuggetLogger Logger { get; }

        /// <summary>
        /// Gets the event bus for inter-plugin communication.
        /// </summary>
        INuggetEventBus EventBus { get; }

        /// <summary>
        /// Gets the plugin manager for accessing other plugins.
        /// </summary>
        INuggetPluginManager PluginManager { get; }

        /// <summary>
        /// Gets the application context information.
        /// </summary>
        string ApplicationName { get; }

        /// <summary>
        /// Gets the application version.
        /// </summary>
        string ApplicationVersion { get; }

        /// <summary>
        /// Gets the base directory for the application.
        /// </summary>
        string BaseDirectory { get; }

        /// <summary>
        /// Gets whether the application is running in debug mode.
        /// </summary>
        bool IsDebugMode { get; }
    }

    /// <summary>
    /// Simple logger interface for nugget plugins.
    /// </summary>
    public interface INuggetLogger
    {
        void LogDebug(string message);
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogError(Exception exception, string message = null);
    }

    /// <summary>
    /// Event bus for inter-plugin communication.
    /// </summary>
    public interface INuggetEventBus
    {
        void Publish<T>(T eventMessage) where T : class;
        void Subscribe<T>(Action<T> handler) where T : class;
        void Unsubscribe<T>(Action<T> handler) where T : class;
    }

    /// <summary>
    /// Plugin manager interface for accessing and managing other plugins.
    /// </summary>
    public interface INuggetPluginManager
    {
        IEnumerable<INuggetPlugin> GetLoadedPlugins();
        INuggetPlugin GetPlugin(string id);
        T GetPlugin<T>() where T : class, INuggetPlugin;
        IEnumerable<T> GetPlugins<T>() where T : class, INuggetPlugin;
        bool IsPluginLoaded(string id);
        NuggetPluginStatus GetPluginStatus(string id);
    }
}