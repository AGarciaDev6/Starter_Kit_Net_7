namespace Starter_NET_7.AppSettings
{
    public class ConfigApp
    {
        private readonly IConfiguration _config;


        // Format
        public string DateFormar { get; }

        /**
         * Auth
         */

        // In hours
        public int ExpireToken { get; }
        // In Days
        public int ExpireRefreshToken { get; }
        // Hours
        public int ExpireTokenForgot { get; }

        public ConfigApp(IConfiguration config)
        {
            _config = config;

            ExpireToken = 8;
            ExpireRefreshToken = 7;
            ExpireTokenForgot = 2;
            DateFormar = "dd/MM/yyyy h:m:s";
        }

        
    }
}
