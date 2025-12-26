using Microsoft.EntityFrameworkCore;
using OrderService.Models;
using OrderService.Models.enums;

namespace OrderService.Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _context;
        private readonly ILogger<OrderRepository> _logger;

        public OrderRepository(OrderDbContext context, ILogger<OrderRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

     
        public async Task<Order?> GetByIdAsync(int orderId, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Payment)
                .Include(o => o.Delivery)
                .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        }

        public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Payment)
                .Include(o => o.Delivery)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
        }

        public async Task<Order?> GetOrderWithDetailsAsync(int orderId, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Payment)
                .Include(o => o.Delivery)
                .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        }

        public async Task<Order?> GetOrderWithDetailsAsync(string orderNumber, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Payment)
                .Include(o => o.Delivery)
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(Guid userId, bool? activeOnly = null, CancellationToken cancellationToken = default)
        {
            var query = _context.Orders
                .Include(o => o.Items)
                .Include(o => o.Payment)
                .Include(o => o.Delivery)
                .Where(o => o.UserId == userId);

            if (activeOnly.HasValue)
            {
                if (activeOnly.Value)
                {
                    query = query.Where(o => o.Status != OrderStatus.Delivered && o.Status != OrderStatus.Cancelled);
                }
                else
                {
                    query = query.Where(o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Cancelled);
                }
            }

            return await query
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Order?> GetActiveCartOrderAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == OrderStatus.Pending, cancellationToken);
        }

        public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
        {
            await _context.Orders.AddAsync(order, cancellationToken);
        }

        public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
        {
            _context.Orders.Update(order);
            await Task.CompletedTask;
        }
   

      
        public async Task<Delivery?> GetDeliveryByOrderIdAsync(int orderId, CancellationToken cancellationToken = default)
        {
            return await _context.Deliveries
                .FirstOrDefaultAsync(d => d.OrderId == orderId, cancellationToken);
        }

        public async Task UpdateDeliveryAsync(Delivery delivery, CancellationToken cancellationToken = default)
        {
            _context.Deliveries.Update(delivery);
            await Task.CompletedTask;
        }
    

 
        public async Task<Payment?> GetPaymentByOrderIdAsync(int orderId, CancellationToken cancellationToken = default)
        {
            return await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);
        }

        public async Task AddPaymentAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            await _context.Payments.AddAsync(payment, cancellationToken);
        }

        public async Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            _context.Payments.Update(payment);
            await Task.CompletedTask;
        }



        public async Task AddRatingAsync(Rating rating, CancellationToken cancellationToken = default)
        {
            await _context.Ratings.AddAsync(rating, cancellationToken);
        }

        public async Task<bool> HasUserRatedOrderItemAsync(Guid userId, int orderItemId, CancellationToken cancellationToken = default)
        {
            return await _context.Ratings
                .AnyAsync(r => r.UserId == userId && r.OrderItemId == orderItemId, cancellationToken);
        }

        public async Task<IEnumerable<Rating>> GetProductRatingsAsync(int productId, CancellationToken cancellationToken = default)
        {
            return await _context.Ratings
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.RatedAt)
                .ToListAsync(cancellationToken);
        }
 
        public async Task<int> CountUserOrdersAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .CountAsync(o => o.UserId == userId, cancellationToken);
        }

        public async Task<decimal> GetUserTotalSpentAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var total = await _context.Orders
                .Where(o => o.UserId == userId && o.PaymentStatus == PaymentStatus.Completed)
                .SumAsync(o => o.Total, cancellationToken);

            return total;
        }


        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _context.SaveChangesAsync(cancellationToken);
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save changes to database");
                throw;
            }
        }


        public async Task<int> SaveChangesWithIncludesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
             
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Failed to save changes with includes");

            
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, "Inner exception details");
                }

            
                if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx)
                {
                    switch (sqlEx.Number)
                    {
                        case 2627: 
                            throw new InvalidOperationException("Duplicate entry found. Please check your data.");
                        case 547:  
                            throw new InvalidOperationException("Foreign key constraint violation. Check related entities.");
                        default:
                            throw;
                    }
                }

                throw;
            }
        }

        public async Task AddRangeAsync(IEnumerable<Order> orders, CancellationToken cancellationToken = default)
        {
            await _context.Orders.AddRangeAsync(orders, cancellationToken);
        }

        public async Task UpdateRangeAsync(IEnumerable<Order> orders, CancellationToken cancellationToken = default)
        {
            _context.Orders.UpdateRange(orders);
            await Task.CompletedTask;
        }
    }
}
