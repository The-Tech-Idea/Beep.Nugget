using System;
using System.Threading.Tasks;

namespace Beep.Nugget.Engine.Examples
{
    /// <summary>
    /// Simple usage examples for the unified NuggetManager
    /// </summary>
    public class NuggetManagerUsageExamples
    {
        /// <summary>
        /// Basic usage example showing all major operations
        /// </summary>
        public static async Task BasicUsageExample()
        {
            Console.WriteLine("=== Unified NuggetManager Usage Example ===\n");

            // Create the unified manager
            using var manager = new NuggetManager();

            // Set up event handlers for notifications
            manager.NuggetLoaded += (sender, nugget) => 
                Console.WriteLine($"? Nugget loaded: {nugget.PackageId} with {nugget.Plugins.Count} plugins");
            
            manager.NuggetUnloaded += (sender, packageId) => 
                Console.WriteLine($"? Nugget unloaded: {packageId}");
            
            manager.NuggetError += (sender, error) => 
                Console.WriteLine($"? Error with {error.PackageId}: {error.Error.Message}");

            try
            {
                Console.WriteLine("1. Searching for packages...");
                var searchResults = await manager.SearchPackagesAsync("TheTechIdea", 5);
                foreach (var result in searchResults.Take(3))
                {
                    Console.WriteLine($"   Found: {result.Identity.Id} v{result.Identity.Version}");
                }

                Console.WriteLine("\n2. Getting company nuggets...");
                await manager.RetrieveCompanyNuggetsAsync(AppContext.BaseDirectory, "TheTechIdea");
                Console.WriteLine($"   Retrieved {manager.Definitions.Count} company nuggets");

                Console.WriteLine("\n3. Database catalog operations...");
                var allDatabases = manager.GetBuiltInDatabaseNuggets();
                Console.WriteLine($"   Available database nuggets: {allDatabases.Count}");

                var popularDbs = manager.GetPopularDatabaseNuggets();
                Console.WriteLine($"   Popular databases: {popularDbs.Count}");
                foreach (var db in popularDbs.Take(3))
                {
                    Console.WriteLine($"     • {db.Name} ({db.DatabaseType}) - Port: {db.DefaultPort}");
                }

                Console.WriteLine("\n4. Connection string templates...");
                var sqlServerTemplate = manager.GetConnectionStringTemplate(TheTechIdea.Beep.Utilities.DataSourceType.SqlServer);
                Console.WriteLine($"   SQL Server template: {sqlServerTemplate}");

                Console.WriteLine("\n5. Plugin management example...");
                // In a real scenario, you would install an actual package:
                // var loadedNugget = await manager.InstallAndLoadNuggetAsync("MyPlugin.Package", "1.0.0");

                // Show currently loaded plugins
                var plugins = manager.GetPlugins();
                Console.WriteLine($"   Currently loaded plugins: {plugins.Count()}");

                // Get specific plugin types
                var dataSourcePlugins = manager.GetPlugins<IDataSourceNuggetPlugin>();
                Console.WriteLine($"   Data source plugins: {dataSourcePlugins.Count()}");

                Console.WriteLine("\n6. Get all nuggets (online + catalog)...");
                var allNuggets = await manager.GetAllNuggetsAsync(includeBuiltInDatabases: true);
                Console.WriteLine($"   Total nuggets available: {allNuggets.Count}");

                Console.WriteLine("\n? All operations completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n? Error during example: {ex.Message}");
            }

            Console.WriteLine("\n=== Example Complete ===");
        }

        /// <summary>
        /// Example focusing on plugin lifecycle management
        /// </summary>
        public static async Task PluginLifecycleExample()
        {
            Console.WriteLine("=== Plugin Lifecycle Management Example ===\n");

            using var manager = new NuggetManager();

            try
            {
                Console.WriteLine("1. Install and load a package with plugins...");
                // In real usage: var nugget = await manager.InstallAndLoadNuggetAsync("MyPluginPackage", "1.0.0");
                
                Console.WriteLine("2. Discover and work with plugins...");
                var allPlugins = manager.GetPlugins();
                Console.WriteLine($"   Total plugins: {allPlugins.Count()}");

                foreach (var plugin in allPlugins)
                {
                    Console.WriteLine($"   Plugin: {plugin.Name} v{plugin.Version}");
                    Console.WriteLine($"           {plugin.Description}");
                }

                Console.WriteLine("\n3. Get specific plugin types...");
                var dataSourcePlugins = manager.GetPlugins<IDataSourceNuggetPlugin>();
                foreach (var dsPlugin in dataSourcePlugins)
                {
                    Console.WriteLine($"   DataSource Plugin: {dsPlugin.Name}");
                    Console.WriteLine($"   Supported Types: {string.Join(", ", dsPlugin.SupportedDataSourceTypes)}");
                    
                    // Test functionality
                    bool canConnect = dsPlugin.TestConnection("TestDB", "test-connection");
                    Console.WriteLine($"   Test connection: {(canConnect ? "?" : "?")}");
                }

                Console.WriteLine("\n4. Plugin by ID lookup...");
                var specificPlugin = manager.GetPlugin("my-plugin-id");
                if (specificPlugin != null)
                {
                    Console.WriteLine($"   Found plugin: {specificPlugin.Name}");
                }
                else
                {
                    Console.WriteLine("   Plugin with ID 'my-plugin-id' not found");
                }

                Console.WriteLine("\n5. Loaded nuggets information...");
                var loadedNuggets = manager.GetLoadedNuggets();
                foreach (var nugget in loadedNuggets)
                {
                    Console.WriteLine($"   Nugget: {nugget.PackageId}");
                    Console.WriteLine($"   - Loaded: {nugget.LoadedAt:yyyy-MM-dd HH:mm:ss}");
                    Console.WriteLine($"   - Assemblies: {nugget.Assemblies.Count}");
                    Console.WriteLine($"   - Plugins: {nugget.Plugins.Count}");
                    Console.WriteLine($"   - Active: {nugget.IsActive}");
                }

                Console.WriteLine("\n6. Unload packages...");
                var packageIds = manager.GetLoadedNuggetNames().ToList();
                foreach (var packageId in packageIds)
                {
                    bool unloaded = manager.UnloadNugget(packageId);
                    Console.WriteLine($"   Unloaded {packageId}: {(unloaded ? "?" : "?")}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n? Error: {ex.Message}");
            }

            Console.WriteLine("\n=== Plugin Lifecycle Example Complete ===");
        }

        /// <summary>
        /// Example showing database catalog operations
        /// </summary>
        public static async Task DatabaseCatalogExample()
        {
            Console.WriteLine("=== Database Catalog Example ===\n");

            var manager = new NuggetManager();

            try
            {
                Console.WriteLine("1. All database nuggets...");
                var allDbs = manager.GetBuiltInDatabaseNuggets();
                Console.WriteLine($"   Total database nuggets: {allDbs.Count}");

                Console.WriteLine("\n2. Databases by category...");
                var relationalDbs = manager.GetRelationalDatabaseNuggets();
                Console.WriteLine($"   Relational databases: {relationalDbs.Count}");
                
                var nosqlDbs = manager.GetNoSQLDatabaseNuggets();
                Console.WriteLine($"   NoSQL databases: {nosqlDbs.Count}");
                
                var cloudDbs = manager.GetCloudDatabaseNuggets();
                Console.WriteLine($"   Cloud databases: {cloudDbs.Count}");

                Console.WriteLine("\n3. Popular databases...");
                var popularDbs = manager.GetPopularDatabaseNuggets();
                foreach (var db in popularDbs.Take(5))
                {
                    Console.WriteLine($"   • {db.Name} ({db.DatabaseType})");
                    Console.WriteLine($"     Port: {db.DefaultPort}");
                    Console.WriteLine($"     Packages: {string.Join(", ", db.RequiredDriverPackages)}");
                    Console.WriteLine($"     Template: {db.ConnectionStringTemplate}");
                    Console.WriteLine();
                }

                Console.WriteLine("4. Search databases...");
                var searchResults = manager.SearchDatabaseNuggets("SQL");
                Console.WriteLine($"   Found {searchResults.Count} databases matching 'SQL'");
                foreach (var db in searchResults.Take(3))
                {
                    Console.WriteLine($"   • {db.Name} - {db.Description}");
                }

                Console.WriteLine("\n5. Connection templates and utilities...");
                var dbTypes = new[] { 
                    TheTechIdea.Beep.Utilities.DataSourceType.SqlServer,
                    TheTechIdea.Beep.Utilities.DataSourceType.Mysql,
                    TheTechIdea.Beep.Utilities.DataSourceType.Postgre
                };

                foreach (var dbType in dbTypes)
                {
                    try
                    {
                        var template = manager.GetConnectionStringTemplate(dbType);
                        var port = manager.GetDefaultPort(dbType);
                        var packages = manager.GetRequiredDriverPackages(dbType);
                        
                        Console.WriteLine($"   {dbType}:");
                        Console.WriteLine($"     Template: {template}");
                        Console.WriteLine($"     Default Port: {port}");
                        Console.WriteLine($"     Packages: {string.Join(", ", packages)}");
                        Console.WriteLine();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"   {dbType}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n? Error: {ex.Message}");
            }

            Console.WriteLine("=== Database Catalog Example Complete ===");
        }

        /// <summary>
        /// Quick start example for new users
        /// </summary>
        public static async Task QuickStartExample()
        {
            Console.WriteLine("=== Quick Start Example ===\n");

            // Create manager (use 'using' for proper disposal)
            using var manager = new NuggetManager();

            // Example 1: Search for packages
            Console.WriteLine("Searching for packages...");
            var packages = await manager.SearchPackagesAsync("sample");
            Console.WriteLine($"Found {packages.Count()} packages\n");

            // Example 2: Get database catalog
            Console.WriteLine("Getting database catalog...");
            var databases = manager.GetPopularDatabaseNuggets();
            Console.WriteLine($"Popular databases: {databases.Count}\n");

            // Example 3: Install and load (in real scenario)
            Console.WriteLine("Install and load example:");
            Console.WriteLine("// var nugget = await manager.InstallAndLoadNuggetAsync(\"MyPackage\", \"1.0.0\");");
            Console.WriteLine("// var plugins = manager.GetPlugins();");
            Console.WriteLine("// manager.UnloadNugget(\"MyPackage\");");

            Console.WriteLine("\n=== Quick Start Complete ===");
        }
    }

    /// <summary>
    /// Program entry point for running examples
    /// </summary>
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                await NuggetManagerUsageExamples.BasicUsageExample();
                
                Console.WriteLine("\nPress any key to continue to plugin example...");
                Console.ReadKey();
                
                await NuggetManagerUsageExamples.PluginLifecycleExample();
                
                Console.WriteLine("\nPress any key to continue to database example...");
                Console.ReadKey();
                
                await NuggetManagerUsageExamples.DatabaseCatalogExample();
                
                Console.WriteLine("\nAll examples completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled error: {ex}");
            }
        }
    }
}