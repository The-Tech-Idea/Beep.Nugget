using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.Utilities;

namespace Beep.Nugget.Logic
{
    /// <summary>
    /// Manages built-in database nuggets catalog with predefined database packages
    /// </summary>
    public class DatabaseNuggetsCatalog
    {
        private readonly List<DatabaseNuggetDefinition> _builtInDatabaseNuggets;

        public DatabaseNuggetsCatalog()
        {
            _builtInDatabaseNuggets = DatabaseNuggetDefinition.GetAllFromRegistry();
        }

        /// <summary>
        /// Gets all built-in database nuggets
        /// </summary>
        public IReadOnlyList<DatabaseNuggetDefinition> GetAllDatabaseNuggets()
        {
            return _builtInDatabaseNuggets.AsReadOnly();
        }

        /// <summary>
        /// Gets database nuggets by category
        /// </summary>
        public List<DatabaseNuggetDefinition> GetDatabaseNuggetsByCategory(DatasourceCategory category)
        {
            return _builtInDatabaseNuggets.Where(n => n.DatabaseCategory == category).ToList();
        }

        /// <summary>
        /// Gets a specific database nugget by database type
        /// </summary>
        public DatabaseNuggetDefinition? GetDatabaseNugget(DataSourceType databaseType)
        {
            return _builtInDatabaseNuggets.FirstOrDefault(n => n.DatabaseType == databaseType);
        }

        /// <summary>
        /// Gets database nuggets by name pattern
        /// </summary>
        public List<DatabaseNuggetDefinition> SearchDatabaseNuggets(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return _builtInDatabaseNuggets.ToList();

            var term = searchTerm.ToLowerInvariant();
            return _builtInDatabaseNuggets.Where(n => 
                n.Name.ToLowerInvariant().Contains(term) ||
                n.NuggetName.ToLowerInvariant().Contains(term) ||
                n.Description.ToLowerInvariant().Contains(term) ||
                n.DatabaseType.ToString().ToLowerInvariant().Contains(term)
            ).ToList();
        }

        /// <summary>
        /// Gets all available database categories
        /// </summary>
        public List<DatasourceCategory> GetAvailableCategories()
        {
            return _builtInDatabaseNuggets
                .Select(n => n.DatabaseCategory)
                .Distinct()
                .OrderBy(c => c.ToString())
                .ToList();
        }

        /// <summary>
        /// Gets popular database nuggets (commonly used databases)
        /// </summary>
        public List<DatabaseNuggetDefinition> GetPopularDatabaseNuggets()
        {
            var popularTypes = new[]
            {
                DataSourceType.SqlServer,
                DataSourceType.Mysql,
                DataSourceType.Postgre,
                DataSourceType.Oracle,
                DataSourceType.SqlLite,
                DataSourceType.MongoDB,
                DataSourceType.Redis,
                DataSourceType.ElasticSearch
            };

            return _builtInDatabaseNuggets.Where(n => popularTypes.Contains(n.DatabaseType)).ToList();
        }

        /// <summary>
        /// Gets cloud database nuggets
        /// </summary>
        public List<DatabaseNuggetDefinition> GetCloudDatabaseNuggets()
        {
            var cloudCategories = new[]
            {
                DatasourceCategory.CLOUD,
                DatasourceCategory.DataWarehouse
            };

            var cloudTypes = new[]
            {
                DataSourceType.AzureSQL,
                DataSourceType.AWSRDS,
                DataSourceType.SnowFlake,
                DataSourceType.DynamoDB,
                DataSourceType.Firebase,
                DataSourceType.AWSRedshift,
                DataSourceType.GoogleBigQuery
            };

            return _builtInDatabaseNuggets.Where(n => 
                cloudCategories.Contains(n.DatabaseCategory) || 
                cloudTypes.Contains(n.DatabaseType)
            ).ToList();
        }

        /// <summary>
        /// Gets NoSQL database nuggets
        /// </summary>
        public List<DatabaseNuggetDefinition> GetNoSQLDatabaseNuggets()
        {
            var noSqlCategories = new[]
            {
                DatasourceCategory.DocumentDB,
                DatasourceCategory.KeyValueDB,
                DatasourceCategory.GraphDB,
                DatasourceCategory.ColumnarDB,
                DatasourceCategory.VectorDB
            };

            return _builtInDatabaseNuggets.Where(n => noSqlCategories.Contains(n.DatabaseCategory)).ToList();
        }

        /// <summary>
        /// Gets relational database nuggets
        /// </summary>
        public List<DatabaseNuggetDefinition> GetRelationalDatabaseNuggets()
        {
            return _builtInDatabaseNuggets.Where(n => n.DatabaseCategory == DatasourceCategory.RDBMS).ToList();
        }

        /// <summary>
        /// Gets connection string template for a specific database type
        /// </summary>
        public string GetConnectionStringTemplate(DataSourceType databaseType)
        {
            var dbInfo = DatabaseNuggetRegistry.GetDatabaseInfo(databaseType);
            return dbInfo?.ConnectionStringTemplate ?? "Connection string template not available";
        }

        /// <summary>
        /// Gets default port for a specific database type
        /// </summary>
        public int GetDefaultPort(DataSourceType databaseType)
        {
            var dbInfo = DatabaseNuggetRegistry.GetDatabaseInfo(databaseType);
            return dbInfo?.DefaultPort ?? 0;
        }

        /// <summary>
        /// Gets required driver packages for a specific database type
        /// </summary>
        public List<string> GetRequiredDriverPackages(DataSourceType databaseType)
        {
            var dbInfo = DatabaseNuggetRegistry.GetDatabaseInfo(databaseType);
            return dbInfo?.RequiredDriverPackages ?? new List<string>();
        }

        /// <summary>
        /// Gets the official nugget package name for a specific database type
        /// </summary>
        public string GetOfficialNuggetPackage(DataSourceType databaseType)
        {
            var dbInfo = DatabaseNuggetRegistry.GetDatabaseInfo(databaseType);
            return dbInfo?.OfficialNuggetPackage ?? $"TheTechIdea.Beep.{databaseType}";
        }

        /// <summary>
        /// Gets the friendly name for a specific database type
        /// </summary>
        public string GetFriendlyName(DataSourceType databaseType)
        {
            var dbInfo = DatabaseNuggetRegistry.GetDatabaseInfo(databaseType);
            return dbInfo?.FriendlyName ?? databaseType.ToString();
        }

        /// <summary>
        /// Gets the description for a specific database type
        /// </summary>
        public string GetDescription(DataSourceType databaseType)
        {
            var dbInfo = DatabaseNuggetRegistry.GetDatabaseInfo(databaseType);
            return dbInfo?.Description ?? $"Database connector for {databaseType}";
        }

        /// <summary>
        /// Checks if authentication is required for a specific database type
        /// </summary>
        public bool RequiresAuthentication(DataSourceType databaseType)
        {
            var dbInfo = DatabaseNuggetRegistry.GetDatabaseInfo(databaseType);
            return dbInfo?.RequiresAuthentication ?? true;
        }

        /// <summary>
        /// Checks if transactions are supported for a specific database type
        /// </summary>
        public bool SupportsTransactions(DataSourceType databaseType)
        {
            var dbInfo = DatabaseNuggetRegistry.GetDatabaseInfo(databaseType);
            return dbInfo?.SupportsTransactions ?? true;
        }
    }
}