using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheTechIdea.Beep.Utilities;

namespace Beep.Nugget.Engine
{
    class BeepNuggetManagerCLI
    {
        static async Task Main(string[] args)
        {
            var manager = new NuggetManager(); // Use unified NuggetManager

            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            try
            {
                switch (args[0].ToLower())
                {
                    case "install":
                        if (args.Length >= 2)
                        {
                            string version = args.Length >= 3 ? args[2] : "latest";
                            await InstallPackage(manager, args[1], version);
                        }
                        else
                        {
                            Console.WriteLine("Usage: install <package-name> [version]");
                        }
                        break;

                    case "search":
                        if (args.Length >= 2)
                        {
                            await SearchPackages(manager, args[1]);
                        }
                        else
                        {
                            Console.WriteLine("Usage: search <search-term>");
                        }
                        break;

                    case "list":
                        if (args.Length >= 2)
                        {
                            await ListCompanyPackages(manager, args[1]);
                        }
                        else
                        {
                            await ListCompanyPackages(manager, ".");
                        }
                        break;

                    case "databases":
                        await ShowAllDatabases(manager);
                        break;

                    case "db-category":
                        if (args.Length >= 2)
                        {
                            if (Enum.TryParse<DatasourceCategory>(args[1], true, out var category))
                            {
                                await ShowDatabasesByCategory(manager, category);
                            }
                            else
                            {
                                Console.WriteLine($"Invalid category: {args[1]}");
                                ShowCategories();
                            }
                        }
                        else
                        {
                            ShowCategories();
                        }
                        break;

                    case "db-popular":
                        await ShowPopularDatabases(manager);
                        break;

                    case "db-cloud":
                        await ShowCloudDatabases(manager);
                        break;

                    case "db-nosql":
                        await ShowNoSQLDatabases(manager);
                        break;

                    case "db-relational":
                        await ShowRelationalDatabases(manager);
                        break;

                    case "connection":
                        if (args.Length >= 2)
                        {
                            if (Enum.TryParse<DataSourceType>(args[1], true, out var dbType))
                            {
                                ShowConnectionTemplate(manager, dbType);
                            }
                            else
                            {
                                Console.WriteLine($"Invalid database type: {args[1]}");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Usage: connection <database-type>");
                        }
                        break;

                    case "plugins":
                        await ShowLoadedPlugins(manager);
                        break;

                    case "loaded":
                        await ShowLoadedNuggets(manager);
                        break;

                    case "unload":
                        if (args.Length >= 2)
                        {
                            await UnloadPackage(manager, args[1]);
                        }
                        else
                        {
                            Console.WriteLine("Usage: unload <package-name>");
                        }
                        break;

                    case "registry":
                        ShowRegistryInfo();
                        break;

                    case "help":
                    default:
                        ShowHelp();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Beep Nugget Manager CLI");
            Console.WriteLine("Usage: BeepNuggetManager <command> [options]");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  install <package> [version]    Install and load a nugget package");
            Console.WriteLine("  search <term>                  Search for packages");
            Console.WriteLine("  list [project-path]            List company packages");
            Console.WriteLine("  databases                      Show all database nuggets");
            Console.WriteLine("  db-category [category]         Show databases by category");
            Console.WriteLine("  db-popular                     Show popular databases");
            Console.WriteLine("  db-cloud                       Show cloud databases");
            Console.WriteLine("  db-nosql                       Show NoSQL databases");
            Console.WriteLine("  db-relational                  Show relational databases");
            Console.WriteLine("  connection <db-type>           Show connection template");
            Console.WriteLine("  plugins                        Show loaded plugins");
            Console.WriteLine("  loaded                         Show loaded nuggets");
            Console.WriteLine("  unload <package>               Unload a nugget package");
            Console.WriteLine("  registry                       Show registry information");
            Console.WriteLine("  help                           Show this help");
        }

        private static async Task InstallPackage(NuggetManager manager, string packageName, string version)
        {
            Console.WriteLine($"Installing package: {packageName} (version: {version})");
            try
            {
                var loadedNugget = await manager.InstallAndLoadNuggetAsync(packageName, version);
                Console.WriteLine($"✓ Successfully installed and loaded: {packageName}");
                Console.WriteLine($"  - Assemblies loaded: {loadedNugget.Assemblies.Count}");
                Console.WriteLine($"  - Plugins discovered: {loadedNugget.Plugins.Count}");
                
                foreach (var plugin in loadedNugget.Plugins)
                {
                    Console.WriteLine($"    • {plugin.Name} v{plugin.Version}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed to install package: {ex.Message}");
            }
        }

        private static async Task SearchPackages(NuggetManager manager, string searchTerm)
        {
            Console.WriteLine($"Searching for packages: {searchTerm}");
            try
            {
                var results = await manager.SearchPackagesAsync(searchTerm, 10);
                foreach (var result in results)
                {
                    Console.WriteLine($"  {result.Identity.Id} v{result.Identity.Version}");
                    Console.WriteLine($"    {result.Description}");
                    Console.WriteLine($"    Authors: {result.Authors}");
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Search failed: {ex.Message}");
            }
        }

        private static async Task ListCompanyPackages(NuggetManager manager, string projectPath)
        {
            Console.WriteLine("Retrieving company packages...");
            try
            {
                await manager.RetrieveCompanyNuggetsAsync(projectPath, "TheTechIdea");
                foreach (var package in manager.Definitions)
                {
                    string status = package.Installed ? "✓ Installed" : "  Available";
                    Console.WriteLine($"{status} {package.NuggetName} v{package.Version}");
                    Console.WriteLine($"         {package.Description}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Failed to retrieve packages: {ex.Message}");
            }
        }

        private static async Task ShowAllDatabases(NuggetManager manager)
        {
            Console.WriteLine("All Database Nuggets:");
            var databases = manager.GetBuiltInDatabaseNuggets();
            foreach (var db in databases)
            {
                Console.WriteLine($"  {db.Name} ({db.DatabaseType})");
                Console.WriteLine($"    Category: {db.DatabaseCategory}");
                Console.WriteLine($"    Default Port: {db.DefaultPort}");
                Console.WriteLine($"    Packages: {string.Join(", ", db.RequiredDriverPackages)}");
                Console.WriteLine();
            }
        }

        private static async Task ShowDatabasesByCategory(NuggetManager manager, DatasourceCategory category)
        {
            Console.WriteLine($"Database Nuggets - Category: {category}");
            var databases = manager.GetDatabaseNuggetsByCategory(category);
            foreach (var db in databases)
            {
                Console.WriteLine($"  {db.Name} ({db.DatabaseType})");
                Console.WriteLine($"    Default Port: {db.DefaultPort}");
                Console.WriteLine($"    Packages: {string.Join(", ", db.RequiredDriverPackages)}");
                Console.WriteLine();
            }
        }

        private static async Task ShowPopularDatabases(NuggetManager manager)
        {
            Console.WriteLine("Popular Database Nuggets:");
            var databases = manager.GetPopularDatabaseNuggets();
            foreach (var db in databases)
            {
                Console.WriteLine($"  {db.Name} ({db.DatabaseType})");
                Console.WriteLine($"    {db.Description}");
                Console.WriteLine();
            }
        }

        private static async Task ShowCloudDatabases(NuggetManager manager)
        {
            Console.WriteLine("Cloud Database Nuggets:");
            var databases = manager.GetCloudDatabaseNuggets();
            foreach (var db in databases)
            {
                Console.WriteLine($"  {db.Name} ({db.DatabaseType})");
                Console.WriteLine($"    {db.Description}");
                Console.WriteLine();
            }
        }

        private static async Task ShowNoSQLDatabases(NuggetManager manager)
        {
            Console.WriteLine("NoSQL Database Nuggets:");
            var databases = manager.GetNoSQLDatabaseNuggets();
            foreach (var db in databases)
            {
                Console.WriteLine($"  {db.Name} ({db.DatabaseType})");
                Console.WriteLine($"    {db.Description}");
                Console.WriteLine();
            }
        }

        private static async Task ShowRelationalDatabases(NuggetManager manager)
        {
            Console.WriteLine("Relational Database Nuggets:");
            var databases = manager.GetRelationalDatabaseNuggets();
            foreach (var db in databases)
            {
                Console.WriteLine($"  {db.Name} ({db.DatabaseType})");
                Console.WriteLine($"    {db.Description}");
                Console.WriteLine();
            }
        }

        private static void ShowConnectionTemplate(NuggetManager manager, DataSourceType databaseType)
        {
            try
            {
                var template = manager.GetConnectionStringTemplate(databaseType);
                var port = manager.GetDefaultPort(databaseType);
                
                Console.WriteLine($"Connection Template for {databaseType}:");
                Console.WriteLine($"  Template: {template}");
                Console.WriteLine($"  Default Port: {port}");
                
                var packages = manager.GetRequiredDriverPackages(databaseType);
                Console.WriteLine($"  Required Packages: {string.Join(", ", packages)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error getting connection template: {ex.Message}");
            }
        }

        private static async Task ShowLoadedPlugins(NuggetManager manager)
        {
            Console.WriteLine("Loaded Plugins:");
            var plugins = manager.GetPlugins();
            foreach (var plugin in plugins)
            {
                Console.WriteLine($"  {plugin.Name} v{plugin.Version} (ID: {plugin.Id})");
                Console.WriteLine($"    {plugin.Description}");
                Console.WriteLine();
            }
        }

        private static async Task ShowLoadedNuggets(NuggetManager manager)
        {
            Console.WriteLine("Loaded Nuggets:");
            var nuggets = manager.GetLoadedNuggets();
            foreach (var nugget in nuggets)
            {
                Console.WriteLine($"  {nugget.PackageId}");
                Console.WriteLine($"    Loaded: {nugget.LoadedAt}");
                Console.WriteLine($"    Assemblies: {nugget.Assemblies.Count}");
                Console.WriteLine($"    Plugins: {nugget.Plugins.Count}");
                Console.WriteLine($"    Active: {nugget.IsActive}");
                Console.WriteLine();
            }
        }

        private static async Task UnloadPackage(NuggetManager manager, string packageName)
        {
            Console.WriteLine($"Unloading package: {packageName}");
            try
            {
                bool success = manager.UnloadNugget(packageName);
                if (success)
                {
                    Console.WriteLine($"✓ Successfully unloaded: {packageName}");
                }
                else
                {
                    Console.WriteLine($"✗ Failed to unload: {packageName} (not loaded or already unloaded)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error unloading package: {ex.Message}");
            }
        }

        private static void ShowRegistryInfo()
        {
            Console.WriteLine("Database Registry Information:");
            Console.WriteLine("  Built-in database definitions provide connection templates,");
            Console.WriteLine("  default ports, and required NuGet packages for various database types.");
            Console.WriteLine("  Use 'db-category' to explore databases by category.");
        }

        private static void ShowCategories()
        {
            Console.WriteLine("Available database categories:");
            foreach (var category in Enum.GetValues<DatasourceCategory>())
            {
                Console.WriteLine($"  {category}");
            }
        }
    }
}
