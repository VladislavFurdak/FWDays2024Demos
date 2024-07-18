using System.Globalization;
using FWDays2024.JsInterop.Services;

namespace FWDays2024.JsInterop.ViewModels;

public interface IChatViewModel : IBaseViewModel
{
    int CurrentPage { get; }
    IEnumerable<ChatMessageRecord> Messages { get; }
    bool IsLoading { get; }
    Guid? PositionOfDividerMessageId { get; }
    Task LoadNextMessages();
    Task Initialize();
}

public class ChatViewModel : BaseViewModel, IChatViewModel
{
    private const int BunchSize = 20;

    private readonly IMessageService _messageService;

    public ChatViewModel(IMessageService messageService)
    {
        _messageService = messageService;
    }

    public int CurrentPage { get; private set; }
    public IEnumerable<ChatMessageRecord> Messages { get; private set; } = new List<ChatMessageRecord>();
    public bool IsLoading { get; private set; }
    public Guid? PositionOfDividerMessageId { get; private set; }
    
    private bool _firstLoad = true;
    
    public async Task Initialize()
    {
        if (_firstLoad)
        {
            await LoadNextMessages();
            SetPositionOfDivider();
            NotifyStateChanged();
            _firstLoad = false;
        }
    }

    public async Task LoadNextMessages()
    {
        IsLoading = true;
        NotifyStateChanged();
        var result = await _messageService.LoadNextBunch(BunchSize * CurrentPage, BunchSize);
        Messages = result.Concat(Messages).ToList();
        CurrentPage++;
        IsLoading = false;
        NotifyStateChanged();
    }

    private void SetPositionOfDivider()
    {
        var id = Messages?
            .OrderByDescending(x =>
                DateTime.ParseExact(x.DateTime, "dd MMM h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None))
            .TakeWhile(x => !x.IsMyMessage)
            .Last().Id;

        PositionOfDividerMessageId = id;
    }
}