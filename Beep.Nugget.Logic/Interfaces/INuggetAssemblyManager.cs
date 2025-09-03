using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Beep.Nugget.Engine.Interfaces
{
    /// <summary>
    /// Interface for managing nugget assembly loading and unloading.
    /// </summary>
    public interface INuggetAssemblyManager
    {
        /// <summary>
        /// Loads a nugget from a package file.
        /// </summary>
        /// <param name="packagePath">Path to the nugget package file.</param>
        /// <param name="packageId">Unique identifier for the package.</param>
        /// <returns>The loaded nugget information.</returns>
        Task<LoadedNugget> LoadNuggetAsync(string packagePath, string packageId);

        /// <summary>
        /// Unloads a previously loaded nugget.
        /// </summary>
        /// <param name="packageId">The unique identifier of the package to unload.</param>
        /// <returns>True if unloaded successfully, otherwise false.</returns>
        bool UnloadNugget(string packageId);

        /// <summary>
        /// Gets information about all currently loaded nuggets.
        /// </summary>
        /// <returns>Collection of loaded nugget information.</returns>
        IEnumerable<LoadedNugget> GetLoadedNuggets();

        /// <summary>
        /// Gets information about a specific loaded nugget.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <returns>The loaded nugget information, or null if not found.</returns>
        LoadedNugget GetLoadedNugget(string packageId);

        /// <summary>
        /// Discovers and returns all plugins from loaded nuggets.
        /// </summary>
        /// <returns>Collection of discovered plugins.</returns>
        IEnumerable<INuggetPlugin> DiscoverPlugins();

        /// <summary>
        /// Discovers plugins of a specific type from loaded nuggets.
        /// </summary>
        /// <typeparam name="T">The plugin type to discover.</typeparam>
        /// <returns>Collection of discovered plugins of the specified type.</returns>
        IEnumerable<T> DiscoverPlugins<T>() where T : class, INuggetPlugin;

        /// <summary>
        /// Event raised when a nugget is loaded.
        /// </summary>
        event EventHandler<NuggetLoadedEventArgs> NuggetLoaded;

        /// <summary>
        /// Event raised when a nugget is unloaded.
        /// </summary>
        event EventHandler<NuggetUnloadedEventArgs> NuggetUnloaded;

        /// <summary>
        /// Event raised when an error occurs during nugget operations.
        /// </summary>
        event EventHandler<NuggetErrorEventArgs> NuggetError;
    }

    /// <summary>
    /// Represents information about a loaded nugget.
    /// </summary>
    public class LoadedNugget
    {
        /// <summary>
        /// Gets the unique identifier of the nugget.
        /// </summary>
        public string PackageId { get; set; }

        /// <summary>
        /// Gets the path where the nugget was loaded from.
        /// </summary>
        public string PackagePath { get; set; }

        /// <summary>
        /// Gets the assemblies loaded as part of this nugget.
        /// </summary>
        public IList<Assembly> Assemblies { get; set; }

        /// <summary>
        /// Gets the plugins discovered in this nugget.
        /// </summary>
        public IList<INuggetPlugin> Plugins { get; set; }

        /// <summary>
        /// Gets the nugget definition metadata.
        /// </summary>
        public NuggetDefinition Definition { get; set; }

        /// <summary>
        /// Gets the load context used for this nugget.
        /// </summary>
        public object LoadContext { get; set; }

        /// <summary>
        /// Gets the date and time when the nugget was loaded.
        /// </summary>
        public DateTime LoadedAt { get; set; }

        /// <summary>
        /// Gets whether the nugget is currently active.
        /// </summary>
        public bool IsActive { get; set; }

        public LoadedNugget()
        {
            Assemblies = new List<Assembly>();
            Plugins = new List<INuggetPlugin>();
            LoadedAt = DateTime.UtcNow;
            IsActive = true;
        }
    }

    /// <summary>
    /// Event arguments for nugget loaded events.
    /// </summary>
    public class NuggetLoadedEventArgs : EventArgs
    {
        public LoadedNugget LoadedNugget { get; }

        public NuggetLoadedEventArgs(LoadedNugget loadedNugget)
        {
            LoadedNugget = loadedNugget;
        }
    }

    /// <summary>
    /// Event arguments for nugget unloaded events.
    /// </summary>
    public class NuggetUnloadedEventArgs : EventArgs
    {
        public string PackageId { get; }
        public DateTime UnloadedAt { get; }

        public NuggetUnloadedEventArgs(string packageId)
        {
            PackageId = packageId;
            UnloadedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Event arguments for nugget error events.
    /// </summary>
    public class NuggetErrorEventArgs : EventArgs
    {
        public string PackageId { get; }
        public Exception Exception { get; }
        public string Operation { get; }

        public NuggetErrorEventArgs(string packageId, Exception exception, string operation)
        {
            PackageId = packageId;
            Exception = exception;
            Operation = operation;
        }
    }
}