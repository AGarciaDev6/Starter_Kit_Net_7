namespace Starter_NET_7.Config
{
    public class EmailSmtpSettings
    {
        public string From { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Host { get; set; } = null!;
        public string Username { get; set; } = null!;
        public int Port { get; set; }
        public bool UseSSL { get; set; }
    }
}
