using System;

namespace Beep.Nugget.Engine.Samples
{
    /// <summary>
    /// Sample basic plugin demonstrating how to create a nugget plugin
    /// </summary>
    [NuggetPlugin("sample-basic-plugin", 
        Name = "Sample Basic Plugin", 
        Description = "A basic sample plugin",
        Version = "1.0.0")]
    public class SampleBasicPlugin : NuggetPluginBase
    {
        public override string Id => "sample-basic-plugin";
        public override string Name => "Sample Basic Plugin";
        public override string Version => "1.0.0";
        public override string Description => "A basic sample plugin demonstrating nugget plugin capabilities";

        protected override bool OnInitialize()
        {
            Console.WriteLine("Sample Basic Plugin: Initializing...");
            return true;
        }

        protected override bool OnStart()
        {
            Console.WriteLine("Sample Basic Plugin: Starting...");
            return true;
        }

        protected override bool OnStop()
        {
            Console.WriteLine("Sample Basic Plugin: Stopping...");
            return true;
        }
    }

    /// <summary>
    /// Sample data source plugin demonstrating database connectivity
    /// </summary>
    [NuggetPlugin("sample-datasource-plugin",
        Name = "Sample DataSource Plugin",
        Description = "A sample data source provider",
        Version = "1.0.0")]
    public class SampleDataSourcePlugin : DataSourceNuggetPluginBase
    {
        public override string Id => "sample-datasource-plugin";
        public override string Name => "Sample DataSource Plugin";
        public override string Version => "1.0.0";
        public override string Description => "A sample data source provider";

        public override string[] SupportedDataSourceTypes => new[] { "SampleDB", "MockDB" };

        protected override bool OnStart()
        {
            Console.WriteLine("Sample DataSource Plugin: Starting data source services...");
            return true;
        }

        protected override bool OnStop()
        {
            Console.WriteLine("Sample DataSource Plugin: Stopping data source services...");
            return true;
        }

        public override object CreateDataSource(string dataSourceType, string connectionString)
        {
            Console.WriteLine($"Creating data source of type: {dataSourceType}");
            
            return dataSourceType.ToLower() switch
            {
                "sampledb" => new SampleDataSource(connectionString),
                "mockdb" => new MockDataSource(connectionString),
                _ => throw new NotSupportedException($"Data source type '{dataSourceType}' is not supported")
            };
        }

        public override bool TestConnection(string dataSourceType, string connectionString)
        {
            Console.WriteLine($"Testing connection for {dataSourceType}: {connectionString}");
            
            // Simple validation - in real implementation, this would test actual connectivity
            return !string.IsNullOrEmpty(connectionString) && 
                   Array.Exists(SupportedDataSourceTypes, t => t.Equals(dataSourceType, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Sample data source implementation
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

        public string[] GetTables()
        {
            // Simulate getting table list
            return new[] { "Table1", "Table2", "Table3" };
        }
    }

    /// <summary>
    /// Mock data source implementation
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
}