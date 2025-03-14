namespace tuskar.statisticApp.Models;

public static class MongoDB
{
    public record User
    {
        public User(DateTime registerT, long chatId, string nickName)
        {
            this.registerT = registerT;
            this.chatId = chatId;
            this.nickName = nickName;
        }

        public string nickName { get; set; }
        public long chatId { get; set; }
        public DateTime registerT { get; set; }
    };
}