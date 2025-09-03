using System;
using System.Collections.Generic;
using System.Linq;
using Beep.Nugget.Engine.Interfaces;

namespace Beep.Nugget.Engine.Samples
{
    /// <summary>
    /// Sample plugin demonstrating how to create a basic nugget plugin.
    /// </summary>
    [NuggetPlugin("sample-basic-plugin", 
        Name = "Sample Basic Plugin", 
        Description = "A basic sample plugin demonstrating nugget plugin capabilities",
        Author = "TheTechIdea",
        Version = "1.0.0")]
    public class SampleBasicPlugin : NuggetPluginBase
    {
        public override string Id => "sample-basic-plugin";
        public override string Name => "Sample Basic Plugin";
        public override string Version => "1.0.0";
        public override string Description => "A basic sample plugin demonstrating nugget plugin capabilities";
        public override string Author => "TheTechIdea";

        protected override bool OnInitialize()
        {
            Logger?.LogInfo("Sample Basic Plugin: Initializing...");
            
            // Subscribe to events
            SubscribeToEvent<SampleEvent>(OnSampleEventReceived);
            
            // Get configuration
            var sampleConfig = GetConfigurationValue("sample-setting", "default-value");
            Logger?.LogInfo($"Sample configuration value: {sampleConfig}");
            
            return true;
        }

        protected override bool OnStart()
        {
            Logger?.LogInfo("Sample Basic Plugin: Starting...");
            
            // Publish a startup event
            PublishEvent(new SampleEvent
            {
                PluginId = Id,
                Message = "Plugin started successfully",
                Timestamp = DateTime.UtcNow
            });
            
            return true;
        }

        protected override bool OnStop()
        {
            Logger?.LogInfo("Sample Basic Plugin: Stopping...");
            
            // Unsubscribe from events
            UnsubscribeFromEvent<SampleEvent>(OnSampleEventReceived);
            
            // Publish a shutdown event
            PublishEvent(new SampleEvent
            {
                PluginId = Id,
                Message = "Plugin stopped",
                Timestamp = DateTime.UtcNow
            });
            
            return true;
        }

        private void OnSampleEventReceived(SampleEvent sampleEvent)
        {
            if (sampleEvent.PluginId != Id) // Don't process our own events
            {
                Logger?.LogInfo($"Received event from {sampleEvent.PluginId}: {sampleEvent.Message}");
            }
        }
    }

    /// <summary>
    /// Sample data source plugin demonstrating how to create a data source nugget plugin.
    /// </summary>
    [NuggetPlugin("sample-datasource-plugin",
        Name = "Sample DataSource Plugin",
        Description = "A sample plugin demonstrating data source capabilities",
        Author = "TheTechIdea",
        Version = "1.0.0")]
    public class SampleDataSourcePlugin : DataSourceNuggetPluginBase
    {
        public override string Id => "sample-datasource-plugin";
        public override string Name => "Sample DataSource Plugin";
        public override string Version => "1.0.0";
        public override string Description => "A sample plugin demonstrating data source capabilities";
        public override string Author => "TheTechIdea";

        public override IEnumerable<string> SupportedDataSourceTypes => new[] { "SampleDB", "MockDB" };

        protected override bool OnInitialize()
        {
            base.OnInitialize(); // Call base implementation
            Logger?.LogInfo("Sample DataSource Plugin: Custom initialization logic");
            return true;
        }

        protected override bool OnStart()
        {
            Logger?.LogInfo("Sample DataSource Plugin: Starting data source services...");
            return true;
        }

        protected override bool OnStop()
        {
            Logger?.LogInfo("Sample DataSource Plugin: Stopping data source services...");
            return true;
        }

        public override object CreateDataSource(string dataSourceType, string connectionString)
        {
            Logger?.LogInfo($"Creating data source of type: {dataSourceType}");
            
            switch (dataSourceType.ToLower())
            {
                case "sampledb":
                    return new SampleDataSource(connectionString);
                case "mockdb":
                    return new MockDataSource(connectionString);
                default:
                    throw new NotSupportedException($"Data source type '{dataSourceType}' is not supported");
            }
        }

        public override bool TestConnection(string dataSourceType, string connectionString)
        {
            Logger?.LogInfo($"Testing connection for {dataSourceType}: {connectionString}");
            
            // Simple validation - in real implementation, this would test actual connectivity
            return !string.IsNullOrEmpty(connectionString) && 
                   SupportedDataSourceTypes.Contains(dataSourceType, StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Sample event for inter-plugin communication.
    /// </summary>
    public class SampleEvent
    {
        public string PluginId { get; set; }
        public string Message { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Sample data source implementation.
    /// </summary>
    public class SampleDataSource
    {
        public string ConnectionString { get; }

        public SampleDataSource(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public bool Connect()
        {
            // Simulate connection logic
            return !string.IsNullOrEmpty(ConnectionString);
        }

        public void Disconnect()
        {
            // Simulate disconnection logic
        }

        public List<string> GetTables()
        {
            // Simulate getting table list
            return new List<string> { "Table1", "Table2", "Table3" };
        }
    }

    /// <summary>
    /// Mock data source implementation.
    /// </summary>
    public class MockDataSource
    {
        public string ConnectionString { get; }

        public MockDataSource(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public bool IsConnected => !string.IsNullOrEmpty(ConnectionString);

        public object ExecuteQuery(string query)
        {
            // Return mock data
            return new { Result = "Mock data result", Query = query };
        }
    }

    /// <summary>
    /// Sample extension plugin that extends functionality of other plugins.
    /// </summary>
    [NuggetPlugin("sample-extension-plugin",
        Name = "Sample Extension Plugin",
        Description = "A sample plugin that extends other plugins",
        Author = "TheTechIdea",
        Version = "1.0.0",
        Dependencies = new[] { "sample-basic-plugin" })]
    public class SampleExtensionPlugin : NuggetPluginBase
    {
        public override string Id => "sample-extension-plugin";
        public override string Name => "Sample Extension Plugin";
        public override string Version => "1.0.0";
        public override string Description => "A sample plugin that extends other plugins";
        public override string Author => "TheTechIdea";
        public override IEnumerable<string> Dependencies => new[] { "sample-basic-plugin" };

        protected override bool OnInitialize()
        {
            Logger?.LogInfo("Sample Extension Plugin: Checking dependencies...");
            
            // Check if required plugin is loaded
            var requiredPlugin = PluginManager?.GetPlugin("sample-basic-plugin");
            if (requiredPlugin == null)
            {
                Logger?.LogError("Required plugin 'sample-basic-plugin' is not loaded");
                return false;
            }
            
            Logger?.LogInfo($"Found required plugin: {requiredPlugin.Name}");
            return true;
        }

        protected override bool OnStart()
        {
            Logger?.LogInfo("Sample Extension Plugin: Starting extension services...");
            
            // Subscribe to events from other plugins
            SubscribeToEvent<SampleEvent>(OnSampleEventFromOtherPlugin);
            
            return true;
        }

        protected override bool OnStop()
        {
            Logger?.LogInfo("Sample Extension Plugin: Stopping extension services...");
            
            UnsubscribeFromEvent<SampleEvent>(OnSampleEventFromOtherPlugin);
            
            return true;
        }

        private void OnSampleEventFromOtherPlugin(SampleEvent sampleEvent)
        {
            Logger?.LogInfo($"Extension plugin processing event from {sampleEvent.PluginId}: {sampleEvent.Message}");
            
            // Extend the event with additional data
            var extendedEvent = new SampleEvent
            {
                PluginId = Id,
                Message = $"Extended: {sampleEvent.Message}",
                Timestamp = DateTime.UtcNow,
                Data = { ["original_plugin"] = sampleEvent.PluginId, ["original_message"] = sampleEvent.Message }
            };
            
            PublishEvent(extendedEvent);
        }
    }
}