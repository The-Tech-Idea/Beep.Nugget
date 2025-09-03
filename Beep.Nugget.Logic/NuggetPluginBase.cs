using System;
using System.Collections.Generic;
using System.Linq;
using Beep.Nugget.Engine.Interfaces;

namespace Beep.Nugget.Engine
{
    /// <summary>
    /// Abstract base class for nugget plugins that provides common functionality and simplifies plugin development.
    /// </summary>
    public abstract class NuggetPluginBase : INuggetPlugin
    {
        #region Properties
        public abstract string Id { get; }
        public abstract string Name { get; }
        public abstract string Version { get; }
        public abstract string Description { get; }
        public abstract string Author { get; }
        public virtual IEnumerable<string> Dependencies => Enumerable.Empty<string>();

        private NuggetPluginStatus _status = NuggetPluginStatus.Unloaded;
        public NuggetPluginStatus Status
        {
            get => _status;
            protected set
            {
                var oldStatus = _status;
                _status = value;
                if (oldStatus != value)
                {
                    OnStatusChanged(oldStatus, value);
                }
            }
        }

        protected INuggetPluginContext Context { get; private set; }
        protected INuggetLogger Logger => Context?.Logger;
        protected INuggetEventBus EventBus => Context?.EventBus;
        protected INuggetPluginManager PluginManager => Context?.PluginManager;
        #endregion

        #region Events
        public event EventHandler<NuggetPluginStatusChangedEventArgs> StatusChanged;

        protected virtual void OnStatusChanged(NuggetPluginStatus oldStatus, NuggetPluginStatus newStatus, string message = null)
        {
            StatusChanged?.Invoke(this, new NuggetPluginStatusChangedEventArgs(oldStatus, newStatus, message));
        }
        #endregion

        #region INuggetPlugin Implementation
        public virtual bool Initialize(INuggetPluginContext context)
        {
            try
            {
                if (context == null)
                    throw new ArgumentNullException(nameof(context));

                Context = context;
                Status = NuggetPluginStatus.Loaded;

                Logger?.LogInfo($"Initializing plugin: {Id}");

                var result = OnInitialize();
                if (result)
                {
                    Status = NuggetPluginStatus.Initialized;
                    Logger?.LogInfo($"Successfully initialized plugin: {Id}");
                }
                else
                {
                    Status = NuggetPluginStatus.Error;
                    Logger?.LogError($"Failed to initialize plugin: {Id}");
                }

                return result;
            }
            catch (Exception ex)
            {
                Status = NuggetPluginStatus.Error;
                Logger?.LogError(ex, $"Error initializing plugin: {Id}");
                return false;
            }
        }

        public virtual bool Start()
        {
            try
            {
                if (Status != NuggetPluginStatus.Initialized)
                {
                    Logger?.LogWarning($"Cannot start plugin '{Id}' - not initialized");
                    return false;
                }

                Logger?.LogInfo($"Starting plugin: {Id}");

                var result = OnStart();
                if (result)
                {
                    Status = NuggetPluginStatus.Started;
                    Logger?.LogInfo($"Successfully started plugin: {Id}");
                }
                else
                {
                    Status = NuggetPluginStatus.Error;
                    Logger?.LogError($"Failed to start plugin: {Id}");
                }

                return result;
            }
            catch (Exception ex)
            {
                Status = NuggetPluginStatus.Error;
                Logger?.LogError(ex, $"Error starting plugin: {Id}");
                return false;
            }
        }

        public virtual bool Stop()
        {
            try
            {
                if (Status != NuggetPluginStatus.Started)
                {
                    Logger?.LogWarning($"Cannot stop plugin '{Id}' - not started");
                    return false;
                }

                Logger?.LogInfo($"Stopping plugin: {Id}");

                var result = OnStop();
                Status = NuggetPluginStatus.Stopped;
                
                if (result)
                {
                    Logger?.LogInfo($"Successfully stopped plugin: {Id}");
                }
                else
                {
                    Logger?.LogWarning($"Plugin '{Id}' reported stop failure but status changed to stopped");
                }

                return result;
            }
            catch (Exception ex)
            {
                Status = NuggetPluginStatus.Error;
                Logger?.LogError(ex, $"Error stopping plugin: {Id}");
                return false;
            }
        }
        #endregion

        #region Abstract/Virtual Methods for Subclasses
        /// <summary>
        /// Called during plugin initialization. Override to provide custom initialization logic.
        /// </summary>
        /// <returns>True if initialization was successful, otherwise false.</returns>
        protected abstract bool OnInitialize();

        /// <summary>
        /// Called when the plugin is started. Override to provide custom start logic.
        /// </summary>
        /// <returns>True if start was successful, otherwise false.</returns>
        protected abstract bool OnStart();

        /// <summary>
        /// Called when the plugin is stopped. Override to provide custom stop and cleanup logic.
        /// </summary>
        /// <returns>True if stop was successful, otherwise false.</returns>
        protected abstract bool OnStop();

        /// <summary>
        /// Gets a configuration value from the plugin context.
        /// </summary>
        /// <typeparam name="T">The type of the configuration value.</typeparam>
        /// <param name="key">The configuration key.</param>
        /// <param name="defaultValue">The default value if the key is not found.</param>
        /// <returns>The configuration value or the default value.</returns>
        protected T GetConfigurationValue<T>(string key, T defaultValue = default)
        {
            if (Context?.Configuration?.TryGetValue(key, out var value) == true)
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    Logger?.LogWarning($"Failed to convert configuration value '{key}' to type {typeof(T).Name}");
                }
            }
            return defaultValue;
        }

        /// <summary>
        /// Gets a service from the service provider.
        /// </summary>
        /// <typeparam name="T">The type of service to get.</typeparam>
        /// <returns>The service instance or null if not found.</returns>
        protected T GetService<T>() where T : class
        {
            return Context?.ServiceProvider?.GetService(typeof(T)) as T;
        }

        /// <summary>
        /// Publishes an event to the event bus.
        /// </summary>
        /// <typeparam name="T">The type of event to publish.</typeparam>
        /// <param name="eventMessage">The event message to publish.</param>
        protected void PublishEvent<T>(T eventMessage) where T : class
        {
            EventBus?.Publish(eventMessage);
        }

        /// <summary>
        /// Subscribes to an event on the event bus.
        /// </summary>
        /// <typeparam name="T">The type of event to subscribe to.</typeparam>
        /// <param name="handler">The event handler.</param>
        protected void SubscribeToEvent<T>(Action<T> handler) where T : class
        {
            EventBus?.Subscribe(handler);
        }

        /// <summary>
        /// Unsubscribes from an event on the event bus.
        /// </summary>
        /// <typeparam name="T">The type of event to unsubscribe from.</typeparam>
        /// <param name="handler">The event handler to remove.</param>
        protected void UnsubscribeFromEvent<T>(Action<T> handler) where T : class
        {
            EventBus?.Unsubscribe(handler);
        }
        #endregion
    }

    /// <summary>
    /// Attribute to mark classes as nugget plugins and provide metadata.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class NuggetPluginAttribute : Attribute
    {
        public string Id { get; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string[] Dependencies { get; set; } = Array.Empty<string>();

        public NuggetPluginAttribute(string id)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }
    }

    /// <summary>
    /// Interface for data source nugget plugins that provide database connectivity.
    /// </summary>
    public interface IDataSourceNuggetPlugin : INuggetPlugin
    {
        /// <summary>
        /// Gets the data source types supported by this plugin.
        /// </summary>
        IEnumerable<string> SupportedDataSourceTypes { get; }

        /// <summary>
        /// Creates a data source instance for the specified type and connection string.
        /// </summary>
        /// <param name="dataSourceType">The type of data source to create.</param>
        /// <param name="connectionString">The connection string for the data source.</param>
        /// <returns>The created data source instance.</returns>
        object CreateDataSource(string dataSourceType, string connectionString);

        /// <summary>
        /// Tests a connection string for the specified data source type.
        /// </summary>
        /// <param name="dataSourceType">The data source type.</param>
        /// <param name="connectionString">The connection string to test.</param>
        /// <returns>True if the connection is valid, otherwise false.</returns>
        bool TestConnection(string dataSourceType, string connectionString);
    }

    /// <summary>
    /// Base class for data source nugget plugins.
    /// </summary>
    public abstract class DataSourceNuggetPluginBase : NuggetPluginBase, IDataSourceNuggetPlugin
    {
        public abstract IEnumerable<string> SupportedDataSourceTypes { get; }

        public abstract object CreateDataSource(string dataSourceType, string connectionString);

        public abstract bool TestConnection(string dataSourceType, string connectionString);

        protected override bool OnInitialize()
        {
            Logger?.LogInfo($"Initializing data source plugin: {Id}");
            Logger?.LogInfo($"Supported data source types: {string.Join(", ", SupportedDataSourceTypes)}");
            return true;
        }
    }
}