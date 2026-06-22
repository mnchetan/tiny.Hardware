# tiny.Hardware

## Overview

**tiny.Hardware** is an enterprise-grade, configuration-driven hardware integration gateway built on **.NET 10**.

While frameworks such as `tiny.WebApi` standardize **Data at Rest** (databases, APIs, and services), `tiny.Hardware` standardizes **Data in Motion**. It abstracts the complexity of edge devices—including TCP barcode scanners, Modbus PLCs, weighing scales, and serial devices—into a unified, plug-and-play event processing platform.

By defining hardware connections in a simple JSON configuration file and loading processing plugins dynamically at runtime, the framework completely decouples business applications, WMS platforms, and logistics systems from proprietary hardware protocols.

The result is a scalable, maintainable, and vendor-agnostic hardware integration layer that can be deployed at warehouse edges, manufacturing facilities, distribution centers, or cloud-connected industrial environments.

---

## Features

### Configuration-Driven Hardware Integration

Add or modify hardware devices without changing application code. Hardware definitions, communication settings, and processing behavior are managed entirely through configuration.

### High-Throughput Event Processing

Built on `System.Threading.Channels` to provide an efficient internal message bus capable of handling thousands of hardware events per second while maintaining system responsiveness.

### Dynamic Plugin Architecture

Raw hardware data can be processed using built-in encoding strategies or custom processing plugins loaded dynamically through .NET Reflection.

### Protocol Abstraction

Provides a unified programming model for multiple communication protocols, allowing applications to interact with hardware consistently regardless of the underlying transport.

### Long-Running Connection Management

Uses `CancellationToken`-based lifecycle management to safely start, stop, and monitor persistent hardware connections.

### Cloud and Edge Ready

Supports deployment as:

- ASP.NET Core Web API
- Windows Service
- Linux Service (systemd)
- Docker Container
- Kubernetes Workload

---

## Core Architecture

The framework is designed to process high-frequency hardware events while maintaining reliability, scalability, and operational stability.

### Configuration Layer

Hardware definitions are stored in `hardware.dev.json`.

The framework resolves:

- Communication protocol
- Connection parameters
- Processing strategy
- Plugin implementation
- Data encoding format

entirely from configuration.

### Hardware Providers

Protocol-specific providers establish and maintain communication with physical devices.

| Protocol | Mode | Typical Use Cases |
|-----------|--------|-------------------|
| TCP/IP | Stream | Barcode scanners, vision systems |
| Modbus TCP | Poll | PLCs, conveyors, industrial controllers |
| Serial (RS-232/RS-485) | Stream | Weighing scales, legacy hardware |
| Mock | Stream/Poll | Development and testing |

### Internal Message Bus

The framework uses `System.Threading.Channels` as a high-performance in-memory event bus.

When hardware emits data:

1. Providers receive data.
2. Events are published to the internal channel.
3. Background workers consume events asynchronously.
4. Processing plugins transform raw payloads into business-ready events.

This architecture prevents hardware communication threads from being blocked by downstream processing workloads.

### Processing Pipeline

Incoming hardware data can be processed using:

- Built-in encoding handlers
- Reflection-based plugin execution
- Custom external assemblies

### Lifecycle Management

All long-running hardware operations are controlled through managed cancellation tokens, ensuring:

- Graceful shutdowns
- Safe resource cleanup
- Reliable reconnection strategies
- No orphaned connections

---

## Solution Structure

```text
📦 tiny.Hardware (Solution)

┣ 📂 tiny.Hardware.Core
┃ ┣ 📂 Bus
┃ ┃ ┣ 📄 HardwareEvent.cs
┃ ┃ ┗ 📄 InternalHardwareBus.cs
┃ ┃
┃ ┣ 📂 Configurations
┃ ┃ ┗ 📄 ITinyHardwareConfigurations.cs
┃ ┃
┃ ┣ 📂 DataObjects
┃ ┃ ┗ 📄 HardwareSpecification.cs
┃ ┃
┃ ┣ 📂 Engine
┃ ┃ ┗ 📄 HardwareOrchestrator.cs
┃ ┃
┃ ┣ 📂 Extensions
┃ ┃ ┗ 📄 ProcessHardwareDataExtensions.cs
┃ ┃
┃ ┗ 📂 Providers
┃   ┣ 📄 TcpHardwareProvider.cs
┃   ┣ 📄 ModbusHardwareProvider.cs
┃   ┣ 📄 SerialHardwareProvider.cs
┃   ┣ 📄 MockHardwareProvider.cs
┃   ┗ 📄 HardwareProviderFactory.cs
┃
┣ 📂 tiny.Hardware.Api
┃ ┣ 📂 Controllers
┃ ┃ ┗ 📄 HardwareController.cs
┃ ┃
┃ ┣ 📂 Plugins
┃ ┃ ┗ 📄 Sample Processing Plugins
┃ ┃
┃ ┣ 📂 Services
┃ ┃ ┗ 📄 HardwareProcessorService.cs
┃ ┃
┃ ┣ 📝 hardware.dev.json
┃ ┃
┃ ┗ 📄 Program.cs
┃
┗ 📂 tiny.Hardware.Simulator
  ┗ 📄 Program.cs
```

---

## Configuration (`hardware.dev.json`)

The framework requires **zero code changes** to onboard new hardware.

Simply define a unique hardware identifier and specify the communication and processing settings.

### Plugin Loading Configuration

| Property | Description |
|-----------|-------------|
| `ExternalDllPathImplementingIProcessHardwareData_PostProcessing` | Directory containing the plugin assembly. If empty, the current application assembly is used. |
| `ExternalDllNameImplementingIProcessHardwareData_PostProcessing` | Name of the plugin DLL to load. |
| `FullyQualifiedNameOfClass_PostProcessing` | Fully qualified class name implementing the processing logic. |

### Sample Configuration

```json
{
  "Scanner1_TCP": {
    "Query": "ContinuousRead",
    "ExecutionType": "Stream",
    "Protocol": "TcpIp",
    "IpAddress": "127.0.0.1",
    "Port": 9000,
    "DefaultFallbackEncoding": "ASCII",
    "ExternalDllPathImplementingIProcessHardwareData_PostProcessing": "",
    "ExternalDllNameImplementingIProcessHardwareData_PostProcessing": "",
    "FullyQualifiedNameOfClass_PostProcessing": "tiny.Hardware.Api.Plugins.ScannerMockParser"
  },
  "Conveyor_Modbus": {
    "ExecutionType": "Poll",
    "Protocol": "Modbus",
    "IpAddress": "192.168.1.50",
    "Port": 502,
    "DefaultFallbackEncoding": "HEX",
    "FullyQualifiedNameOfClass_PostProcessing": "tiny.Hardware.Api.Plugins.ConveyorMockParser"
  },
  "Scale_Serial": {
    "ExecutionType": "Stream",
    "Protocol": "Serial",
    "IpAddress": "COM3",
    "Port": 9600,
    "DefaultFallbackEncoding": "UTF8",
    "FullyQualifiedNameOfClass_PostProcessing": "tiny.Hardware.Api.Plugins.ScaleMockParser"
  }
}
```

---

## API Endpoints

Hardware devices are controlled through REST endpoints exposed by the API.

Each endpoint references a unique hardware key defined in `hardware.dev.json`.

### Start Hardware

#### TCP Scanner

```http
GET /api/Hardware/Start/Scanner1_TCP
```

#### Modbus Device

```http
GET /api/Hardware/Start/Conveyor_Modbus
```

#### Serial Device

```http
GET /api/Hardware/Start/Scale_Serial
```

### Stop Hardware

#### TCP Scanner

```http
GET /api/Hardware/Stop/Scanner1_TCP
```

#### Modbus Device

```http
GET /api/Hardware/Stop/Conveyor_Modbus
```

#### Serial Device

```http
GET /api/Hardware/Stop/Scale_Serial
```

### Write Data to Hardware

```http
POST /api/Hardware/Write/Scanner_Mock
Content-Type: application/json

{
  "Data": "RESET_COMMAND_01",
  "EncodingFormat": "ASCII"
}
```

---

## Running Locally

### 1. Start the Simulator

Run:

```bash
tiny.Hardware.Simulator
```

The simulator starts a TCP server listening on port `9000`.

### 2. Start the API

Run:

```bash
tiny.Hardware.Api
```

During startup the application:

1. Loads hardware configuration.
2. Registers hardware providers.
3. Initializes background services.
4. Starts the processing pipeline.

### 3. Start a Hardware Stream

Using Swagger, Postman, Insomnia, or curl:

```http
GET http://localhost:5168/api/Hardware/Start/Scanner1_TCP
```

### Expected Simulator Output

```text
BOX_ID=10001
WEIGHT=12.45
BOX_ID=10002
WEIGHT=10.18
```

### Processing Flow

```text
TCP Scanner
      │
      ▼
Hardware Provider
      │
      ▼
InternalHardwareBus
      │
      ▼
HardwareProcessorService
      │
      ▼
Processing Plugin
      │
      ▼
Business Event
```

The framework:

1. Receives data from hardware.
2. Publishes events to the internal channel.
3. Processes events asynchronously.
4. Executes configured plugins.
5. Produces structured business-ready output.

---

## Creating Custom Processing Plugins

```csharp
public sealed class ScannerParser : IProcessHardwareData
{
    public Task<object?> ProcessAsync(
        string hardwareKey,
        byte[] payload,
        CancellationToken cancellationToken)
    {
        string barcode = Encoding.ASCII.GetString(payload);

        return Task.FromResult<object?>(
            new
            {
                Hardware = hardwareKey,
                Barcode = barcode,
                Timestamp = DateTime.UtcNow
            });
    }
}
```

After compiling the assembly:

1. Deploy the DLL.
2. Update `hardware.dev.json`.
3. Restart the service.

No framework code changes are required.

---

## Hosting and Deployment

### Windows Service

```csharp
builder.Host.UseWindowsService();
```

Benefits:

- Automatic startup after reboot
- Native Windows Service integration
- Ideal for warehouse edge deployments

### Linux Service (systemd)

```csharp
builder.Host.UseSystemd();
```

Benefits:

- Native Linux service management
- Centralized logging
- Automatic restart policies

### Docker

The framework is fully containerizable and can be deployed using standard Docker images and mounted configuration files.

### Kubernetes

Because all runtime behavior is configuration-driven and the application remains stateless, it can be deployed using:

- ConfigMaps
- Secrets
- Persistent Volumes
- Horizontal scaling strategies

---

## Design Goals

- Eliminate hardware-specific code from business applications
- Standardize communication across heterogeneous devices
- Support high-throughput event processing
- Enable plug-and-play hardware onboarding
- Provide enterprise-grade reliability
- Support cloud-native and edge deployments

---

## Technology Stack

- .NET 10
- ASP.NET Core
- Background Services
- System.Threading.Channels
- TCP/IP Networking
- Modbus TCP
- Serial Communications (RS-232 / RS-485)
- Reflection-Based Plugin Loading
- Dependency Injection
- Configuration-Driven Architecture

---

## Related Projects

### tiny.WebApi

Standardized access to:

- Databases
- APIs
- Files
- External systems

### tiny.Hardware

Standardized access to:

- Barcode scanners
- PLCs
- Weighing scales
- Industrial hardware
- Edge devices

Together they provide a complete platform for managing both **Data at Rest** and **Data in Motion**.