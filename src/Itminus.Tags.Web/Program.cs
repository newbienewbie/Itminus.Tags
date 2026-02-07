using Itminus.Tags;
using Itminus.Tags.S7;
using Itminus.Tags.ZLan;
using Itminus.Tags.ModbusTcp;
using Itminus.Tags.OpcUaClient;

using Itminus.Tags.Web;
using Itminus.Tags.Web.Components;
using Itminus.Tags.Web.Tags;

using MudBlazor.Services;
using System.Reflection;
using Itminus.Tags.ComScanner;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(lb =>{
    lb.AddConsole();
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddTags();

builder.Services.AddMudServices();



var app = builder.Build();


_ = Task.Run(() =>
{
    var projctrl = app.Services.GetRequiredService<TagsProjectCtrl>();
    _ = projctrl.StartPoll(dir: null);
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
