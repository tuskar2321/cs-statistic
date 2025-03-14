using MongoDB.Driver;

namespace tuskar.statisticApp.Services.DataBase;

public class MongoDbClient(Configs.Db.MongoDbConfig config)
{
    private readonly MongoClient _client = new(config.ConnectionUrl);
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    
    public MongoClient GetClient() { return _client; }

    public Task Start()
    {
        return Task.Run(() => _client.StartSession(), _cancellationTokenSource.Token);
    }

    public Task Stop()
    {
        return Task.Run(() => _cancellationTokenSource.Cancel());
    }
}