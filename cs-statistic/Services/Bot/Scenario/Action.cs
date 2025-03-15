using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using tuskar.statisticApp.Services.DataBase;

namespace tuskar.statisticApp.Services.Scenario;

public class Action
{
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; } = Guid.NewGuid();
    private ActionStatus Status { get; set; } = ActionStatus.Waiting;
    [BsonRepresentation(BsonType.String)]
    private ActionType Type { get; set; }
    [BsonIgnore]
    public Guid ScenarioId { get; set; }
    public Dictionary<string, string> Parameters { get; private set; }

    public new ActionType GetType() => Type;
    public ActionStatus GetStatus() => Status;

    // ReSharper disable once ConvertToPrimaryConstructor
    public Action(Models.MongoDB.ScenarioAction scheme, Guid scenarioId)
    {
        Type = scheme.Type;
        Parameters = scheme.Parameters ?? new Dictionary<string, string>();
        ScenarioId = scenarioId;
    }

    public async Task SetStatus(
        ActionStatus status,
        Dictionary<string, string>? parameters,
        MainDbProvider provider
    )
    {
        Status = status;
        if (parameters != null) Parameters = parameters;
        await provider.UpdateActionStatus(ScenarioId, Id, status);
    }

    public async Task Execute(
        ScenarioExecutor executor,
        long chatId,
        Dictionary<string, string>? parameters = null,
        bool isLast = false
    )
    {
        await SetStatus(ActionStatus.InProgress, parameters, executor.GetProvider);
        await executor.ExecuteAction(this, chatId, isLast);
    }
}

public enum ActionStatus
{
    Waiting,
    Success,
    InProgress,
    UserAwait,
    Failure
}

public enum ActionType
{
    SendMessage,
    SendMessageThenWait,
    EditMessage
}