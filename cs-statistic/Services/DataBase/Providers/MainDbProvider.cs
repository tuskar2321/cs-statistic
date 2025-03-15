using MongoDB.Driver;
using tuskar.statisticApp.Services.Scenario;

namespace tuskar.statisticApp.Services.DataBase;

public class MainDbProvider(MongoDbClient client)
{
    private IMongoDatabase Database { get; } = client.GetClient().GetDatabase("cs-statistic");

    private IMongoCollection<Models.MongoDB.User> Users => Database.GetCollection<Models.MongoDB.User>("users");

    private IMongoCollection<Services.Scenario.Scenario> Scenarios =>
        Database.GetCollection<Services.Scenario.Scenario>("scenarios");

    private IMongoCollection<Models.MongoDB.ScenarioSchema> ScenarioSchemas =>
        Database.GetCollection<Models.MongoDB.ScenarioSchema>("scenario_schemas");

    public async Task ReplaceScenario(Scenario.Scenario scenario)
    {
        await Scenarios
            .ReplaceOneAsync(
                doc => doc.ChatId == scenario.ChatId && doc.Status == ScenarioStatus.Current,
                scenario,
                new ReplaceOptions { IsUpsert = true }
            );
    }

    public async Task UpdateScenarioStatus(Guid scenarioId, ScenarioStatus newStatus)
    {
        var filter = Builders<Scenario.Scenario>.Filter.Eq(sc => sc.Id, scenarioId);
        var update = Builders<Scenario.Scenario>.Update.Set(sc => sc.Status, newStatus);
        await Scenarios
            .UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true });
    }

    public async Task SkipAllScenarios(long chatId)
    {
        var filterBuilder = Builders<Scenario.Scenario>.Filter;
        var filters = filterBuilder.And(
            filterBuilder.Eq(sc => sc.ChatId, chatId),
            filterBuilder.Eq(sc => sc.Status, ScenarioStatus.Current)
        );
        var update = Builders<Scenario.Scenario>.Update.Set(sc => sc.Status, ScenarioStatus.Skipped);
        await Scenarios
            .UpdateManyAsync(filters, update);
    }

    public async Task<Services.Scenario.Scenario?> GetScenarioByChatId(long chatId)
    {
        return await Scenarios
            .Find(scenario => scenario.ChatId == chatId && scenario.Status == ScenarioStatus.Current)
            .ToListAsync()
            .ContinueWith(task => task.Result.FirstOrDefault());
    }

    public async Task<Models.MongoDB.ScenarioSchema?> GetSchemaByTitle(ScenarioTitle title)
    {
        return await ScenarioSchemas
            .Find(schema => schema.Title == title)
            .ToListAsync()
            .ContinueWith(task => task.Result.FirstOrDefault());
    }

    public async Task<ReplaceOneResult> ReplaceSchema(Models.MongoDB.ScenarioSchema schema)
    {
        return await ScenarioSchemas.ReplaceOneAsync(sc => sc.Title == schema.Title, schema,
            new ReplaceOptions { IsUpsert = true });
    }

    public async Task UpdateActionStatus(Guid scenarioId, Guid actionId, ActionStatus newStatus)
    {
        var filterBuilder = Builders<Scenario.Scenario>.Filter;
        var filters = filterBuilder.And(
            filterBuilder.Eq(sc => sc.Id, scenarioId),
            filterBuilder.ElemMatch(sc => sc.Actions, act => act.Id == actionId)
        );
        var update = Builders<Scenario.Scenario>.Update.Set("Actions.$.Status", newStatus);
        await Scenarios.UpdateOneAsync(filters, update);
    }

    public async Task<Models.MongoDB.User?> GetUserByChatId(long chatId)
    {
        return await Users
            .Find(user => user.chatId == chatId)
            .ToListAsync()
            .ContinueWith(task => task.Result.FirstOrDefault());
    }

    public async Task AddUser(Models.MongoDB.User user)
    {
        await Users.ReplaceOneAsync(_ => true, user, new ReplaceOptions { IsUpsert = true });
    }
}