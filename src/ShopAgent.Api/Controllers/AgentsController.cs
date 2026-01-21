using Microsoft.AspNetCore.Mvc;
using ShopAgent.Api.BLL.Services;

namespace ShopAgent.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentsController : ControllerBase
    {
        private readonly McpClient _mcpClient;
        //private readonly OpenAiAssistantService _assistantService;
        private readonly Dictionary<string, string> _userThreads = new();

        public AgentsController(McpClient mcpClient)
            //OpenAiAssistantService assistantService)
        {
            _mcpClient = mcpClient;
            //_assistantService = assistantService;
        }

        [HttpGet("test")]
        public async Task Test()
        {
            //await _mcpClient.ConnectAsync();
            await _mcpClient.GetAllToolAsync();
        }

        //[HttpPost("create-thread")]
        //public async Task<IActionResult> CreateThread([FromBody] CreateThreadRequest request)
        //{
        //    var thread = await _assistantService.CreateThreadAsync();
        //    _userThreads[request.UserId] = thread.Id;

        //    return Ok(new { threadId = thread.Id });
        //}

        //[HttpPost("query")]
        //public async Task<IActionResult> QueryAgent([FromBody] AgentQueryRequest request)
        //{
        //    if (!_userThreads.TryGetValue(request.UserId, out var threadId))
        //    {
        //        return BadRequest("Thread not found. Create a thread first.");
        //    }

        //    var response = await _assistantService.ProcessMessageWithMcpAsync(
        //        threadId,
        //        request.Message
        //    );

        //    return Ok(new { response });
        //}

        [HttpGet("tools")]
        public IActionResult GetAvailableTools()
        {
            // Можно вернуть список доступных MCP инструментов
            var tools = new[]
            {
            new { name = "file_read", description = "Read file contents" },
            new { name = "file_write", description = "Write to file" },
            new { name = "search_web", description = "Search the web" }
        };

            return Ok(tools);
        }
    }

    public class CreateThreadRequest
    {
        public string UserId { get; set; }
    }

    public class AgentQueryRequest
    {
        public string UserId { get; set; }
        public string Message { get; set; }
    }
}
