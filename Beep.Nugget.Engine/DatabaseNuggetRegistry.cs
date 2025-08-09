using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.Utilities;

namespace Beep.Nugget.Engine
{
    /// <summary>
    /// Contains information about a database nugget including official package name and connection details
    /// </summary>
    public class DatabaseNuggetInfo
    {
        public string FriendlyName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string OfficialNuggetPackage { get; set; } = string.Empty;
        public DatasourceCategory Category { get; set; }
        public string ConnectionStringTemplate { get; set; } = string.Empty;
        public int DefaultPort { get; set; }
        public bool RequiresAuthentication { get; set; } = true;
        public bool SupportsTransactions { get; set; } = true;
        public List<string> RequiredDriverPackages { get; set; } = new List<string>();
    }

    /// <summary>
    /// Static registry containing all database nugget definitions mapped to DataSourceType
    /// </summary>
    public static class DatabaseNuggetRegistry
    {
        private static readonly Dictionary<DataSourceType, DatabaseNuggetInfo> _databaseNuggets = new()
        {
            // ============= RELATIONAL DATABASES (RDBMS) =============
            [DataSourceType.SqlServer] = new DatabaseNuggetInfo
            {
                FriendlyName = "SQL Server",
                Description = "Microsoft SQL Server relational database management system",
                OfficialNuggetPackage = "TheTechIdea.Beep.SqlServer",
                Category = DatasourceCategory.RDBMS,
                ConnectionStringTemplate = "Server={Server},{Port};Database={Database};User Id={Username};Password={Password};",
                DefaultPort = 1433,
                RequiredDriverPackages = new List<string> { "Microsoft.Data.SqlClient", "TheTechIdea.Beep.SqlServer" }
            },

            [DataSourceType.Oracle] = new DatabaseNuggetInfo
            {
                FriendlyName = "Oracle Database",
                Description = "Enterprise-grade relational database management system by Oracle Corporation",
                OfficialNuggetPackage = "TheTechIdea.Beep.Oracle",
                Category = DatasourceCategory.RDBMS,
                ConnectionStringTemplate = "Data Source={Server}:{Port}/{ServiceName};User Id={Username};Password={Password};",
                DefaultPort = 1521,
                RequiredDriverPackages = new List<string> { "Oracle.ManagedDataAccess.Core", "TheTechIdea.Beep.Oracle" }
            },

            [DataSourceType.Mysql] = new DatabaseNuggetInfo
            {
                FriendlyName = "MySQL",
                Description = "Open-source relational database management system",
                OfficialNuggetPackage = "TheTechIdea.Beep.MySQL",
                Category = DatasourceCategory.RDBMS,
                ConnectionStringTemplate = "Server={Server};Port={Port};Database={Database};Uid={Username};Pwd={Password};",
                DefaultPort = 3306,
                RequiredDriverPackages = new List<string> { "MySql.Data", "TheTechIdea.Beep.MySQL" }
            },

            [DataSourceType.Postgre] = new DatabaseNuggetInfo
            {
                FriendlyName = "PostgreSQL",
                Description = "Advanced open-source relational database with extensive SQL compliance",
                OfficialNuggetPackage = "TheTechIdea.Beep.PostgreSQL",
                Category = DatasourceCategory.RDBMS,
                ConnectionStringTemplate = "Host={Server};Port={Port};Database={Database};Username={Username};Password={Password};",
                DefaultPort = 5432,
                RequiredDriverPackages = new List<string> { "Npgsql", "TheTechIdea.Beep.PostgreSQL" }
            },

            [DataSourceType.SqlLite] = new DatabaseNuggetInfo
            {
                FriendlyName = "SQLite",
                Description = "Lightweight, embedded SQL database engine",
                OfficialNuggetPackage = "TheTechIdea.Beep.SQLite",
                Category = DatasourceCategory.RDBMS,
                ConnectionStringTemplate = "Data Source={DatabasePath};",
                DefaultPort = 0,
                RequiresAuthentication = false,
                RequiredDriverPackages = new List<string> { "Microsoft.Data.Sqlite", "TheTechIdea.Beep.SQLite" }
            },

            [DataSourceType.FireBird] = new DatabaseNuggetInfo
            {
                FriendlyName = "Firebird",
                Description = "Open-source SQL relational database management system",
                OfficialNuggetPackage = "TheTechIdea.Beep.Firebird",
                Category = DatasourceCategory.RDBMS,
                ConnectionStringTemplate = "Database={Server}:{DatabasePath};User={Username};Password={Password};",
                DefaultPort = 3050,
                RequiredDriverPackages = new List<string> { "FirebirdSql.Data.FirebirdClient", "TheTechIdea.Beep.Firebird" }
            },

            [DataSourceType.DB2] = new DatabaseNuggetInfo
            {
                FriendlyName = "IBM DB2",
                Description = "IBM's relational database management system",
                OfficialNuggetPackage = "TheTechIdea.Beep.DB2",
                Category = DatasourceCategory.RDBMS,
                ConnectionStringTemplate = "Server={Server}:{Port};Database={Database};UID={Username};PWD={Password};",
                DefaultPort = 50000,
                RequiredDriverPackages = new List<string> { "IBM.Data.Db2.Core", "TheTechIdea.Beep.DB2" }
            },

            [DataSourceType.Cockroach] = new DatabaseNuggetInfo
            {
                FriendlyName = "CockroachDB",
                Description = "Distributed SQL database built on a transactional and strongly-consistent key-value store",
                OfficialNuggetPackage = "TheTechIdea.Beep.CockroachDB",
                Category = DatasourceCategory.RDBMS,
                ConnectionStringTemplate = "Host={Server};Port={Port};Database={Database};Username={Username};Password={Password};Ssl Mode=Require;",
                DefaultPort = 26257,
                RequiredDriverPackages = new List<string> { "Npgsql", "TheTechIdea.Beep.CockroachDB" }
            },

            // ============= CLOUD DATABASES =============
            [DataSourceType.AzureSQL] = new DatabaseNuggetInfo
            {
                FriendlyName = "Azure SQL Database",
                Description = "Microsoft Azure cloud-based SQL database service",
                OfficialNuggetPackage = "TheTechIdea.Beep.AzureSQL",
                Category = DatasourceCategory.CLOUD,
                ConnectionStringTemplate = "Server={Server}.database.windows.net;Database={Database};User Id={Username};Password={Password};Encrypt=True;",
                DefaultPort = 1433,
                RequiredDriverPackages = new List<string> { "Microsoft.Data.SqlClient", "TheTechIdea.Beep.AzureSQL" }
            },

            [DataSourceType.AWSRDS] = new DatabaseNuggetInfo
            {
                FriendlyName = "AWS RDS",
                Description = "Amazon Web Services relational database service",
                OfficialNuggetPackage = "TheTechIdea.Beep.AWSRDS",
                Category = DatasourceCategory.CLOUD,
                ConnectionStringTemplate = "Server={Server};Database={Database};User Id={Username};Password={Password};",
                DefaultPort = 3306, // Default for MySQL, varies by engine
                RequiredDriverPackages = new List<string> { "MySql.Data", "TheTechIdea.Beep.AWSRDS" }
            },

            [DataSourceType.SnowFlake] = new DatabaseNuggetInfo
            {
                FriendlyName = "Snowflake",
                Description = "Cloud-based data platform and data warehouse",
                OfficialNuggetPackage = "TheTechIdea.Beep.Snowflake",
                Category = DatasourceCategory.DataWarehouse,
                ConnectionStringTemplate = "account={Account};user={Username};password={Password};db={Database};schema={Schema};warehouse={Warehouse};",
                DefaultPort = 443,
                RequiredDriverPackages = new List<string> { "Snowflake.Data", "TheTechIdea.Beep.Snowflake" }
            },

            [DataSourceType.AWSRedshift] = new DatabaseNuggetInfo
            {
                FriendlyName = "AWS Redshift",
                Description = "Amazon Redshift data warehouse service",
                OfficialNuggetPackage = "TheTechIdea.Beep.Redshift",
                Category = DatasourceCategory.DataWarehouse,
                ConnectionStringTemplate = "Server={Server};Port={Port};Database={Database};User Id={Username};Password={Password};",
                DefaultPort = 5439,
                RequiredDriverPackages = new List<string> { "Npgsql", "TheTechIdea.Beep.Redshift" }
            },

            [DataSourceType.GoogleBigQuery] = new DatabaseNuggetInfo
            {
                FriendlyName = "Google BigQuery",
                Description = "Google Cloud serverless data warehouse",
                OfficialNuggetPackage = "TheTechIdea.Beep.BigQuery",
                Category = DatasourceCategory.DataWarehouse,
                ConnectionStringTemplate = "ProjectId={ProjectId};JsonCredentials={JsonCredentials};",
                DefaultPort = 443,
                RequiredDriverPackages = new List<string> { "Google.Cloud.BigQuery.V2", "TheTechIdea.Beep.BigQuery" }
            },

            // ============= DOCUMENT DATABASES =============
            [DataSourceType.MongoDB] = new DatabaseNuggetInfo
            {
                FriendlyName = "MongoDB",
                Description = "Document-oriented NoSQL database program",
                OfficialNuggetPackage = "TheTechIdea.Beep.MongoDB",
                Category = DatasourceCategory.DocumentDB,
                ConnectionStringTemplate = "mongodb://{Username}:{Password}@{Server}:{Port}/{Database}",
                DefaultPort = 27017,
                SupportsTransactions = false,
                RequiredDriverPackages = new List<string> { "MongoDB.Driver", "TheTechIdea.Beep.MongoDB" }
            },

            [DataSourceType.CouchDB] = new DatabaseNuggetInfo
            {
                FriendlyName = "Apache CouchDB",
                Description = "Open-source document-oriented NoSQL database",
                OfficialNuggetPackage = "TheTechIdea.Beep.CouchDB",
                Category = DatasourceCategory.DocumentDB,
                ConnectionStringTemplate = "http://{Username}:{Password}@{Server}:{Port}/{Database}",
                DefaultPort = 5984,
                SupportsTransactions = false,
                RequiredDriverPackages = new List<string> { "MyCouch", "TheTechIdea.Beep.CouchDB" }
            },

            [DataSourceType.RavenDB] = new DatabaseNuggetInfo
            {
                FriendlyName = "RavenDB",
                Description = ".NET document database with ACID guarantees",
                OfficialNuggetPackage = "TheTechIdea.Beep.RavenDB",
                Category = DatasourceCategory.DocumentDB,
                ConnectionStringTemplate = "Url=http://{Server}:{Port};Database={Database};",
                DefaultPort = 8080,
                RequiredDriverPackages = new List<string> { "RavenDB.Client", "TheTechIdea.Beep.RavenDB" }
            },

            [DataSourceType.DynamoDB] = new DatabaseNuggetInfo
            {
                FriendlyName = "Amazon DynamoDB",
                Description = "Amazon's NoSQL database service",
                OfficialNuggetPackage = "TheTechIdea.Beep.DynamoDB",
                Category = DatasourceCategory.DocumentDB,
                ConnectionStringTemplate = "AccessKey={AccessKey};SecretKey={SecretKey};Region={Region};",
                DefaultPort = 443,
                SupportsTransactions = false,
                RequiredDriverPackages = new List<string> { "AWSSDK.DynamoDBv2", "TheTechIdea.Beep.DynamoDB" }
            },

            [DataSourceType.Firebase] = new DatabaseNuggetInfo
            {
                FriendlyName = "Google Firebase",
                Description = "Google's mobile and web application development platform",
                OfficialNuggetPackage = "TheTechIdea.Beep.Firebase",
                Category = DatasourceCategory.DocumentDB,
                ConnectionStringTemplate = "ProjectId={ProjectId};JsonCredentials={JsonCredentials};",
                DefaultPort = 443,
                SupportsTransactions = false,
                RequiredDriverPackages = new List<string> { "Google.Cloud.Firestore", "TheTechIdea.Beep.Firebase" }
            },

            // ============= KEY-VALUE DATABASES =============
            [DataSourceType.Redis] = new DatabaseNuggetInfo
            {
                FriendlyName = "Redis",
                Description = "In-memory data structure store, used as database, cache, and message broker",
                OfficialNuggetPackage = "TheTechIdea.Beep.Redis",
                Category = DatasourceCategory.KeyValueDB,
                ConnectionStringTemplate = "{Server}:{Port}",
                DefaultPort = 6379,
                SupportsTransactions = false,
                RequiredDriverPackages = new List<string> { "StackExchange.Redis", "TheTechIdea.Beep.Redis" }
            },

            // ============= GRAPH DATABASES =============
            [DataSourceType.Neo4j] = new DatabaseNuggetInfo
            {
                FriendlyName = "Neo4j Graph Database",
                Description = "Graph database management system for connected data",
                OfficialNuggetPackage = "TheTechIdea.Beep.Neo4j",
                Category = DatasourceCategory.GraphDB,
                ConnectionStringTemplate = "bolt://{Server}:{Port}",
                DefaultPort = 7687,
                RequiredDriverPackages = new List<string> { "Neo4j.Driver", "TheTechIdea.Beep.Neo4j" }
            },

            // ============= COLUMNAR DATABASES =============
            [DataSourceType.Cassandra] = new DatabaseNuggetInfo
            {
                FriendlyName = "Apache Cassandra",
                Description = "Wide-column store NoSQL distributed database management system",
                OfficialNuggetPackage = "TheTechIdea.Beep.Cassandra",
                Category = DatasourceCategory.ColumnarDB,
                ConnectionStringTemplate = "{Server}:{Port}",
                DefaultPort = 9042,
                RequiredDriverPackages = new List<string> { "CassandraCSharpDriver", "TheTechIdea.Beep.Cassandra" }
            },

            [DataSourceType.ClickHouse] = new DatabaseNuggetInfo
            {
                FriendlyName = "ClickHouse",
                Description = "Column-oriented database management system for online analytical processing",
                OfficialNuggetPackage = "TheTechIdea.Beep.ClickHouse",
                Category = DatasourceCategory.ColumnarDB,
                ConnectionStringTemplate = "Host={Server};Port={Port};Database={Database};Username={Username};Password={Password};",
                DefaultPort = 9000,
                RequiredDriverPackages = new List<string> { "ClickHouse.Client", "TheTechIdea.Beep.ClickHouse" }
            },

            // ============= SEARCH ENGINES =============
            [DataSourceType.ElasticSearch] = new DatabaseNuggetInfo
            {
                FriendlyName = "Elasticsearch",
                Description = "Distributed, RESTful search and analytics engine",
                OfficialNuggetPackage = "TheTechIdea.Beep.Elasticsearch",
                Category = DatasourceCategory.SearchEngine,
                ConnectionStringTemplate = "http://{Server}:{Port}",
                DefaultPort = 9200,
                SupportsTransactions = false,
                RequiredDriverPackages = new List<string> { "Elasticsearch.Net", "TheTechIdea.Beep.Elasticsearch" }
            },

            // ============= TIME SERIES DATABASES =============
            [DataSourceType.InfluxDB] = new DatabaseNuggetInfo
            {
                FriendlyName = "InfluxDB",
                Description = "Time series database designed for high-performance",
                OfficialNuggetPackage = "TheTechIdea.Beep.InfluxDB",
                Category = DatasourceCategory.TimeSeriesDB,
                ConnectionStringTemplate = "http://{Server}:{Port}",
                DefaultPort = 8086,
                SupportsTransactions = false,
                RequiredDriverPackages = new List<string> { "InfluxDB.Client", "TheTechIdea.Beep.InfluxDB" }
            },

            // ============= DATA WAREHOUSES =============
            [DataSourceType.TerraData] = new DatabaseNuggetInfo
            {
                FriendlyName = "Teradata",
                Description = "Enterprise data warehouse and analytics platform",
                OfficialNuggetPackage = "TheTechIdea.Beep.Teradata",
                Category = DatasourceCategory.DataWarehouse,
                ConnectionStringTemplate = "Data Source={Server};User Id={Username};Password={Password};",
                DefaultPort = 1025,
                RequiredDriverPackages = new List<string> { "Teradata.Client.Provider", "TheTechIdea.Beep.Teradata" }
            },

            [DataSourceType.Vertica] = new DatabaseNuggetInfo
            {
                FriendlyName = "Vertica",
                Description = "Column-oriented analytics platform",
                OfficialNuggetPackage = "TheTechIdea.Beep.Vertica",
                Category = DatasourceCategory.DataWarehouse,
                ConnectionStringTemplate = "Server={Server};Port={Port};Database={Database};User={Username};Password={Password};",
                DefaultPort = 5433,
                RequiredDriverPackages = new List<string> { "Vertica.Data", "TheTechIdea.Beep.Vertica" }
            },

            // ============= VECTOR DATABASES =============
            [DataSourceType.ChromaDB] = new DatabaseNuggetInfo
            {
                FriendlyName = "ChromaDB",
                Description = "Open-source embedding database for AI applications",
                OfficialNuggetPackage = "TheTechIdea.Beep.ChromaDB",
                Category = DatasourceCategory.VectorDB,
                ConnectionStringTemplate = "http://{Server}:{Port}",
                DefaultPort = 8000,
                SupportsTransactions = false,
                RequiredDriverPackages = new List<string> { "ChromaDB.Client", "TheTechIdea.Beep.ChromaDB" }
            },

            [DataSourceType.PineCone] = new DatabaseNuggetInfo
            {
                FriendlyName = "Pinecone",
                Description = "Managed vector database for machine learning applications",
                OfficialNuggetPackage = "TheTechIdea.Beep.Pinecone",
                Category = DatasourceCategory.VectorDB,
                ConnectionStringTemplate = "ApiKey={ApiKey};Environment={Environment};",
                DefaultPort = 443,
                SupportsTransactions = false,
                RequiredDriverPackages = new List<string> { "Pinecone.NET", "TheTechIdea.Beep.Pinecone" }
            },

            [DataSourceType.Qdrant] = new DatabaseNuggetInfo
            {
                FriendlyName = "Qdrant",
                Description = "Vector database for next generation AI applications",
                OfficialNuggetPackage = "TheTechIdea.Beep.Qdrant",
                Category = DatasourceCategory.VectorDB,
                ConnectionStringTemplate = "http://{Server}:{Port}",
                DefaultPort = 6333,
                SupportsTransactions = false,
                RequiredDriverPackages = new List<string> { "Qdrant.Client", "TheTechIdea.Beep.Qdrant" }
            },

            [DataSourceType.Weaviate] = new DatabaseNuggetInfo
            {
                FriendlyName = "Weaviate",
                Description = "Open-source vector database with semantic search capabilities",
                OfficialNuggetPackage = "TheTechIdea.Beep.Weaviate",
                Category = DatasourceCategory.VectorDB,
                ConnectionStringTemplate = "http://{Server}:{Port}",
                DefaultPort = 8080,
                SupportsTransactions = false,
                RequiredDriverPackages = new List<string> { "Weaviate.NET", "TheTechIdea.Beep.Weaviate" }
            },

            [DataSourceType.Milvus] = new DatabaseNuggetInfo
            {
                FriendlyName = "Milvus",
                Description = "Open-source vector database built for scalable similarity search",
                OfficialNuggetPackage = "TheTechIdea.Beep.Milvus",
                Category = DatasourceCategory.VectorDB,
                ConnectionStringTemplate = "{Server}:{Port}",
                DefaultPort = 19530,
                SupportsTransactions = false,
                RequiredDriverPackages = new List<string> { "Milvus.Client", "TheTechIdea.Beep.Milvus" }
            }
        };

        /// <summary>
        /// Gets database nugget information for a specific DataSourceType
        /// </summary>
        public static DatabaseNuggetInfo? GetDatabaseInfo(DataSourceType dataSourceType)
        {
            return _databaseNuggets.TryGetValue(dataSourceType, out var info) ? info : null;
        }

        /// <summary>
        /// Gets all registered database types
        /// </summary>
        public static IEnumerable<DataSourceType> GetAllDatabaseTypes()
        {
            return _databaseNuggets.Keys;
        }

        /// <summary>
        /// Gets all database nugget information
        /// </summary>
        public static IReadOnlyDictionary<DataSourceType, DatabaseNuggetInfo> GetAllDatabaseInfo()
        {
            return _databaseNuggets;
        }

        /// <summary>
        /// Gets database types by category
        /// </summary>
        public static IEnumerable<DataSourceType> GetDatabaseTypesByCategory(DatasourceCategory category)
        {
            return _databaseNuggets.Where(kvp => kvp.Value.Category == category).Select(kvp => kvp.Key);
        }

        /// <summary>
        /// Checks if a DataSourceType is registered in the database nuggets registry
        /// </summary>
        public static bool IsRegisteredDatabase(DataSourceType dataSourceType)
        {
            return _databaseNuggets.ContainsKey(dataSourceType);
        }
    }
}