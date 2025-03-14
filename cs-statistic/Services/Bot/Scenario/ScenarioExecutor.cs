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
        bool isNewScenario = true
    )
    {
        try
        {
            Scenario scenario;
            if (isNewScenario)
            {
                // scenarioColl.updateMany(chatId: ChatId, {$set: {status: Skipped}})
                await Task.Delay(1000);
                var schema = await mainDbProviderDbProvider.GetSchemaByTitle(title) ??
                             throw new NullReferenceException();
                scenario = new Scenario(chatId, schema);
                // scenario save
            }
            else
            {
                scenario = await mainDbProviderDbProvider.GetScenarioByChatId(chatId) ??
                           throw new NullReferenceException();
            }
            await scenario.Execute(this);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await tgClient
                .SendMessage(
                    new ChatId(chatId),
                    "Произошла непредвиденная ошибка, пожалуйста, перейдите к начальному списку команд."
                );
        }
        finally
        {
            Console.WriteLine("Критическая ошибка, невозможно отправить сообщение об ошибке в чат " + chatId);
        }
    }

    public async Task ExecuteAction(Action action)
    {
        // ???
    }
}