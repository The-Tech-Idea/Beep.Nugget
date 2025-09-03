using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.Nugget.Engine;
using Beep.Nugget.Engine.Interfaces;

namespace Beep.Nugget.Engine.Examples
{
    /// <summary>
    /// Example demonstrating how to use the refactored NuggetAssemblyManager framework.
    /// </summary>
    public class NuggetFrameworkUsageExample
    {
        private INuggetAssemblyManager _nuggetManager;
        private INuggetLogger _logger;

        public async Task RunExample()
        {
            Console.WriteLine("=== Nugget Framework Usage Example ===\n");

            // Example 1: Create manager with default settings
            await CreateManagerWithDefaults();

            // Example 2: Create manager with custom configuration
            await CreateManagerWithCustomConfiguration();

            // Example 3: Load and manage nuggets
            await LoadAndManageNuggets();

            // Example 4: Plugin discovery and communication
            await PluginDiscoveryAndCommunication();

            // Example 5: Event handling
            await EventHandlingExample();

            // Example 6: Cleanup
            await CleanupExample();

            Console.WriteLine("\n=== Example Complete ===");
        }

        private async Task CreateManagerWithDefaults()
        {
            Console.WriteLine("1. Creating NuggetAssemblyManager with default settings...");

            // Simple creation with defaults
            _nuggetManager = NuggetAssemblyManagerFactory.CreateDefault();

            Console.WriteLine("   ? Manager created with default settings\n");
        }

        private async Task CreateManagerWithCustomConfiguration()
        {
            Console.WriteLine("2. Creating NuggetAssemblyManager with custom configuration...");

            // Create with builder pattern for advanced configuration
            _nuggetManager = NuggetAssemblyManagerFactory.CreateBuilder()
                .WithConsoleLogging("CustomNuggetManager")
                .WithEventBus()
                .WithConfiguration("sample-setting", "custom-value")
                .WithConfiguration("debug-mode", true)
                .Build();

            // Setup event handlers
            _nuggetManager.NuggetLoaded += OnNuggetLoaded;
            _nuggetManager.NuggetUnloaded += OnNuggetUnloaded;
            _nuggetManager.NuggetError += OnNuggetError;

            Console.WriteLine("   ? Manager created with custom configuration");
            Console.WriteLine("   ? Event handlers registered\n");
        }

        private async Task LoadAndManageNuggets()
        {
            Console.WriteLine("3. Loading and managing nuggets...");

            try
            {
                // Example: Load multiple nuggets
                var nuggetPaths = new Dictionary<string, string>
                {
                    // These would be real paths to .nupkg files in a real scenario
                    // { "sample-plugin-nugget", @"C:\Packages\SamplePlugin.1.0.0.nupkg" },
                    // { "datasource-nugget", @"C:\Packages\DataSourcePlugin.1.0.0.nupkg" }
                };

                Console.WriteLine("   Loading nuggets (simulated - no actual files)...");

                // In a real scenario, you would load actual nugget files:
                // var loadResults = await _nuggetManager.LoadNuggetsAsync(nuggetPaths);

                // Get summary of loaded nuggets
                var summary = _nuggetManager.GetSummary();
                Console.WriteLine($"   ?? Current status: {summary}");

                Console.WriteLine("   ? Nugget loading demonstrated\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ? Error loading nuggets: {ex.Message}\n");
            }
        }

        private async Task PluginDiscoveryAndCommunication()
        {
            Console.WriteLine("4. Plugin discovery and communication...");

            // Discover all plugins
            var allPlugins = _nuggetManager.DiscoverPlugins();
            Console.WriteLine($"   ?? Discovered {allPlugins.Count()} plugins total");

            // Discover specific plugin types
            var dataSourcePlugins = _nuggetManager.DiscoverPlugins<IDataSourceNuggetPlugin>();
            Console.WriteLine($"   ?? Discovered {dataSourcePlugins.Count()} data source plugins");

            // Get plugins by interface (extension method)
            var interfacePlugins = _nuggetManager.GetPluginsByInterface<INuggetPlugin>();
            Console.WriteLine($"   ?? Found {interfacePlugins.Count()} plugins implementing INuggetPlugin");

            // Access plugin manager functionality
            if (_nuggetManager is INuggetPluginManager pluginManager)
            {
                var loadedPlugins = pluginManager.GetLoadedPlugins();
                Console.WriteLine($"   ?? Plugin manager reports {loadedPlugins.Count()} loaded plugins");

                // Check specific plugin
                var isLoaded = pluginManager.IsPluginLoaded("sample-plugin-id");
                Console.WriteLine($"   ?? Sample plugin loaded: {isLoaded}");
            }

            Console.WriteLine("   ? Plugin discovery demonstrated\n");
        }

        private async Task EventHandlingExample()
        {
            Console.WriteLine("5. Event handling example...");

            // Simulate plugin events (in real scenario, plugins would fire these)
            Console.WriteLine("   ?? Event system is ready for plugin communication");
            Console.WriteLine("   ?? Plugins can publish and subscribe to events via the event bus");

            Console.WriteLine("   ? Event handling demonstrated\n");
        }

        private async Task CleanupExample()
        {
            Console.WriteLine("6. Cleanup example...");

            // Get list of loaded nuggets
            var loadedNuggets = _nuggetManager.GetLoadedNuggets();
            var nuggetIds = loadedNuggets.Select(n => n.PackageId).ToList();

            if (nuggetIds.Any())
            {
                Console.WriteLine($"   ?? Unloading {nuggetIds.Count} nuggets...");

                // Unload all nuggets
                var unloadResults = _nuggetManager.UnloadNuggets(nuggetIds);

                foreach (var result in unloadResults)
                {
                    var status = result.Value ? "?" : "?";
                    Console.WriteLine($"     {status} {result.Key}: {(result.Value ? "Unloaded" : "Failed")}");
                }
            }
            else
            {
                Console.WriteLine("   ?? No nuggets to unload");
            }

            // Dispose manager if it implements IDisposable
            if (_nuggetManager is IDisposable disposable)
            {
                disposable.Dispose();
                Console.WriteLine("   ?? Manager disposed");
            }

            Console.WriteLine("   ? Cleanup completed\n");
        }

        #region Event Handlers
        private void OnNuggetLoaded(object sender, NuggetLoadedEventArgs e)
        {
            var nugget = e.LoadedNugget;
            Console.WriteLine($"   ?? Nugget loaded: {nugget.PackageId}");
            Console.WriteLine($"      - Assemblies: {nugget.Assemblies.Count}");
            Console.WriteLine($"      - Plugins: {nugget.Plugins.Count}");
            Console.WriteLine($"      - Load time: {nugget.LoadedAt:HH:mm:ss}");
        }

        private void OnNuggetUnloaded(object sender, NuggetUnloadedEventArgs e)
        {
            Console.WriteLine($"   ?? Nugget unloaded: {e.PackageId} at {e.UnloadedAt:HH:mm:ss}");
        }

        private void OnNuggetError(object sender, NuggetErrorEventArgs e)
        {
            Console.WriteLine($"   ? Nugget error in {e.Operation}: {e.PackageId}");
            Console.WriteLine($"      Error: {e.Exception.Message}");
        }
        #endregion

        #region Static Usage Examples
        /// <summary>
        /// Simple usage example for quick start.
        /// </summary>
        public static async Task SimpleUsageExample()
        {
            Console.WriteLine("=== Simple Usage Example ===");

            // Create manager
            using var manager = NuggetAssemblyManagerFactory.CreateDefault();

            try
            {
                // Load a nugget (replace with actual path)
                // var nugget = await manager.LoadNuggetAsync(@"C:\Path\To\Package.nupkg", "my-package");

                // Discover plugins
                var plugins = manager.DiscoverPlugins();
                Console.WriteLine($"Found {plugins.Count()} plugins");

                // Use plugins...

                // Unload when done
                // manager.UnloadNugget("my-package");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Advanced usage example with custom configuration.
        /// </summary>
        public static async Task AdvancedUsageExample()
        {
            Console.WriteLine("=== Advanced Usage Example ===");

            // Create custom logger
            var logger = new ConsoleNuggetLogger("AdvancedExample");

            // Create custom service provider (if using DI container)
            // var serviceProvider = new ServiceCollection()
            //     .AddSingleton<IMyService, MyService>()
            //     .BuildServiceProvider();

            // Create manager with advanced configuration
            using var manager = NuggetAssemblyManagerFactory.CreateBuilder()
                .WithLogger(logger)
                .WithConsoleLogging("AdvancedNuggets")
                .WithEventBus()
                .WithConfiguration("advanced-setting", "advanced-value")
                // .WithServiceProvider(serviceProvider)
                .Build();

            // Set up event handling
            manager.NuggetLoaded += (s, e) => logger.LogInfo($"Loaded: {e.LoadedNugget.PackageId}");
            manager.NuggetError += (s, e) => logger.LogError(e.Exception, $"Error in {e.Operation}");

            // Load multiple nuggets concurrently
            var nuggetPaths = new Dictionary<string, string>
            {
                // Add your nugget paths here
            };

            var loadResults = await manager.LoadNuggetsAsync(nuggetPaths);
            logger.LogInfo($"Loaded {loadResults.Count} nuggets successfully");

            // Get summary
            var summary = manager.GetSummary();
            logger.LogInfo($"Manager summary: {summary}");

            // Discover and use data source plugins
            var dataSourcePlugins = manager.DiscoverPlugins<IDataSourceNuggetPlugin>();
            foreach (var dsPlugin in dataSourcePlugins)
            {
                logger.LogInfo($"Data source plugin: {dsPlugin.Name}");
                logger.LogInfo($"  Supports: {string.Join(", ", dsPlugin.SupportedDataSourceTypes)}");
            }
        }
        #endregion
    }

    /// <summary>
    /// Program entry point for running the example.
    /// </summary>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                var example = new NuggetFrameworkUsageExample();
                await example.RunExample();

                Console.WriteLine("\nPress any key to continue...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled error: {ex}");
            }
        }
    }
}