# Beep.Nugget - Runtime NuGet Package Manager

A comprehensive .NET library for managing NuGet packages at runtime, with a specialized focus on TheTechIdea packages and built-in database connectors.

## Features

### ?? Core Package Management
- **Runtime Package Discovery**: Search and discover TheTechIdea packages from NuGet.org
- **Runtime Package Download**: Download packages and their dependencies automatically
- **Runtime Assembly Loading**: Load assemblies into running applications dynamically
- **Installation Tracking**: Track which packages are installed vs available

### ??? Built-in Database Catalog
- **40+ Database Connectors**: Pre-configured nuggets for popular databases
- **Dictionary-Based Registry**: Centralized database information with no switch statements
- **Smart Categorization**: Organized by RDBMS, NoSQL, Cloud, Graph, Vector, etc.
- **Extension Methods**: Clean, readable API with DataSourceType extensions
- **Connection Templates**: Ready-to-use connection string templates
- **Port & Configuration**: Default ports and authentication requirements
- **Driver Dependencies**: Required NuGet packages for each database type

### ?? Multiple Interfaces
- **Programmatic API**: Use directly in your .NET applications
- **WinForms GUI**: Visual package browser and manager
- **Command Line Interface**: Automate operations via CLI
- **MVVM Support**: Built-in ViewModel for MVVM applications

## New Dictionary-Based Architecture

The library now uses a centralized `DatabaseNuggetRegistry` instead of switch statements:

```csharp
// Before: Switch statements in multiple places
// After: Centralized dictionary with extension methods

var sqlServer = DataSourceType.SqlServer;
var packageName = sqlServer.GetOfficialNuggetPackage();    // "TheTechIdea.Beep.SqlServer"
var friendlyName = sqlServer.GetFriendlyName();            // "SQL Server"
var category = sqlServer.GetDatabaseCategory();            // DatasourceCategory.RDBMS
var port = sqlServer.GetDefaultPort();                     // 1433
var template = sqlServer.GetConnectionStringTemplate();    // "Server={Server}..."
var isCloud = sqlServer.IsCloudDatabase();                 // false
var isNoSQL = sqlServer.IsNoSQLDatabase();                 // false
```

## Supported Database Types

### Relational Databases (RDBMS)
- **SQL Server** - `TheTechIdea.Beep.SqlServer`
- **Oracle** - `TheTechIdea.Beep.Oracle`
- **MySQL** - `TheTechIdea.Beep.MySQL`
- **PostgreSQL** - `TheTechIdea.Beep.PostgreSQL`
- **SQLite** - `TheTechIdea.Beep.SQLite`
- **Firebird** - `TheTechIdea.Beep.Firebird`
- **IBM DB2** - `TheTechIdea.Beep.DB2`
- **CockroachDB** - `TheTechIdea.Beep.CockroachDB`

### Cloud Databases
- **Azure SQL** - `TheTechIdea.Beep.AzureSQL`
- **AWS RDS** - `TheTechIdea.Beep.AWSRDS`
- **Snowflake** - `TheTechIdea.Beep.Snowflake`
- **AWS Redshift** - `TheTechIdea.Beep.Redshift`
- **Google BigQuery** - `TheTechIdea.Beep.BigQuery`

### Document Databases
- **MongoDB** - `TheTechIdea.Beep.MongoDB`
- **CouchDB** - `TheTechIdea.Beep.CouchDB`
- **RavenDB** - `TheTechIdea.Beep.RavenDB`
- **Amazon DynamoDB** - `TheTechIdea.Beep.DynamoDB`
- **Google Firebase** - `TheTechIdea.Beep.Firebase`

### Specialized Databases
- **Redis** (Key-Value) - `TheTechIdea.Beep.Redis`
- **Neo4j** (Graph) - `TheTechIdea.Beep.Neo4j`
- **Cassandra** (Columnar) - `TheTechIdea.Beep.Cassandra`
- **ClickHouse** (Analytics) - `TheTechIdea.Beep.ClickHouse`
- **Elasticsearch** (Search) - `TheTechIdea.Beep.Elasticsearch`
- **InfluxDB** (Time Series) - `TheTechIdea.Beep.InfluxDB`
- **Teradata** (Data Warehouse) - `TheTechIdea.Beep.Teradata`
- **Vertica** (Data Warehouse) - `TheTechIdea.Beep.Vertica`

### Vector Databases
- **ChromaDB** - `TheTechIdea.Beep.ChromaDB`
- **Pinecone** - `TheTechIdea.Beep.Pinecone`
- **Qdrant** - `TheTechIdea.Beep.Qdrant`
- **Weaviate** - `TheTechIdea.Beep.Weaviate`
- **Milvus** - `TheTechIdea.Beep.Milvus`

## Quick Start

### Using Extension Methods (Recommended)

```csharp
using Beep.Nugget.Logic;
using TheTechIdea.Beep.Utilities;

// Get database information using extension methods
var dbType = DataSourceType.PostgreSQL;
var packageName = dbType.GetOfficialNuggetPackage();  // "TheTechIdea.Beep.PostgreSQL"
var friendlyName = dbType.GetFriendlyName();          // "PostgreSQL"
var connectionTemplate = dbType.GetConnectionStringTemplate();
var defaultPort = dbType.GetDefaultPort();            // 5432
var requiredDrivers = dbType.GetRequiredDriverPackages();

// Check database characteristics
bool isCloud = dbType.IsCloudDatabase();              // false
bool isNoSQL = dbType.IsNoSQLDatabase();              // false
bool isRelational = dbType.IsRelationalDatabase();    // true
bool requiresAuth = dbType.RequiresAuthentication();  // true

// Get similar databases in the same category
var similarDatabases = dbType.GetSameCategoryDatabases();
```

### Working with Categories

```csharp
// Get all databases in a category using extension methods
var vectorDatabases = DatasourceCategory.VectorDB.GetDatabaseTypes();
var vectorNuggets = DatasourceCategory.VectorDB.GetNuggetDefinitions();

// Category information
var categoryName = DatasourceCategory.RDBMS.GetFriendlyName(); // "Relational Databases"
```

### Basic Package Management

```csharp
// Create manager
var nugetManager = new NuGetManager();

// Search for packages
var packages = await nugetManager.SearchPackagesAsync("TheTechIdea");

// Download and install a package using extension method
var dbType = DataSourceType.SqlServer;
string packagePath = await nugetManager.DownloadNuGetAsync(
    dbType.GetOfficialNuggetPackage(), 
    "latest"
);
await nugetManager.AddNuGetToRunningApplication(packagePath);
```

### Database Catalog Usage

```csharp
// Get all database nuggets
var allDatabases = nugetManager.GetBuiltInDatabaseNuggets();

// Get databases by category
var rdbmsDatabases = nugetManager.GetRelationalDatabaseNuggets();
var nosqlDatabases = nugetManager.GetNoSQLDatabaseNuggets();
var cloudDatabases = nugetManager.GetCloudDatabaseNuggets();
var vectorDatabases = nugetManager.GetDatabaseNuggetsByCategory(DatasourceCategory.VectorDB);

// Search databases
var searchResults = nugetManager.SearchDatabaseNuggets("sql");
```

### MVVM Pattern

```csharp
var viewModel = new BeepNuggetListViewModel();

// Get all packages (includes database catalog)
await viewModel.GetListCommand.ExecuteAsync(null);

// Get only database nuggets
await viewModel.GetDatabaseNuggetsCommand.ExecuteAsync(null);

// Filter by category
await viewModel.GetDatabaseNuggetsByCategoryCommand.ExecuteAsync(DatasourceCategory.VectorDB);

// Install a package
await viewModel.InstallCommand.ExecuteAsync(selectedPackage);
```

### Command Line Interface

```bash
# Search for packages
BeepNuggetManagerCLI search "Beep"

# Install a package using database type
BeepNuggetManagerCLI install "TheTechIdea.Beep.SqlServer"

# Show all databases
BeepNuggetManagerCLI databases

# Show databases by category
BeepNuggetManagerCLI databases VectorDB

# Show popular databases
BeepNuggetManagerCLI popular

# Show cloud databases
BeepNuggetManagerCLI cloud

# Show connection template with enhanced info
BeepNuggetManagerCLI connection SqlServer

# Show registry statistics
BeepNuggetManagerCLI registry

# Show all categories
BeepNuggetManagerCLI categories
```

## Architecture

### Project Structure
- **Beep.Nugget.Logic** (.NET 8): Core functionality and database catalog
- **Beep.Nugget.Winform** (.NET 9): Windows Forms GUI application
- **Beep.Nugget.Demo** (.NET 8): Example usage patterns

### Key Classes
- **`DatabaseNuggetRegistry`**: Centralized dictionary of all database information
- **`DatabaseNuggetInfo`**: Data structure containing database properties
- **`DataSourceTypeExtensions`**: Extension methods for clean API access
- **`DatasourceCategoryExtensions`**: Category-based extension methods
- **`NuGetManager`**: Main package management engine
- **`DatabaseNuggetsCatalog`**: Simplified catalog using the registry
- **`DatabaseNuggetDefinition`**: Database nugget with registry integration
- **`BeepNuggetListViewModel`**: MVVM support with database filtering

### Benefits of Dictionary Approach
- ? **No Switch Statements**: Centralized data structure
- ? **Easy to Extend**: Just add entries to the dictionary
- ? **Type Safe**: Compile-time checking with extensions
- ? **Clean Code**: Readable extension method syntax
- ? **Consistent**: Single source of truth for all database info
- ? **Maintainable**: Changes in one place affect everything

## Installation

```xml
<PackageReference Include="TheTechIdea.Beep.Nugget" Version="latest" />
```

## Requirements

- .NET 8 or higher
- NuGet.Protocol packages
- TheTechIdea.Beep.DataManagementModels

## License

© 2025 TheTechIdea - All rights reserved

## Contributing

This is part of the TheTechIdea Beep ecosystem. For contributions and issues, please contact TheTechIdea.