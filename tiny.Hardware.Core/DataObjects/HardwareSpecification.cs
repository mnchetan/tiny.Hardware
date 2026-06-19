using Newtonsoft.Json;
using System.Diagnostics;

namespace tiny.Hardware.Core.DataObjects
{
    [DebuggerStepThrough]
    public class HardwareSpecification
    {
        [DebuggerHidden]
        [JsonProperty(PropertyName = "Query", Required = Required.Default)]
        public string Query { get; set; } = "";

        [DebuggerHidden]
        [JsonProperty(PropertyName = "ExecutionType", Required = Required.Always)]
        public string ExecutionType { get; set; } = "Stream";

        [DebuggerHidden]
        [JsonProperty(PropertyName = "Protocol", Required = Required.Always)]
        public string Protocol { get; set; } = "";

        [DebuggerHidden]
        [JsonProperty(PropertyName = "IpAddress", Required = Required.Always)]
        public string IpAddress { get; set; } = "";

        [DebuggerHidden]
        [JsonProperty(PropertyName = "Port", Required = Required.Always)]
        public int Port { get; set; }

        /// <summary>
        /// Defines how raw bytes are parsed if no post-processing plugin is used.
        /// Valid values: "ASCII", "UTF8", "UTF16", "HEX"
        /// </summary>
        [DebuggerHidden]
        [JsonProperty(PropertyName = "DefaultFallbackEncoding", Required = Required.Default)]
        public string DefaultFallbackEncoding { get; set; } = "ASCII";

        // --- PRE-PROCESSING PROPERTIES ---
        [DebuggerHidden]
        [JsonProperty(PropertyName = "ExternalDllPathImplementingIProcessHardwareData_PreProcessing", Required = Required.Default)]
        public string ExternalDllPathImplementingIProcessHardwareData_PreProcessing { get; set; } = "";

        [DebuggerHidden]
        [JsonProperty(PropertyName = "ExternalDllNameImplementingIProcessHardwareData_PreProcessing", Required = Required.Default)]
        public string ExternalDllNameImplementingIProcessHardwareData_PreProcessing { get; set; } = "";

        [DebuggerHidden]
        [JsonProperty(PropertyName = "FullyQualifiedNameOfClass_PreProcessing", Required = Required.Default)]
        public string FullyQualifiedNameOfClass_PreProcessing { get; set; } = "";

        // --- POST-PROCESSING PROPERTIES ---
        [DebuggerHidden]
        [JsonProperty(PropertyName = "ExternalDllPathImplementingIProcessHardwareData_PostProcessing", Required = Required.Default)]
        public string ExternalDllPathImplementingIProcessHardwareData_PostProcessing { get; set; } = "";

        [DebuggerHidden]
        [JsonProperty(PropertyName = "ExternalDllNameImplementingIProcessHardwareData_PostProcessing", Required = Required.Default)]
        public string ExternalDllNameImplementingIProcessHardwareData_PostProcessing { get; set; } = "";

        [DebuggerHidden]
        [JsonProperty(PropertyName = "FullyQualifiedNameOfClass_PostProcessing", Required = Required.Default)]
        public string FullyQualifiedNameOfClass_PostProcessing { get; set; } = "";

        [DebuggerHidden]
        public string ResolvePostProcessingPluginPath()
        {
            // If the DLL Name is missing, there is no plugin to load.
            if (string.IsNullOrWhiteSpace(ExternalDllNameImplementingIProcessHardwareData_PostProcessing))
                return string.Empty;

            // FALLBACK LOGIC: If path is empty, assume native executing assembly path
            string basePath = string.IsNullOrWhiteSpace(ExternalDllPathImplementingIProcessHardwareData_PostProcessing)
                ? AppDomain.CurrentDomain.BaseDirectory
                : ExternalDllPathImplementingIProcessHardwareData_PostProcessing;

            return Path.Combine(basePath, ExternalDllNameImplementingIProcessHardwareData_PostProcessing);
        }

        [DebuggerHidden]
        public string ResolvePreProcessingPluginPath()
        {
            // If the DLL Name is missing, there is no plugin to load.
            if (string.IsNullOrWhiteSpace(ExternalDllNameImplementingIProcessHardwareData_PreProcessing))
                return string.Empty;

            // FALLBACK LOGIC: If path is empty, assume native executing assembly path
            string basePath = string.IsNullOrWhiteSpace(ExternalDllPathImplementingIProcessHardwareData_PreProcessing)
                ? AppDomain.CurrentDomain.BaseDirectory
                : ExternalDllPathImplementingIProcessHardwareData_PreProcessing;

            return Path.Combine(basePath, ExternalDllNameImplementingIProcessHardwareData_PreProcessing);
        }
    }
}