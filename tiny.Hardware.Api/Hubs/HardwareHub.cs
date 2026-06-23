using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace tiny.Hardware.Api.Hubs
{
    [DebuggerStepThrough]
    public class HardwareHub : Hub
    {
        // The Angular client will connect here. 
        // We don't need methods here unless the client directly invokes the hub, 
        // because our background service will broadcast to the clients natively.
    }
}