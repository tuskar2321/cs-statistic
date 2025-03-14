namespace tuskar.statisticApp.Services.Scenario;

public class Action
{
    private StepStatus Status { get; set; } = StepStatus.Waiting;
    private ActionType Type { get; set; }
    private Guid ScenarioId { get; set; }
    public Dictionary<string, string> Parameters { get; private set; }
    
    public new ActionType GetType() => Type;
    public StepStatus GetStatus() => Status;

    public Action(Models.MongoDB.ScenarioAction scheme, Guid scenarioId)
    {
        Type = scheme.Type;
        Parameters = scheme.Parameters ?? new Dictionary<string, string>();
        ScenarioId = scenarioId;
    }
    
    public async Task<Action> SetStatus(StepStatus status, Dictionary<string, string>? parameters)
    {
        Status = status;
        if (parameters != null) Parameters = parameters;
        //mongoDb status update
        await Task.Delay(1000);
        return this;
    }
    
    public async Task Execute(ScenarioExecutor executor, Dictionary<string, string>? parameters = null)
    {
        await SetStatus(StepStatus.InProgress, parameters);
        await executor.ExecuteAction(this);
    }
}

public enum StepStatus
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
    EditMessage
}