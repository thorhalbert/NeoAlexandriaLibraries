using BlazorVideoStreaming;
using Microsoft.Fast.Components.FluentUI;
using MudBlazor.Services;
using NeoCMS.Client.Pages;
using NeoCMS.Components;

namespace NeoCMS;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddInteractiveWebAssemblyComponents();

        builder.Services.AddFluentUIComponents();
        builder.Services.AddMudServices();
        builder.Services.AddSignalR();
        //builder.Services.AddHostedService<VideoStreamingService>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles();
        app.UseAntiforgery();

    


        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(Counter).Assembly);

        //app.MapHub<VideoStreamingHub>("/videostream");
       
        app.Run();
    }
}

