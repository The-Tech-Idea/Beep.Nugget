using System;
using System.Collections.Generic;
using Beep.Nugget.Engine.Interfaces;
using Beep.Nugget.Engine.Implementation;

namespace Beep.Nugget.Engine
{
    /// <summary>
    /// Factory class for creating and configuring NuggetAssemblyManager instances.
    /// </summary>
    public static class NuggetAssemblyManagerFactory
    {
        /// <summary>
        /// Creates a basic NuggetAssemblyManager with default settings.
        /// </summary>
        /// <returns>A configured NuggetAssemblyManager instance.</returns>
        public static NuggetAssemblyManager CreateDefault()
        {
            return new NuggetAssemblyManager();
        }

        /// <summary>
        /// Creates a NuggetAssemblyManager with custom logger.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <returns>A configured NuggetAssemblyManager instance.</returns>
        public static NuggetAssemblyManager CreateWithLogger(INuggetLogger logger)
        {
            return new NuggetAssemblyManager(logger: logger);
        }

        /// <summary>
        /// Creates a NuggetAssemblyManager with custom configuration.
        /// </summary>
        /// <param name="logger">The logger to use (optional).</param>
        /// <param name="eventBus">The event bus to use (optional).</param>
        /// <param name="serviceProvider">The service provider for dependency injection (optional).</param>
        /// <returns>A configured NuggetAssemblyManager instance.</returns>
        public static NuggetAssemblyManager CreateCustom(
            INuggetLogger logger = null,
            INuggetEventBus eventBus = null,
            IServiceProvider serviceProvider = null)
        {
            return new NuggetAssemblyManager(
                logger: logger,
                eventBus: eventBus,
                serviceProvider: serviceProvider);
        }

        /// <summary>
        /// Creates a NuggetAssemblyManager with a builder pattern for advanced configuration.
        /// </summary>
        /// <returns>A builder instance for configuring the manager.</returns>
        public static NuggetAssemblyManagerBuilder CreateBuilder()
        {
            return new NuggetAssemblyManagerBuilder();
        }
    }

    /// <summary>
    /// Builder class for configuring NuggetAssemblyManager instances with a fluent API.
    /// </summary>
    public class NuggetAssemblyManagerBuilder
    {
        private INuggetLogger _logger;
        private INuggetEventBus _eventBus;
        private IServiceProvider _serviceProvider;
        private readonly Dictionary<string, object> _configuration = new();

        /// <summary>
        /// Sets the logger for the manager.
        /// </summary>
        /// <param name="logger">The logger to use.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public NuggetAssemblyManagerBuilder WithLogger(INuggetLogger logger)
        {
            _logger = logger;
            return this;
        }

        /// <summary>
        /// Sets the event bus for the manager.
        /// </summary>
        /// <param name="eventBus">The event bus to use.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public NuggetAssemblyManagerBuilder WithEventBus(INuggetEventBus eventBus)
        {
            _eventBus = eventBus;
            return this;
        }

        /// <summary>
        /// Sets the service provider for dependency injection.
        /// </summary>
        /// <param name="serviceProvider">The service provider to use.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public NuggetAssemblyManagerBuilder WithServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            return this;
        }

        /// <summary>
        /// Enables console logging with the specified category.
        /// </summary>
        /// <param name="category">The logging category (optional).</param>
        /// <returns>The builder instance for method chaining.</returns>
        public NuggetAssemblyManagerBuilder WithConsoleLogging(string category = "NuggetAssemblyManager")
        {
            _logger = new ConsoleNuggetLogger(category);
            return this;
        }

        /// <summary>
        /// Enables the default event bus.
        /// </summary>
        /// <returns>The builder instance for method chaining.</returns>
        public NuggetAssemblyManagerBuilder WithEventBus()
        {
            _eventBus = new SimpleNuggetEventBus();
            return this;
        }

        /// <summary>
        /// Adds a configuration value.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <param name="value">The configuration value.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public NuggetAssemblyManagerBuilder WithConfiguration(string key, object value)
        {
            _configuration[key] = value;
            return this;
        }

        /// <summary>
        /// Adds multiple configuration values.
        /// </summary>
        /// <param name="configuration">The configuration dictionary.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public NuggetAssemblyManagerBuilder WithConfiguration(IDictionary<string, object> configuration)
        {
            foreach (var kvp in configuration)
            {
                _configuration[kvp.Key] = kvp.Value;
            }
            return this;
        }

        /// <summary>
        /// Builds the configured NuggetAssemblyManager instance.
        /// </summary>
        /// <returns>The configured NuggetAssemblyManager instance.</returns>
        public NuggetAssemblyManager Build()
        {
            // Use defaults if not specified
            _logger ??= new ConsoleNuggetLogger();
            _eventBus ??= new SimpleNuggetEventBus();

            var manager = new NuggetAssemblyManager(_logger, _eventBus, _serviceProvider);

            return manager;
        }
    }
}