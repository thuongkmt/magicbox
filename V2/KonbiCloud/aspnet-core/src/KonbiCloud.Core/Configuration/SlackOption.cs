namespace KonbiCloud.Configuration
{
   public class SlackOption
    {
        public string HookUrl { get; set; }
        public string UserName { get; set; }
        //public string ChannelName { get; set; } get from appsetting for each tenant will be different
        public string ServerName { get; set; }
    }
}
