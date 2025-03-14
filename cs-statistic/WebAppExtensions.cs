using Microsoft.Extensions.Options;
using MongoDB.Driver;
using tuskar.statisticApp.Configs;
using tuskar.statisticApp.Services;
using tuskar.statisticApp.Services.DataBase;

namespace tuskar.statisticApp;

public static class WebAppExtensions
{
    public static void ConfigureServices(this ConfigurationManager configuration, IServiceCollection services)
    {
        //main configuration file
        var configPath = Path.Combine(Directory.GetCurrentDirectory(), "Configs", "appsettings.json");
        configuration.AddJsonFile(configPath, false, true);

        //configure
        services.Configure<Web.HttpClientConfig>(configuration.GetSection("Web:HttpClient"));
        services.Configure<Web.BotConfig>(configuration.GetSection("Bot"));
        services.Configure<Db.MongoDbConfig>(configuration.GetSection("MongoDB"));
    }

    public static void RegisterWeb(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddControllers();

        //httpClient
        serviceCollection
            .AddHttpClient("HttpClient", (provider, client) =>
            {
                var settings = provider.GetRequiredService<IOptions<Web.HttpClientConfig>>().Value;
                client.BaseAddress = new Uri(settings.Protocol + settings.BaseAddress);

                foreach (var (headerName, headerValue) in settings.DefaultHeaders)
                    client.DefaultRequestHeaders.Add(headerName, headerValue);
            });

        //internal services
        serviceCollection.AddTransient<InternalApiClient>(provider =>
        {
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            return new InternalApiClient(httpClientFactory);
        });
    }

    public static void RegisterDb(this IServiceCollection serviceCollection)
    {
        //client
        serviceCollection.AddSingleton<MongoDbClient>(provider =>
        {
            var config = provider.GetRequiredService<IOptions<Db.MongoDbConfig>>().Value;
            return new MongoDbClient(config);
        });
        
        //providers
        serviceCollection.AddSingleton<MainDbProvider>(provider =>
        {
            var mongoClient = provider.GetRequiredService<MongoDbClient>();
            return new MainDbProvider(mongoClient);
        });
    }

    public static void RegisterBot(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<Bot>(provider => new Bot(provider));
    }

    public static async Task InitServices(this WebApplication app)
    {
        Task[] initTasks =
        [
            app.Services.GetRequiredService<Bot>().Start(),
            app.Services.GetRequiredService<MongoDbClient>().Start()
        ];

        await Task.WhenAll(initTasks);
    }
}