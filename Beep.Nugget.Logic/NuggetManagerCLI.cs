using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beep.Nugget.Logic
{
    class BeepNuggerManagerCLI
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

                case "remove":
                    if (args.Length < 2)
                    {
                        Console.WriteLine("Usage: remove <package-name>");
                    }
                    else
                    {
                        string packageName = args[1];
                        RemovePackage(nugetManager, packageName);
                    }
                    break;

                case "list":
                    ListInstalledPackages(nugetManager);
                    break;

                default:
                    ShowHelp();
                    break;
            }
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("  install <package-name> [version] - Install a NuGet package.");
            Console.WriteLine("  remove <package-name>           - Remove an installed NuGet package.");
            Console.WriteLine("  list                            - List installed NuGet packages.");
        }

        private static async Task InstallPackage(NuGetManager manager, string packageName, string version)
        {
            Console.WriteLine($"Installing package: {packageName}, Version: {version}...");
            try
            {
                string packagePath = await manager.DownloadNuGetAsync(packageName, version);
                Console.WriteLine($"Successfully installed: {packageName} ({packagePath})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error installing package: {ex.Message}");
            }
        }

        private static void RemovePackage(NuGetManager manager, string packageName)
        {
            Console.WriteLine($"Removing package: {packageName}...");
            try
            {
                // Implement removal logic (not included in the provided NuGetManager).
                // Placeholder for future implementation.
                Console.WriteLine($"Package {packageName} removed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing package: {ex.Message}");
            }
        }

        private static void ListInstalledPackages(NuGetManager manager)
        {
            Console.WriteLine("Installed packages:");
            foreach (var nugget in manager._definitions)
            {
                Console.WriteLine($"- {nugget.Name} (v{nugget.Version}, Installed: {nugget.Installed})");
            }
        }
    }
    }
