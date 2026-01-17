namespace OrderService.Services
{
    public class ExternalServicesSettings
    {
        public string CartServiceBaseUrl { get; set; } = string.Empty;
        public string InventoryServiceBaseUrl { get; set; } = string.Empty;
        public string ProfileServiceBaseUrl { get; set; } = string.Empty;
        public int TimeoutInSeconds { get; set; } = 30;
        public int RetryCount { get; set; } = 3;
        public int InventoryLockTimeoutInSeconds { get; set; } = 30;
    }
}
