using Newtonsoft.Json;
using System.Diagnostics;
namespace tiny.Hardware.Core.DataObjects
{
    /// <summary>
    /// The HardwareSpecification class defines the configuration details for a specific hardware device within the tiny.Hardware framework. It includes properties such as the query string to identify the device, the execution type (e.g., "Stream"), the communication protocol, IP address, port, and encoding settings. Additionally, it provides properties for specifying external DLLs and classes that implement pre-processing and post-processing logic for handling hardware data. This class serves as a blueprint for how to interact with and process data from different hardware devices, allowing for dynamic plugin resolution and flexible handling of hardware events based on the defined specifications.
    /// </summary>
    [DebuggerStepThrough]
    public class HardwareSpecification
    {
        /// <summary>
        /// Query is a string that serves as an identifier or command to interact with the hardware device. It can be used to specify the type of data to retrieve or the action to perform on the device. The exact format and content of the query depend on the specific hardware and its communication protocol. This property allows the application to send specific instructions or requests to the hardware device, enabling dynamic interaction based on the defined specifications.
        /// </summary>
        [DebuggerHidden]
        [JsonProperty(PropertyName = "Query", Required = Required.Default)]
        public string Query { get; set; } = "";
        /// <summary>
        /// ExecutionType defines how the application should interact with the hardware device. Valid values include "Stream" for continuous data streaming or "RequestResponse" for one-time queries. This property determines the mode of communication with the device, influencing how data is received and processed. For example, a "Stream" execution type would indicate that the application should expect a continuous flow of data from the device, while "RequestResponse" would suggest that the application should send a query and wait for a single response. This setting is crucial for configuring the appropriate handling logic in the HardwareProcessorService based on the nature of the hardware interaction.
        /// </summary>
        [DebuggerHidden]
        [JsonProperty(PropertyName = "ExecutionType", Required = Required.Always)]
        public string ExecutionType { get; set; } = "Stream";
        /// <summary>
        /// Protocol specifies the communication protocol used to interact with the hardware device, such as "TCP", "UDP", "Serial", etc. This property is essential for establishing the correct type of connection and ensuring that data is transmitted and received in a manner compatible with the device's requirements. The protocol setting influences how the application connects to the device, how it sends commands, and how it listens for incoming data, making it a critical component of the hardware specification for proper communication and data processing.
        /// </summary>
        [DebuggerHidden]
        [JsonProperty(PropertyName = "Protocol", Required = Required.Always)]
        public string Protocol { get; set; } = "";
        /// <summary>
        /// IpAddress is the network address of the hardware device when using network-based communication protocols such as TCP or UDP. This property is required for establishing a connection to the device and allows the application to send commands and receive data over the network. The IP address should be specified in a valid format (e.g., "
        /// </summary>
        [DebuggerHidden]
        [JsonProperty(PropertyName = "IpAddress", Required = Required.Always)]
        public string IpAddress { get; set; } = "";
        /// <summary>
        /// Port is the network port number used to communicate with the hardware device when using network-based protocols. This property is essential for establishing a connection to the device and ensuring that data is transmitted to and received from the correct endpoint. The port number should be specified as an integer and must correspond to the port that the hardware device is configured to listen on for incoming connections. Properly setting the port is crucial for successful communication and data exchange with the hardware device.
        /// </summary>
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
        /// <summary>
        /// The following properties are used for dynamic plugin resolution. They specify the path and class information
        /// </summary>
        // --- PRE-PROCESSING PROPERTIES ---
        [DebuggerHidden]
        [JsonProperty(PropertyName = "ExternalDllPathImplementingIProcessHardwareData_PreProcessing", Required = Required.Default)]
        public string ExternalDllPathImplementingIProcessHardwareData_PreProcessing { get; set; } = "";
        /// <summary>
        /// This property specifies the name of the external DLL that contains the implementation of the IProcessHardwareData interface for pre-processing hardware data. If this property is set, the HardwareProcessorService will attempt to load the specified DLL and use the defined class for pre-processing incoming hardware data before any post-processing logic is applied. This allows for dynamic extension of the hardware processing capabilities by simply providing the appropriate DLL and class information
        /// </summary>
        [DebuggerHidden]
        [JsonProperty(PropertyName = "ExternalDllNameImplementingIProcessHardwareData_PreProcessing", Required = Required.Default)]
        public string ExternalDllNameImplementingIProcessHardwareData_PreProcessing { get; set; } = "";
        /// <summary>
        /// This property specifies the fully qualified name of the class that implements the IProcessHardwareData interface for pre-processing hardware data. The fully qualified name should include the namespace and class name (e.g., "MyNamespace.MyPreProcessingClass"). If this property is set, the HardwareProcessorService will attempt to instantiate the specified class from the provided DLL and use it for pre-processing incoming hardware data. This allows for flexible and dynamic handling of hardware data by enabling custom pre-processing logic to be defined and applied based on the hardware specifications.
        /// </summary>
        [DebuggerHidden]
        [JsonProperty(PropertyName = "FullyQualifiedNameOfClass_PreProcessing", Required = Required.Default)]
        public string FullyQualifiedNameOfClass_PreProcessing { get; set; } = "";
        /// <summary>
        /// The following properties are used for dynamic plugin resolution for post-processing hardware data. They specify the path and class information for the external DLL that contains the implementation of the IProcessHardwareData interface for post-processing. If these properties are set, the HardwareProcessorService will attempt to load the specified DLL and use the defined class for post-processing incoming hardware data after any pre-processing logic is applied. This allows for dynamic extension of the hardware processing capabilities by simply providing the appropriate DLL and class information
        /// </summary>
        // --- POST-PROCESSING PROPERTIES ---
        [DebuggerHidden]
        [JsonProperty(PropertyName = "ExternalDllPathImplementingIProcessHardwareData_PostProcessing", Required = Required.Default)]
        public string ExternalDllPathImplementingIProcessHardwareData_PostProcessing { get; set; } = "";
        /// <summary>
        /// This property specifies the name of the external DLL that contains the implementation of the IProcessHardwareData interface for post-processing hardware data. If this property is set, the HardwareProcessorService will attempt to load the specified DLL and use the defined class for post-processing incoming hardware data after any pre-processing logic is applied. This allows for dynamic extension of the hardware processing capabilities by simply providing the appropriate DLL and class information
        /// </summary>
        [DebuggerHidden]
        [JsonProperty(PropertyName = "ExternalDllNameImplementingIProcessHardwareData_PostProcessing", Required = Required.Default)]
        public string ExternalDllNameImplementingIProcessHardwareData_PostProcessing { get; set; } = "";
        /// <summary>
        /// This property specifies the fully qualified name of the class that implements the IProcessHardwareData interface for post-processing hardware data. The fully qualified name should include the namespace and class name (e.g., "MyNamespace.MyPostProcessingClass"). If this property is set, the HardwareProcessorService will attempt to instantiate the specified class from the provided DLL and use it for post-processing incoming hardware data after any pre-processing logic is applied. This allows for flexible and dynamic handling of hardware data by enabling custom post-processing logic to be defined and applied based on the hardware specifications.
        /// </summary>
        [DebuggerHidden]
        [JsonProperty(PropertyName = "FullyQualifiedNameOfClass_PostProcessing", Required = Required.Default)]
        public string FullyQualifiedNameOfClass_PostProcessing { get; set; } = "";
        /// <summary>
        /// The ResolvePostProcessingPluginPath method is responsible for constructing the full file path to the external DLL that contains the implementation of the IProcessHardwareData interface for post-processing hardware data. It checks if the DLL name is provided and, if so, combines it with the specified path. If the path is not provided, it defaults to using the base directory of the executing assembly. This method ensures that the HardwareProcessorService can dynamically locate and load the appropriate plugin for post-processing based on the configuration defined in the HardwareSpecification.
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// The ResolvePreProcessingPluginPath method is responsible for constructing the full file path to the external DLL that contains the implementation of the IProcessHardwareData interface for pre-processing hardware data. It checks if the DLL name is provided and, if so, combines it with the specified path. If the path is not provided, it defaults to using the base directory of the executing assembly. This method ensures that the HardwareProcessorService can dynamically locate and load the appropriate plugin for pre-processing based on the configuration defined in the HardwareSpecification.
        /// </summary>
        /// <returns></returns>
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