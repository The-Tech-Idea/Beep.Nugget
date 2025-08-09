using System;
using System.Threading.Tasks;

namespace Beep.Nugget.Engine.Examples
{
    /// <summary>
    /// Example showing how to use the unified NuggetManager with plugin system
    /// </summary>
    public class NuggetUsageExample
    {
        public static async Task RunExample()
        {
            Console.WriteLine("=== Unified NuggetManager Plugin System Example ===\n");

            // Create the unified nugget manager
            using var manager = new NuggetManager();

            // Set up event handlers
            manager.NuggetLoaded += OnNuggetLoaded;
            manager.NuggetUnloaded += OnNuggetUnloaded;
            manager.NuggetError += OnNuggetError;

            try
            {
                Console.WriteLine("1. Installing and loading a nugget package...");
                // In a real scenario, you would install an actual package
                // var nugget = await manager.InstallAndLoadNuggetAsync("MyPlugin.Package", "1.0.0");

                Console.WriteLine("2. Discovering plugins...");
                var allPlugins = manager.GetPlugins();
                Console.WriteLine($"   Found {allPlugins.Count()} plugins total");

                // Get specific plugin types
                var dataSourcePlugins = manager.GetPlugins<IDataSourceNuggetPlugin>();
                Console.WriteLine($"   Found {dataSourcePlugins.Count()} data source plugins");

                // Work with plugins
                foreach (var plugin in allPlugins)
                {
                    Console.WriteLine($"   Plugin: {plugin.Name} v{plugin.Version} - {plugin.Description}");
                }

                Console.WriteLine("\n3. Working with data source plugins...");
                foreach (var dsPlugin in dataSourcePlugins)
                {
                    Console.WriteLine($"   Data Source Plugin: {dsPlugin.Name}");
                    Console.WriteLine($"   Supported Types: {string.Join(", ", dsPlugin.SupportedDataSourceTypes)}");
                    
                    // Test connection
                    bool canConnect = dsPlugin.TestConnection("SampleDB", "test-connection-string");
                    Console.WriteLine($"   Test Connection: {(canConnect ? "Success" : "Failed")}");
                    
                    if (canConnect)
                    {
                        // Create data source
                        var dataSource = dsPlugin.CreateDataSource("SampleDB", "test-connection-string");
                        Console.WriteLine($"   Created data source: {dataSource?.GetType().Name}");
                    }
                }

                Console.WriteLine("\n4. Getting loaded nuggets info...");
                var loadedNuggets = manager.GetLoadedNuggets();
                foreach (var nugget in loadedNuggets)
                {
                    Console.WriteLine($"   Nugget: {nugget.PackageId}");
                    Console.WriteLine($"   - Assemblies: {nugget.Assemblies.Count}");
                    Console.WriteLine($"   - Plugins: {nugget.Plugins.Count}");
                    Console.WriteLine($"   - Loaded at: {nugget.LoadedAt:yyyy-MM-dd HH:mm:ss}");
                    Console.WriteLine($"   - Active: {nugget.IsActive}");
                }

                Console.WriteLine("\n5. Database catalog operations...");
                var popularDatabases = manager.GetPopularDatabaseNuggets();
                Console.WriteLine($"   Popular databases available: {popularDatabases.Count}");
                foreach (var db in popularDatabases.Take(3))
                {
                    Console.WriteLine($"   • {db.Name} ({db.DatabaseType}) - Port: {db.DefaultPort}");
                }

                // Get connection templates
                var sqlServerTemplate = manager.GetConnectionStringTemplate(TheTechIdea.Beep.Utilities.DataSourceType.SqlServer);
                Console.WriteLine($"   SQL Server template: {sqlServerTemplate}");

                Console.WriteLine("\n6. Plugin management...");
                // Get specific plugin
                var specificPlugin = manager.GetPlugin("sample-basic-plugin");
                if (specificPlugin != null)
                {
                    Console.WriteLine($"   Found plugin: {specificPlugin.Name}");
                }
                else
                {
                    Console.WriteLine("   No sample plugin loaded (install a package first)");
                }

                Console.WriteLine("\n7. Company nuggets...");
                await manager.RetrieveCompanyNuggetsAsync(AppContext.BaseDirectory, "TheTechIdea");
                Console.WriteLine($"   Retrieved {manager.Definitions.Count} company nuggets");

                Console.WriteLine("\n8. All available nuggets...");
                var allNuggets = await manager.GetAllNuggetsAsync(includeBuiltInDatabases: true);
                Console.WriteLine($"   Total nuggets (online + catalog): {allNuggets.Count}");

                Console.WriteLine("\n9. Unloading nuggets...");
                var nuggetNames = manager.GetLoadedNuggetNames().ToList();
                if (nuggetNames.Any())
                {
                    foreach (var nuggetName in nuggetNames)
                    {
                        bool unloaded = manager.UnloadNugget(nuggetName);
                        Console.WriteLine($"   Unloaded {nuggetName}: {(unloaded ? "Success" : "Failed")}");
                    }
                }
                else
                {
                    Console.WriteLine("   No nuggets currently loaded");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("\n=== Example Complete ===");
        }

        private static void OnNuggetLoaded(object sender, LoadedNugget nugget)
        {
            Console.WriteLine($"   ? Nugget loaded: {nugget.PackageId} with {nugget.Plugins.Count} plugins");
        }

        private static void OnNuggetUnloaded(object sender, string packageId)
        {
            Console.WriteLine($"   ? Nugget unloaded: {packageId}");
        }

        private static void OnNuggetError(object sender, (string PackageId, Exception Error) errorInfo)
        {
            Console.WriteLine($"   ? Error with nugget {errorInfo.PackageId}: {errorInfo.Error.Message}");
        }
    }

    /// <summary>
    /// Simple usage example for quick start
    /// </summary>
    public class SimpleUsageExample
    {
        public static async Task QuickStart()
        {
            Console.WriteLine("=== Quick Start Example ===");

            // Create unified manager
            using var manager = new NuggetManager();

            try
            {
                Console.WriteLine("1. Search for packages...");
                var searchResults = await manager.SearchPackagesAsync("TheTechIdea", 3);
                foreach (var result in searchResults)
                {
                    Console.WriteLine($"   Found: {result.Identity.Id} v{result.Identity.Version}");
                }

                Console.WriteLine("\n2. Database catalog...");
                var popularDbs = manager.GetPopularDatabaseNuggets();
                Console.WriteLine($"   Popular databases: {popularDbs.Count}");
                foreach (var db in popularDbs.Take(3))
                {
                    Console.WriteLine($"   • {db.Name} - {db.Description}");
                }

                Console.WriteLine("\n3. Install and load example:");
                Console.WriteLine("   // var nugget = await manager.InstallAndLoadNuggetAsync(\"MyPackage\", \"1.0.0\");");
                Console.WriteLine("   // var plugins = manager.GetPlugins();");
                Console.WriteLine("   // manager.UnloadNugget(\"MyPackage\");");

                Console.WriteLine("\n4. Plugin discovery...");
                var plugins = manager.GetPlugins();
                Console.WriteLine($"   Current plugins: {plugins.Count()}");

                // Get data source plugins
                var dataSourcePlugins = manager.GetPlugins<IDataSourceNuggetPlugin>();
                foreach (var plugin in dataSourcePlugins)
                {
                    Console.WriteLine($"   Data Source: {plugin.Name}");
                    Console.WriteLine($"     Types: {string.Join(", ", plugin.SupportedDataSourceTypes)}");
                }

                Console.WriteLine("\n5. Connection templates...");
                try
                {
                    var sqlTemplate = manager.GetConnectionStringTemplate(TheTechIdea.Beep.Utilities.DataSourceType.SqlServer);
                    var mysqlTemplate = manager.GetConnectionStringTemplate(TheTechIdea.Beep.Utilities.DataSourceType.Mysql);
                    Console.WriteLine($"   SQL Server: {sqlTemplate}");
                    Console.WriteLine($"   MySQL: {mysqlTemplate}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"   Template error: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine("\n=== Quick Start Complete ===");
        }
    }

    /// <summary>
    /// Advanced usage example showing integration scenarios
    /// </summary>
    public class AdvancedUsageExample
    {
        public static async Task AdvancedScenarios()
        {
            Console.WriteLine("=== Advanced Usage Scenarios ===\n");

            using var manager = new NuggetManager();

            // Set up comprehensive event handling
            manager.NuggetLoaded += (s, n) => Console.WriteLine($"?? Loaded: {n.PackageId} ({n.Plugins.Count} plugins)");
            manager.NuggetUnloaded += (s, id) => Console.WriteLine($"?? Unloaded: {id}");
            manager.NuggetError += (s, e) => Console.WriteLine($"? Error: {e.Error.Message}");

            try
            {
                Console.WriteLine("1. Comprehensive package management...");
                
                // Get company nuggets
                await manager.RetrieveCompanyNuggetsAsync(AppContext.BaseDirectory, "TheTechIdea");
                
                // Show installation status
                foreach (var nugget in manager.Definitions.Take(5))
                {
                    string status = nugget.Installed ? "? Installed" : "? Available";
                    Console.WriteLine($"   {status} {nugget.NuggetName} v{nugget.Version}");
                }

                Console.WriteLine("\n2. Database ecosystem...");
                
                // Categorized database access
                var categories = new[] {
                    (Category: TheTechIdea.Beep.Utilities.DatasourceCategory.RDBMS, Name: "Relational"),
                    (Category: TheTechIdea.Beep.Utilities.DatasourceCategory.NOSQL, Name: "NoSQL"),
                    (Category: TheTechIdea.Beep.Utilities.DatasourceCategory.CLOUD, Name: "Cloud")
                };

                foreach (var (category, name) in categories)
                {
                    var databases = manager.GetDatabaseNuggetsByCategory(category);
                    Console.WriteLine($"   {name} databases: {databases.Count}");
                }

                Console.WriteLine("\n3. Plugin ecosystem analysis...");
                
                var allPlugins = manager.GetPlugins();
                var dataSourcePlugins = manager.GetPlugins<IDataSourceNuggetPlugin>();
                
                Console.WriteLine($"   Total plugins: {allPlugins.Count()}");
                Console.WriteLine($"   Data source plugins: {dataSourcePlugins.Count()}");

                // Plugin capability analysis
                foreach (var dsPlugin in dataSourcePlugins)
                {
                    Console.WriteLine($"   ?? {dsPlugin.Name}:");
                    Console.WriteLine($"      Supports: {string.Join(", ", dsPlugin.SupportedDataSourceTypes)}");
                    
                    // Test capabilities
                    foreach (var dbType in dsPlugin.SupportedDataSourceTypes.Take(2))
                    {
                        bool canTest = dsPlugin.TestConnection(dbType, "test-connection");
                        Console.WriteLine($"      {dbType}: {(canTest ? "?" : "?")} Connection test");
                    }
                }

                Console.WriteLine("\n4. Integration workflows...");
                
                // Workflow: Search -> Install -> Use -> Unload
                Console.WriteLine("   Workflow demonstration:");
                Console.WriteLine("   1. Search: var packages = await manager.SearchPackagesAsync(\"MyTech\");");
                Console.WriteLine("   2. Install: var nugget = await manager.InstallAndLoadNuggetAsync(package.Id);");
                Console.WriteLine("   3. Use: var plugins = manager.GetPlugins<IMyPluginType>();");
                Console.WriteLine("   4. Unload: manager.UnloadNugget(package.Id);");

                Console.WriteLine("\n5. Configuration and templates...");
                
                // Show connection configuration for popular databases
                var popularDbTypes = new[] {
                    TheTechIdea.Beep.Utilities.DataSourceType.SqlServer,
                    TheTechIdea.Beep.Utilities.DataSourceType.Mysql,
                    TheTechIdea.Beep.Utilities.DataSourceType.Postgre,
                    TheTechIdea.Beep.Utilities.DataSourceType.MongoDB
                };

                foreach (var dbType in popularDbTypes)
                {
                    try
                    {
                        var template = manager.GetConnectionStringTemplate(dbType);
                        var port = manager.GetDefaultPort(dbType);
                        var packages = manager.GetRequiredDriverPackages(dbType);
                        
                        Console.WriteLine($"   ?? {dbType}:");
                        Console.WriteLine($"      Port: {port}");
                        Console.WriteLine($"      Packages: {string.Join(", ", packages)}");
                        Console.WriteLine($"      Template: {template.Substring(0, Math.Min(50, template.Length))}...");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"   ?? {dbType}: Configuration not available ({ex.Message})");
                    }
                }

                Console.WriteLine("\n6. Performance and monitoring...");
                
                var loadedNuggets = manager.GetLoadedNuggets();
                if (loadedNuggets.Any())
                {
                    foreach (var nugget in loadedNuggets)
                    {
                        var uptime = DateTime.UtcNow - nugget.LoadedAt;
                        Console.WriteLine($"   ?? {nugget.PackageId}: Uptime {uptime.TotalMinutes:F1}m, {nugget.Assemblies.Count} assemblies");
                    }
                }
                else
                {
                    Console.WriteLine("   ?? No nuggets currently loaded");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n? Advanced scenario error: {ex.Message}");
            }

            Console.WriteLine("\n=== Advanced Scenarios Complete ===");
        }
    }
}