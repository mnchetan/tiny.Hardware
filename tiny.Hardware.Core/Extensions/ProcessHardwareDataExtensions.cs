using tiny.Hardware.Core.DataObjects;
using tiny.Hardware.Core.Helpers;
namespace tiny.Hardware.Core.Extensions
{
    /// <summary>
    /// The ProcessHardwareDataExtensions class provides extension methods for the HardwareSpecification class, specifically for loading post-processing plugins that implement the IProcessHardwareData interface. This class allows for dynamic resolution of plugins based on the configuration specified in the HardwareSpecification, enabling flexible and extensible processing of hardware data. The LoadPostProcessingPlugin method checks for the presence of a fully qualified class name and an optional external DLL name to determine how to load the plugin, either through native execution or by using an external assembly helper. This extension method simplifies the process of retrieving and instantiating post-processing plugins, making it easier to apply custom logic to hardware data processing within the tiny.Hardware framework.
    /// </summary>
    public static class ProcessHardwareDataExtensions
    {
        /// <summary>
        /// Loads a post-processing plugin that implements the IProcessHardwareData interface based on the configuration specified in the HardwareSpecification. The method checks for the presence of a fully qualified class name and an optional external DLL name to determine how to load the plugin, either through native execution or by using an external assembly helper. If the plugin is successfully loaded, it returns an instance of IProcessHardwareData; otherwise, it returns null.
        /// </summary>
        /// <param name="spec"></param>
        /// <returns></returns>
        public static IProcessHardwareData LoadPostProcessingPlugin(this HardwareSpecification spec)
        {
            if (string.IsNullOrWhiteSpace(spec.FullyQualifiedNameOfClass_PostProcessing))
                return null;
            // NATIVE EXECUTION (If DLL Name is empty)
            if (string.IsNullOrWhiteSpace(spec.ExternalDllNameImplementingIProcessHardwareData_PostProcessing))
            {
                Type type = Type.GetType(spec.FullyQualifiedNameOfClass_PostProcessing);
                return type != null && typeof(IProcessHardwareData).IsAssignableFrom(type)
                    ? Activator.CreateInstance(type) as IProcessHardwareData
                    : null;
            }
            // EXTERNAL EXECUTION (Using the helper from the tiny.WebApi NuGet package!)
            string path = spec.ResolvePostProcessingPluginPath();
            return File.Exists(path)
                ? ExternalAssemblyExecutionHelper.LoadPluginFromFile<IProcessHardwareData>(
                    path,
                    spec.FullyQualifiedNameOfClass_PostProcessing
                )
                : null;
        }
    }
}