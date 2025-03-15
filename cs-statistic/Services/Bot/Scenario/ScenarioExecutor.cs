using Telegram.Bot;
using Telegram.Bot.Types;
using tuskar.statisticApp.Services.DataBase;

namespace tuskar.statisticApp.Services.Scenario;

public class ScenarioExecutor(MainDbProvider mainDbProviderDbProvider, ITelegramBotClient tgClient)
{
    public MainDbProvider GetProvider => mainDbProviderDbProvider;

    public async Task ExecuteScenario(
        long chatId,
        ScenarioTitle title,
        Update update,
        bool isNewScenario = true
    )
    {
        try
        {
            Scenario scenario;
            if (isNewScenario)
            {
                await mainDbProviderDbProvider.SkipAllScenarios(chatId);
                var schema = await mainDbProviderDbProvider.GetSchemaByTitle(title) ??
                             throw new NullReferenceException();
                scenario = new Scenario(chatId, schema);
                await mainDbProviderDbProvider.ReplaceScenario(scenario);
            }
            else
            {
                scenario = await mainDbProviderDbProvider.GetScenarioByChatId(chatId) ??
                           throw new NullReferenceException();
            }

            await scenario.Execute(this, update);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await tgClient
                .SendMessage(
                    new ChatId(chatId),
                    "Произошла непредвиденная ошибка, пожалуйста, перейдите к начальному списку команд."
                );
            
            //проставить fail сценарию
        }
        finally
        {
            Console.WriteLine("Критическая ошибка, невозможно отправить сообщение об ошибке в чат " + chatId);
        }
    }

    public async Task ExecuteAction(Action action, long chatId, bool isLast)
    {
        try
        {
            switch (action.GetType())
            {
                case ActionType.SendMessage:
                    await tgClient
                        .SendMessage(
                            chatId: new ChatId(chatId),
                            text: action.Parameters["messageText"]
                        );
                    await action.SetStatus(ActionStatus.Success, null, mainDbProviderDbProvider);
                    break;

                case ActionType.SendMessageThenWait:
                    await tgClient
                        .SendMessage(
                            chatId: new ChatId(chatId),
                            text: action.Parameters["messageText"]
                        );
                    await action.SetStatus(ActionStatus.UserAwait, null, mainDbProviderDbProvider);
                    break;

                default:
                    Console.WriteLine(
                        $"[ExecuteAction] scenarioId {action.ScenarioId.ToString()} actionId {action.Id.ToString()} failed by unknown action type {action.GetType()}");
                    break;
            }
            await action.SetStatus(ActionStatus.Success, null, mainDbProviderDbProvider);
            if (isLast)
            {
                await mainDbProviderDbProvider.UpdateScenarioStatus(action.ScenarioId, ScenarioStatus.Finished);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await action.SetStatus(ActionStatus.Failure, null, mainDbProviderDbProvider);
        }
    }
}