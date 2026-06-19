using System.Diagnostics;
using System.Runtime.CompilerServices;
using tiny.Hardware.Core.DataObjects;
using tiny.WebApi.DataObjects;

namespace tiny.Hardware.Extensions
{
    [DebuggerStepThrough]
    public static class ProcessHardwareDataExtensions
    {
        [DebuggerStepThrough]
        [DebuggerHidden]
        public static string ResolvePluginPath(this HardwareSpecification spec, [CallerMemberName] string callerName = "")
        {
            Global.LogDebug("Resolving Hardware Plugin Path.");
            // Mirrors your exact ProcessDataExtensions logic
            return !string.IsNullOrWhiteSpace(spec.ExternalDllNameImplementingIProcessHardwareData_PostProcessing)
                && !string.IsNullOrWhiteSpace(spec.FullyQualifiedNameOfClass_PostProcessing)
                && string.IsNullOrWhiteSpace(spec.ExternalDllPathImplementingIProcessHardwareData_PostProcessing)
                ? Path.Combine(Global.ConfigurationDirectoryPath, spec.ExternalDllNameImplementingIProcessHardwareData_PostProcessing)
                : Path.Combine(spec.ExternalDllPathImplementingIProcessHardwareData_PostProcessing, spec.ExternalDllNameImplementingIProcessHardwareData_PostProcessing);
        }

        [DebuggerStepThrough]
        [DebuggerHidden]
        public static IProcessHardwareData LoadPostProcessingPlugin(this HardwareSpecification spec)
        {
            string path = spec.ResolvePluginPath();
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;

            Global.LogDebug($"Loading hardware plugin from {path}");
            // Assuming ExternalAssemblyExecutionHelper from your core
            return tiny.WebApi.Helpers.ExternalAssemblyExecutionHelper.LoadPluginFromFile<IProcessHardwareData>(path, spec.FullyQualifiedNameOfClass_PostProcessing);
        }
    }
}