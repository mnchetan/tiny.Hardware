using System.Net;
using System.Net.Sockets;
using System.Text;

/// PROACTIVE NOTE: This is a simple TCP server that simulates an overhead scanner by sending formatted box scan data to a connected client (the WMS API) every few seconds. It includes proactive checks to detect if the client has disconnected and handles exceptions gracefully to allow for continuous testing of the WMS API's ability to handle hardware interactions, including connection drops and reconnections, without crashing the simulator.
namespace tiny.Hardware.Simulator
{
    /// <summary>
    /// The Program class serves as the entry point for the mock hardware simulator application.
    /// </summary>
    internal class Program
    {
#pragma warning disable IDE0060 // Remove unused parameter
        private static async Task Main(string[] args)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            /// PROACTIVE NOTE: In a real-world scenario, you might want to make the port configurable via command-line arguments or environment variables for flexibility. For this mock simulator, we're hardcoding it for simplicity.
            int port = 9000;
            TcpListener server = new(IPAddress.Any, port);
            server.Start();
            Console.WriteLine($"[Mock Scanner] Started on Port {port}.");
            int boxCount = 1;
            /// PROACTIVE NOTE: The outer loop keeps the simulator alive forever, waiting for new connections
            while (true)
            {
                Console.WriteLine("\n[Mock Scanner] Waiting for WMS API connection...");
                using TcpClient client = await server.AcceptTcpClientAsync();
                Console.WriteLine("[Mock Scanner] WMS API Connected! Simulating bi-directional flow...");
                using NetworkStream stream = client.GetStream();
                // =========================================================================
                // NEW: START THE BACKGROUND LISTENER FOR INCOMING COMMANDS (RX)
                // =========================================================================
                _ = Task.Run(async () =>
                {
                    byte[] buffer = new byte[1024];
                    try
                    {
                        while (true)
                        {
                            int bytesRead = await stream.ReadAsync(buffer);
                            if (bytesRead == 0) break; // Client disconnected
                            string receivedData = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                            Console.WriteLine($"\n  <-- RECEIVED FROM API: {receivedData}");
                            // Formulate and send the ACK back over the TCP pipe
                            string ackMessage = $"TCP_ACK_RECEIVED: [{receivedData}]\r\n";
                            byte[] ackBytes = Encoding.ASCII.GetBytes(ackMessage);
                            await stream.WriteAsync(ackBytes);
                        }
                    }
                    catch { /* Connection dropped, handled gracefully by the main loop */ }
                });
                // =========================================================================

                try
                {
                    // Inner loop sends data as long as the connection is alive (TX)
                    while (true)
                    {
                        await Task.Delay(3000);
                        // PROACTIVE CHECK: Did the WMS API quietly drop the connection?
                        // This checks if the socket has been closed by the host before we try to write to it.
                        if (client.Client.Poll(0, SelectMode.SelectRead) && client.Client.Available == 0)
                        {
                            Console.WriteLine("[Mock Scanner] Detected WMS API disconnect. Resetting...");
                            break; // Break the inner loop, go back to waiting for a connection
                        }
                        /// PROACTIVE NOTE: Generate a simulated box scan payload with a unique ID and a random weight
                        string payload = $"BOX_ID_000{boxCount}|WEIGHT_{new Random().Next(10, 50)}KG\r\n";
                        byte[] data = Encoding.ASCII.GetBytes(payload);
                        // If the API disconnects exactly at this millisecond, the catch block still handles it safely
                        await stream.WriteAsync(data);
                        /// PROACTIVE NOTE: Log the emitted data for debugging purposes
                        Console.WriteLine($"   -> Emitted to API: {payload.Trim()}");
                        boxCount++;
                    }
                }
                catch (IOException)
                {
                    // Catch the exact exception you encountered and gracefully exit the inner loop
                    Console.WriteLine("[Mock Scanner] WMS API Disconnected (Connection aborted). Resetting...");
                }
                catch (SocketException)
                {
                    Console.WriteLine("[Mock Scanner] Socket error. WMS API dropped. Resetting...");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Mock Scanner] Unexpected error: {ex.Message}");
                }
                /// PROACTIVE NOTE: The 'using' blocks will automatically dispose of the broken stream and client here.
                /// PROACTIVE NOTE: The outer loop restarts, listening for the API to connect again.
            }
        }
    }
}