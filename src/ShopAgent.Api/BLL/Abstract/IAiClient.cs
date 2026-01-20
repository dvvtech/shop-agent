namespace ShopAgent.Api.BLL.Abstract
{
    public interface IAiClient
    {
        Task<string> GetTextResponseAsync(string prompt, string systemPrompt);
    }
}
