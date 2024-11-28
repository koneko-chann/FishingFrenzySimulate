using FishingFrenzy;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class WebSocketClient
{
    public event Action<string> OnMessageReceived; 
    private ClientWebSocket webSocket;

    public async Task ConnectAsync(string uri)
    {
        webSocket = new ClientWebSocket();
        Console.WriteLine("Connecting to WebSocket server...");
        await webSocket.ConnectAsync(new Uri(uri), CancellationToken.None);
        Console.WriteLine("Connected!");

        _ = Task.Run(() => ReceiveMessagesAsync());
    }

    public async Task SendMessageAsync(string message)
    {
        if (webSocket.State == WebSocketState.Open)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine($"Message sent: {message}");
        }
        else
        {
            Console.WriteLine("WebSocket is not open.");
        }
    }

    private async Task ReceiveMessagesAsync()
    {
        byte[] buffer = new byte[1024];

        while (webSocket.State == WebSocketState.Open)
        {
            try
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    Console.WriteLine("WebSocket connection closed.");
                }
                else
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"Message received: {message}");

                    // Kích hoạt sự kiện OnMessageReceived
                    OnMessageReceived?.Invoke(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in receiving message: {ex.Message}");
                break;
            }
        }
    }

    public async Task DisconnectAsync()
    {
        if (webSocket != null && webSocket.State == WebSocketState.Open)
        {
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client Disconnecting", CancellationToken.None);
            Console.WriteLine("Disconnected from WebSocket server.");
        }
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        var webSocketClient = new WebSocketClient();
        webSocketClient.OnMessageReceived += (message) =>
        {
            Console.WriteLine($"Received via event: {message}");
        };
        await webSocketClient.ConnectAsync("wss://fishing-frenzy-api-0c12a800fbfe.herokuapp.com/?token=");
        await webSocketClient.SendMessageAsync("{\"cmd\":\"prepare\",\"range\":\"short_range\"}");
        await Task.Delay(5000);
        await webSocketClient.SendMessageAsync("{\"cmd\":\"start\"}");
        await Task.Delay(30000);
        await webSocketClient.DisconnectAsync();
        Console.ReadLine();
    }
}
