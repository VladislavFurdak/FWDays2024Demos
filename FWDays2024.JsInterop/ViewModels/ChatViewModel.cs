using System.Globalization;
using FWDays2024.JsInterop.Services;
using Microsoft.JSInterop;

namespace FWDays2024.JsInterop.ViewModels;

public interface IChatViewModel : IBaseViewModel
{
    int CurrentPage { get; }
    IEnumerable<ChatMessageRecord> Messages { get; }
    bool IsLoading { get; }
    Guid? PositionOfDividerMessageId { get; }
    Task LoadPreviousMessages();
    Task Initialize();
}

public class ChatViewModel : BaseViewModel, IChatViewModel
{
    private const int BunchSize = 20;
    private readonly IJSRuntime _jsRuntime;

    private readonly IMessageService _messageService;

    private bool _firstLoad = true;

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
        if (_firstLoad)
        {
            await _jsRuntime.InvokeVoidAsync("BrowserHelpers.Initialize", DotNetObjectReference.Create(this));
            await _jsRuntime.InvokeVoidAsync("BrowserHelpers.InitScrollWatch");

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

        _firstLoad = false;
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

    private IEnumerable<Guid>? GetLastNewMessages()
    {
        return Messages?
            .OrderByDescending(x =>
                DateTime.ParseExact(x.DateTime, "dd MMM h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None))
            .TakeWhile(x => !x.IsMyMessage)
            .Select(x => x.Id)
            .ToList();
    }

    private void SetPositionOfDivider()
    {
        var id = GetLastNewMessages()?.Last();
        PositionOfDividerMessageId = id;
    }

    [JSInvokable]
    public async Task LoadMoreMessages()
    {
        await LoadPreviousMessages();
    }
}