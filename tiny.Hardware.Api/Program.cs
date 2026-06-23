using Newtonsoft.Json;
using tiny.Hardware.Api.Hubs;
using tiny.Hardware.Api.Services;
using tiny.Hardware.Core.Bus;
using tiny.Hardware.Core.Configurations;
using tiny.Hardware.Core.DataObjects;
using tiny.Hardware.Core.Engine;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Host.UseWindowsService();
// 1. ADD CORS FOR ANGULAR (Usually runs on http://localhost:4200)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200") // Added 127.0.0.1
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Crucial for SignalR WebSockets
    });
});
string hardwareConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "hardware.dev.json");
TinyHardwareConfigurations hardwareConfigs = new();
if (File.Exists(hardwareConfigPath))
{
    string jsonContent = File.ReadAllText(hardwareConfigPath);
    hardwareConfigs.HardwareSpecifications = JsonConvert.DeserializeObject<Dictionary<string, HardwareSpecification>>(jsonContent)
                                             ?? [];
}
builder.Services.AddSingleton<ITinyHardwareConfigurations>(hardwareConfigs);
builder.Services.AddSingleton<InternalHardwareBus>();
builder.Services.AddSingleton<HardwareOrchestrator>();
builder.Services.AddHostedService<HardwareProcessorService>();
builder.Services.AddControllers();
builder.Services.AddSignalR(); // NEW: Register SignalR
WebApplication app = builder.Build();
app.UseRouting();
app.UseCors("AngularCorsPolicy"); // NEW: Apply CORS
app.MapControllers();
app.MapHub<HardwareHub>("/hubs/hardware"); // NEW: Map the WebSocket endpoint
app.Run();