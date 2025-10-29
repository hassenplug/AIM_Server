
using System.Text.Json;
using System.Net.WebSockets;
using System.Text;
using MRR.Robot;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();

var app = builder.Build();
app.UseWebSockets();

// Example of using the AIMRobot class
app.MapGet("/test-aim", async () =>
{
//    await using var robot = new AIMRobot("192.168.1.150"); // Replace with your robot's IP
    var robot = new AIMRobot("192.168.1.150:80"); // Replace with your robot's IP
    await robot.ConnectAsync();

    // Example commands
    await robot.ClearScreenAsync();
    await robot.ShowAIAsync();

    await robot.PrintAsync("Hello from C#!");
    await robot.SetLedAsync(1, 0, 255, 0); // Green front LED
    
    // Move forward
    await robot.MoveAsync(270, 100); // 0 degrees (forward), 100mm/s speed
    await Task.Delay(20); // Wait 2 seconds
    await robot.StopAsync();

    await robot.TurnAsync(90, 100); // Turn right 90 degrees at 100mm/s
    await Task.Delay(20); // Wait 2 seconds
    await robot.StopAsync();


    return "Commands sent successfully!";
});


app.Run();