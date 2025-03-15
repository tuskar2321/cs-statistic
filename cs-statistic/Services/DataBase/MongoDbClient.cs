using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using tuskar.statisticApp.Services.Scenario;

namespace tuskar.statisticApp.Services.DataBase;

public class MongoDbClient(Configs.Db.MongoDbConfig config)
{
    private readonly MongoClient _client = new(config.ConnectionUrl);
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public MongoClient GetClient()
    {
        return _client;
    }

    public Task Start()
    {
        BsonSerializer.RegisterSerializer(new EnumSerializer<ScenarioStatus>(BsonType.String));
        BsonSerializer.RegisterSerializer(new EnumSerializer<ScenarioTitle>(BsonType.String));
        BsonSerializer.RegisterSerializer(new EnumSerializer<ActionStatus>(BsonType.String));
        BsonSerializer.RegisterSerializer(new EnumSerializer<ActionType>(BsonType.String));
        return Task.Run(() => _client.StartSession(), _cancellationTokenSource.Token);
    }

    public Task InitDefaultSchemas()
    {
        Models.MongoDB.ScenarioSchema[] startSchemas =
        [
            new()
            {
                Title = ScenarioTitle.Start,
                Actions =
                [
                    new Models.MongoDB.ScenarioAction(ActionType.SendMessage,
                        new Dictionary<string, string> { { "messageText", "Приветствуем Вас в боте." } })
                ]
            },
            new()
            {
                Title = ScenarioTitle.StartWithAwait,
                Actions =
                [
                    new Models.MongoDB.ScenarioAction(ActionType.SendMessageThenWait,
                        new Dictionary<string, string> { { "messageText", "Приветствуем Вас в боте." } })
                ]
            }
        ];
        var replaceTasks = startSchemas.Select(sc => new MainDbProvider(this).ReplaceSchema(sc));
        Task.WhenAll(replaceTasks).Wait();
        return Task.CompletedTask;
    }

    public Task Stop()
    {
        return Task.Run(() => _cancellationTokenSource.Cancel());
    }
}