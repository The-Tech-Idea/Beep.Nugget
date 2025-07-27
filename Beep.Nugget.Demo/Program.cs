using Beep.Nugget.Logic;
using TheTechIdea.Beep.Utilities;

namespace Beep.Nugget.Demo
{
    /// <summary>
    /// Example program demonstrating how to use the Beep.Nugget library
    /// to download and manage TheTechIdea packages at runtime.
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Beep Nugget Manager Demo ===");
            Console.WriteLine("This demo shows how to use the library to manage TheTechIdea packages at runtime.\n");

            try
            {
                // Create a new NuGet manager instance
                var nugetManager = new NuGetManager();
                Console.WriteLine("? NuGet Manager initialized");

                // Example 1: Search for TheTechIdea packages
                Console.WriteLine("\n1. Searching for TheTechIdea packages...");
                var searchResults = await nugetManager.SearchPackagesAsync("TheTechIdea", 5);
                
                Console.WriteLine($"Found {searchResults.Count()} packages:");
                foreach (var package in searchResults.Take(3))
                {
                    Console.WriteLine($"  • {package.Identity.Id} (v{package.Identity.Version})");
                    Console.WriteLine($"    Authors: {package.Authors}");
                    Console.WriteLine($"    Downloads: {package.DownloadCount ?? 0}");
                }

                // Example 2: Get company packages list
                Console.WriteLine("\n2. Retrieving TheTechIdea company packages...");
                await nugetManager.RetrieveCompanyNuggetsAsync(AppContext.BaseDirectory, "TheTechIdea");
                
                Console.WriteLine($"Found {nugetManager._definitions.Count} company packages:");
                foreach (var nugget in nugetManager._definitions.Take(3))
                {
                    var status = nugget.Installed ? "? Installed" : "? Not Installed";
                    Console.WriteLine($"  {status} {nugget.NuggetName} (v{nugget.Version})");
                }

                // Example 3: Download a package (if any company packages exist)
                if (nugetManager._definitions.Any())
                {
                    var firstPackage = nugetManager._definitions.First();
                    Console.WriteLine($"\n3. Downloading package: {firstPackage.NuggetName}...");
                    
                    try
                    {
                        string packagePath = await nugetManager.DownloadNuGetAsync(
                            firstPackage.NuggetName, 
                            firstPackage.Version);
                        Console.WriteLine($"? Package downloaded to: {packagePath}");

                        // Example 4: Load the package into the running application
                        Console.WriteLine("\n4. Loading package into running application...");
                        await nugetManager.AddNuGetToRunningApplication(packagePath);
                        Console.WriteLine("? Package loaded successfully");

                        // Update the nugget definition
                        firstPackage.Installed = true;
                        Console.WriteLine($"? {firstPackage.NuggetName} marked as installed");
                    }
                    catch (InvalidOperationException ex) when (ex.Message.Contains("not a valid company package"))
                    {
                        Console.WriteLine($"? Skipping {firstPackage.NuggetName}: Not a valid TheTechIdea package");
                    }
                }

                // Example 5: Show loaded assemblies
                Console.WriteLine("\n5. Checking loaded TheTechIdea assemblies...");
                var loadedAssemblies = nugetManager.GetLoadedCompanyAssemblies();
                
                if (loadedAssemblies.Count > 0)
                {
                    Console.WriteLine($"Found {loadedAssemblies.Count} loaded TheTechIdea assemblies:");
                    foreach (var assembly in loadedAssemblies)
                    {
                        var name = assembly.GetName();
                        Console.WriteLine($"  • {name.Name} (v{name.Version})");
                    }
                }
                else
                {
                    Console.WriteLine("  No TheTechIdea assemblies currently loaded");
                }

                // Example 6: Show built-in database nuggets using the new registry approach
                Console.WriteLine("\n6. Exploring built-in database nuggets with dictionary approach...");
                var databaseNuggets = nugetManager.GetBuiltInDatabaseNuggets();
                Console.WriteLine($"Found {databaseNuggets.Count} built-in database nuggets:");
                
                // Show popular databases
                var popularDatabases = nugetManager.GetPopularDatabaseNuggets();
                Console.WriteLine($"\nPopular databases ({popularDatabases.Count}):");
                foreach (var db in popularDatabases.Take(5))
                {
                    Console.WriteLine($"  • {db.Name} ({db.DatabaseType}) - Port: {(db.DefaultPort > 0 ? db.DefaultPort.ToString() : "N/A")}");
                    Console.WriteLine($"    Package: {db.NuggetName}");
                }

                // Example 7: Using extension methods for cleaner code
                Console.WriteLine("\n7. Using extension methods for database info...");
                var sqlServerType = TheTechIdea.Beep.Utilities.DataSourceType.SqlServer;
                Console.WriteLine($"SQL Server Info:");
                Console.WriteLine($"  Friendly Name: {sqlServerType.GetFriendlyName()}");
                Console.WriteLine($"  Official Package: {sqlServerType.GetOfficialNuggetPackage()}");
                Console.WriteLine($"  Category: {sqlServerType.GetDatabaseCategory().GetFriendlyName()}");
                Console.WriteLine($"  Default Port: {sqlServerType.GetDefaultPort()}");
                Console.WriteLine($"  Requires Auth: {sqlServerType.RequiresAuthentication()}");
                Console.WriteLine($"  Supports Transactions: {sqlServerType.SupportsTransactions()}");
                Console.WriteLine($"  Is Cloud Database: {sqlServerType.IsCloudDatabase()}");
                Console.WriteLine($"  Is NoSQL: {sqlServerType.IsNoSQLDatabase()}");
                Console.WriteLine($"  Is Relational: {sqlServerType.IsRelationalDatabase()}");

                var mongoType = TheTechIdea.Beep.Utilities.DataSourceType.MongoDB;
                Console.WriteLine($"\nMongoDB Info:");
                Console.WriteLine($"  Friendly Name: {mongoType.GetFriendlyName()}");
                Console.WriteLine($"  Official Package: {mongoType.GetOfficialNuggetPackage()}");
                Console.WriteLine($"  Category: {mongoType.GetDatabaseCategory().GetFriendlyName()}");
                Console.WriteLine($"  Default Port: {mongoType.GetDefaultPort()}");
                Console.WriteLine($"  Is NoSQL: {mongoType.IsNoSQLDatabase()}");
                Console.WriteLine($"  Connection Template: {mongoType.GetConnectionStringTemplate()}");

                // Example 8: Working with categories using extension methods
                Console.WriteLine("\n8. Working with database categories...");
                var rdbmsCategory = DatasourceCategory.RDBMS;
                Console.WriteLine($"{rdbmsCategory.GetFriendlyName()} databases:");
                foreach (var dbType in rdbmsCategory.GetDatabaseTypes().Take(3))
                {
                    Console.WriteLine($"  • {dbType.GetFriendlyName()} - {dbType.GetOfficialNuggetPackage()}");
                }

                var vectorCategory = DatasourceCategory.VectorDB;
                Console.WriteLine($"\n{vectorCategory.GetFriendlyName()} databases:");
                foreach (var dbType in vectorCategory.GetDatabaseTypes())
                {
                    Console.WriteLine($"  • {dbType.GetFriendlyName()} - {dbType.GetOfficialNuggetPackage()}");
                }

                // Example 9: Show registry statistics
                Console.WriteLine("\n9. Database registry statistics...");
                var allDbTypes = DatabaseNuggetRegistry.GetAllDatabaseTypes().ToList();
                Console.WriteLine($"Total registered databases: {allDbTypes.Count}");
                
                var categoriesCount = allDbTypes.GroupBy(db => db.GetDatabaseCategory());
                foreach (var categoryGroup in categoriesCount.OrderBy(g => g.Key.ToString()))
                {
                    Console.WriteLine($"  {categoryGroup.Key.GetFriendlyName()}: {categoryGroup.Count()} databases");
                }

                Console.WriteLine("\n=== Demo completed successfully! ===");
                Console.WriteLine("\nNew Dictionary-Based Approach Benefits:");
                Console.WriteLine("? Centralized database information in DatabaseNuggetRegistry");
                Console.WriteLine("? No more switch statements - just dictionary lookups");
                Console.WriteLine("? Extension methods for clean, readable code");
                Console.WriteLine("? Easy to add new databases - just add to the dictionary");
                Console.WriteLine("? Consistent data structure for all database types");
                Console.WriteLine("? Type-safe access to database properties");
                
                Console.WriteLine("\nTo use this library in your application:");
                Console.WriteLine("1. Create a NuGetManager instance");
                Console.WriteLine("2. Use SearchPackagesAsync() or RetrieveCompanyNuggetsAsync() to find packages");
                Console.WriteLine("3. Use DownloadNuGetAsync() to download packages");
                Console.WriteLine("4. Use AddNuGetToRunningApplication() to load them at runtime");
                Console.WriteLine("5. Use extension methods like DataSourceType.SqlServer.GetOfficialNuggetPackage()");
                Console.WriteLine("6. Use GetBuiltInDatabaseNuggets() to access database catalog");
                Console.WriteLine("7. Use GetConnectionStringTemplate() for database connections");
                Console.WriteLine("8. Use the WinForms UI for a graphical interface");
                Console.WriteLine("9. Use the CLI for command-line operations");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n? Error during demo: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}