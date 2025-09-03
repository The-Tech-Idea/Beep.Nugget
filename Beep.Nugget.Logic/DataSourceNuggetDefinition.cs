using Beep.Nugget.Logic;
using TheTechIdea.Beep.ConfigUtil;

namespace Beep.Nugget.Logic
{
    /// <summary>
    /// Represents a nugget definition specifically for data sources, extending the base NuggetDefinition.
    /// </summary>
    public class DataSourceNuggetDefinition : NuggetDefinition
    {
        /// <summary>
        /// Gets or sets the type of the data source.
        /// </summary>
        public DataSourceType Type { get; set; }

        /// <summary>
        /// Gets or sets the required package for the data source driver.
        /// </summary>
        public string RequiredPackage { get; set; }

        /// <summary>
        /// Gets or sets the version of the data source driver.
        /// </summary>
        public string DriverVersion { get; set; }
    }
}
