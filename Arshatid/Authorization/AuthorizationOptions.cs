namespace Arshatid.Authorization
{
    public class AuthorizationOptions
    {
        public int CacheTimeoutMinutes { get; set; } = 60;
        public List<string> SecurityGroups { get; set; } = new();
        public List<string> DevelopmentUsers { get; set; } = new();
    }
}
