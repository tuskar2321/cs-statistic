using System.Net;
using tuskar.statisticApp.Configs;

namespace tuskar.statisticApp;

internal static class Program
{
    private static async Task Main()
    {
        var builder = WebApplication.CreateBuilder();
        var services = builder.Services;
        var configuration = builder.Configuration;

        builder.Configuration.ConfigureServices(services);
        builder.Services.RegisterWeb();
        builder.Services.RegisterDb();
        builder.Services.RegisterBot();

        builder.WebHost.ConfigureKestrel(options =>
        {
            var config = configuration.GetSection("Web:Kestrel").Get<Web.KestrelConfig>() ??
                         throw new Exception("Invalid Kestrel configuration");
            var host = IPAddress.Parse(config.Host);
            
            options.Listen(host, config.Port);
        });

        var app = builder.Build();
        app.UsePathBase("/api");
        app.MapControllers();
        await app.InitServices();
        await app.RunAsync();
    }
}