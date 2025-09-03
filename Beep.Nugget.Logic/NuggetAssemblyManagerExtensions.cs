using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.Nugget.Engine.Interfaces;

namespace Beep.Nugget.Engine
{
    /// <summary>
    /// Extension methods for the NuggetAssemblyManager to provide additional convenience methods.
    /// </summary>
    public static class NuggetAssemblyManagerExtensions
    {
        /// <summary>
        /// Loads multiple nuggets concurrently.
        /// </summary>
        /// <param name="manager">The nugget assembly manager.</param>
        /// <param name="nuggetPaths">Dictionary of package ID to package path mappings.</param>
        /// <returns>A dictionary of package ID to LoadedNugget results.</returns>
        public static async Task<Dictionary<string, LoadedNugget>> LoadNuggetsAsync(
            this INuggetAssemblyManager manager,
            Dictionary<string, string> nuggetPaths)
        {
            var results = new Dictionary<string, LoadedNugget>();
            var tasks = new List<Task>();

            foreach (var kvp in nuggetPaths)
            {
                var packageId = kvp.Key;
                var packagePath = kvp.Value;

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var result = await manager.LoadNuggetAsync(packagePath, packageId);
                        lock (results)
                        {
                            results[packageId] = result;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error but don't fail other loads
                        Console.WriteLine($"Failed to load nugget {packageId}: {ex.Message}");
                    }
                }));
            }

            await Task.WhenAll(tasks);
            return results;
        }

        /// <summary>
        /// Unloads multiple nuggets.
        /// </summary>
        /// <param name="manager">The nugget assembly manager.</param>
        /// <param name="packageIds">The package IDs to unload.</param>
        /// <returns>A dictionary of package ID to unload success status.</returns>
        public static Dictionary<string, bool> UnloadNuggets(
            this INuggetAssemblyManager manager,
            IEnumerable<string> packageIds)
        {
            var results = new Dictionary<string, bool>();

            foreach (var packageId in packageIds)
            {
                try
                {
                    results[packageId] = manager.UnloadNugget(packageId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to unload nugget {packageId}: {ex.Message}");
                    results[packageId] = false;
                }
            }

            return results;
        }

        /// <summary>
        /// Gets all plugins of a specific interface type from all loaded nuggets.
        /// </summary>
        /// <typeparam name="T">The interface type to search for.</typeparam>
        /// <param name="manager">The nugget assembly manager.</param>
        /// <returns>Collection of plugins implementing the specified interface.</returns>
        public static IEnumerable<T> GetPluginsByInterface<T>(this INuggetAssemblyManager manager) 
            where T : class
        {
            var plugins = new List<T>();

            foreach (var nugget in manager.GetLoadedNuggets())
            {
                foreach (var assembly in nugget.Assemblies)
                {
                    try
                    {
                        var interfaceTypes = assembly.GetTypes()
                            .Where(type => typeof(T).IsAssignableFrom(type))
                            .Where(type => !type.IsInterface && !type.IsAbstract);

                        foreach (var type in interfaceTypes)
                        {
                            try
                            {
                                var instance = Activator.CreateInstance(type) as T;
                                if (instance != null)
                                {
                                    plugins.Add(instance);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Failed to create instance of {type.FullName}: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to scan assembly {assembly.FullName}: {ex.Message}");
                    }
                }
            }

            return plugins;
        }

        /// <summary>
        /// Gets summary information about all loaded nuggets.
        /// </summary>
        /// <param name="manager">The nugget assembly manager.</param>
        /// <returns>Summary information about loaded nuggets.</returns>
        public static NuggetManagerSummary GetSummary(this INuggetAssemblyManager manager)
        {
            var nuggets = manager.GetLoadedNuggets().ToList();
            
            return new NuggetManagerSummary
            {
                TotalNuggets = nuggets.Count,
                TotalAssemblies = nuggets.Sum(n => n.Assemblies.Count),
                TotalPlugins = nuggets.Sum(n => n.Plugins.Count),
                ActiveNuggets = nuggets.Count(n => n.IsActive),
                LoadedNuggetIds = nuggets.Select(n => n.PackageId).ToList(),
                LoadedPluginIds = nuggets.SelectMany(n => n.Plugins).Select(p => p.Id).ToList()
            };
        }
    }

    /// <summary>
    /// Summary information about the nugget manager state.
    /// </summary>
    public class NuggetManagerSummary
    {
        public int TotalNuggets { get; set; }
        public int TotalAssemblies { get; set; }
        public int TotalPlugins { get; set; }
        public int ActiveNuggets { get; set; }
        public List<string> LoadedNuggetIds { get; set; } = new();
        public List<string> LoadedPluginIds { get; set; } = new();

        public override string ToString()
        {
            return $"Nuggets: {TotalNuggets} ({ActiveNuggets} active), Assemblies: {TotalAssemblies}, Plugins: {TotalPlugins}";
        }
    }
}