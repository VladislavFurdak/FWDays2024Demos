using FWDays2024.JsInterop.ViewModels;
using Microsoft.AspNetCore.Components;

public abstract class BasePage<TViewModel> : ComponentBase where TViewModel : IBaseViewModel
{
    [Inject] public TViewModel ViewModel { get; set; } = default!;
    protected override void OnInitialized()
    {
        base.OnInitialized();
        ViewModel.StateHasChanged += (_, _) => StateHasChanged();
    }
}