namespace tuskar.statisticApp.Services.Scenario;

public class Step(string id)
{
    private string Id { get; init; } = id;

    private StepStatus Status { get; set; } = StepStatus.Waiting;

    public async Task<Step> SetStatus(StepStatus status)
    {
        Status = status;
        //mongoDb status update
        await Task.Delay(1000);
        return this;
    }

    public StepStatus GetStatus() => Status;
    
    public async Task Execute()
    {
        await SetStatus(StepStatus.InProgress);
        //then body
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
