using System.Collections;

namespace tuskar.statisticApp.Services.Scenario;

public class Scenario
{
    private string ChatId { get; init; }

    private readonly ArrayList _steps;
    
    private ScenarioStatus Status { get; set; }
    
    private Step CurrentStep { get; set; }
    
    public Step GetCurrentStep() => CurrentStep;

    public Scenario(string Id, ArrayList steps)
    {
        ChatId = Id;
        _steps = steps;
    }

    public async Task<Scenario> SetStatus(ScenarioStatus status)
    {
        //mongoDB save
        await Task.Delay(1000);
        Status = status;
        return this;
    }

    // start new Scenario
    public async Task Execute()
    {
        //mongoDB insert new scenario
        // scenarioColl.updateMany(chatId: ChatId, {$set: {status: Skipped}})
        await Task.Delay(1000);
        
    }
}

public enum ScenarioStatus
{
    New,
    Finished,
    Skipped
}