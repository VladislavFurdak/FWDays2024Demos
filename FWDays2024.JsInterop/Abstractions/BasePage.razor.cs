using Microsoft.AspNetCore.Components;

namespace FWDays2024.JsInterop.Abstractions;

public abstract partial class BasePage<TViewModel> : ComponentBase where TViewModel : IBaseViewModel
{
    [Inject] public TViewModel ViewModel { get; set; } = default!;
    protected override void OnInitialized()
    {
        base.OnInitialized();
        ViewModel.StateHasChanged += (_, _) => StateHasChanged();
    }
}