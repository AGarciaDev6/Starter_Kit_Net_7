namespace Starter_NET_7.Config
{
    public class AppSettings
    {
        public string DateFormar { get; }
        public int ExpireToken { get; }
        public int ExpireRefreshToken { get; }
        public int ExpireTokenForgot { get; }
        public string PathViewEmail { get;  }
        public string PathFrontend { get; }

        public AppSettings(IWebHostEnvironment env)
        {
            DateFormar = "dd/MM/yyyy h:m:s";
            ExpireToken = env.IsProduction() ? 2 : 8; // In hours
            ExpireRefreshToken = 7; // In Days
            ExpireTokenForgot = 2; // In Days
            PathViewEmail = env.ContentRootPath + @"\Views\Email";
            PathFrontend = env.IsProduction() ? "" : "http://localhost:4200";
        }
    }
}
