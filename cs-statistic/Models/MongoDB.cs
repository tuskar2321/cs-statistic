using MongoDB.Bson.Serialization.Attributes;
using tuskar.statisticApp.Services.Scenario;

namespace tuskar.statisticApp.Models;

public static class MongoDB
{
    public record User(DateTime registerT, long chatId, string nickName)
    {
        public string nickName { get; set; } = nickName;
        public long chatId { get; set; } = chatId;
        public DateTime registerT { get; set; } = registerT;
    }

    public record ScenarioAction(
        ActionType Type,
        Dictionary<string, string>? Parameters
    );

    public record ScenarioSchema
    {
        [BsonId] public ScenarioTitle Title { get; init; }
        public ScenarioAction[] Actions { get; init; }
    }
}