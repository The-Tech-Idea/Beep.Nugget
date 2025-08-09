using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Tools.PluginSystem;
using TheTechIdea.Beep.Logger;

namespace Beep.Nugget.Engine
{
    /// <summary>
    /// Simple plugin interface for nugget components
    /// </summary>
    public interface INuggetPlugin
    {
        string Id { get; }
        string Name { get; }
        string Version { get; }
        string Description { get; }
        bool Initialize();
        bool Start();
        bool Stop();
    }

    /// <summary>
    /// Information about a loaded nugget
    /// </summary>
    public class LoadedNugget
    {
        public string PackageId { get; set; }
        public string PackagePath { get; set; }
        public List<Assembly> Assemblies { get; set; } = new();
        public List<INuggetPlugin> Plugins { get; set; } = new();
        public DateTime LoadedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Unified Nugget Manager - the main class for all nugget operations including downloading, loading, plugin management, and database catalog access
    /// Integrated with SharedContextManager for true isolation and unloading
    /// </summary>
    public class NuggetManager : IDisposable
    {
        #region Fields
        private readonly string _repositoryUrl;
        private readonly SourceCacheContext _sourceCacheContext;
        private readonly ILogger _logger;
        private readonly PackageSource _packageSource;
        private readonly string _runtimeFramework;
        private readonly DatabaseNuggetsCatalog _databaseCatalog;
        private SharedContextManager _sharedContextManager; // Modified to allow setter

        // Nugget definitions and tracking
        public ObservableCollection<NuggetDefinition> Definitions { get; private set; }

        // Assembly loading and plugin management - mapping between packageId and SharedContext nuggetId
        private readonly Dictionary<string, string> _packageToNuggetMapping = new();
        private readonly Dictionary<string, INuggetPlugin> _loadedPlugins = new();
        private bool _disposed = false;
        #endregion

        #region Events
        public event EventHandler<LoadedNugget> NuggetLoaded;
        public event EventHandler<string> NuggetUnloaded;
        public event EventHandler<(string PackageId, Exception Error)> NuggetError;
        #endregion

        #region Constructor
        public NuggetManager(string repositoryUrl = "https://api.nuget.org/v3/index.json", SharedContextManager sharedContextManager = null)
        {
            _repositoryUrl = repositoryUrl;
            _sourceCacheContext = new SourceCacheContext();
            _logger = NullLogger.Instance;
            _packageSource = new PackageSource(_repositoryUrl);
            _runtimeFramework = GetRuntimeFramework();
            Definitions = new ObservableCollection<NuggetDefinition>();
            _databaseCatalog = new DatabaseNuggetsCatalog();

            // SharedContextManager is REQUIRED for NuggetManager to work properly
            _sharedContextManager = sharedContextManager ?? throw new ArgumentNullException(nameof(sharedContextManager), "SharedContextManager is required for true assembly isolation and unloading");
            
            // Subscribe to SharedContextManager events for nugget lifecycle management
            _sharedContextManager.NuggetLoaded += OnSharedContextNuggetLoaded;
            _sharedContextManager.NuggetUnloaded += OnSharedContextNuggetUnloaded;
        }
        #endregion

        #region SharedContextManager Integration
        /// <summary>
        /// Gets the integrated SharedContextManager instance
        /// </summary>
        public SharedContextManager GetSharedContextManager() => _sharedContextManager;

        /// <summary>
        /// Replaces the current SharedContextManager with a new one
        /// Unloads all current nuggets and resubscribes to events
        /// </summary>
        public void ReplaceSharedContextManager(SharedContextManager newSharedContextManager)
        {
            if (newSharedContextManager == null)
                throw new ArgumentNullException(nameof(newSharedContextManager));

            // Unsubscribe from old manager
            if (_sharedContextManager != null)
            {
                _sharedContextManager.NuggetLoaded -= OnSharedContextNuggetLoaded;
                _sharedContextManager.NuggetUnloaded -= OnSharedContextNuggetUnloaded;
            }

            // Clear current state
            _packageToNuggetMapping.Clear();
            _loadedPlugins.Clear();

            // Set new manager and subscribe to events
            _sharedContextManager = newSharedContextManager;
            _sharedContextManager.NuggetLoaded += OnSharedContextNuggetLoaded;
            _sharedContextManager.NuggetUnloaded += OnSharedContextNuggetUnloaded;
        }
        #endregion

        #region NuGet Package Operations (Download, Search, etc.
        /// <summary>
        /// Searches for NuGet packages in the repository
        /// </summary>
        public async Task<IEnumerable<IPackageSearchMetadata>> SearchPackagesAsync(string searchTerm, int take = 10)
        {
            var repository = Repository.Factory.GetCoreV3(_repositoryUrl);
            var resource = await repository.GetResourceAsync<PackageSearchResource>();
            var searchFilter = new SearchFilter(includePrerelease: false);

            return await resource.SearchAsync(searchTerm, searchFilter, skip: 0, take: take, log: _logger, cancellationToken: CancellationToken.None);
        }

        /// <summary>
        /// Downloads a NuGet package by ID and version
        /// </summary>
        public async Task<string> DownloadNuGetAsync(string packageId, string version = "latest")
        {
            try
            {
                var isCompany = await IsCompanyPackage(packageId);
                if (!isCompany)
                {
                    throw new InvalidOperationException($"Package {packageId} is not a valid company package.");
                }

                var repository = Repository.Factory.GetCoreV3(_repositoryUrl);
                var resource = await repository.GetResourceAsync<FindPackageByIdResource>();

                NuGetVersion nuGetVersion;
                if (version.ToLower() == "latest")
                {
                    var versions = await resource.GetAllVersionsAsync(packageId, _sourceCacheContext, _logger, CancellationToken.None);
                    nuGetVersion = versions.Max();
                }
                else
                {
                    nuGetVersion = NuGetVersion.Parse(version);
                }

                var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempFolder);

                var packagePath = Path.Combine(tempFolder, $"{packageId}.{nuGetVersion}.nupkg");

                using (var packageStream = new FileStream(packagePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    if (!await resource.CopyNupkgToStreamAsync(packageId, nuGetVersion, packageStream, _sourceCacheContext, _logger, CancellationToken.None))
                    {
                        throw new Exception($"Failed to download the NuGet package: {packageId} (v{nuGetVersion})");
                    }
                }

                _logger.LogInformation($"Successfully downloaded package: {packageId} (v{nuGetVersion})");
                return packagePath;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error downloading package {packageId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Downloads and loads a nugget package with plugin discovery
        /// </summary>
        public async Task<LoadedNugget> InstallAndLoadNuggetAsync(string packageId, string version = "latest")
        {
            // First download the package
            var packagePath = await DownloadNuGetAsync(packageId, version);

            // Then load it with plugin discovery
            return await LoadNuggetAsync(packagePath, packageId);
        }

        /// <summary>
        /// Retrieves company nuggets from the repository
        /// </summary>
        public async Task RetrieveCompanyNuggetsAsync(string projectPath, string searchTerm = "")
        {
            var repository = Repository.Factory.GetCoreV3(_repositoryUrl);
            var resource = await repository.GetResourceAsync<PackageSearchResource>();
            var searchFilter = new SearchFilter(includePrerelease: false);

            var results = await resource.SearchAsync(searchTerm, searchFilter, 0, 50, _logger, CancellationToken.None);

            foreach (var package in results)
            {
                var isCompanyPackage = await IsCompanyPackage(package.Identity.Id);
                if (isCompanyPackage)
                {
                    var nuggetDefinition = new NuggetDefinition
                    {
                        NuggetName = package.Identity.Id,
                        Name = package.Title,
                        Description = package.Description,
                        Version = package.Identity.Version.ToString(),
                        Author = package.Authors,
                        Installed = IsPackageInstalledInProjectAssetsJson(projectPath, package.Identity.Id, package.Identity.Version.ToString())
                    };
                    Definitions.Add(nuggetDefinition);
                }
            }
        }
        #endregion

        #region Assembly Loading and Plugin Management
        /// <summary>
        /// Loads a nugget package and discovers plugins using SharedContextManager exclusively
        /// </summary>
        public async Task<LoadedNugget> LoadNuggetAsync(string packagePath, string packageId)
        {
            if (string.IsNullOrEmpty(packagePath))
                throw new ArgumentException("Package path cannot be null or empty", nameof(packagePath));

            if (string.IsNullOrEmpty(packageId))
                throw new ArgumentException("Package ID cannot be null or empty", nameof(packageId));

            // Check if already loaded
            if (_packageToNuggetMapping.ContainsKey(packageId))
                throw new InvalidOperationException($"Nugget '{packageId}' is already loaded.");

            try
            {
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

                var assemblyFiles = Directory.GetFiles(compatibleFrameworkFolder, "*.dll");
                if (!assemblyFiles.Any())
                {
                    throw new FileNotFoundException($"No assemblies found in the compatible framework folder for nugget '{packageId}'.");
                }

                // Generate nugget ID for SharedContextManager
                var nuggetId = $"NuggetManager_{packageId}_{DateTime.UtcNow.Ticks}";
                
                // Use SharedContextManager exclusively - no fallback!
                var loadMethod = _sharedContextManager.GetType().GetMethod("LoadNuggetAsync");
                var loadTask = loadMethod?.Invoke(_sharedContextManager, new object[] { compatibleFrameworkFolder, nuggetId });
                
                if (loadTask is Task task)
                {
                    await task;
                    var nuggetInfoProp = task.GetType().GetProperty("Result");
                    var nuggetInfo = nuggetInfoProp?.GetValue(task);
                    
                    if (nuggetInfo != null)
                    {
                        var assembliesProp = nuggetInfo.GetType().GetProperty("LoadedAssemblies");
                        var loadedAssemblies = assembliesProp?.GetValue(nuggetInfo) as List<Assembly> ?? new List<Assembly>();
                        
                        // Store mapping for future reference
                        _packageToNuggetMapping[packageId] = nuggetId;
                        
                        // Events will be handled by OnSharedContextNuggetLoaded
                        // Schedule cleanup of extracted files
                        _ = Task.Run(async () => await CleanupExtractedFiles(extractPath));
                        
                        // Create LoadedNugget with data from SharedContextManager
                        var loadedNugget = new LoadedNugget
                        {
                            PackageId = packageId,
                            PackagePath = packagePath,
                            Assemblies = loadedAssemblies,
                            LoadedAt = DateTime.UtcNow,
                            IsActive = true
                        };

                        // Discover INuggetPlugin instances using SharedContextManager
                        var nuggetPlugins = DiscoverNuggetPluginsFromAssemblies(loadedAssemblies, nuggetId);
                        loadedNugget.Plugins.AddRange(nuggetPlugins);

                        // Update tracking
                        foreach (var plugin in nuggetPlugins)
                        {
                            _loadedPlugins[plugin.Id] = plugin;
                        }

                        // Update definition as installed
                        var definition = Definitions.FirstOrDefault(d => d.NuggetName == packageId);
                        if (definition != null)
                        {
                            definition.Installed = true;
                        }

                        NuggetLoaded?.Invoke(this, loadedNugget);
                        return loadedNugget;
                    }
                }
                
                throw new Exception($"SharedContextManager failed to load nugget '{packageId}'");
            }
            catch (Exception ex)
            {
                NuggetError?.Invoke(this, (packageId, ex));
                throw;
            }
        }

        /// <summary>
        /// Reloads an already loaded nugget
        /// </summary>
        private async Task<LoadedNugget> ReloadNuggetAsync(string packageId)
        {
            // Unload the existing nugget
            UnloadNugget(packageId);

            // Get the package metadata from the repository
            var repository = Repository.Factory.GetCoreV3(_repositoryUrl);
            var resource = await repository.GetResourceAsync<PackageMetadataResource>();
            var metadata = await resource.GetMetadataAsync(packageId, includePrerelease: false, includeUnlisted: false, _sourceCacheContext, _logger, CancellationToken.None);

            // Get the latest version if available
            var version = metadata.OrderByDescending(m => m.Identity.Version).FirstOrDefault()?.Identity.Version.ToString();

            // Re-download and load the nugget
            return await InstallAndLoadNuggetAsync(packageId, version);
        }

        /// <summary>
        /// Unloads a nugget using SharedContextManager exclusively
        /// </summary>
        public bool UnloadNugget(string packageId)
        {
            if (!_packageToNuggetMapping.TryGetValue(packageId, out var nuggetId))
            {
                Console.WriteLine($"Cannot unload nugget '{packageId}' - not found in tracking");
                return false;
            }

            try
            {
                // Stop nugget plugins first
                var pluginsToStop = _loadedPlugins.Where(kvp => kvp.Value.Id.StartsWith(nuggetId)).ToList();
                foreach (var plugin in pluginsToStop)
                {
                    try
                    {
                        plugin.Value.Stop();
                        _loadedPlugins.Remove(plugin.Key);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error stopping nugget plugin {plugin.Value.Id}: {ex.Message}");
                    }
                }

                // Use SharedContextManager to unload
                var unloadMethod = _sharedContextManager.GetType().GetMethod("UnloadNugget", new[] { typeof(string) });
                var result = unloadMethod?.Invoke(_sharedContextManager, new object[] { nuggetId });
                
                if (result is bool success && success)
                {
                    _packageToNuggetMapping.Remove(packageId);
                    
                    // Update definition as not installed
                    var definition = Definitions.FirstOrDefault(d => d.NuggetName == packageId);
                    if (definition != null)
                    {
                        definition.Installed = false;
                    }

                    // Event will be handled by OnSharedContextNuggetUnloaded
                    return true;
                }
                else
                {
                    Console.WriteLine($"SharedContextManager failed to unload nugget '{nuggetId}'");
                    return false;
                }
            }
            catch (Exception ex)
            {
                NuggetError?.Invoke(this, (packageId, ex));
                return false;
            }
        }

        /// <summary>
        /// Gets all loaded nuggets from SharedContextManager
        /// </summary>
        public IEnumerable<LoadedNugget> GetLoadedNuggets()
        {
            var loadedNuggets = new List<LoadedNugget>();

            try
            {
                // Use SharedContextManager to get loaded nuggets
                var getLoadedNuggetsMethod = _sharedContextManager.GetType().GetMethod("GetLoadedNuggets");
                var nuggetInfos = getLoadedNuggetsMethod?.Invoke(_sharedContextManager, null) as IEnumerable<object>;
                
                if (nuggetInfos != null)
                {
                    foreach (var nuggetInfo in nuggetInfos)
                    {
                        try
                        {
                            var idProp = nuggetInfo.GetType().GetProperty("Id");
                            var nameProp = nuggetInfo.GetType().GetProperty("Name");
                            var assembliesProp = nuggetInfo.GetType().GetProperty("LoadedAssemblies");
                            var loadedAtProp = nuggetInfo.GetType().GetProperty("LoadedAt");
                            var isActiveProp = nuggetInfo.GetType().GetProperty("IsActive");
                            var sourcePathProp = nuggetInfo.GetType().GetProperty("SourcePath");

                            var nuggetId = idProp?.GetValue(nuggetInfo)?.ToString();
                            var nuggetName = nameProp?.GetValue(nuggetInfo)?.ToString();
                            var assemblies = assembliesProp?.GetValue(nuggetInfo) as List<Assembly> ?? new List<Assembly>();
                            var loadedAt = loadedAtProp?.GetValue(nuggetInfo) is DateTime dt ? dt : DateTime.UtcNow;
                            var isActive = isActiveProp?.GetValue(nuggetInfo) is bool active && active;
                            var sourcePath = sourcePathProp?.GetValue(nuggetInfo)?.ToString();

                            if (!string.IsNullOrEmpty(nuggetId) && isActive)
                            {
                                // Find corresponding package ID
                                var packageId = _packageToNuggetMapping.FirstOrDefault(kvp => kvp.Value == nuggetId).Key;
                                if (string.IsNullOrEmpty(packageId))
                                {
                                    packageId = nuggetName; // Fallback to nugget name
                                }

                                var loadedNugget = new LoadedNugget
                                {
                                    PackageId = packageId,
                                    PackagePath = sourcePath,
                                    Assemblies = assemblies,
                                    LoadedAt = loadedAt,
                                    IsActive = isActive
                                };

                                // Add nugget plugins
                                var plugins = _loadedPlugins.Where(p => p.Value.Id.StartsWith(nuggetId)).Select(p => p.Value).ToList();
                                loadedNugget.Plugins.AddRange(plugins);

                                loadedNuggets.Add(loadedNugget);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error processing nugget info: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting loaded nuggets from SharedContextManager: {ex.Message}");
            }

            return loadedNuggets;
        }

        /// <summary>
        /// Gets all discovered plugins
        /// </summary>
        public IEnumerable<INuggetPlugin> GetPlugins()
        {
            return _loadedPlugins.Values.ToList();
        }

        /// <summary>
        /// Gets plugins of a specific type
        /// </summary>
        public IEnumerable<T> GetPlugins<T>() where T : class, INuggetPlugin
        {
            return _loadedPlugins.Values.OfType<T>().ToList();
        }

        /// <summary>
        /// Gets a specific plugin by ID
        /// </summary>
        public INuggetPlugin GetPlugin(string id)
        {
            return _loadedPlugins.TryGetValue(id, out var plugin) ? plugin : null;
        }
        #endregion

        #region Database Catalog Operations
        /// <summary>
        /// Gets the database nuggets catalog
        /// </summary>
        public DatabaseNuggetsCatalog GetDatabaseCatalog() => _databaseCatalog;

        /// <summary>
        /// Gets all built-in database nuggets
        /// </summary>
        public List<DatabaseNuggetDefinition> GetBuiltInDatabaseNuggets()
        {
            return _databaseCatalog.GetAllDatabaseNuggets().ToList();
        }

        /// <summary>
        /// Gets database nuggets by category
        /// </summary>
        public List<DatabaseNuggetDefinition> GetDatabaseNuggetsByCategory(DatasourceCategory category)
        {
            return _databaseCatalog.GetDatabaseNuggetsByCategory(category);
        }

        /// <summary>
        /// Searches database nuggets
        /// </summary>
        public List<DatabaseNuggetDefinition> SearchDatabaseNuggets(string searchTerm)
        {
            return _databaseCatalog.SearchDatabaseNuggets(searchTerm);
        }

        /// <summary>
        /// Gets popular database nuggets
        /// </summary>
        public List<DatabaseNuggetDefinition> GetPopularDatabaseNuggets()
        {
            return _databaseCatalog.GetPopularDatabaseNuggets();
        }

        /// <summary>
        /// Gets cloud database nuggets
        /// </summary>
        public List<DatabaseNuggetDefinition> GetCloudDatabaseNuggets()
        {
            return _databaseCatalog.GetCloudDatabaseNuggets();
        }

        /// <summary>
        /// Gets NoSQL database nuggets
        /// </summary>
        public List<DatabaseNuggetDefinition> GetNoSQLDatabaseNuggets()
        {
            return _databaseCatalog.GetNoSQLDatabaseNuggets();
        }

        /// <summary>
        /// Gets relational database nuggets
        /// </summary>
        public List<DatabaseNuggetDefinition> GetRelationalDatabaseNuggets()
        {
            return _databaseCatalog.GetRelationalDatabaseNuggets();
        }

        /// <summary>
        /// Gets connection string template for a database type
        /// </summary>
        public string GetConnectionStringTemplate(DataSourceType databaseType)
        {
            return _databaseCatalog.GetConnectionStringTemplate(databaseType);
        }

        /// <summary>
        /// Gets default port for a database type
        /// </summary>
        public int GetDefaultPort(DataSourceType databaseType)
        {
            return _databaseCatalog.GetDefaultPort(databaseType);
        }

        /// <summary>
        /// Gets required driver packages for a database type
        /// </summary>
        public List<string> GetRequiredDriverPackages(DataSourceType databaseType)
        {
            return _databaseCatalog.GetRequiredDriverPackages(databaseType);
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// Gets all nuggets (online + database catalog)
        /// </summary>
        public async Task<List<NuggetDefinition>> GetAllNuggetsAsync(bool includeBuiltInDatabases = true)
        {
            var allNuggets = new List<NuggetDefinition>();
            allNuggets.AddRange(Definitions);

            if (includeBuiltInDatabases)
            {
                var databaseNuggets = GetBuiltInDatabaseNuggets();
                allNuggets.AddRange(databaseNuggets.Cast<NuggetDefinition>());
            }

            return allNuggets;
        }

        /// <summary>
        /// Checks if a package is installed in project assets
        /// </summary>
        public bool IsPackageInstalledInProjectAssetsJson(string projectDir, string packageId, string version)
        {
            string assetsFilePath = Path.Combine(projectDir, "obj", "project.assets.json");
            if (!File.Exists(assetsFilePath)) return false;

            try
            {
                var assetsFileContent = File.ReadAllText(assetsFilePath);
                var assets = System.Text.Json.JsonDocument.Parse(assetsFileContent);

                if (assets.RootElement.TryGetProperty("libraries", out var libraries))
                {
                    foreach (var dependency in libraries.EnumerateObject())
                    {
                        if (dependency.Name.StartsWith(packageId, StringComparison.OrdinalIgnoreCase) &&
                            dependency.Value.TryGetProperty("version", out var versionProperty) &&
                            versionProperty.GetString() == version)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch { return false; }
        }

        /// <summary>
        /// Gets simple package names for backward compatibility
        /// </summary>
        public IEnumerable<string> GetLoadedNuggetNames() => _packageToNuggetMapping.Keys.ToList();
        #endregion

        #region Private Helper Methods
        private async Task<bool> IsCompanyPackage(string packageId)
        {
            var repository = Repository.Factory.GetCoreV3(_repositoryUrl);
            var resource = await repository.GetResourceAsync<PackageMetadataResource>();
            var metadata = await resource.GetMetadataAsync(packageId, includePrerelease: false, includeUnlisted: false, _sourceCacheContext, _logger, CancellationToken.None);
            return metadata.Any(m => m.Authors.Contains("TheTechIdea", StringComparison.OrdinalIgnoreCase));
        }

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

        private IEnumerable<INuggetPlugin> DiscoverPlugins(IEnumerable<Assembly> assemblies)
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
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to create plugin instance {pluginType.FullName}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to scan assembly for plugins {assembly.FullName}: {ex.Message}");
                }
            }

            return plugins;
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
                    }
                    return;
                }
                catch (IOException)
                {
                    await Task.Delay(200);
                }
                catch (Exception)
                {
                    break;
                }
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
            var runtimeVersion = ExtractFrameworkVersion(_runtimeFramework);

            var compatibleFramework = availableFrameworks
                .Select(f => new { Framework = f, Version = ExtractFrameworkVersion(f) })
                .Where(f => f.Version <= runtimeVersion)
                .OrderByDescending(f => f.Version)
                .FirstOrDefault();

            if (compatibleFramework != null)
            {
                return Path.Combine(libFolderPath, compatibleFramework.Framework);
            }

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
        #endregion

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                // Unsubscribe from SharedContextManager events
                _sharedContextManager.NuggetLoaded -= OnSharedContextNuggetLoaded;
                _sharedContextManager.NuggetUnloaded -= OnSharedContextNuggetUnloaded;

                // Unload all packages using SharedContextManager
                var loadedPackageIds = _packageToNuggetMapping.Keys.ToList();
                foreach (var packageId in loadedPackageIds)
                {
                    try
                    {
                        UnloadNugget(packageId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error unloading nugget {packageId} during disposal: {ex.Message}");
                    }
                }

                _sourceCacheContext?.Dispose();
                _disposed = true;
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles SharedContextManager nugget loaded events
        /// </summary>
        private void OnSharedContextNuggetLoaded(object sender, object eventArgs)
        {
            // Use reflection to access NuggetEventArgs properties since they're in different project
            var nuggetInfoProp = eventArgs.GetType().GetProperty("NuggetInfo");
            var nuggetInfo = nuggetInfoProp?.GetValue(eventArgs);
            
            if (nuggetInfo != null)
            {
                var idProp = nuggetInfo.GetType().GetProperty("Id");
                var nameProp = nuggetInfo.GetType().GetProperty("Name");
                var assembliesProp = nuggetInfo.GetType().GetProperty("LoadedAssemblies");
                var loadedAtProp = nuggetInfo.GetType().GetProperty("LoadedAt");
                var isActiveProp = nuggetInfo.GetType().GetProperty("IsActive");
                var sourcePathProp = nuggetInfo.GetType().GetProperty("SourcePath");

                var nuggetId = idProp?.GetValue(nuggetInfo)?.ToString();
                var nuggetName = nameProp?.GetValue(nuggetInfo)?.ToString();
                var assemblies = assembliesProp?.GetValue(nuggetInfo) as List<Assembly> ?? new List<Assembly>();
                var loadedAt = loadedAtProp?.GetValue(nuggetInfo) is DateTime dt ? dt : DateTime.UtcNow;
                var isActive = isActiveProp?.GetValue(nuggetInfo) is bool active && active;
                var sourcePath = sourcePathProp?.GetValue(nuggetInfo)?.ToString();

                if (!string.IsNullOrEmpty(nuggetId))
                {
                    // Find corresponding package ID from mapping
                    var packageId = _packageToNuggetMapping.FirstOrDefault(kvp => kvp.Value == nuggetId).Key;
                    if (string.IsNullOrEmpty(packageId))
                    {
                        packageId = nuggetName; // Fallback to nugget name
                    }

                    // Create LoadedNugget for the event
                    var loadedNugget = new LoadedNugget
                    {
                        PackageId = packageId,
                        PackagePath = sourcePath,
                        Assemblies = assemblies,
                        LoadedAt = loadedAt,
                        IsActive = isActive
                    };

                    // Discover INuggetPlugin instances from SharedContext assemblies
                    var nuggetPlugins = DiscoverNuggetPluginsFromAssemblies(assemblies, nuggetId);
                    loadedNugget.Plugins.AddRange(nuggetPlugins);

                    // Update tracking
                    foreach (var plugin in nuggetPlugins)
                    {
                        _loadedPlugins[plugin.Id] = plugin;
                    }

                    // Update definition as installed
                    var definition = Definitions.FirstOrDefault(d => d.NuggetName == packageId);
                    if (definition != null)
                    {
                        definition.Installed = true;
                    }

                    NuggetLoaded?.Invoke(this, loadedNugget);
                }
            }
        }

        /// <summary>
        /// Handles SharedContextManager nugget unloaded events
        /// </summary>
        private void OnSharedContextNuggetUnloaded(object sender, object eventArgs)
        {
            // Use reflection to access NuggetEventArgs properties
            var nuggetInfoProp = eventArgs.GetType().GetProperty("NuggetInfo");
            var nuggetInfo = nuggetInfoProp?.GetValue(eventArgs);
            
            if (nuggetInfo != null)
            {
                var idProp = nuggetInfo.GetType().GetProperty("Id");
                var nameProp = nuggetInfo.GetType().GetProperty("Name");
                var nuggetId = idProp?.GetValue(nuggetInfo)?.ToString();
                var nuggetName = nameProp?.GetValue(nuggetInfo)?.ToString();

                if (!string.IsNullOrEmpty(nuggetId))
                {
                    // Find corresponding package ID
                    var packageId = _packageToNuggetMapping.FirstOrDefault(kvp => kvp.Value == nuggetId).Key;
                    if (string.IsNullOrEmpty(packageId))
                    {
                        packageId = nuggetName; // Fallback to nugget name
                    }

                    // Remove plugins from tracking
                    var pluginsToRemove = _loadedPlugins.Where(kvp => kvp.Value.Id.StartsWith(nuggetId)).ToList();
                    foreach (var plugin in pluginsToRemove)
                    {
                        _loadedPlugins.Remove(plugin.Key);
                    }

                    // Clean up mapping
                    _packageToNuggetMapping.Remove(packageId);

                    // Update definition as not installed
                    var definition = Definitions.FirstOrDefault(d => d.NuggetName == packageId);
                    if (definition != null)
                    {
                        definition.Installed = false;
                    }

                    NuggetUnloaded?.Invoke(this, packageId);
                }
            }
        }

        /// <summary>
        /// Discovers INuggetPlugin instances from assemblies using SharedContextManager
        /// </summary>
        private List<INuggetPlugin> DiscoverNuggetPluginsFromAssemblies(List<Assembly> assemblies, string nuggetId)
        {
            var nuggetPlugins = new List<INuggetPlugin>();

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
                            // Use SharedContextManager to create instance
                            var createInstanceMethod = _sharedContextManager.GetType().GetMethod("CreateInstance", new[] { typeof(string), typeof(object[]) });
                            var instance = createInstanceMethod?.Invoke(_sharedContextManager, new object[] { pluginType.FullName, new object[0] });
                            var plugin = instance as INuggetPlugin;
                            
                            if (plugin != null)
                            {
                                if (plugin.Initialize())
                                {
                                    plugin.Start();
                                    nuggetPlugins.Add(plugin);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to create nugget plugin instance {pluginType.FullName}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to scan assembly for nugget plugins {assembly.FullName}: {ex.Message}");
                }
            }

            return nuggetPlugins;
        }
        #endregion
    }
}