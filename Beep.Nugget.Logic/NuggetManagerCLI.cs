using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheTechIdea.Beep.Utilities;

namespace Beep.Nugget.Logic
{
    class BeepNuggetManagerCLI
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            string command = args[0].ToLower();
            var nugetManager = new NuGetManager();

            switch (command)
            {
                case "install":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Usage: install <package-name> [version]");
                    }
                    else
                    {
                        string packageName = args[1];
                        string version = args.Length > 2 ? args[2] : "latest";
                        await InstallPackage(nugetManager, packageName, version);
                    }
                    break;

                case "search":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Usage: search <search-term>");
                    }
                    else
                    {
                        string searchTerm = args[1];
                        await SearchPackages(nugetManager, searchTerm);
                    }
                    break;

                case "list":
                    string projectPath = args.Length > 1 ? args[1] : Directory.GetCurrentDirectory();
                    await ListCompanyPackages(nugetManager, projectPath);
                    break;

                case "databases":
                case "db":
                    if (args.Length > 1)
                    {
                        await ShowDatabasesByCategory(nugetManager, args[1]);
                    }
                    else
                    {
                        await ShowAllDatabases(nugetManager);
                    }
                    break;

                case "popular":
                    await ShowPopularDatabases(nugetManager);
                    break;

                case "cloud":
                    await ShowCloudDatabases(nugetManager);
                    break;

                case "nosql":
                    await ShowNoSQLDatabases(nugetManager);
                    break;

                case "relational":
                case "rdbms":
                    await ShowRelationalDatabases(nugetManager);
                    break;

                case "connection":
                case "conn":
                    if (args.Length > 1)
                    {
                        ShowConnectionTemplate(nugetManager, args[1]);
                    }
                    else
                    {
                        Console.WriteLine("Usage: connection <database-type>");
                        Console.WriteLine("Example: connection SqlServer");
                    }
                    break;

                case "registry":
                case "info":
                    ShowRegistryInfo();
                    break;

                case "categories":
                    ShowCategories();
                    break;

                case "help":
                default:
                    ShowHelp();
                    break;
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Beep Nugget Manager CLI");
            Console.WriteLine("=======================");
            Console.WriteLine("Usage:");
            Console.WriteLine("  install <package-name> [version] - Install a NuGet package from TheTechIdea.");
            Console.WriteLine("  search <search-term>              - Search for TheTechIdea packages.");
            Console.WriteLine("  list [project-path]               - List TheTechIdea packages and their installation status.");
            Console.WriteLine("  databases [category]              - Show built-in database nuggets (optional: by category).");
            Console.WriteLine("  popular                           - Show popular database nuggets.");
            Console.WriteLine("  cloud                             - Show cloud database nuggets.");
            Console.WriteLine("  nosql                             - Show NoSQL database nuggets.");
            Console.WriteLine("  relational                        - Show relational database nuggets.");
            Console.WriteLine("  connection <database-type>        - Show connection string template for database type.");
            Console.WriteLine("  registry                          - Show database registry statistics.");
            Console.WriteLine("  categories                        - Show all available database categories.");
            Console.WriteLine("  help                              - Show this help message.");
            Console.WriteLine("");
            Console.WriteLine("Database Categories:");
            Console.WriteLine("  RDBMS, NOSQL, CLOUD, GraphDB, TimeSeriesDB, DocumentDB, KeyValueDB, ColumnarDB, VectorDB");
            Console.WriteLine("");
            Console.WriteLine("Examples:");
            Console.WriteLine("  BeepNuggetManagerCLI install TheTechIdea.Beep.Core");
            Console.WriteLine("  BeepNuggetManagerCLI search Beep");
            Console.WriteLine("  BeepNuggetManagerCLI databases RDBMS");
            Console.WriteLine("  BeepNuggetManagerCLI popular");
            Console.WriteLine("  BeepNuggetManagerCLI connection SqlServer");
            Console.WriteLine("  BeepNuggetManagerCLI registry");
            Console.WriteLine("  BeepNuggetManagerCLI categories");
            Console.WriteLine("  BeepNuggetManagerCLI list C:\\MyProject");
        }

        private static async Task InstallPackage(NuGetManager manager, string packageName, string version)
        {
            Console.WriteLine($"Installing package: {packageName}, Version: {version}...");
            try
            {
                string packagePath = await manager.DownloadNuGetAsync(packageName, version);
                Console.WriteLine($"Successfully downloaded: {packageName} to {packagePath}");
                
                // Load into running application
                await manager.AddNuGetToRunningApplication(packagePath);
                Console.WriteLine($"Successfully loaded {packageName} into the running application.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error installing package: {ex.Message}");
            }
        }

        private static async Task SearchPackages(NuGetManager manager, string searchTerm)
        {
            Console.WriteLine($"Searching for packages containing '{searchTerm}'...");
            try
            {
                var packages = await manager.SearchPackagesAsync(searchTerm, 20);
                
                if (!packages.Any())
                {
                    Console.WriteLine("No packages found.");
                    return;
                }

                Console.WriteLine($"Found {packages.Count()} packages:");
                Console.WriteLine("");
                
                foreach (var package in packages)
                {
                    Console.WriteLine($"Package: {package.Identity.Id}");
                    Console.WriteLine($"  Version: {package.Identity.Version}");
                    Console.WriteLine($"  Authors: {package.Authors}");
                    Console.WriteLine($"  Description: {package.Description ?? "No description available"}");
                    Console.WriteLine($"  Download Count: {package.DownloadCount ?? 0}");
                    Console.WriteLine("");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error searching packages: {ex.Message}");
            }
        }

        private static async Task ListCompanyPackages(NuGetManager manager, string projectPath)
        {
            Console.WriteLine($"Retrieving TheTechIdea packages for project at: {projectPath}");
            try
            {
                await manager.RetrieveCompanyNuggetsAsync(projectPath, "TheTechIdea");
                
                if (!manager._definitions.Any())
                {
                    Console.WriteLine("No TheTechIdea packages found.");
                    return;
                }

                Console.WriteLine($"Found {manager._definitions.Count} TheTechIdea packages:");
                Console.WriteLine("");
                
                foreach (var nugget in manager._definitions)
                {
                    var status = nugget.Installed ? "✓ Installed" : "✗ Not Installed";
                    Console.WriteLine($"{status} - {nugget.NuggetName} (v{nugget.Version})");
                    Console.WriteLine($"  Name: {nugget.Name}");
                    Console.WriteLine($"  Author: {nugget.Author}");
                    Console.WriteLine($"  Description: {nugget.Description ?? "No description available"}");
                    Console.WriteLine("");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing packages: {ex.Message}");
            }
        }

        private static async Task ShowAllDatabases(NuGetManager manager)
        {
            Console.WriteLine("Built-in Database Nuggets:");
            Console.WriteLine("=========================");
            try
            {
                var databases = manager.GetBuiltInDatabaseNuggets();
                
                if (!databases.Any())
                {
                    Console.WriteLine("No built-in database nuggets found.");
                    return;
                }

                var groupedByCategory = databases.GroupBy(d => d.DatabaseCategory);
                
                foreach (var category in groupedByCategory.OrderBy(g => g.Key.ToString()))
                {
                    Console.WriteLine($"\n{category.Key}:");
                    foreach (var db in category.OrderBy(d => d.Name))
                    {
                        Console.WriteLine($"  • {db.Name} ({db.DatabaseType})");
                        Console.WriteLine($"    Package: {db.NuggetName}");
                        Console.WriteLine($"    Port: {(db.DefaultPort > 0 ? db.DefaultPort.ToString() : "N/A")}")
;                        Console.WriteLine($"    Description: {db.Description}");
                        Console.WriteLine("");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing databases: {ex.Message}");
            }
        }

        private static async Task ShowDatabasesByCategory(NuGetManager manager, string categoryName)
        {
            Console.WriteLine($"Database Nuggets - Category: {categoryName}");
            Console.WriteLine("===========================================");
            try
            {
                if (!Enum.TryParse<DatasourceCategory>(categoryName, true, out var category))
                {
                    Console.WriteLine($"Invalid category: {categoryName}");
                    Console.WriteLine("Valid categories: RDBMS, NOSQL, CLOUD, GraphDB, TimeSeriesDB, DocumentDB, KeyValueDB, ColumnarDB");
                    return;
                }

                var databases = manager.GetDatabaseNuggetsByCategory(category);
                
                if (!databases.Any())
                {
                    Console.WriteLine($"No database nuggets found for category: {category}");
                    return;
                }

                foreach (var db in databases.OrderBy(d => d.Name))
                {
                    Console.WriteLine($"• {db.Name} ({db.DatabaseType})");
                    Console.WriteLine($"  Package: {db.NuggetName}");
                    Console.WriteLine($"  Port: {(db.DefaultPort > 0 ? db.DefaultPort.ToString() : "N/A")}")
;
                    Console.WriteLine($"  Authentication: {(db.RequiresAuthentication ? "Required" : "Optional")}");
                    Console.WriteLine($"  Transactions: {(db.SupportsTransactions ? "Supported" : "Not Supported")}");
                    Console.WriteLine($"  Description: {db.Description}");
                    Console.WriteLine("");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing databases by category: {ex.Message}");
            }
        }

        private static async Task ShowPopularDatabases(NuGetManager manager)
        {
            Console.WriteLine("Popular Database Nuggets:");
            Console.WriteLine("========================");
            try
            {
                var databases = manager.GetPopularDatabaseNuggets();
                
                foreach (var db in databases)
                {
                    Console.WriteLine($"• {db.Name} ({db.DatabaseCategory})");
                    Console.WriteLine($"  Package: {db.NuggetName}");
                    Console.WriteLine($"  Default Port: {(db.DefaultPort > 0 ? db.DefaultPort.ToString() : "N/A")}" );
                    Console.WriteLine($"  Description: {db.Description}");
                    Console.WriteLine("");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing popular databases: {ex.Message}");
            }
        }

        private static async Task ShowCloudDatabases(NuGetManager manager)
        {
            Console.WriteLine("Cloud Database Nuggets:");
            Console.WriteLine("======================");
            try
            {
                var databases = manager.GetCloudDatabaseNuggets();
                
                foreach (var db in databases)
                {
                    Console.WriteLine($"• {db.Name} ({db.DatabaseType})");
                    Console.WriteLine($"  Package: {db.NuggetName}");
                    Console.WriteLine($"  Category: {db.DatabaseCategory}");
                    Console.WriteLine($"  Description: {db.Description}");
                    Console.WriteLine("");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing cloud databases: {ex.Message}");
            }
        }

        private static async Task ShowNoSQLDatabases(NuGetManager manager)
        {
            Console.WriteLine("NoSQL Database Nuggets:");
            Console.WriteLine("======================");
            try
            {
                var databases = manager.GetNoSQLDatabaseNuggets();
                
                foreach (var db in databases)
                {
                    Console.WriteLine($"• {db.Name} ({db.DatabaseCategory})");
                    Console.WriteLine($"  Package: {db.NuggetName}");
                    Console.WriteLine($"  Default Port: {(db.DefaultPort > 0 ? db.DefaultPort.ToString() : "N/A")}" );
                    Console.WriteLine($"  Transactions: {(db.SupportsTransactions ? "Supported" : "Not Supported")}");
                    Console.WriteLine($"  Description: {db.Description}");
                    Console.WriteLine("");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing NoSQL databases: {ex.Message}");
            }
        }

        private static async Task ShowRelationalDatabases(NuGetManager manager)
        {
            Console.WriteLine("Relational Database Nuggets:");
            Console.WriteLine("===========================");
            try
            {
                var databases = manager.GetRelationalDatabaseNuggets();
                
                foreach (var db in databases)
                {
                    Console.WriteLine($"• {db.Name} ({db.DatabaseType})");
                    Console.WriteLine($"  Package: {db.NuggetName}");
                    Console.WriteLine($"  Default Port: {db.DefaultPort}");
                    Console.WriteLine($"  Description: {db.Description}");
                    Console.WriteLine("");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing relational databases: {ex.Message}");
            }
        }

        private static void ShowConnectionTemplate(NuGetManager manager, string databaseTypeName)
        {
            Console.WriteLine($"Connection Template for {databaseTypeName}:");
            Console.WriteLine("==========================================");
            try
            {
                if (!Enum.TryParse<DataSourceType>(databaseTypeName, true, out var dbType))
                {
                    Console.WriteLine($"Invalid database type: {databaseTypeName}");
                    Console.WriteLine("Examples: SqlServer, Oracle, Mysql, Postgre, MongoDB, Redis");
                    return;
                }

                // Use the new extension methods for cleaner code
                Console.WriteLine($"Database: {dbType.GetFriendlyName()}");
                Console.WriteLine($"Type: {dbType}");
                Console.WriteLine($"Category: {dbType.GetDatabaseCategory().GetFriendlyName()}");
                Console.WriteLine($"Official Package: {dbType.GetOfficialNuggetPackage()}");
                Console.WriteLine($"Default Port: {(dbType.GetDefaultPort() > 0 ? dbType.GetDefaultPort().ToString() : "N/A")}");
                Console.WriteLine($"Authentication: {(dbType.RequiresAuthentication() ? "Required" : "Optional")}");
                Console.WriteLine($"Transactions: {(dbType.SupportsTransactions() ? "Supported" : "Not Supported")}");
                Console.WriteLine($"Cloud Database: {(dbType.IsCloudDatabase() ? "Yes" : "No")}");
                Console.WriteLine($"NoSQL Database: {(dbType.IsNoSQLDatabase() ? "Yes" : "No")}");
                Console.WriteLine($"Vector Database: {(dbType.IsVectorDatabase() ? "Yes" : "No")}");
                Console.WriteLine("");
                Console.WriteLine("Connection String Template:");
                Console.WriteLine(dbType.GetConnectionStringTemplate());
                Console.WriteLine("");
                
                var requiredDrivers = dbType.GetRequiredDriverPackages();
                if (requiredDrivers.Any())
                {
                    Console.WriteLine("Required Driver Packages:");
                    foreach (var driver in requiredDrivers)
                    {
                        Console.WriteLine($"  • {driver}");
                    }
                    Console.WriteLine("");
                }

                // Show similar databases in the same category
                var similarDatabases = dbType.GetSameCategoryDatabases().Where(d => d != dbType).Take(3);
                if (similarDatabases.Any())
                {
                    Console.WriteLine($"Other {dbType.GetDatabaseCategory().GetFriendlyName()}:");
                    foreach (var similar in similarDatabases)
                    {
                        Console.WriteLine($"  • {similar.GetFriendlyName()} - {similar.GetOfficialNuggetPackage()}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing connection template: {ex.Message}");
            }
        }

        private static void ShowRegistryInfo()
        {
            Console.WriteLine("Database Nugget Registry Information:");
            Console.WriteLine("====================================");
            try
            {
                var allDbTypes = DatabaseNuggetRegistry.GetAllDatabaseTypes().ToList();
                Console.WriteLine($"Total registered databases: {allDbTypes.Count}");
                Console.WriteLine("");
                
                var categoriesCount = allDbTypes.GroupBy(db => db.GetDatabaseCategory()).OrderBy(g => g.Key.ToString());
                
                Console.WriteLine("Databases by Category:");
                foreach (var categoryGroup in categoriesCount)
                {
                    var category = categoryGroup.Key;
                    var count = categoryGroup.Count();
                    Console.WriteLine($"  {category.GetFriendlyName()}: {count} databases");
                    
                    foreach (var dbType in categoryGroup.OrderBy(db => db.GetFriendlyName()))
                    {
                        Console.WriteLine($"    • {dbType.GetFriendlyName()} ({dbType}) - {dbType.GetOfficialNuggetPackage()}");
                    }
                    Console.WriteLine("");
                }

                Console.WriteLine("Registry Statistics:");
                Console.WriteLine($"  Cloud Databases: {allDbTypes.Count(db => db.IsCloudDatabase())}");
                Console.WriteLine($"  NoSQL Databases: {allDbTypes.Count(db => db.IsNoSQLDatabase())}");
                Console.WriteLine($"  Relational Databases: {allDbTypes.Count(db => db.IsRelationalDatabase())}");
                Console.WriteLine($"  Vector Databases: {allDbTypes.Count(db => db.IsVectorDatabase())}");
                Console.WriteLine($"  Databases requiring authentication: {allDbTypes.Count(db => db.RequiresAuthentication())}");
                Console.WriteLine($"  Databases supporting transactions: {allDbTypes.Count(db => db.SupportsTransactions())}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing registry info: {ex.Message}");
            }
        }

        private static void ShowCategories()
        {
            Console.WriteLine("Available Database Categories:");
            Console.WriteLine("=============================");
            try
            {
                var allCategories = Enum.GetValues<DatasourceCategory>()
                    .Where(c => c != DatasourceCategory.NONE)
                    .Where(c => DatabaseNuggetRegistry.GetDatabaseTypesByCategory(c).Any())
                    .OrderBy(c => c.ToString());

                foreach (var category in allCategories)
                {
                    var dbTypes = DatabaseNuggetRegistry.GetDatabaseTypesByCategory(category).ToList();
                    Console.WriteLine($"{category.GetFriendlyName()} ({category}):");
                    Console.WriteLine($"  {dbTypes.Count} databases available");
                    Console.WriteLine($"  Examples: {string.Join(", ", dbTypes.Take(3).Select(db => db.GetFriendlyName()))}");
                    Console.WriteLine("");
                }

                Console.WriteLine("Usage Examples:");
                Console.WriteLine("  BeepNuggetManagerCLI databases RDBMS");
                Console.WriteLine("  BeepNuggetManagerCLI databases VectorDB");
                Console.WriteLine("  BeepNuggetManagerCLI databases DocumentDB");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing categories: {ex.Message}");
            }
        }
    }
}
