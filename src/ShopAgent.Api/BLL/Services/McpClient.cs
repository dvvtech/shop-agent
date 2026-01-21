
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace ShopAgent.Api.BLL.Services
{
    public class McpClient : IDisposable
    {
        private readonly ClientWebSocket _webSocket;
        private readonly Uri _serverUri;

        public McpClient(string serverUrl)
        {
            _webSocket = new ClientWebSocket();
            _serverUri = new Uri(serverUrl);
        }

        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            await _webSocket.ConnectAsync(_serverUri, cancellationToken);
        }

        public async Task GetAllToolAsync()
        {
            using var httpClient = new HttpClient();
            var url = "https://mcp001.vkusvill.ru/mcp";

            // JSON-RPC запрос
            var json = """
            {
                "jsonrpc": "2.0",
                "id": 1,
                "method": "tools/list"
            }
            """;

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

            var response = await httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseBody);
        }

        public async Task<McpResponse> CallToolAsync(string toolName, object parameters)
        {
            var request = new
            {
                jsonrpc = "2.0",
                method = "tools/call",
                @params = new
                {
                    name = toolName,
                    arguments = parameters
                },
                id = Guid.NewGuid().ToString()
            };

            var json = JsonSerializer.Serialize(request);
            var bytes = Encoding.UTF8.GetBytes(json);

            await _webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );

            // Чтение ответа
            var buffer = new byte[1024 * 4];
            var result = await _webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                CancellationToken.None
            );

            var responseJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
            return JsonSerializer.Deserialize<McpResponse>(responseJson);
        }

        public void Dispose()
        {
            _webSocket.Dispose();
        }
    }

    public class McpResponse
    {
        public string Jsonrpc { get; set; }
        public object Result { get; set; }
        public McpError Error { get; set; }
        public string Id { get; set; }
    }

    public class McpError
    {
        public int Code { get; set; }
        public string Message { get; set; }
    }
}
