namespace ahydrax.Servitor
{
    public class Settings
    {
        public TelegramSettings Telegram { get; set; }
        public Socks5Settings Socks5 { get; set; }
        public WebServerSettings WebServer { get; set; }
        public TeamspeakSettings Teamspeak { get; set; }
    }

    public class TelegramSettings
    {
        public string BotApiKey { get; set; }
        public long HostGroupId { get; set; }
    }

    public class Socks5Settings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class WebServerSettings
    {
        public string IpAddress { get; set; }
        public int Port { get; set; } = 8088;
        public string AdminUsername { get; set; }
        public string AdminPassword { get; set; }
    }

    public class TeamspeakSettings
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Key { get; set; }
        public string Password { get; set; }
    }
}
