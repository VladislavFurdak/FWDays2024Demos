using System.Globalization;
using FWDays2024.JsInterop.Abstractions;
using FWDays2024.JsInterop.Services;
using Microsoft.JSInterop;

namespace FWDays2024.JsInterop.ViewModels;

public interface IChatViewModel : IBaseViewModel
{
    IEnumerable<ChatMessageRecord> Messages { get; }
    bool IsLoading { get; }
    Guid? PositionOfDividerMessageId { get; }
    Task Initialize();
    bool FirstLoad { get; }
    Task ScrollDown();
}

public class ChatViewModel : BaseViewModel, IChatViewModel
{
    private const int BunchSize = 20;
    private readonly IJSRuntime _jsRuntime;
    private readonly IMessageService _messageService;
    public bool FirstLoad { get; private set; } = true;

    public ChatViewModel(IMessageService messageService, IJSRuntime jsRuntime)
    {
        _messageService = messageService;
        _jsRuntime = jsRuntime;
    }

    public int CurrentPage { get; private set; }
    public IEnumerable<ChatMessageRecord> Messages { get; private set; } = new List<ChatMessageRecord>();
    public bool IsLoading { get; private set; }
    public Guid? PositionOfDividerMessageId { get; private set; }
    
    public async Task Initialize()
    {
        await _jsRuntime.InvokeVoidAsync("BrowserHelpers.Initialize", DotNetObjectReference.Create(this));
        await _jsRuntime.InvokeVoidAsync("BrowserHelpers.InitScrollWatch");
        
        if (FirstLoad)
        {
            await LoadPreviousMessages();

            NotifyStateChanged();
            var newMessages = GetLastNewMessages();

            if (newMessages != null)
            {
                var messagesHeight = await _jsRuntime.InvokeAsync<int>("BrowserHelpers.HeightOfNewMessages", newMessages);
                var areaHeight = await _jsRuntime.InvokeAsync<int>("BrowserHelpers.HeightOfChatArea");

                if (messagesHeight > areaHeight)
                {
                    SetPositionOfDivider();
                    NotifyStateChanged();
                    await _jsRuntime.InvokeVoidAsync("BrowserHelpers.ScrollToSeparator");
                }
            }
        }

        FirstLoad = false;
    }
    
    public async Task ScrollDown()
    {
        await _jsRuntime.InvokeVoidAsync("BrowserHelpers.ScrollToBottom");
    }
    
    public async Task LoadPreviousMessages()
    {
        IsLoading = true;
        NotifyStateChanged();
        var result = await _messageService.LoadPrevBunch(BunchSize * CurrentPage, BunchSize);
        Messages = result.Concat(Messages).ToList();
        CurrentPage++;
        IsLoading = false;
        NotifyStateChanged();
    }

    private IEnumerable<Guid> GetLastNewMessages()
    {
        return Messages
            .OrderByDescending(x =>
                DateTime.ParseExact(x.DateTime, "dd MMM h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None))
            .TakeWhile(x => !x.IsMyMessage)
            .Select(x => x.Id)
            .ToList();
    }

    private void SetPositionOfDivider()
    {
        var id = GetLastNewMessages().Last();
        PositionOfDividerMessageId = id;
    }

    [JSInvokable]
    public async Task LoadMoreMessages()
    {
        await LoadPreviousMessages();
    }
}