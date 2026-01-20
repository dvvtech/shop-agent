using OpenAI.Assistants;
using OpenAI;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.Json;

namespace ShopAgent.Api.BLL.Services
{
    public class OpenAiAssistantService
    {
        private readonly OpenAIClient _openAiClient;
        private readonly McpClient _mcpClient;
        private readonly string _assistantId;

        public OpenAiAssistantService(
            string openAiApiKey,
            string mcpServerUrl,
            string assistantId = null)
        {
            _openAiClient = new OpenAIClient(openAiApiKey);
            _mcpClient = new McpClient(mcpServerUrl);
            _assistantId = assistantId;
        }

        public async Task<string> CreateAssistantWithMcpToolsAsync(
            string name,
            string instructions,
            List<McpToolDefinition> mcpTools)
        {
            // Конвертируем MCP инструменты в OpenAI tools
            var openAiTools = new List<Tool>();

            foreach (var mcpTool in mcpTools)
            {
                openAiTools.Add(new Tool
                {
                    Function = new Function
                    {
                        Name = mcpTool.Name,
                        Description = mcpTool.Description,
                        Parameters = mcpTool.Parameters
                    }
                });
            }

            // Создаем ассистента
            var assistant = await _openAiClient.AssistantsEndpoint.CreateAssistantAsync(
                name: name,
                instructions: instructions,
                tools: openAiTools,
                model: "gpt-4-turbo-preview"
            );

            _assistantId = assistant.Id;
            return assistant.Id;
        }

        public async Task<string> ProcessMessageWithMcpAsync(
            string threadId,
            string message,
            CancellationToken cancellationToken = default)
        {
            // 1. Добавляем сообщение в тред
            await _openAiClient.AssistantsEndpoint.CreateMessageAsync(
                threadId,
                message,
                cancellationToken: cancellationToken
            );

            // 2. Запускаем ран
            var run = await _openAiClient.AssistantsEndpoint.CreateRunAsync(
                threadId,
                new CreateRunRequest(_assistantId),
                cancellationToken: cancellationToken
            );

            // 3. Обрабатываем tool calls
            while (run.Status == "requires_action")
            {
                var toolCalls = run.RequiredAction.SubmitToolOutputs.ToolCalls;
                var toolOutputs = new List<ToolOutput>();

                foreach (var toolCall in toolCalls)
                {
                    // Вызываем MCP инструмент
                    var mcpResponse = await _mcpClient.CallToolAsync(
                        toolCall.Function.Name,
                        JsonSerializer.Deserialize<Dictionary<string, object>>(
                            toolCall.Function.Arguments.ToString()
                        )
                    );

                    toolOutputs.Add(new ToolOutput
                    {
                        ToolCallId = toolCall.Id,
                        Output = JsonSerializer.Serialize(mcpResponse.Result)
                    });
                }

                // Отправляем результаты в OpenAI
                run = await _openAiClient.AssistantsEndpoint.SubmitToolOutputsAsync(
                    threadId,
                    run.Id,
                    toolOutputs,
                    cancellationToken: cancellationToken
                );
            }

            // 4. Получаем результат
            var messages = await _openAiClient.AssistantsEndpoint.ListMessagesAsync(
                threadId,
                cancellationToken: cancellationToken
            );

            return messages.Data
                .First(m => m.Role == "assistant")
                .Content
                .First()
                .Text
                .Value;
        }
    }

    public class McpToolDefinition
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public JsonElement Parameters { get; set; }
    }
}
