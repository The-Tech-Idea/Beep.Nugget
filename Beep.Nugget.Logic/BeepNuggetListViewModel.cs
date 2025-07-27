using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheTechIdea.Beep.Utilities;

namespace Beep.Nugget.Logic
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

        public readonly NuGetManager NugetManager;

        public BeepNuggetListViewModel()
        {
            NuggetDefinitions = new List<NuggetDefinition>();
            DatabaseNuggets = new List<DatabaseNuggetDefinition>();
            NugetManager = new NuGetManager(); // Initialize NuGetManager
        }
        [RelayCommand]
        private async Task<bool> Install(NuggetDefinition nugget)
        {
            // Implement installation logic here
            // For example:
            if (nugget != null && !nugget.Installed)
            {
                // Perform installation
                await NugetManager.DownloadNuGetAsync(nugget.NuggetName, nugget.Version);
                nugget.Installed = true;
                // Notify UI of property change if necessary
            }
            return nugget.Installed;
        }
        [RelayCommand]
        private async Task<int> GetList()
        {
            // Get online company nuggets
            await NugetManager.RetrieveCompanyNuggetsAsync(AppContext.BaseDirectory, "TheTechIdea");
            
            // Get all nuggets including built-in databases if enabled
            var allNuggets = await NugetManager.GetAllNuggetsAsync(IncludeBuiltInDatabases);
            NuggetDefinitions = allNuggets;
            
            // Separate database nuggets for specialized views
            DatabaseNuggets = NugetManager.GetBuiltInDatabaseNuggets();
            
            return NuggetDefinitions.Count;
        }

        [RelayCommand]
        private async Task<int> GetDatabaseNuggets()
        {
            // Get all database nuggets
            DatabaseNuggets = NugetManager.GetBuiltInDatabaseNuggets();
            return DatabaseNuggets.Count;
        }

        [RelayCommand]
        private async Task<int> GetDatabaseNuggetsByCategory(DatasourceCategory category)
        {
            // Get database nuggets by category
            DatabaseNuggets = NugetManager.GetDatabaseNuggetsByCategory(category);
            SelectedCategory = category;
            return DatabaseNuggets.Count;
        }

        [RelayCommand]
        private async Task<int> SearchDatabaseNuggets(string searchTerm)
        {
            // Search database nuggets
            DatabaseNuggets = NugetManager.SearchDatabaseNuggets(searchTerm);
            return DatabaseNuggets.Count;
        }

        [RelayCommand]
        private async Task<int> GetPopularDatabases()
        {
            // Get popular database nuggets
            DatabaseNuggets = NugetManager.GetPopularDatabaseNuggets();
            return DatabaseNuggets.Count;
        }

        [RelayCommand]
        private async Task<int> GetCloudDatabases()
        {
            // Get cloud database nuggets
            DatabaseNuggets = NugetManager.GetCloudDatabaseNuggets();
            return DatabaseNuggets.Count;
        }

        [RelayCommand]
        private async Task<int> GetNoSQLDatabases()
        {
            // Get NoSQL database nuggets
            DatabaseNuggets = NugetManager.GetNoSQLDatabaseNuggets();
            return DatabaseNuggets.Count;
        }

        [RelayCommand]
        private async Task<int> GetRelationalDatabases()
        {
            // Get relational database nuggets
            DatabaseNuggets = NugetManager.GetRelationalDatabaseNuggets();
            return DatabaseNuggets.Count;
        }

        [RelayCommand]
        private async Task<bool> Uninstall(NuggetDefinition nugget)
        {
            if (nugget != null && nugget.Installed)
            {
                bool success = NugetManager.RemovePackageFromRuntime(nugget.NuggetName, nugget.Version);
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
