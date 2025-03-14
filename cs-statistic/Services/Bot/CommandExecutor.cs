using Telegram.Bot;

namespace tuskar.statisticApp.Services;

public static class CommandExecutor
{
    public static async Task Me(DataBase.MainDbProvider mainDbProviderDbProvider, ITelegramBotClient tgClient,
        long chatId)
    {
        var existingUser = await mainDbProviderDbProvider.GetUserByChatId(chatId);
        if (existingUser != null)
        {
            await tgClient.SendMessage(
                chatId,
                "Ваш никнейм - " + existingUser.nickName
            );
        }
        else
        {
            await tgClient.SendMessage(
                chatId,
                "Такой юзер отсутствует"
            );
        }
    }
}