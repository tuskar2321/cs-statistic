using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Telegram.Bot.Types;
using tuskar.statisticApp.Services.DataBase;

namespace tuskar.statisticApp.Services.Scenario;

public class Scenario
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; } = Guid.NewGuid();

    private ScenarioTitle Title { get; set; }
    public long ChatId { get; init; }
    public List<Action> Actions { get; init; }
    public ScenarioStatus Status { get; private set; }

    public Scenario(long chatId, Models.MongoDB.ScenarioSchema schema)
    {
        ChatId = chatId;
        Title = schema.Title;
        Actions = schema.Actions.Select(actionSchema => new Action(actionSchema, Id)).ToList();
        Status = ScenarioStatus.Current;
    }

    private async Task SetStatus(MainDbProvider provider, ScenarioStatus status)
    {
        await provider.UpdateScenarioStatus(Id, status);
        Status = status;
    }

    public async Task Execute(ScenarioExecutor executor, Update update)
    {
        var currentAction = Actions.Find(action =>
            new List<ActionStatus> { ActionStatus.Waiting, ActionStatus.UserAwait }.Contains(action.GetStatus()));
        if (currentAction != null)
        {
            switch (currentAction.GetStatus())
            {
                case ActionStatus.Waiting:
                {
                    var isLast = Actions.IndexOf(currentAction).Equals(Actions.Count - 1);
                    await currentAction.Execute(executor, ChatId, currentAction.Parameters, isLast);
                    break;
                }
                case ActionStatus.UserAwait:
                {
                    await currentAction.SetStatus(ActionStatus.Success, null, executor.GetProvider);
                    //TODO: подумать как правильно передавать апдейты в следующий шаг
                    var updateParams = new Dictionary<string, string>
                    {
                        {
                            "userAwaitActionMessage", update.Message?.Text ?? string.Empty
                        }
                    };
                    var nextAction = Actions.ElementAtOrDefault(Actions.IndexOf(currentAction) + 1) ?? throw new Exception("UserAwait action cannot be last action");
                    var mergedParams = nextAction.Parameters.Concat(updateParams).ToDictionary(t => t.Key, t => t.Value);
                    await nextAction.Execute(executor, ChatId, mergedParams, Actions.IndexOf(nextAction).Equals(Actions.Count - 1));
                    break;
                }
            }
        }
        else
        {
            await SetStatus(executor.GetProvider, ScenarioStatus.Finished);
        }
    }
}

public enum ScenarioStatus
{
    Current,
    Finished,
    Skipped
}

//TODO: move to config
public enum ScenarioTitle
{
    Start,
    StartWithAwait,
    Me,
    AddFaceItAccount
}