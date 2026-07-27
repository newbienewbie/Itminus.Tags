using Itminus.Tags;
using Itminus.Tags.SimpleFiles;
using Itminus.Tags.BlazorLib;
using NixMonitor.Tags;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAntiforgery();
builder.Services.AddTagsProjectServices(builder =>
{
    builder.AddSimpleFilesSupport();
});
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddTagsBlazorLibCore();

builder.Services.AddHostedService<NixMonitorBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAntiforgery();
app.MapRazorComponents<Itminus.Tags.BlazorLib.Apps.TagsApp>()
    .AddInteractiveServerRenderMode();
app.Run();