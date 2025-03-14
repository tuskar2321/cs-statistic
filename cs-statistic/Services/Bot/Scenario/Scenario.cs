using MongoDB.Bson.Serialization.Attributes;

namespace tuskar.statisticApp.Services.Scenario;

public class Scenario
{
    [BsonId]
    private Guid Id { get; set; } = Guid.NewGuid();
    private ScenarioTitle Title { get; set; }
    public long ChatId { get; init; }
    private List<Action> Actions { get; init; }
    private ScenarioStatus Status { get; set; }
    
    public Scenario(long chatId, Models.MongoDB.ScenarioSchema schema)
    {
        ChatId = chatId;
        Title = schema.Title;
        Actions = schema.Actions.Select(actionSchema => new Action(actionSchema, Id)).ToList();
        Status = ScenarioStatus.Current;
    }

    public async Task<Scenario> SetStatus(DataBase.MainDbProvider mainDbProviderDbProvider, ScenarioStatus status)
    {
        //mongoDB save
        await Task.Delay(1000);
        Status = status;
        return this;
    }

    // start new Scenario
    public async Task Execute(ScenarioExecutor executor)
    {
        var currentAction = Actions.Find(action => action.GetStatus() == StepStatus.Waiting);
        if (currentAction != null)
        {
            await currentAction.Execute(executor, currentAction.Parameters);
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

//вынести в конфиг
public enum ScenarioTitle
{
    Me
}