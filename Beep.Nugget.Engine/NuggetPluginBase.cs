using System;

namespace Beep.Nugget.Engine
{
    /// <summary>
    /// Abstract base class for nugget plugins that simplifies plugin development
    /// </summary>
    public abstract class NuggetPluginBase : INuggetPlugin
    {
        public abstract string Id { get; }
        public abstract string Name { get; }
        public abstract string Version { get; }
        public virtual string Description => "Nugget Plugin";

        protected bool IsInitialized { get; private set; }
        protected bool IsStarted { get; private set; }

        public virtual bool Initialize()
        {
            try
            {
                if (IsInitialized)
                    return true;

                var result = OnInitialize();
                IsInitialized = result;
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing plugin {Id}: {ex.Message}");
                return false;
            }
        }

        public virtual bool Start()
        {
            try
            {
                if (!IsInitialized)
                {
                    Console.WriteLine($"Plugin {Id} must be initialized before starting");
                    return false;
                }

                if (IsStarted)
                    return true;

                var result = OnStart();
                IsStarted = result;
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting plugin {Id}: {ex.Message}");
                return false;
            }
        }

        public virtual bool Stop()
        {
            try
            {
                if (!IsStarted)
                    return true;

                var result = OnStop();
                IsStarted = false;
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping plugin {Id}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Override this method to provide custom initialization logic
        /// </summary>
        protected abstract bool OnInitialize();

        /// <summary>
        /// Override this method to provide custom start logic
        /// </summary>
        protected abstract bool OnStart();

        /// <summary>
        /// Override this method to provide custom stop logic
        /// </summary>
        protected abstract bool OnStop();
    }

    /// <summary>
    /// Attribute to mark classes as nugget plugins
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class NuggetPluginAttribute : Attribute
    {
        public string Id { get; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }

        public NuggetPluginAttribute(string id)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }
    }

    /// <summary>
    /// Interface for data source plugins
    /// </summary>
    public interface IDataSourceNuggetPlugin : INuggetPlugin
    {
        /// <summary>
        /// Gets the data source types supported by this plugin
        /// </summary>
        string[] SupportedDataSourceTypes { get; }

        /// <summary>
        /// Creates a data source connection
        /// </summary>
        object CreateDataSource(string dataSourceType, string connectionString);

        /// <summary>
        /// Tests a connection
        /// </summary>
        bool TestConnection(string dataSourceType, string connectionString);
    }

    /// <summary>
    /// Base class for data source plugins
    /// </summary>
    public abstract class DataSourceNuggetPluginBase : NuggetPluginBase, IDataSourceNuggetPlugin
    {
        public abstract string[] SupportedDataSourceTypes { get; }

        public abstract object CreateDataSource(string dataSourceType, string connectionString);

        public abstract bool TestConnection(string dataSourceType, string connectionString);

        protected override bool OnInitialize()
        {
            Console.WriteLine($"Initializing data source plugin: {Name}");
            Console.WriteLine($"Supported types: {string.Join(", ", SupportedDataSourceTypes)}");
            return true;
        }
    }
}