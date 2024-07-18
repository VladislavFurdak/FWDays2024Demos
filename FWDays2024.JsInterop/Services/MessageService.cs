using System.Net.Http.Json;

namespace FWDays2024.JsInterop.Services;

public record ChatMessageRecord(
    string FullName,
    string DateTime,
    string Picture,
    string Content,
    bool IsMyMessage,
    Guid Id);

public interface IMessageService
{
    Task<IEnumerable<ChatMessageRecord>> LoadNextBunch(int skip, int take);
}

public class MessageService : IMessageService
{
    private readonly HttpClient _httpClient;

    public MessageService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<ChatMessageRecord>> LoadNextBunch(int skip, int take)
    {
        await Task.Delay(700);

        var result = (await _httpClient.GetFromJsonAsync<ChatMessageRecord[]>("sample-data/chat.json"))
            .Reverse()
            .Skip(skip).Take(take)
            .Select(x => x with {Id = Guid.NewGuid()})
            .Reverse()
            .ToList();

        return result;
    }
}