using MongoDB.Driver;

namespace tuskar.statisticApp.Services.DataBase;

public class MainDbProvider(MongoDbClient client)
{
    private IMongoDatabase Database { get; } = client.GetClient().GetDatabase("cs-statistic");
    
    private IMongoCollection<Models.MongoDB.User> Users => Database.GetCollection<Models.MongoDB.User>("users");
    
    public async Task<Models.MongoDB.User?> GetUserByChatId(long chatId)
    {
        var findTask = Users.Find(user => user.chatId == chatId).ToListAsync()
            .ContinueWith(task => task.Result.FirstOrDefault());
        
        try { return await findTask; } catch { return null; }
    }

    public async Task AddUser(Models.MongoDB.User user)
    {
        await Users.ReplaceOneAsync(_ => true, user, new ReplaceOptions { IsUpsert = true });
    }
}