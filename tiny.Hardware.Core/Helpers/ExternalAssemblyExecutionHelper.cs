using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using tiny.WebApi.DataObjects; // Global.cs

namespace tiny.Hardware.Core.Helpers
{
    [DebuggerStepThrough]
    public static class ExternalAssemblyExecutionHelper
    {
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