namespace CartService.Shared
{
    public class ExternalServicesSettings
    {
        public string InventoryServiceBaseUrl { get; set; } = default!;
        public string ProfileServiceBaseUrl { get; set; } = default!;
        public int TimeoutInSeconds { get; set; } = 30;
        public int RetryCount { get; set; } = 3;
    }

}
