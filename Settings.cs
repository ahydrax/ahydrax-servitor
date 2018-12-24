namespace ahydrax.Servitor
{
    public class Settings
    {
        public string BindAddress { get; set; } = "127.0.0.1";
        public int BindPort { get; set; } = 8088;
        public string Username { get; set; }
        public string Password { get; set; }
        public string TelegramBotApiKey { get; set; }
        public long TelegramHostGroupId { get; set; }
        public string TeamspeakHost { get; set; }
        public int TeamspeakPort { get; set; }
        public string TeamspeakUsername { get; set; }
        public string TeamspeakPassword { get; set; }
    }
}
