using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.Utilities;

namespace Beep.Nugget.Engine
{
    /// <summary>
    /// Represents a built-in database nugget definition with predefined database packages
    /// </summary>
    public class DatabaseNuggetDefinition : NuggetDefinition
    {
        /// <summary>
        /// The database type this nugget represents
        /// </summary>
        public DataSourceType DatabaseType { get; set; }

        /// <summary>
        /// The category this database belongs to
        /// </summary>
        public DatasourceCategory DatabaseCategory { get; set; }

        /// <summary>
        /// Connection string template for this database type
        /// </summary>
        public string ConnectionStringTemplate { get; set; } = string.Empty;

        /// <summary>
        /// Default port for this database type
        /// </summary>
        public int DefaultPort { get; set; }

        /// <summary>
        /// Whether this database requires authentication
        /// </summary>
        public bool RequiresAuthentication { get; set; } = true;

        /// <summary>
        /// Whether this database supports transactions
        /// </summary>
        public bool SupportsTransactions { get; set; } = true;

        /// <summary>
        /// List of required connection driver packages
        /// </summary>
        public List<string> RequiredDriverPackages { get; set; } = new List<string>();

        public DatabaseNuggetDefinition()
        {
            Category = "Database";
            IsActive = true;
            CreatedDate = DateTime.UtcNow;
        }

        public DatabaseNuggetDefinition(DataSourceType dbType, string version = "latest") : this()
        {
            var dbInfo = DatabaseNuggetRegistry.GetDatabaseInfo(dbType);
            if (dbInfo == null)
            {
                throw new ArgumentException($"Database type {dbType} is not registered in the DatabaseNuggetRegistry", nameof(dbType));
            }

            DatabaseType = dbType;
            NuggetName = dbInfo.OfficialNuggetPackage;
            Name = dbInfo.FriendlyName;
            Version = version;
            Author = "TheTechIdea";
            Description = dbInfo.Description;
            DatabaseCategory = dbInfo.Category;
            ConnectionStringTemplate = dbInfo.ConnectionStringTemplate;
            DefaultPort = dbInfo.DefaultPort;
            RequiresAuthentication = dbInfo.RequiresAuthentication;
            SupportsTransactions = dbInfo.SupportsTransactions;
            RequiredDriverPackages = new List<string>(dbInfo.RequiredDriverPackages);
        }

        /// <summary>
        /// Creates a DatabaseNuggetDefinition from registry information
        /// </summary>
        public static DatabaseNuggetDefinition CreateFromRegistry(DataSourceType dbType, string version = "latest")
        {
            return new DatabaseNuggetDefinition(dbType, version);
        }

        /// <summary>
        /// Gets all available database nugget definitions from the registry
        /// </summary>
        public static List<DatabaseNuggetDefinition> GetAllFromRegistry(string version = "latest")
        {
            var nuggets = new List<DatabaseNuggetDefinition>();
            
            foreach (var dbType in DatabaseNuggetRegistry.GetAllDatabaseTypes())
            {
                try
                {
                    nuggets.Add(new DatabaseNuggetDefinition(dbType, version));
                }
                catch (ArgumentException)
                {
                    // Skip any database types that aren't properly registered
                    continue;
                }
            }
            
            return nuggets;
        }

        /// <summary>
        /// Gets database nugget definitions by category from the registry
        /// </summary>
        public static List<DatabaseNuggetDefinition> GetByCategoryFromRegistry(DatasourceCategory category, string version = "latest")
        {
            var nuggets = new List<DatabaseNuggetDefinition>();
            
            foreach (var dbType in DatabaseNuggetRegistry.GetDatabaseTypesByCategory(category))
            {
                try
                {
                    nuggets.Add(new DatabaseNuggetDefinition(dbType, version));
                }
                catch (ArgumentException)
                {
                    // Skip any database types that aren't properly registered
                    continue;
                }
            }
            
            return nuggets;
        }
    }
}