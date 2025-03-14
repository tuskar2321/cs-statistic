namespace tuskar.statisticApp.Configs;

public static class Web
{

    public record KestrelConfig
    {
        public required string Host { get; init; }
        public required int Port { get; init; }
    }

    public record HttpClientConfig
    {
        public required string Protocol { get; init; } = "http://";
        public required string BaseAddress { get; init; }
        public TimeSpan Timeout { get; init; }
        public required Dictionary<string, string> DefaultHeaders { get; init; }
    }

    public record BotConfig
    {
        public required string Token { get; init; }
    }
}