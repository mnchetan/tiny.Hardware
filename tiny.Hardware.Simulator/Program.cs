using System.Net;
using System.Net.Sockets;
using System.Text;

namespace tiny.Hardware.Simulator
{
    internal class Program
    {
#pragma warning disable IDE0060 // Remove unused parameter
        private static async Task Main(string[] args)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            int port = 9000;
            TcpListener server = new(IPAddress.Any, port);
            server.Start();

            Console.WriteLine($"[Mock Scanner] Started on Port {port}.");

            int boxCount = 1;

            // Outer loop keeps the simulator alive forever, waiting for new connections
            while (true)
            {
                Console.WriteLine("\n[Mock Scanner] Waiting for WMS API connection...");

                using TcpClient client = await server.AcceptTcpClientAsync();
                Console.WriteLine("[Mock Scanner] WMS API Connected! Simulating box scans...");

                using NetworkStream stream = client.GetStream();

                try
                {
                    // Inner loop sends data as long as the connection is alive
                    // Inner loop sends data as long as the connection is alive
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

                        string payload = $"BOX_ID_000{boxCount}|WEIGHT_{new Random().Next(10, 50)}KG\r\n";
                        byte[] data = Encoding.ASCII.GetBytes(payload);

                        // If the API disconnects exactly at this millisecond, the catch block still handles it safely
                        await stream.WriteAsync(data);

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

                // The 'using' blocks will automatically dispose of the broken stream and client here.
                // The outer loop restarts, listening for the API to connect again.
            }
        }
    }
}