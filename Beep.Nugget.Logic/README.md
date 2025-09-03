# Beep Nugget Engine - Pluggable Framework

A powerful, extensible framework for loading and managing nugget packages at runtime with full support for plugin discovery, lifecycle management, and inter-plugin communication.

## Overview

The Beep Nugget Engine provides a complete pluggable architecture that allows applications to:

- **Load and unload nugget packages** dynamically at runtime
- **Discover and manage plugins** with automatic lifecycle management
- **Enable inter-plugin communication** through an event bus system
- **Support dependency injection** for modern application architectures
- **Provide proper isolation** using collectible AssemblyLoadContext
- **Handle errors gracefully** with comprehensive logging and event handling

## Key Features

### ?? Plugin Architecture
- **Interface-based design** with `INuggetPlugin` for standardized plugin development
- **Base classes** (`NuggetPluginBase`, `DataSourceNuggetPluginBase`) for rapid plugin development
- **Plugin discovery** automatically finds and initializes plugins from loaded assemblies
- **Dependency management** supports plugin dependencies and load ordering

### ??? Assembly Management
- **Collectible contexts** allow true unloading of assemblies and plugins
- **Framework compatibility** automatically selects compatible .NET framework versions
- **Isolation** prevents conflicts between different nugget packages
- **Memory management** with proper cleanup and garbage collection

### ?? Communication & Events
- **Event bus system** enables publish/subscribe communication between plugins
- **Status tracking** monitors plugin lifecycle and health
- **Error handling** comprehensive error reporting and recovery
- **Logging integration** supports custom logging implementations

### ??? Developer Experience
- **Fluent API** builder pattern for easy configuration
- **Extension methods** provide convenient utility functions
- **Sample plugins** demonstrate best practices and usage patterns
- **Comprehensive documentation** with examples and API reference

## Quick Start

### Basic Usage

```csharp
// Create a nugget manager with default settings
using var manager = NuggetAssemblyManagerFactory.CreateDefault();

// Load a nugget package
var nugget = await manager.LoadNuggetAsync(@"C:\Packages\MyPlugin.nupkg", "my-plugin");

// Discover plugins
var plugins = manager.DiscoverPlugins();
foreach (var plugin in plugins)
{
    Console.WriteLine($"Loaded plugin: {plugin.Name} v{plugin.Version}");
}

// Use specific plugin types
var dataSourcePlugins = manager.DiscoverPlugins<IDataSourceNuggetPlugin>();

// Unload when done
manager.UnloadNugget("my-plugin");
```

### Advanced Configuration

```csharp
// Create manager with custom configuration
using var manager = NuggetAssemblyManagerFactory.CreateBuilder()
    .WithConsoleLogging("MyApp")
    .WithEventBus()
    .WithConfiguration("debug-mode", true)
    .WithServiceProvider(serviceProvider)
    .Build();

// Set up event handling
manager.NuggetLoaded += (s, e) => Console.WriteLine($"Loaded: {e.LoadedNugget.PackageId}");
manager.NuggetError += (s, e) => Console.WriteLine($"Error: {e.Exception.Message}");

// Load multiple nuggets concurrently
var nuggetPaths = new Dictionary<string, string>
{
    { "plugin1", @"C:\Packages\Plugin1.nupkg" },
    { "plugin2", @"C:\Packages\Plugin2.nupkg" }
};

var results = await manager.LoadNuggetsAsync(nuggetPaths);
```

## Creating Plugins

### Basic Plugin

```csharp
[NuggetPlugin("my-basic-plugin", 
    Name = "My Basic Plugin", 
    Description = "A simple example plugin",
    Author = "Your Name",
    Version = "1.0.0")]
public class MyBasicPlugin : NuggetPluginBase
{
    public override string Id => "my-basic-plugin";
    public override string Name => "My Basic Plugin";
    public override string Version => "1.0.0";
    public override string Description => "A simple example plugin";
    public override string Author => "Your Name";

    protected override bool OnInitialize()
    {
        Logger?.LogInfo("Initializing my plugin...");
        
        // Subscribe to events
        SubscribeToEvent<MyEvent>(OnMyEventReceived);
        
        return true;
    }

    protected override bool OnStart()
    {
        Logger?.LogInfo("Starting my plugin...");
        
        // Publish startup event
        PublishEvent(new MyEvent { Message = "Plugin started" });
        
        return true;
    }

    protected override bool OnStop()
    {
        Logger?.LogInfo("Stopping my plugin...");
        return true;
    }

    private void OnMyEventReceived(MyEvent myEvent)
    {
        Logger?.LogInfo($"Received event: {myEvent.Message}");
    }
}
```

### Data Source Plugin

```csharp
[NuggetPlugin("my-datasource-plugin",
    Name = "My DataSource Plugin",
    Description = "Custom data source provider",
    Author = "Your Name",
    Version = "1.0.0")]
public class MyDataSourcePlugin : DataSourceNuggetPluginBase
{
    public override string Id => "my-datasource-plugin";
    public override string Name => "My DataSource Plugin";
    public override string Version => "1.0.0";
    public override string Description => "Custom data source provider";
    public override string Author => "Your Name";

    public override IEnumerable<string> SupportedDataSourceTypes => 
        new[] { "MyDB", "CustomDB" };

    public override object CreateDataSource(string dataSourceType, string connectionString)
    {
        switch (dataSourceType.ToLower())
        {
            case "mydb":
                return new MyDataSource(connectionString);
            case "customdb":
                return new CustomDataSource(connectionString);
            default:
                throw new NotSupportedException($"Unsupported data source: {dataSourceType}");
        }
    }

    public override bool TestConnection(string dataSourceType, string connectionString)
    {
        // Implement connection testing logic
        return !string.IsNullOrEmpty(connectionString);
    }
}
```

## Architecture

### Core Interfaces

- **`INuggetAssemblyManager`** - Main interface for loading/unloading nuggets
- **`INuggetPlugin`** - Base interface all plugins must implement
- **`INuggetPluginContext`** - Provides services and context to plugins
- **`INuggetPluginManager`** - Interface for managing loaded plugins
- **`INuggetLogger`** - Logging interface for plugins and manager
- **`INuggetEventBus`** - Event bus for inter-plugin communication

### Key Classes

- **`NuggetAssemblyManager`** - Main implementation of the assembly manager
- **`NuggetPluginBase`** - Abstract base class for plugins
- **`DataSourceNuggetPluginBase`** - Base class for data source plugins
- **`NuggetAssemblyManagerFactory`** - Factory for creating manager instances
- **`LoadedNugget`** - Information about a loaded nugget package

### Plugin Lifecycle

1. **Load** - Nugget package is extracted and assemblies are loaded
2. **Discover** - Plugin types are discovered in loaded assemblies
3. **Initialize** - Plugins are created and `Initialize()` is called
4. **Start** - `Start()` is called to begin plugin operation
5. **Run** - Plugin operates normally, can communicate via event bus
6. **Stop** - `Stop()` is called to shut down plugin
7. **Unload** - Assembly context is unloaded and resources are cleaned up

## Event System

The framework includes a powerful event bus system for inter-plugin communication:

```csharp
// Define custom events
public class MyCustomEvent
{
    public string Message { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Data { get; set; }
}

// Publish events from plugins
PublishEvent(new MyCustomEvent 
{ 
    Message = "Something happened",
    Timestamp = DateTime.UtcNow 
});

// Subscribe to events in plugins
SubscribeToEvent<MyCustomEvent>(eventData =>
{
    Logger?.LogInfo($"Received: {eventData.Message}");
});
```

## Configuration

Plugins can access configuration through the plugin context:

```csharp
protected override bool OnInitialize()
{
    // Get configuration values
    var setting1 = GetConfigurationValue("my-setting", "default-value");
    var setting2 = GetConfigurationValue<int>("numeric-setting", 42);
    
    // Access services via dependency injection
    var myService = GetService<IMyService>();
    
    return true;
}
```

## Error Handling

The framework provides comprehensive error handling:

```csharp
// Manager-level error handling
manager.NuggetError += (sender, args) =>
{
    Console.WriteLine($"Error in {args.Operation}: {args.Exception.Message}");
    // Implement recovery logic
};

// Plugin-level error handling
protected override bool OnStart()
{
    try
    {
        // Plugin startup logic
        return true;
    }
    catch (Exception ex)
    {
        Logger?.LogError(ex, "Failed to start plugin");
        return false;
    }
}
```

## Best Practices

### Plugin Development

1. **Always inherit from `NuggetPluginBase`** for standard functionality
2. **Use the provided logging** through `Logger` property
3. **Handle errors gracefully** and return appropriate status
4. **Clean up resources** in the `OnStop()` method
5. **Use events for communication** instead of direct plugin references
6. **Keep plugins focused** on a single responsibility

### Manager Usage

1. **Use the factory methods** for creating manager instances
2. **Set up event handlers** for important notifications
3. **Dispose the manager** when no longer needed
4. **Handle load/unload errors** appropriately
5. **Monitor plugin status** for health checking

### Performance

1. **Load nuggets asynchronously** when possible
2. **Unload unused nuggets** to free memory
3. **Use lazy loading** for expensive plugin operations
4. **Monitor memory usage** with large numbers of plugins

## Extensions and Integration

The framework is designed to integrate with existing Beep framework components:

- **Assembly class definitions** for UI component discovery
- **Data source management** for database connectivity
- **Configuration systems** for settings management
- **Logging frameworks** for centralized logging

## Troubleshooting

### Common Issues

1. **Assembly not found** - Check framework compatibility and file paths
2. **Plugin not discovered** - Ensure plugin implements `INuggetPlugin`
3. **Unload failures** - Release all references to plugin objects
4. **Event not received** - Verify event subscriptions and types match

### Debugging

1. **Enable debug logging** to see detailed operation logs
2. **Use breakpoints** in plugin lifecycle methods
3. **Check plugin status** via the plugin manager
4. **Monitor events** through the event bus

## License

This framework is part of the Beep project and follows the same licensing terms.

## Contributing

Contributions are welcome! Please follow the existing code style and include appropriate tests for new functionality.