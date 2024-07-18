namespace FWDays2024.JsInterop.Abstractions;

public interface IBaseViewModel
{
    public event EventHandler StateHasChanged;
}

public class BaseViewModel : IBaseViewModel
{
    public  BaseViewModel()
    {
    }
    
    protected void NotifyStateChanged() => StateHasChanged?.Invoke(this, EventArgs.Empty);

    public event EventHandler? StateHasChanged;
}
