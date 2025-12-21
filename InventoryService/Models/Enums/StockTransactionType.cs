namespace InventoryService.Models.Enums
{
    public enum StockTransactionType
    {
        InitialStock = 1,
        StockAdded = 2,
        StockReserved = 3,
        StockReservationCancelled = 4,
        StockSold = 5,
        StockAdjusted = 6,
        StockReturned = 7
    }
}
