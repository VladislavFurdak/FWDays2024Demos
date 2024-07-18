using BlazorNexsus.Navigation;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using FWDays2024.JsInterop;
using FWDays2024.JsInterop.Services;
using FWDays2024.JsInterop.ViewModels;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient {BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)});

builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IChatViewModel, ChatViewModel>();

builder.Services.AddBlazorNexusNavigation<Navigation>();

await builder.Build().RunAsync();