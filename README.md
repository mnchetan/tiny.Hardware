# tiny.Hardware

## Overview

**tiny.Hardware** is an enterprise-grade, configuration-driven hardware integration gateway built on .NET 10. 

While tools like `tiny.WebApi` standardize "Data at Rest" (Databases, APIs), `tiny.Hardware` standardizes **"Data in Motion."** It abstracts the physical complexity of edge devices—such as TCP barcode scanners, Modbus PLCs, and RS-232 serial scales—into a unified, plug-and-play stream processor. 

By defining physical hardware connections in a simple JSON file and injecting dynamic processing plugins at runtime, this engine completely decouples your core logistics and WMS (Warehouse Management System) applications from proprietary hardware protocols.

---

## 🏗️ Core Architecture

The system is designed to handle high-frequency hardware events without thread starvation or memory leaks.

1. **Configuration-Driven:** Hardware nodes are defined in a JSON file (`hardware.dev.json`). The API resolves connections, protocols, and data-parsing strategies entirely from this file.
2. **Internal Message Bus (Backpressure Management):** Uses `System.Threading.Channels` as an ultra-fast internal memory queue. If a high-speed scanner blasts 1,000 events per second, the hardware socket reads them instantly and drops them onto the bus, preventing the API from crashing or dropping packets while the background service processes them at its own pace.
3. **Dynamic Plugin Execution:** Raw bytes from hardware can be parsed using `DefaultFallbackEncoding` (e.g., ASCII, HEX, UTF8) or sent through a custom `.dll` plugin loaded dynamically via .NET Reflection (`ExternalAssemblyExecutionHelper`).
4. **State Management:** Devices are actively monitored using `CancellationToken` logic, allowing endpoints to cleanly start and stop long-lived TCP streams without leaving "zombie sockets" open.

---

## 📂 Project Structure

The solution enforces a strict separation of concerns between the core engine, the executing host, and external testing tools.

```text
📦 tiny.Hardware (Solution)
 ┣ 📂 tiny.Hardware.Core (Class Library - The Engine)
 ┃ ┣ 📂 Bus             # Internal System.Threading.Channels queue (HardwareEvent.cs, InternalHardwareBus.cs)
 ┃ ┣ 📂 Configurations  # Interfaces mapping the JSON dictionary (ITinyHardwareConfigurations.cs)
 ┃ ┣ 📂 DataObjects     # Core configuration models (HardwareSpecification.cs)
 ┃ ┣ 📂 Engine          # State management and socket orchestration (HardwareOrchestrator.cs)
 ┃ ┣ 📂 Extensions      # Dynamic plugin loading logic (ProcessHardwareDataExtensions.cs)
 ┃ ┗ 📂 Providers       # Protocol implementations (Tcp, Modbus, Serial, Mock) & Factory
 ┃
 ┣ 📂 tiny.Hardware.Api (ASP.NET Core Web API - The Host)
 ┃ ┣ 📂 Controllers     # REST endpoints for Start/Stop commands (HardwareController.cs)
 ┃ ┣ 📂 Plugins         # Native Mock Parsers for local testing
 ┃ ┣ 📂 Services        # IHostedService processing the internal queue (HardwareProcessorService.cs)
 ┃ ┣ 📝 hardware.dev.json # The master hardware configuration dictionary
 ┃ ┗ 📄 Program.cs      # DI Registration and Config Loading
 ┃
 ┗ 📂 tiny.Hardware.Simulator (Console App)
   ┗ 📄 Program.cs      # Standalone TCP server emitting mock barcode/weight data for testing

    ⚙️ Configuration (`hardware.dev.json`)

The engine requires **zero code changes** to add new hardware. Simply define a unique key (for example, `Scanner1_TCP`) and specify its connection parameters.

 Plugin Segregation Logic

The engine uses a segregated three-part pathing system for external plugins:

| Property                    | Description                                                                                      |
| --------------------------- | ------------------------------------------------------------------------------------------------ |
| `ExternalDllPath`           | Physical folder containing the plugin DLL. If empty, the native executing assembly path is used. |
| `ExternalDllName`           | Plugin DLL file name. If empty, execution occurs natively via reflection.                        |
| `FullyQualifiedNameOfClass` | Fully qualified class name to instantiate.                                                       |

Sample Configuration

json
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


---

# 🚀 API Usage & Endpoints

Hardware streams are controlled through standard HTTP `POST` requests. Each endpoint references the unique hardware key defined in the configuration file.

## Start Hardware Streams

Starting a stream provisions the appropriate worker and begins piping data into the internal processing bus.

### Scanner (TCP/IP)

```http
GET http://localhost:5168/api/Hardware/Start/Scanner1_TCP
```

### Conveyor (Modbus)

```http
GET http://localhost:5168/api/Hardware/Start/Conveyor_Modbus
```

### Scale (Serial Port)

```http
GET http://localhost:5168/api/Hardware/Start/Scale_Serial
```

## Stop Hardware Streams

Stopping a stream triggers the `CancellationToken`, ensuring connections are closed cleanly and resources are released properly.

```http
GET http://localhost:5168/api/Hardware/Stop/Scanner1_TCP
GET http://localhost:5168/api/Hardware/Stop/Conveyor_Modbus
GET http://localhost:5168/api/Hardware/Stop/Scale_Serial
```

### Write back to the hardware

```http
POST http://localhost:5168/api/Hardware/Write/Scanner_Mock
Content-Type: application/json
{
  "Data": "RESET_COMMAND_01",
  "EncodingFormat": "ASCII"
}
```
---

# 🖥️ Running & Testing Locally

## 1. Launch the Simulator

Run **tiny.Hardware.Simulator**.

The simulator starts a console application that listens on **Port 9000** and waits for incoming connections.

## 2. Launch the API

Run **tiny.Hardware.Api**.

During startup, the application loads the hardware configuration from `hardware.dev.json` into memory.

## 3. Start a Hardware Connection

Use **Postman**, **Insomnia**, **curl**, or the built-in **Swagger UI** to invoke the scanner start endpoint:

```http
GET http://localhost:5168/api/Hardware/Start/Scanner1_TCP
```

### Expected Output

#### Simulator Console

The simulator accepts the connection and begins transmitting payloads such as:

```text
BOX_ID=10001
WEIGHT=12.45
```

#### API Console

The API:

1. Receives packets through the `TcpHardwareProvider`.
2. Publishes them to the `InternalHardwareBus`.
3. Processes them via `HardwareProcessorService`.
4. Applies the configured parsing plugin.
5. Outputs structured, normalized hardware events.

---

# 🌐 Hosting & Deployment

`tiny.Hardware` is built on **.NET BackgroundService** and **Kestrel**, making it deployment agnostic and suitable for both edge and cloud environments.

## Windows Service

The application supports Windows Service hosting out of the box:

```csharp
builder.Host.UseWindowsService();
```

Benefits:

* Native Windows Service installation
* Automatic startup after reboot
* Suitable for warehouse edge gateways and Windows Server deployments
* Minimal operational overhead

## Docker & Kubernetes

The application is stateless and configuration-driven.

Because all runtime settings are loaded from mounted JSON configuration files, it can be easily:

* Containerized with Docker
* Deployed to Kubernetes clusters
* Hosted behind Nginx or other reverse proxies
* Scaled independently of hardware integrations

This deployment model mirrors the architecture and deployment strategy used by `tiny.WebApi`.
