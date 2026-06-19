using tiny.Hardware.Core.DataObjects;
using tiny.Hardware.Core.Helpers;

namespace tiny.Hardware.Core.Extensions
{
    public static class ProcessHardwareDataExtensions
    {
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