using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Beep.Nugget.Engine.Interfaces;
using Beep.Nugget.Engine.Implementation;

namespace Beep.Nugget.Engine
{
    /// <summary>
    /// Enhanced nugget assembly manager that provides a pluggable framework for loading and unloading nuggets.
    /// This implementation supports plugin discovery, dependency injection, and proper lifecycle management.
    /// </summary>
    public class NuggetAssemblyManager : INuggetAssemblyManager, INuggetPluginManager, IDisposable
    {
        #region Fields
        private readonly Dictionary<string, LoadedNuggetInfo> _loadedNuggets = new();
        private readonly Dictionary<string, INuggetPlugin> _loadedPlugins = new();
        private readonly string _runtimeFramework;
        private readonly INuggetLogger _logger;
        private readonly INuggetEventBus _eventBus;
        private readonly IServiceProvider _serviceProvider;
        private readonly INuggetPluginContext _pluginContext;
        private bool _disposed = false;

        // Internal class to track loaded nugget information
        private class LoadedNuggetInfo
        {
            public NuggetLoadContext Context { get; set; }
            public string ExtractPath { get; set; }
            public LoadedNugget NuggetInfo { get; set; }
        }
        #endregion

        #region Events
        public event EventHandler<NuggetLoadedEventArgs> NuggetLoaded;
        public event EventHandler<NuggetUnloadedEventArgs> NuggetUnloaded;
        public event EventHandler<NuggetErrorEventArgs> NuggetError;
        #endregion

        #region Constructor
        public NuggetAssemblyManager(
            INuggetLogger logger = null,
            INuggetEventBus eventBus = null,
            IServiceProvider serviceProvider = null)
        {
            _runtimeFramework = GetRuntimeFramework();
            _logger = logger ?? new ConsoleNuggetLogger("NuggetAssemblyManager");
            _eventBus = eventBus ?? new SimpleNuggetEventBus();
            _serviceProvider = serviceProvider;

            // Create plugin context
            _pluginContext = new NuggetPluginContext(
                serviceProvider: _serviceProvider,
                configuration: new Dictionary<string, object>(),
                logger: _logger,
                eventBus: _eventBus,
                pluginManager: this,
                applicationName: Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown",
                applicationVersion: Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "1.0.0",
                baseDirectory: AppContext.BaseDirectory,
                isDebugMode: System.Diagnostics.Debugger.IsAttached
            );

            _logger.LogInfo("NuggetAssemblyManager initialized");
        }
        #endregion

        #region INuggetAssemblyManager Implementation
        /// <summary>
        /// Loads a nugget from a .nupkg file into a new, collectible AssemblyLoadContext.
        /// </summary>
        public async Task<LoadedNugget> LoadNuggetAsync(string packagePath, string packageId)
        {
            if (string.IsNullOrEmpty(packagePath))
                throw new ArgumentException("Package path cannot be null or empty", nameof(packagePath));
            
            if (string.IsNullOrEmpty(packageId))
                throw new ArgumentException("Package ID cannot be null or empty", nameof(packageId));

            if (_loadedNuggets.ContainsKey(packageId))
                throw new InvalidOperationException($"Nugget '{packageId}' is already loaded.");

            try
            {
                _logger.LogInfo($"Loading nugget: {packageId} from {packagePath}");

                var extractPath = Path.Combine(Path.GetTempPath(), $"{packageId}_{Guid.NewGuid()}");
                Directory.CreateDirectory(extractPath);

                // Extract package files
                await ExtractPackageFiles(packagePath, extractPath);

                var libFolderPath = Path.Combine(extractPath, "lib");
                if (!Directory.Exists(libFolderPath))
                {
                    throw new DirectoryNotFoundException($"The package '{packageId}' does not contain a 'lib' folder.");
                }

                // Find compatible framework
                var frameworkFolders = Directory.GetDirectories(libFolderPath).Select(Path.GetFileName).ToArray();
                var compatibleFrameworkFolder = GetNuGetCompatibleFramework(libFolderPath, frameworkFolders);

                if (compatibleFrameworkFolder == null)
                {
                    throw new Exception($"No compatible framework found for '{_runtimeFramework}' in nugget '{packageId}'.");
                }

                // Load assemblies
                var assemblyFiles = Directory.GetFiles(compatibleFrameworkFolder, "*.dll");
                if (!assemblyFiles.Any())
                {
                    throw new FileNotFoundException($"No assemblies found in the compatible framework folder for nugget '{packageId}'.");
                }

                var context = new NuggetLoadContext(packageId, compatibleFrameworkFolder);
                var loadedAssemblies = new List<Assembly>();

                foreach (var dllFile in assemblyFiles)
                {
                    try
                    {
                        var assembly = context.LoadFromAssemblyPath(dllFile);
                        loadedAssemblies.Add(assembly);
                        _logger.LogDebug($"Loaded assembly: {assembly.FullName}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Failed to load assembly {dllFile}: {ex.Message}");
                    }
                }

                // Create loaded nugget info
                var loadedNugget = new LoadedNugget
                {
                    PackageId = packageId,
                    PackagePath = packagePath,
                    Assemblies = loadedAssemblies,
                    LoadContext = context,
                    Definition = CreateNuggetDefinition(packageId, packagePath)
                };

                // Discover plugins in the loaded assemblies
                var discoveredPlugins = DiscoverPluginsInAssemblies(loadedAssemblies);
                loadedNugget.Plugins = discoveredPlugins.ToList();

                // Initialize plugins
                await InitializePlugins(discoveredPlugins);

                // Store loaded nugget info
                var nuggetInfo = new LoadedNuggetInfo
                {
                    Context = context,
                    ExtractPath = extractPath,
                    NuggetInfo = loadedNugget
                };

                _loadedNuggets[packageId] = nuggetInfo;

                // Register plugins
                foreach (var plugin in discoveredPlugins)
                {
                    _loadedPlugins[plugin.Id] = plugin;
                }

                _logger.LogInfo($"Successfully loaded nugget '{packageId}' with {loadedAssemblies.Count} assemblies and {discoveredPlugins.Count()} plugins");

                // Raise event
                NuggetLoaded?.Invoke(this, new NuggetLoadedEventArgs(loadedNugget));

                return loadedNugget;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to load nugget '{packageId}'");
                NuggetError?.Invoke(this, new NuggetErrorEventArgs(packageId, ex, "Load"));
                throw;
            }
        }

        /// <summary>
        /// Unloads a previously loaded nugget and cleans up its resources.
        /// </summary>
        public bool UnloadNugget(string packageId)
        {
            if (string.IsNullOrEmpty(packageId))
                throw new ArgumentException("Package ID cannot be null or empty", nameof(packageId));

            if (!_loadedNuggets.TryGetValue(packageId, out var nuggetInfo))
            {
                _logger.LogWarning($"Nugget '{packageId}' is not loaded or has already been unloaded.");
                return false;
            }

            try
            {
                _logger.LogInfo($"Unloading nugget: {packageId}");

                var loadedNugget = nuggetInfo.NuggetInfo;

                // Stop and cleanup plugins
                foreach (var plugin in loadedNugget.Plugins)
                {
                    try
                    {
                        plugin.Stop();
                        _loadedPlugins.Remove(plugin.Id);
                        _logger.LogDebug($"Stopped plugin: {plugin.Id}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error stopping plugin '{plugin.Id}'");
                    }
                }

                // Mark nugget as inactive
                loadedNugget.IsActive = false;

                // Remove from collections
                _loadedNuggets.Remove(packageId);

                // Unload context
                nuggetInfo.Context.Unload();

                // Schedule cleanup of temporary files
                _ = Task.Run(async () => await CleanupExtractedFiles(nuggetInfo.ExtractPath));

                _logger.LogInfo($"Successfully unloaded nugget '{packageId}'");

                // Raise event
                NuggetUnloaded?.Invoke(this, new NuggetUnloadedEventArgs(packageId));

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to unload nugget '{packageId}'");
                NuggetError?.Invoke(this, new NuggetErrorEventArgs(packageId, ex, "Unload"));
                return false;
            }
        }

        public IEnumerable<LoadedNugget> GetLoadedNuggets()
        {
            return _loadedNuggets.Values.Select(info => info.NuggetInfo).ToList();
        }

        public LoadedNugget GetLoadedNugget(string packageId)
        {
            return _loadedNuggets.TryGetValue(packageId, out var info) ? info.NuggetInfo : null;
        }

        public IEnumerable<INuggetPlugin> DiscoverPlugins()
        {
            return _loadedPlugins.Values.ToList();
        }

        public IEnumerable<T> DiscoverPlugins<T>() where T : class, INuggetPlugin
        {
            return _loadedPlugins.Values.OfType<T>().ToList();
        }
        #endregion

        #region INuggetPluginManager Implementation
        public IEnumerable<INuggetPlugin> GetLoadedPlugins()
        {
            return _loadedPlugins.Values.ToList();
        }

        public INuggetPlugin GetPlugin(string id)
        {
            return _loadedPlugins.TryGetValue(id, out var plugin) ? plugin : null;
        }

        public T GetPlugin<T>() where T : class, INuggetPlugin
        {
            return _loadedPlugins.Values.OfType<T>().FirstOrDefault();
        }

        public IEnumerable<T> GetPlugins<T>() where T : class, INuggetPlugin
        {
            return _loadedPlugins.Values.OfType<T>().ToList();
        }

        public bool IsPluginLoaded(string id)
        {
            return _loadedPlugins.ContainsKey(id);
        }

        public NuggetPluginStatus GetPluginStatus(string id)
        {
            var plugin = GetPlugin(id);
            return plugin?.Status ?? NuggetPluginStatus.Unloaded;
        }
        #endregion

        #region Private Helper Methods
        private async Task ExtractPackageFiles(string packagePath, string extractPath)
        {
            using var packageReader = new PackageArchiveReader(packagePath);
            var files = await packageReader.GetFilesAsync(CancellationToken.None);
            
            foreach (var file in files)
            {
                var destinationPath = Path.Combine(extractPath, file);
                var destinationDir = Path.GetDirectoryName(destinationPath);
                
                if (!string.IsNullOrEmpty(destinationDir))
                {
                    Directory.CreateDirectory(destinationDir);
                }
                
                packageReader.ExtractFile(file, destinationPath, new NuGet.Common.NullLogger());
            }
        }

        private string GetRuntimeFramework()
        {
            return Assembly.GetEntryAssembly()?
                .GetCustomAttribute<TargetFrameworkAttribute>()?
                .FrameworkName ?? throw new Exception("Unable to determine the runtime framework.");
        }

        private string GetNuGetCompatibleFramework(string libFolderPath, string[] availableFrameworks)
        {
            // Simple framework compatibility - look for exact match first
            var runtimeVersion = ExtractFrameworkVersion(_runtimeFramework);
            
            // Find the best matching framework folder
            var compatibleFramework = availableFrameworks
                .Select(f => new { Framework = f, Version = ExtractFrameworkVersion(f) })
                .Where(f => f.Version <= runtimeVersion)
                .OrderByDescending(f => f.Version)
                .FirstOrDefault();

            if (compatibleFramework != null)
            {
                return Path.Combine(libFolderPath, compatibleFramework.Framework);
            }

            // If no compatible framework found, try the first available one
            if (availableFrameworks.Length > 0)
            {
                return Path.Combine(libFolderPath, availableFrameworks[0]);
            }

            return null;
        }

        private int ExtractFrameworkVersion(string frameworkName)
        {
            if (frameworkName.Contains("net"))
            {
                var versionPart = frameworkName.Replace("net", "").Replace(".", "");
                if (int.TryParse(versionPart, out int version))
                {
                    return version;
                }
            }
            throw new Exception($"Unable to parse framework version from {frameworkName}");
        }

        private IEnumerable<INuggetPlugin> DiscoverPluginsInAssemblies(IEnumerable<Assembly> assemblies)
        {
            var plugins = new List<INuggetPlugin>();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var pluginTypes = assembly.GetTypes()
                        .Where(type => typeof(INuggetPlugin).IsAssignableFrom(type))
                        .Where(type => !type.IsInterface && !type.IsAbstract)
                        .ToList();

                    foreach (var pluginType in pluginTypes)
                    {
                        try
                        {
                            var plugin = (INuggetPlugin)Activator.CreateInstance(pluginType);
                            plugins.Add(plugin);
                            _logger.LogDebug($"Discovered plugin: {plugin.Id} ({pluginType.FullName})");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Failed to create instance of plugin type: {pluginType.FullName}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to scan assembly for plugins: {assembly.FullName}");
                }
            }

            return plugins;
        }

        private async Task InitializePlugins(IEnumerable<INuggetPlugin> plugins)
        {
            foreach (var plugin in plugins)
            {
                try
                {
                    _logger.LogDebug($"Initializing plugin: {plugin.Id}");
                    
                    if (plugin.Initialize(_pluginContext))
                    {
                        if (plugin.Start())
                        {
                            _logger.LogDebug($"Successfully started plugin: {plugin.Id}");
                        }
                        else
                        {
                            _logger.LogWarning($"Plugin '{plugin.Id}' initialized but failed to start");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to initialize plugin: {plugin.Id}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error initializing plugin '{plugin.Id}'");
                }
            }
        }

        private NuggetDefinition CreateNuggetDefinition(string packageId, string packagePath)
        {
            return new NuggetDefinition
            {
                NuggetName = packageId,
                Name = packageId,
                Version = "1.0.0", // Could be extracted from package metadata
                Author = "Unknown",
                Description = $"Nugget loaded from {packagePath}",
                Category = "Plugin",
                CreatedDate = DateTime.UtcNow,
                IsActive = true,
                Installed = true
            };
        }

        private async Task CleanupExtractedFiles(string extractPath)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    await Task.Delay(100);
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    
                    if (Directory.Exists(extractPath))
                    {
                        Directory.Delete(extractPath, true);
                        _logger.LogDebug($"Cleaned up extracted files: {extractPath}");
                    }
                    return;
                }
                catch (IOException)
                {
                    // Wait and retry
                    await Task.Delay(200);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to cleanup extracted files: {ex.Message}");
                    break;
                }
            }
        }
        #endregion

        #region IDisposable Implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _logger.LogInfo("Disposing NuggetAssemblyManager");

                // Unload all nuggets
                var loadedPackageIds = _loadedNuggets.Keys.ToList();
                foreach (var packageId in loadedPackageIds)
                {
                    try
                    {
                        UnloadNugget(packageId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error unloading nugget '{packageId}' during disposal");
                    }
                }

                _disposed = true;
            }
        }
        #endregion
    }

    /// <summary>
    /// A collectible AssemblyLoadContext for isolating and unloading nugget assemblies.
    /// </summary>
    internal class NuggetLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver _resolver;

        public NuggetLoadContext(string name, string pluginPath) : base(name, isCollectible: true)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }
            // Defer to the default context for shared dependencies if needed.
            return null;
        }
    }
}
