using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;

namespace Sora.TestWebSocket.Controllers
{
    [Route("/ws")]
    public class WebSocketController : Controller
    {
        private readonly WebSocketHandle _webSocketHandle;

        public WebSocketController(WebSocketHandle webSocketHandle)
        {
            _webSocketHandle = webSocketHandle; 
        }

        [HttpGet]
        public async Task Index()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await _webSocketHandle.Init(Guid.NewGuid().ToString(), webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        //private async Task Echo(WebSocket webSocket)
        //{
        //    var buffer = new byte[1024 * 4];
        //    var receiveResult = await webSocket.ReceiveAsync(
        //        new ArraySegment<byte>(buffer), CancellationToken.None);

        //    while (!receiveResult.CloseStatus.HasValue)
        //    {
        //        await webSocket.SendAsync(
        //            new ArraySegment<byte>(buffer, 0, receiveResult.Count),
        //            WebSocketMessageType.Text,
        //            true,
        //            CancellationToken.None);

        //        receiveResult = await webSocket.ReceiveAsync(
        //            new ArraySegment<byte>(buffer), CancellationToken.None);
        //    }

        //    await webSocket.CloseAsync(
        //        receiveResult.CloseStatus.Value,
        //        receiveResult.CloseStatusDescription,
        //        CancellationToken.None);
        //}
    }

    public class WebSocketHandle
    {
        private List<SocketConnection> socketConnections = new List<SocketConnection>();

        public async Task Init(string id, WebSocket webSocket)
        {
            lock (socketConnections)
            {
                socketConnections.Add(new SocketConnection
                {
                    Id = id,
                    WebSocket = webSocket
                });
            }

            await SendMessageToSockets($"<b>{id}</b> vừa kết nối");

            while(webSocket.State == WebSocketState.Open)
            {
                var message = await ReceiveMessage(id, webSocket);
                if (!string.IsNullOrEmpty(message))
                {
                    await SendMessageToSockets(message);
                }
            }
        }

        public async Task SendMessageToSockets(string message)
        {
            IEnumerable<SocketConnection> toSentTo;

            lock (socketConnections)
            {
                toSentTo = socketConnections.ToList();
            }

            var tasks = toSentTo.Select(async socketConnection =>
            {
                var bytes = Encoding.Default.GetBytes(message);
                var arraySegment = new ArraySegment<byte>(bytes);
                await socketConnection.WebSocket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
            });

            await Task.WhenAll(tasks);
        }

        public async Task<string> ReceiveMessage(string id, WebSocket webSocket)
        {
            var arraySegment = new ArraySegment<byte>(new byte[4096]); // 4kB
            var receiveMessage = await webSocket.ReceiveAsync(arraySegment, CancellationToken.None);
            if(receiveMessage.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.Default.GetString(arraySegment);
                if (!string.IsNullOrWhiteSpace(message))
                {
                    return $"<b>{id}</b>: {message}";
                }
            }
            return string.Empty;
        }
    }

    public class SocketConnection
    {
        public string Id { get; set; }

        public WebSocket WebSocket { get; set; }
    }
}
