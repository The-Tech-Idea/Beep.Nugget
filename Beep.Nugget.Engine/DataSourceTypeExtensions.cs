using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.Utilities;

namespace Beep.Nugget.Engine
{
    /// <summary>
    /// Extension methods for working with DataSourceType and the DatabaseNuggetRegistry
    /// </summary>
    public static class DataSourceTypeExtensions
    {
        /// <summary>
        /// Gets the official TheTechIdea nugget package name for this database type
        /// </summary>
        public static string GetOfficialNuggetPackage(this DataSourceType dataSourceType)
        {
            var info = DatabaseNuggetRegistry.GetDatabaseInfo(dataSourceType);
            return info?.OfficialNuggetPackage ?? $"TheTechIdea.Beep.{dataSourceType}";
        }

        /// <summary>
        /// Gets the friendly display name for this database type
        /// </summary>
        public static string GetFriendlyName(this DataSourceType dataSourceType)
        {
            var info = DatabaseNuggetRegistry.GetDatabaseInfo(dataSourceType);
            return info?.FriendlyName ?? dataSourceType.ToString();
        }

        /// <summary>
        /// Gets the description for this database type
        /// </summary>
        public static string GetDescription(this DataSourceType dataSourceType)
        {
            var info = DatabaseNuggetRegistry.GetDatabaseInfo(dataSourceType);
            return info?.Description ?? $"Database connector for {dataSourceType}";
        }

        /// <summary>
        /// Gets the database category for this database type
        /// </summary>
        public static DatasourceCategory GetDatabaseCategory(this DataSourceType dataSourceType)
        {
            var info = DatabaseNuggetRegistry.GetDatabaseInfo(dataSourceType);
            return info?.Category ?? DatasourceCategory.NONE;
        }

        /// <summary>
        /// Gets the connection string template for this database type
        /// </summary>
        public static string GetConnectionStringTemplate(this DataSourceType dataSourceType)
        {
            var info = DatabaseNuggetRegistry.GetDatabaseInfo(dataSourceType);
            return info?.ConnectionStringTemplate ?? "Connection string template not available";
        }

        /// <summary>
        /// Gets the default port for this database type
        /// </summary>
        public static int GetDefaultPort(this DataSourceType dataSourceType)
        {
            var info = DatabaseNuggetRegistry.GetDatabaseInfo(dataSourceType);
            return info?.DefaultPort ?? 0;
        }

        /// <summary>
        /// Gets the required driver packages for this database type
        /// </summary>
        public static List<string> GetRequiredDriverPackages(this DataSourceType dataSourceType)
        {
            var info = DatabaseNuggetRegistry.GetDatabaseInfo(dataSourceType);
            return info?.RequiredDriverPackages ?? new List<string>();
        }

        /// <summary>
        /// Checks if this database type requires authentication
        /// </summary>
        public static bool RequiresAuthentication(this DataSourceType dataSourceType)
        {
            var info = DatabaseNuggetRegistry.GetDatabaseInfo(dataSourceType);
            return info?.RequiresAuthentication ?? true;
        }

        /// <summary>
        /// Checks if this database type supports transactions
        /// </summary>
        public static bool SupportsTransactions(this DataSourceType dataSourceType)
        {
            var info = DatabaseNuggetRegistry.GetDatabaseInfo(dataSourceType);
            return info?.SupportsTransactions ?? true;
        }

        /// <summary>
        /// Checks if this database type is registered in the nugget registry
        /// </summary>
        public static bool HasNuggetSupport(this DataSourceType dataSourceType)
        {
            return DatabaseNuggetRegistry.IsRegisteredDatabase(dataSourceType);
        }

        /// <summary>
        /// Creates a DatabaseNuggetDefinition for this database type
        /// </summary>
        public static DatabaseNuggetDefinition ToNuggetDefinition(this DataSourceType dataSourceType, string version = "latest")
        {
            return new DatabaseNuggetDefinition(dataSourceType, version);
        }

        /// <summary>
        /// Gets all database types that belong to the same category as this one
        /// </summary>
        public static IEnumerable<DataSourceType> GetSameCategoryDatabases(this DataSourceType dataSourceType)
        {
            var category = dataSourceType.GetDatabaseCategory();
            return DatabaseNuggetRegistry.GetDatabaseTypesByCategory(category);
        }

        /// <summary>
        /// Checks if this is a cloud database
        /// </summary>
        public static bool IsCloudDatabase(this DataSourceType dataSourceType)
        {
            var category = dataSourceType.GetDatabaseCategory();
            return category == DatasourceCategory.CLOUD || 
                   category == DatasourceCategory.DataWarehouse ||
                   new[] { 
                       DataSourceType.AzureSQL, DataSourceType.AWSRDS, DataSourceType.SnowFlake,
                       DataSourceType.DynamoDB, DataSourceType.Firebase, DataSourceType.AWSRedshift,
                       DataSourceType.GoogleBigQuery 
                   }.Contains(dataSourceType);
        }

        /// <summary>
        /// Checks if this is a NoSQL database
        /// </summary>
        public static bool IsNoSQLDatabase(this DataSourceType dataSourceType)
        {
            var category = dataSourceType.GetDatabaseCategory();
            return new[] { 
                DatasourceCategory.DocumentDB, DatasourceCategory.KeyValueDB,
                DatasourceCategory.GraphDB, DatasourceCategory.ColumnarDB,
                DatasourceCategory.VectorDB 
            }.Contains(category);
        }

        /// <summary>
        /// Checks if this is a relational database
        /// </summary>
        public static bool IsRelationalDatabase(this DataSourceType dataSourceType)
        {
            return dataSourceType.GetDatabaseCategory() == DatasourceCategory.RDBMS;
        }

        /// <summary>
        /// Checks if this is a vector database
        /// </summary>
        public static bool IsVectorDatabase(this DataSourceType dataSourceType)
        {
            return dataSourceType.GetDatabaseCategory() == DatasourceCategory.VectorDB;
        }
    }

    /// <summary>
    /// Extension methods for working with DatasourceCategory
    /// </summary>
    public static class DatasourceCategoryExtensions
    {
        /// <summary>
        /// Gets all database types that belong to this category
        /// </summary>
        public static IEnumerable<DataSourceType> GetDatabaseTypes(this DatasourceCategory category)
        {
            return DatabaseNuggetRegistry.GetDatabaseTypesByCategory(category);
        }

        /// <summary>
        /// Gets all database nugget definitions for this category
        /// </summary>
        public static List<DatabaseNuggetDefinition> GetNuggetDefinitions(this DatasourceCategory category, string version = "latest")
        {
            return DatabaseNuggetDefinition.GetByCategoryFromRegistry(category, version);
        }

        /// <summary>
        /// Gets a friendly display name for this category
        /// </summary>
        public static string GetFriendlyName(this DatasourceCategory category)
        {
            return category switch
            {
                DatasourceCategory.RDBMS => "Relational Databases",
                DatasourceCategory.DocumentDB => "Document Databases",
                DatasourceCategory.KeyValueDB => "Key-Value Stores",
                DatasourceCategory.GraphDB => "Graph Databases",
                DatasourceCategory.ColumnarDB => "Columnar Databases",
                DatasourceCategory.VectorDB => "Vector Databases",
                DatasourceCategory.TimeSeriesDB => "Time Series Databases",
                DatasourceCategory.SearchEngine => "Search Engines",
                DatasourceCategory.DataWarehouse => "Data Warehouses",
                DatasourceCategory.CLOUD => "Cloud Databases",
                DatasourceCategory.NOSQL => "NoSQL Databases",
                _ => category.ToString()
            };
        }
    }
}