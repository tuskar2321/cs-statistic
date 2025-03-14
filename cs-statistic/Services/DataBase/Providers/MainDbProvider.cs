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