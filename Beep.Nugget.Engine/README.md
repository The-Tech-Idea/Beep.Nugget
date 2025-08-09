# Beep Nugget Engine - Unified Architecture

A streamlined, unified framework for managing NuGet packages (nuggets) with plugin discovery and database catalog integration.

## Overview

The Beep Nugget Engine has been refactored to provide a single, unified **`NuggetManager`** class that handles all nugget-related operations:

- **NuGet Package Management**: Download, search, and install packages
- **Assembly Loading**: Dynamic loading/unloading with proper isolation
- **Plugin Discovery**: Automatic discovery and lifecycle management
- **Database Catalog**: Built-in database definitions and connection templates

## Architecture

### Core Classes

- **`NuggetManager`** - The main unified class for all nugget operations
- **`NuggetDefinition`** - Represents a nugget package definition
- **`DatabaseNuggetDefinition`** - Specialized nugget for database types
- **`DatabaseNuggetsCatalog`** - Catalog of built-in database nuggets
- **`DatabaseNuggetRegistry`** - Registry of database type information

### Helper Classes

- **`NuggetPluginBase`** - Base class for creating plugins
- **`SamplePlugins`** - Example plugin implementations
- **`BeepNuggetListViewModel`** - ViewModel for UI integration
- **`BeepNuggetManagerCLI`** - Command-line interface

### Plugin System

- **`INuggetPlugin`** - Simple plugin interface
- **`IDataSourceNuggetPlugin`** - Interface for data source plugins
- Plugin discovery and lifecycle management
- Automatic initialization and startup

## Quick Start

### Basic Usage

```csharp
// Create the unified manager
using var manager = new NuggetManager();

// Search for packages
var packages = await manager.SearchPackagesAsync("TheTechIdea");

// Install and load a package with plugin discovery
var nugget = await manager.InstallAndLoadNuggetAsync("MyPlugin", "1.0.0");

// Get all discovered plugins
var plugins = manager.GetPlugins();

// Get specific plugin types
var dataSourcePlugins = manager.GetPlugins<IDataSourceNuggetPlugin>();

// Unload when done
manager.UnloadNugget("MyPlugin");
```

### Database Catalog Operations

```csharp
var manager = new NuggetManager();

// Get all database nuggets
var databases = manager.GetBuiltInDatabaseNuggets();

// Get by category
var nosqlDbs = manager.GetNoSQLDatabaseNuggets();
var cloudDbs = manager.GetCloudDatabaseNuggets();

// Get connection templates
var template = manager.GetConnectionStringTemplate(DataSourceType.SqlServer);
var port = manager.GetDefaultPort(DataSourceType.SqlServer);
var packages = manager.GetRequiredDriverPackages(DataSourceType.SqlServer);
```

### Company Package Management

```csharp
var manager = new NuggetManager();

// Retrieve company nuggets
await manager.RetrieveCompanyNuggetsAsync(projectPath, "TheTechIdea");

// Get all nuggets (online + database catalog)
var allNuggets = await manager.GetAllNuggetsAsync(includeBuiltInDatabases: true);

// Check installation status
foreach (var nugget in manager.Definitions)
{
    Console.WriteLine($"{nugget.NuggetName}: {(nugget.Installed ? "Installed" : "Available")}");
}
```

## Event Handling

The unified manager provides events for monitoring operations:

```csharp
manager.NuggetLoaded += (sender, nugget) => 
    Console.WriteLine($"Loaded: {nugget.PackageId} with {nugget.Plugins.Count} plugins");

manager.NuggetUnloaded += (sender, packageId) => 
    Console.WriteLine($"Unloaded: {packageId}");

manager.NuggetError += (sender, error) => 
    Console.WriteLine($"Error: {error.Error.Message}");
```

## Creating Plugins

### Basic Plugin

```csharp
public class MyPlugin : NuggetPluginBase
{
    public override string Id => "my-plugin";
    public override string Name => "My Plugin";
    public override string Version => "1.0.0";
    public override string Description => "Sample plugin";

    protected override bool OnInitialize()
    {
        // Initialization logic
        return true;
    }

    protected override bool OnStart()
    {
        // Startup logic
        return true;
    }

    protected override bool OnStop()
    {
        // Cleanup logic
        return true;
    }
}
```

### Data Source Plugin

```csharp
public class MyDataSourcePlugin : DataSourceNuggetPluginBase
{
    public override string Id => "my-datasource";
    public override string Name => "My DataSource Plugin";
    public override string Version => "1.0.0";
    
    public override string[] SupportedDataSourceTypes => new[] { "MyDB" };

    public override object CreateDataSource(string type, string connectionString)
    {
        return new MyDataSource(connectionString);
    }

    public override bool TestConnection(string type, string connectionString)
    {
        return !string.IsNullOrEmpty(connectionString);
    }
}
```

## Command Line Interface

The CLI provides easy access to all functionality:

```bash
# Install and load a package
BeepNuggetManager install MyPackage 1.0.0

# Search packages
BeepNuggetManager search TheTechIdea

# List company packages
BeepNuggetManager list

# Show database catalog
BeepNuggetManager databases
BeepNuggetManager db-popular
BeepNuggetManager db-nosql

# Show connection templates
BeepNuggetManager connection SqlServer

# Plugin management
BeepNuggetManager plugins
BeepNuggetManager loaded
BeepNuggetManager unload MyPackage
```

## Benefits of Unified Architecture

1. **Simplified API**: One class for all nugget operations
2. **Reduced Complexity**: No confusion between multiple managers
3. **Better Integration**: Seamless integration between package management and plugin discovery
4. **Unified Events**: Single event system for all operations
5. **Helper Classes**: Supporting classes provide specialized functionality without duplicating core logic
6. **Backward Compatibility**: Maintains existing interfaces while providing new capabilities

## Migration from Previous Architecture

If upgrading from the previous dual-manager system:

- Replace `NuGetManager` usage with `NuggetManager`
- Replace `NuggetAssemblyManager` usage with `NuggetManager`
- Use `InstallAndLoadNuggetAsync()` for combined download and loading
- Access database catalog through the same manager instance
- Plugin discovery is now automatic during package loading

## Examples

See the following files for complete usage examples:

- `NuggetManagerUsageExamples.cs` - Comprehensive usage examples
- `BeepNuggetManagerCLI.cs` - Command-line interface examples
- `SamplePlugins.cs` - Example plugin implementations

## File Structure

```
Beep.Nugget.Logic/
??? NuggetManager.cs                    # Main unified manager
??? NuggetDefinition.cs                 # Package definition
??? DatabaseNuggetDefinition.cs        # Database package definition
??? DatabaseNuggetsCatalog.cs          # Database catalog
??? DatabaseNuggetRegistry.cs          # Database registry
??? NuggetPluginBase.cs                 # Plugin base classes
??? SamplePlugins.cs                    # Example plugins
??? BeepNuggetListViewModel.cs          # UI integration
??? BeepNuggetManagerCLI.cs             # Command-line interface
??? NuggetManagerUsageExamples.cs       # Usage examples
??? DataSourceTypeExtensions.cs         # Utility extensions
```

This unified architecture provides a clean, powerful, and easy-to-use system for all nugget-related operations while maintaining the flexibility to extend functionality through the helper classes.