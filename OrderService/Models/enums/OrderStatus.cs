namespace OrderService.Models.enums
{
    public enum OrderStatus
    {
        Pending = 1,          
        Confirmed = 2,        
        Processing = 3,       
        ReadyForDelivery = 4, 
        OutForDelivery = 5,   
        Delivered = 6,        
        Cancelled = 7,        
        Failed = 8            
    }

}
