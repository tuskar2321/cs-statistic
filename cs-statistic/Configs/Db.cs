
namespace tuskar.statisticApp.Configs;

public static class Db
{
    public record MongoDbConfig
    {
        public required string ConnectionUrl { get; init; }
    }
}