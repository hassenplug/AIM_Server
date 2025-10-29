
using System.Text.Json;
using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSignalR();

var app = builder.Build();


app.UseWebSockets();

async Task<(bool success, WebSocket ws, string result, string error)> ConnectSendMessage(string ip, string path, object command)
{
    var uri = new Uri($"ws://{ip}:80/{path}");
    using var ws = new ClientWebSocket();

    try
    {
        // Use a short timeout for faster testing
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
        await ws.ConnectAsync(uri, cts.Token);

        var jsonCommand = JsonSerializer.Serialize(command);
        var bytes = Encoding.UTF8.GetBytes(jsonCommand);

        await ws.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Binary,
            true,
            CancellationToken.None);

        var buffer = new byte[1024];
        var resultback = await ws.ReceiveAsync(
            new ArraySegment<byte>(buffer),
            CancellationToken.None);

        string result = "blank";

        if (resultback.MessageType == WebSocketMessageType.Binary)
        {
            var response = Encoding.UTF8.GetString(buffer, 0, resultback.Count);
            result = response;
            Console.WriteLine("Response: " + response);
        }


        return (true, ws, result, "Connected successfully");
    }
    catch (Exception ex)
    {
        return (false, ws, "error", $"Connect failed: {ex.Message}");
    }
}


app.MapGet("/api/send/{ipAddress}", async (string ipAddress) =>
{
    var path1 = "ws_status";
    var path2 = "ws_cmd";
    var command1 = new
    {
        cmd_id = "drive",
        angle = 0.0,
        speed = 50.0,
        stacking_type = 0
    };

    var command2 = new
    {
        cmd_id = "drive",
        command = "LED",
        color = "#FF0000",
        angle = 0.0,
        speed = 50.0,
        stacking_type = 0
    };

    var (success, ws, result, error) = await ConnectSendMessage(ipAddress, path1, command1);
    var (success2, ws2, result2, error2) = await ConnectSendMessage(ipAddress, path2, command2);


    return Results.Ok(new
    {
//        details = result,details2 = result2, error1 = error, error2 = error2
        details = result,details2 = result2, error1 = error, error2 = error2
    });
});


app.MapGet("/api/one/{ipAddress?}/{cmd_id_in?}", async (string? ipAddress = "192.168.1.150", string? cmd_id_in = "drive") =>
{
    var path = "ws_cmd";
    //    var path = "ws_status";

    /* commands that almost
    drive
    turn
    drive_for
    turn_for
    turn_to
    */

    var command = new
    {
        cmd_id = cmd_id_in,
        command = "LED",
        color = "#FF0000",
        angle = 0.0,
        speed = 50.0,
        stacking_type = 0
    };

    var (success, ws, result, error) = await ConnectSendMessage(ipAddress, path, command);
    if (success)
    {
        return Results.Ok(result);
    }

    return Results.Ok(new
    {
        details = result, error = error
    });
});


app.Run();