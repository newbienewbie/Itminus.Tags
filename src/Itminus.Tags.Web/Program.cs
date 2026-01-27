using Itminus.Tags;
using Itminus.Tags.ModbusTcp;
using Itminus.Tags.Projects;
using Itminus.Tags.S7;
using Itminus.Tags.Web;
using Itminus.Tags.Web.Components;
using Itminus.Tags.Web.Tags;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(lb =>{
    lb.AddConsole();
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddTagsProjectServices(b =>
{
    b.Services.AddKeyedSingleton<ITagChannelFactory, S7TagChannelFactory>("S7");
    b.Services.AddKeyedSingleton<ITagChannelFactory, ModbusTcpChannelFactory>("ModbusTcp");

    b.ConfigChannelsFactory((sp, factory) => {
        factory.AddFactory(sp.GetRequiredKeyedService<ITagChannelFactory>("S7"));
        factory.AddFactory(sp.GetRequiredKeyedService<ITagChannelFactory>("ModbusTcp"));
    });

    b.ConfigTagsLoader((sp, loader) => {
        loader.AddTagsCbntBuilder<S7TagCbntBuilder>(sp, "S7");
        loader.AddTagsCbntBuilder<ModbusTcpTagCbntBuilder>(sp, "ModbusTcp");
    });
});


builder.Services.AddSingleton<S7TagRuntime_Rx>();
builder.Services.AddHostedService<S7BackgroundService>();
builder.Services.AddMudServices();



var app = builder.Build();



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
