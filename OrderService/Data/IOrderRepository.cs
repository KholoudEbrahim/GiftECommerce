using OrderService.Models;
using OrderService.Models.enums;

namespace OrderService.Data
{
    public interface IOrderRepository
    {
        // Order operations
        Task<Order?> GetByIdAsync(int orderId, CancellationToken cancellationToken = default);
        Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
        Task<Order?> GetOrderWithDetailsAsync(int orderId, CancellationToken cancellationToken = default);
        Task<Order?> GetOrderWithDetailsAsync(string orderNumber, CancellationToken cancellationToken = default);
        Task<IEnumerable<Order>> GetUserOrdersAsync(Guid userId, bool? activeOnly = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);
        Task<Order?> GetActiveCartOrderAsync(Guid userId, CancellationToken cancellationToken = default);

        Task AddAsync(Order order, CancellationToken cancellationToken = default);
        Task UpdateAsync(Order order, CancellationToken cancellationToken = default);

        // Delivery operations
        Task<Delivery?> GetDeliveryByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);
        Task UpdateDeliveryAsync(Delivery delivery, CancellationToken cancellationToken = default);

        // Payment operations
        Task<Payment?> GetPaymentByOrderIdAsync(int orderId, CancellationToken cancellationToken = default);
        Task AddPaymentAsync(Payment payment, CancellationToken cancellationToken = default);
        Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken = default);

        // Rating operations
        Task AddRatingAsync(Rating rating, CancellationToken cancellationToken = default);
        Task<bool> HasUserRatedOrderItemAsync(Guid userId, int orderItemId, CancellationToken cancellationToken = default);
        Task<OrderItem?> GetOrderItemWithOrderAsync(int orderItemId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Rating>> GetProductRatingsAsync(int productId, CancellationToken cancellationToken = default);
        Task UpdateOrderItemAsync(OrderItem orderItem, CancellationToken cancellationToken = default);

        // Statistics
        Task<int> CountUserOrdersAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<decimal> GetUserTotalSpentAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<int> SaveChangesWithIncludesAsync(CancellationToken cancellationToken = default); 

        // Batch operations
        Task AddRangeAsync(IEnumerable<Order> orders, CancellationToken cancellationToken = default);
        Task UpdateRangeAsync(IEnumerable<Order> orders, CancellationToken cancellationToken = default);
    }
}

