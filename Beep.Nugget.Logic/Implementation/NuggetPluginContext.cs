using System;
using System.Collections.Generic;
using Beep.Nugget.Engine.Interfaces;

namespace Beep.Nugget.Engine.Implementation
{
    /// <summary>
    /// Default implementation of the nugget plugin context.
    /// </summary>
    public class NuggetPluginContext : INuggetPluginContext
    {
        public IServiceProvider ServiceProvider { get; }
        public IDictionary<string, object> Configuration { get; }
        public INuggetLogger Logger { get; }
        public INuggetEventBus EventBus { get; }
        public INuggetPluginManager PluginManager { get; }
        public string ApplicationName { get; }
        public string ApplicationVersion { get; }
        public string BaseDirectory { get; }
        public bool IsDebugMode { get; }

        public NuggetPluginContext(
            IServiceProvider serviceProvider,
            IDictionary<string, object> configuration,
            INuggetLogger logger,
            INuggetEventBus eventBus,
            INuggetPluginManager pluginManager,
            string applicationName,
            string applicationVersion,
            string baseDirectory,
            bool isDebugMode = false)
        {
            ServiceProvider = serviceProvider;
            Configuration = configuration ?? new Dictionary<string, object>();
            Logger = logger;
            EventBus = eventBus;
            PluginManager = pluginManager;
            ApplicationName = applicationName;
            ApplicationVersion = applicationVersion;
            BaseDirectory = baseDirectory;
            IsDebugMode = isDebugMode;
        }
    }

    /// <summary>
    /// Simple console logger implementation for nugget plugins.
    /// </summary>
    public class ConsoleNuggetLogger : INuggetLogger
    {
        private readonly string _category;

        public ConsoleNuggetLogger(string category = "NuggetLogger")
        {
            _category = category;
        }

        public void LogDebug(string message)
        {
            Console.WriteLine($"[DEBUG] {_category}: {message}");
        }

        public void LogInfo(string message)
        {
            Console.WriteLine($"[INFO] {_category}: {message}");
        }

        public void LogWarning(string message)
        {
            Console.WriteLine($"[WARN] {_category}: {message}");
        }

        public void LogError(string message)
        {
            Console.WriteLine($"[ERROR] {_category}: {message}");
        }

        public void LogError(Exception exception, string message = null)
        {
            var msg = string.IsNullOrEmpty(message) ? exception.Message : $"{message}: {exception.Message}";
            Console.WriteLine($"[ERROR] {_category}: {msg}");
            Console.WriteLine($"[ERROR] {_category}: {exception.StackTrace}");
        }
    }

    /// <summary>
    /// Simple in-memory event bus implementation.
    /// </summary>
    public class SimpleNuggetEventBus : INuggetEventBus
    {
        private readonly Dictionary<Type, List<object>> _handlers = new();
        private readonly object _lock = new();

        public void Publish<T>(T eventMessage) where T : class
        {
            if (eventMessage == null) return;

            List<object> handlers;
            lock (_lock)
            {
                if (!_handlers.TryGetValue(typeof(T), out handlers))
                    return;

                // Create a copy to avoid collection modification during enumeration
                handlers = new List<object>(handlers);
            }

            foreach (var handler in handlers)
            {
                try
                {
                    ((Action<T>)handler)(eventMessage);
                }
                catch (Exception ex)
                {
                    // Log error but don't stop other handlers
                    Console.WriteLine($"Error in event handler: {ex.Message}");
                }
            }
        }

        public void Subscribe<T>(Action<T> handler) where T : class
        {
            if (handler == null) return;

            lock (_lock)
            {
                if (!_handlers.TryGetValue(typeof(T), out var handlers))
                {
                    handlers = new List<object>();
                    _handlers[typeof(T)] = handlers;
                }
                handlers.Add(handler);
            }
        }

        public void Unsubscribe<T>(Action<T> handler) where T : class
        {
            if (handler == null) return;

            lock (_lock)
            {
                if (_handlers.TryGetValue(typeof(T), out var handlers))
                {
                    handlers.Remove(handler);
                    if (handlers.Count == 0)
                    {
                        _handlers.Remove(typeof(T));
                    }
                }
            }
        }
    }
}