using System;
using System.Collections.Generic;
using System.Reflection;

namespace Beep.Nugget.Engine.Interfaces
{
    /// <summary>
    /// Represents a plugin interface that nuggets must implement to be discoverable and manageable.
    /// </summary>
    public interface INuggetPlugin
    {
        /// <summary>
        /// Gets the unique identifier for this plugin.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets the name of the plugin.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the version of the plugin.
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Gets the description of the plugin.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the author of the plugin.
        /// </summary>
        string Author { get; }

        /// <summary>
        /// Gets the dependencies required by this plugin.
        /// </summary>
        IEnumerable<string> Dependencies { get; }

        /// <summary>
        /// Initializes the plugin with the provided context.
        /// </summary>
        /// <param name="context">The plugin context containing services and configuration.</param>
        /// <returns>True if initialization was successful, otherwise false.</returns>
        bool Initialize(INuggetPluginContext context);

        /// <summary>
        /// Starts the plugin functionality.
        /// </summary>
        /// <returns>True if the plugin started successfully, otherwise false.</returns>
        bool Start();

        /// <summary>
        /// Stops the plugin and releases any resources.
        /// </summary>
        /// <returns>True if the plugin stopped successfully, otherwise false.</returns>
        bool Stop();

        /// <summary>
        /// Gets the current status of the plugin.
        /// </summary>
        NuggetPluginStatus Status { get; }

        /// <summary>
        /// Event raised when the plugin status changes.
        /// </summary>
        event EventHandler<NuggetPluginStatusChangedEventArgs> StatusChanged;
    }

    /// <summary>
    /// Represents the status of a nugget plugin.
    /// </summary>
    public enum NuggetPluginStatus
    {
        Unloaded,
        Loaded,
        Initialized,
        Started,
        Stopped,
        Error
    }

    /// <summary>
    /// Event arguments for plugin status changes.
    /// </summary>
    public class NuggetPluginStatusChangedEventArgs : EventArgs
    {
        public NuggetPluginStatus OldStatus { get; }
        public NuggetPluginStatus NewStatus { get; }
        public string Message { get; }

        public NuggetPluginStatusChangedEventArgs(NuggetPluginStatus oldStatus, NuggetPluginStatus newStatus, string message = null)
        {
            OldStatus = oldStatus;
            NewStatus = newStatus;
            Message = message;
        }
    }
}