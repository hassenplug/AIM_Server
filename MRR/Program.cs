
using System.Net.WebSockets;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSignalR();

var app = builder.Build();


app.UseWebSockets();


// Test helper to check if a port is open
async Task<(bool success, string error)> TestWebSocketConnectionAsync(string ip, int port, string path)
{
    var uri = new Uri($"ws://{ip}:{port}/{path}");
    using var ws = new ClientWebSocket();
    ws.Options.SetRequestHeader("Origin", "http://localhost");  // Some WS servers require this
    
    try
    {
        // Use a short timeout for faster testing
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        await ws.ConnectAsync(uri, cts.Token);
        return (true, "Connected successfully");
    }
    catch (Exception ex)
    {
        return (false, $"Port {port} failed: {ex.Message}");
    }
}

app.MapGet("/api/testrobot/{ipAddress}", async (string ipAddress) =>
{
    var results = new List<string>();
    
    // Common VEX AIM ports and paths to try
    var portsToTry = new[] { 80, 8080, 81, 8081 };
    var pathsToTry = new[] { "", "ws", "aim", "robot" };
    
    foreach (var port in portsToTry)
    {
        foreach (var path in pathsToTry)
        {
            var (success, error) = await TestWebSocketConnectionAsync(ipAddress, port, path);
            if (success)
            {
                return Results.Ok($"Success! Found robot at ws://{ipAddress}:{port}/{path}");
            }
            results.Add($"Tried ws://{ipAddress}:{port}/{path} - {error}");
        }
    }

    // Also try a basic TCP connection test to port 80 to see if web server is up
    using (var tcp = new System.Net.Sockets.TcpClient())
    {
        try
        {
            await tcp.ConnectAsync(ipAddress, 80);
            results.Add("Note: HTTP port 80 is open - robot web server appears to be running");
        }
        catch 
        {
            results.Add("Note: HTTP port 80 is closed - robot web server may not be running");
        }
    }

    return Results.Ok(new { 
        error = "Could not connect to robot websocket server",
        details = results
    });
});



app.Run();