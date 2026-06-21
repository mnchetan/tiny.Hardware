using Newtonsoft.Json;
using tiny.Hardware.Api.Services;
using tiny.Hardware.Core.Bus;
using tiny.Hardware.Core.Configurations;
using tiny.Hardware.Core.DataObjects;
using tiny.Hardware.Core.Engine;
// using tiny.WebApi.Configurations; // Assuming you call services.AddTinyWebApi() here
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsService();
// 1. Initialize your tiny.WebApi NuGet package as usual
// builder.Services.AddTinyWebApi(...);
// 2. Load the Hardware-specific JSON file
string hardwareConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "hardware.dev.json");
TinyHardwareConfigurations hardwareConfigs = new();
if (File.Exists(hardwareConfigPath))
{
    string jsonContent = File.ReadAllText(hardwareConfigPath);
    hardwareConfigs.HardwareSpecifications = JsonConvert.DeserializeObject<Dictionary<string, HardwareSpecification>>(jsonContent)
                                             ?? [];
    Console.WriteLine($"[Startup] Loaded {hardwareConfigs.HardwareSpecifications.Count} hardware configurations.");
}
// 3. Inject our Hardware config interface
builder.Services.AddSingleton<ITinyHardwareConfigurations>(hardwareConfigs);
// 4. Register the Hardware Engine components
builder.Services.AddSingleton<InternalHardwareBus>();
builder.Services.AddSingleton<HardwareOrchestrator>();
builder.Services.AddHostedService<HardwareProcessorService>();
// Inject the interface into the DI container
builder.Services.AddSingleton<ITinyHardwareConfigurations>(hardwareConfigs);
builder.Services.AddControllers();
WebApplication app = builder.Build();
app.MapControllers();
app.Run();