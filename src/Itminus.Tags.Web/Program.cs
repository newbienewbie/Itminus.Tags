using Itminus.Tags;
using Itminus.Tags.ModbusTcp;
using Itminus.Tags.Projects;
using Itminus.Tags.S7;
using Itminus.Tags.Web;
using Itminus.Tags.Web.Components;
using Itminus.Tags.Web.Tags;
using Itminus.Tags.ZLan;
using MudBlazor.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(lb =>{
    lb.AddConsole();
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddTagsProjectServices(b =>
{
    b.Services.AddKeyedSingleton<IChannelFactory, S7TagChannelFactory>("S7");
    b.Services.AddKeyedSingleton<IChannelFactory, ModbusTcpChannelFactory>("ModbusTcp");
    b.Services.AddKeyedSingleton<IChannelFactory, ZLanTcpChannelFactory>("ZLanTcp");

    b.ConfigChannelsFactory((sp, factory) => {
        factory.AddFactory(sp.GetRequiredKeyedService<IChannelFactory>("S7"));
        factory.AddFactory(sp.GetRequiredKeyedService<IChannelFactory>("ModbusTcp"));
        factory.AddFactory(sp.GetRequiredKeyedService<IChannelFactory>("ZLanTcp"));
    });

    b.ConfigTagsLoader((sp, loader) => {
        loader.AddTagsCbntBuilder<S7TagCbntBuilder>(sp, "S7");
        loader.AddTagsCbntBuilder<ModbusTcpTagCbntBuilder>(sp, "ModbusTcp");
        loader.AddTagsCbntBuilder<ZLanDICbntBuilder>(sp, "ZLanTcp", (cbntBuilder) => cbntBuilder.Area == "DI");
        loader.AddTagsCbntBuilder<ZLanDOCbntBuilder>(sp, "ZLanTcp", (cbntBuilder) => cbntBuilder.Area == "DO");
    });
});


builder.Services.AddSingleton(rootsp => {
    var ss = rootsp.CreateScope();
    var sp = ss.ServiceProvider;
    var logger = sp.GetRequiredService<ILogger<ITagsProject>>();
    var factory = sp.GetRequiredService<ITagsProjectFactory>();

    var loc = Assembly.GetExecutingAssembly().Location;
    var dir = Path.GetDirectoryName(loc);
    var proj = factory.Create(dir!);
    proj.TurnStarted += (grp, ch) => {
        return Task.CompletedTask;
    };
    proj.TurnCrashed +=  (grp, ch, ex) => {
        logger.LogError("{grp}: {ex}", grp.Name, ex.Message);
        return Task.CompletedTask;
    };

    proj.Logicets.Clear();
    proj.TryAddLogicet<HandleSnap11>();
    proj.TryAddLogicet<HandleSnap12>();
    proj.TryAddLogicet<HandleSnap13>();
    return proj;
});
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
