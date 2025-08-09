using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheTechIdea.Beep.Utilities;

namespace Beep.Nugget.Engine
{
    public partial class BeepNuggetListViewModel : ObservableObject
    {
        [ObservableProperty]
        List<NuggetDefinition> nuggetDefinitions;

        [ObservableProperty]
        List<DatabaseNuggetDefinition> databaseNuggets;

        [ObservableProperty]
        bool includeBuiltInDatabases = true;

        [ObservableProperty]
        DatasourceCategory selectedCategory = DatasourceCategory.NONE;

        public readonly NuggetManager NuggetManager;

        public BeepNuggetListViewModel()
        {
            NuggetDefinitions = new List<NuggetDefinition>();
            DatabaseNuggets = new List<DatabaseNuggetDefinition>();
            NuggetManager = new NuggetManager(); // Use the unified NuggetManager
        }
        
        [RelayCommand]
        private async Task<bool> Install(NuggetDefinition nugget)
        {
            if (nugget != null && !nugget.Installed)
            {
                try
                {
                    // Use the unified install and load method
                    var loadedNugget = await NuggetManager.InstallAndLoadNuggetAsync(nugget.NuggetName, nugget.Version);
                    nugget.Installed = true;
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error installing nugget {nugget.NuggetName}: {ex.Message}");
                    return false;
                }
            }
            return nugget?.Installed ?? false;
        }
        
        [RelayCommand]
        private async Task<int> GetList()
        {
            // Get online company nuggets
            await NuggetManager.RetrieveCompanyNuggetsAsync(AppContext.BaseDirectory, "TheTechIdea");
            
            // Get all nuggets including built-in databases if enabled
            var allNuggets = await NuggetManager.GetAllNuggetsAsync(IncludeBuiltInDatabases);
            NuggetDefinitions = allNuggets;
            
            // Separate database nuggets for specialized views
            DatabaseNuggets = NuggetManager.GetBuiltInDatabaseNuggets();
            
            return NuggetDefinitions.Count;
        }

        [RelayCommand]
        private async Task<int> GetDatabaseNuggets()
        {
            // Get all database nuggets
            DatabaseNuggets = NuggetManager.GetBuiltInDatabaseNuggets();
            return DatabaseNuggets.Count;
        }

        [RelayCommand]
        private async Task<int> GetDatabaseNuggetsByCategory(DatasourceCategory category)
        {
            // Get database nuggets by category
            DatabaseNuggets = NuggetManager.GetDatabaseNuggetsByCategory(category);
            SelectedCategory = category;
            return DatabaseNuggets.Count;
        }

        [RelayCommand]
        private async Task<int> SearchDatabaseNuggets(string searchTerm)
        {
            // Search database nuggets
            DatabaseNuggets = NuggetManager.SearchDatabaseNuggets(searchTerm);
            return DatabaseNuggets.Count;
        }

        [RelayCommand]
        private async Task<int> GetPopularDatabases()
        {
            // Get popular database nuggets
            DatabaseNuggets = NuggetManager.GetPopularDatabaseNuggets();
            return DatabaseNuggets.Count;
        }

        [RelayCommand]
        private async Task<int> GetCloudDatabases()
        {
            // Get cloud database nuggets
            DatabaseNuggets = NuggetManager.GetCloudDatabaseNuggets();
            return DatabaseNuggets.Count;
        }

        [RelayCommand]
        private async Task<int> GetNoSQLDatabases()
        {
            // Get NoSQL database nuggets
            DatabaseNuggets = NuggetManager.GetNoSQLDatabaseNuggets();
            return DatabaseNuggets.Count;
        }

        [RelayCommand]
        private async Task<int> GetRelationalDatabases()
        {
            // Get relational database nuggets
            DatabaseNuggets = NuggetManager.GetRelationalDatabaseNuggets();
            return DatabaseNuggets.Count;
        }

        [RelayCommand]
        private async Task<bool> Uninstall(NuggetDefinition nugget)
        {
            if (nugget != null && nugget.Installed)
            {
                bool success = NuggetManager.UnloadNugget(nugget.NuggetName);
                if (success)
                {
                    nugget.Installed = false;
                }
                return success;
            }
            return false;
        }
    }
}
