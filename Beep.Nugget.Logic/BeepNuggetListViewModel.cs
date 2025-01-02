using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beep.Nugget.Logic
{
    public partial class BeepNuggetListViewModel : ObservableObject
    {
        [ObservableProperty]
        List<NuggetDefinition> nuggetDefinitions;
        private readonly NuGetManager _nugetmanager;

        public BeepNuggetListViewModel()
        {
            NuggetDefinitions = new List<NuggetDefinition>();
            _nugetmanager = new NuGetManager(); // Initialize NuGetManager
        }
        [RelayCommand]
        private async Task<bool> Install(NuggetDefinition nugget)
        {
            // Implement installation logic here
            // For example:
            if (nugget != null && !nugget.Installed)
            {
                // Perform installation
                await _nugetmanager.DownloadNuGetAsync(nugget.NuggetName, nugget.Version);
                nugget.Installed = true;
                // Notify UI of property change if necessary
            }
            return nugget.Installed;
        }
        [RelayCommand]
        private async Task<int> GetList()
        {
            // Implement installation logic here
            // For example:
            await  _nugetmanager.RetrieveCompanyNuggetsAsync(AppContext.BaseDirectory, "TheTechIdea");
            // Notify UI of property change if necessary
            NuggetDefinitions= _nugetmanager._definitions.ToList();
            return NuggetDefinitions.Count;
        }
    }

}
