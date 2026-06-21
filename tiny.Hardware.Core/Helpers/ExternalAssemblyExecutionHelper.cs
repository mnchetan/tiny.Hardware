using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using tiny.WebApi.DataObjects;
namespace tiny.Hardware.Core.Helpers
{
    /// <summary>
    /// The ExternalAssemblyExecutionHelper class provides a method for dynamically loading and executing code from external assemblies (DLLs) at runtime. This is particularly useful for implementing a plugin architecture, where users can create their own custom processing logic in separate assemblies without needing to modify the core application. The LoadPluginFromFile method takes the path to the DLL and the fully qualified class name of the plugin, loads the assembly, and instantiates the specified class if it implements the required interface. This allows for flexible and extensible functionality within the tiny.Hardware framework, enabling users to easily add new features or processing logic by simply providing a compatible DLL.
    /// </summary>
    [DebuggerStepThrough]
    public static class ExternalAssemblyExecutionHelper
    {
        /// <summary>
        /// Loads a plugin from a specified DLL file and class name, ensuring it implements the required interface (e.g., IProcessHardwareData). This method uses AssemblyLoadContext to load the assembly in an isolated context, allowing for better management of dependencies and potential unloading in the future. It also includes robust error handling to log any issues encountered during the loading process, such as missing files, incorrect class names, or type mismatches. By using this method, users can easily extend the functionality of the tiny.Hardware framework by providing their own custom processing logic in separate assemblies without modifying the core application.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fullDllPath"></param>
        /// <param name="fullyQualifiedClassName"></param>
        /// <returns></returns>
        [DebuggerHidden]
        public static T LoadPluginFromFile<T>(string fullDllPath, string fullyQualifiedClassName) where T : class
        {
            if (string.IsNullOrWhiteSpace(fullDllPath) || !File.Exists(fullDllPath))
            {
                Global.LogDebug($"Plugin DLL not found at: {fullDllPath}");
                return null;
            }
            if (string.IsNullOrWhiteSpace(fullyQualifiedClassName))
            {
                Global.LogDebug("Fully qualified class name was not provided.");
                return null;
            }
            try
            {
                // 1. Load the assembly from the specific path
                AssemblyLoadContext loadContext = new("HardwarePluginContext", isCollectible: true);
                Assembly assembly = loadContext.LoadFromAssemblyPath(Path.GetFullPath(fullDllPath));
                // 2. Find the specific class inside the assembly
                Type pluginType = assembly.GetType(fullyQualifiedClassName);
                if (pluginType == null)
                {
                    Global.LogError($"Class '{fullyQualifiedClassName}' could not be found in assembly '{fullDllPath}'.", null);
                    return null;
                }
                // 3. Ensure the class actually implements our required interface (e.g., IProcessHardwareData)
                if (!typeof(T).IsAssignableFrom(pluginType))
                {
                    Global.LogError($"Class '{fullyQualifiedClassName}' does not implement {typeof(T).Name}.", null);
                    return null;
                }
                // 4. Instantiate and return the plugin
                return Activator.CreateInstance(pluginType) as T;
            }
            catch (Exception ex)
            {
                Global.LogError($"Critical failure loading plugin {fullyQualifiedClassName} from {fullDllPath}", ex);
                return null;
            }
        }
    }
}