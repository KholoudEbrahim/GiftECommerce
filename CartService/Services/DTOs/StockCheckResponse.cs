namespace CartService.Services.DTOs
{
    public class StockCheckResponse
    {
        public bool IsAvailable { get; set; }
        public int AvailableQuantity { get; set; }
        public int CurrentStock { get; set; }
    }
}
