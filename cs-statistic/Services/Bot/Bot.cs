using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using tuskar.statisticApp.Configs;

namespace tuskar.statisticApp.Services;

public class Bot
{
    private readonly ITelegramBotClient _botClient;
    private readonly InternalApiClient _internalApiClient;
    private readonly DataBase.MainDbProvider _mainDbProviderDbProvider;

    public Bot(IServiceProvider provider)
    {
        var config = provider.GetRequiredService<IOptions<Web.BotConfig>>().Value;
        _botClient = new TelegramBotClient(config.Token);
        _internalApiClient = provider.GetRequiredService<InternalApiClient>();
        _mainDbProviderDbProvider = provider.GetRequiredService<DataBase.MainDbProvider>();
    }

    public async Task Start()
    {
        BotCommand[] commands =
        [
            new("ping", "Проверка доступности бота"),
            new("me", "Проверка наличия юзера"),
        ];
        
        using var cancellationTokenSource = new CancellationTokenSource();
        
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = []
        };
        
        _botClient.StartReceiving(
            HandleUpdate,
            HandleErrors,
            receiverOptions,
            cancellationTokenSource.Token
        );
        
        await _botClient.SetMyCommands(
            commands: commands,
            scope: new BotCommandScopeAllPrivateChats(),
            cancellationToken: cancellationTokenSource.Token
        );
    }

    private async Task HandleUpdate(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken cancellationToken
    )
    {
        if (update.Type != UpdateType.Message ||
            (update.Message != null && update.Message.Type != MessageType.Text)) return;

        var message = update.Message;
        var chatId = message!.Chat.Id;

        switch (message.Text)
        {
            case "/start":
            {
                await _botClient.SendMessage(
                    chatId,
                    cancellationToken: cancellationToken,
                    text: "Hello World!"
                );
                break;
            }
            case "/ping":
            {
                var pong = await _internalApiClient.Ping();
                await _botClient.SendMessage(
                    chatId,
                    cancellationToken: cancellationToken,
                    text: pong
                );
                break;
            }
            case "/me":
            {
                await CommandExecutor.Me(_mainDbProviderDbProvider, botClient, chatId);
                break;
            }
            default:
            {
                await _botClient.SendMessage(
                    chatId,
                    cancellationToken: cancellationToken,
                    text: "Вы написали: " + message.Text
                );
                break;
            }
        }
    }

    private static Task HandleErrors(
        ITelegramBotClient botClient,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        Console.WriteLine("Telegram Bot Exception: " + exception.Message);
        return Task.CompletedTask;
    }
}