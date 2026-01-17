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
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private IQueryable<Order> GetOrdersWithIncludes()
        {
            return _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(i => i.Ratings)
                .Include(o => o.Payment)
                .Include(o => o.Delivery);
        }


        public async Task<Order?> GetByIdAsync(int orderId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Fetching order with ID: {OrderId}", orderId);

            return await GetOrdersWithIncludes()
                .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        }

        public async Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(orderNumber))
                throw new ArgumentException("Order number cannot be null or empty", nameof(orderNumber));

            _logger.LogDebug("Fetching order with number: {OrderNumber}", orderNumber);

            return await GetOrdersWithIncludes()
                .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
        }


        public async Task<Order?> GetOrderWithDetailsAsync(int orderId, CancellationToken cancellationToken = default)
        {

            return await GetByIdAsync(orderId, cancellationToken);
        }

        public async Task<Order?> GetOrderWithDetailsAsync(string orderNumber, CancellationToken cancellationToken = default)
        {

            return await GetByOrderNumberAsync(orderNumber, cancellationToken);
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(
            Guid userId,
            bool? activeOnly = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Fetching orders for user: {UserId}, ActiveOnly: {ActiveOnly}", userId, activeOnly);

            var query = GetOrdersWithIncludes()
                .Where(o => o.UserId == userId);

            if (activeOnly.HasValue)
            {
                if (activeOnly.Value)
                {
                    query = query.Where(o =>
                        o.Status != OrderStatus.Delivered &&
                        o.Status != OrderStatus.Cancelled &&
                        o.Status != OrderStatus.Failed);
                }
                else
                {
                    query = query.Where(o =>
                        o.Status == OrderStatus.Delivered ||
                        o.Status == OrderStatus.Cancelled ||
                        o.Status == OrderStatus.Failed);
                }
            }

            return await query
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }
        public async Task<(IEnumerable<Order> Orders, int TotalCount)> GetUserOrdersPagedAsync(
    Guid userId,
    bool? activeOnly,
    int page,
    int pageSize,
    CancellationToken cancellationToken = default)
        {
            _logger.LogDebug(
                "Fetching paged orders for user: {UserId}, ActiveOnly: {ActiveOnly}, Page: {Page}, PageSize: {PageSize}",
                userId, activeOnly, page, pageSize);

            var query = GetOrdersWithIncludes()
                .Where(o => o.UserId == userId);

            if (activeOnly.HasValue)
            {
                if (activeOnly.Value)
                {
                    query = query.Where(o =>
                        o.Status != OrderStatus.Delivered &&
                        o.Status != OrderStatus.Cancelled &&
                        o.Status != OrderStatus.Failed);
                }
                else
                {
                    query = query.Where(o =>
                        o.Status == OrderStatus.Delivered ||
                        o.Status == OrderStatus.Cancelled ||
                        o.Status == OrderStatus.Failed);
                }
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (orders, totalCount);
        }
        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(
            OrderStatus status,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Fetching orders with status: {Status}", status);

            return await GetOrdersWithIncludes()
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Order?> GetActiveCartOrderAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await GetOrdersWithIncludes()
                .FirstOrDefaultAsync(
                    o => o.UserId == userId && o.Status == OrderStatus.Pending,
                    cancellationToken);
        }

        public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            _logger.LogDebug("Adding new order: {OrderNumber}", order.OrderNumber);

            await _context.Orders.AddAsync(order, cancellationToken);
        }


        public Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            _logger.LogDebug("Updating order: {OrderNumber}", order.OrderNumber);

            _context.Orders.Update(order);
            return Task.CompletedTask;
        }

        public async Task<Delivery?> GetDeliveryByOrderIdAsync(
            int orderId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Deliveries
                .Include(d => d.Order)
                .FirstOrDefaultAsync(d => d.OrderId == orderId, cancellationToken);
        }

        public Task UpdateDeliveryAsync(Delivery delivery, CancellationToken cancellationToken = default)
        {
            if (delivery == null)
                throw new ArgumentNullException(nameof(delivery));

            _context.Deliveries.Update(delivery);
            return Task.CompletedTask;
        }


        public async Task<Payment?> GetPaymentByOrderIdAsync(
            int orderId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);
        }

        public async Task AddPaymentAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            await _context.Payments.AddAsync(payment, cancellationToken);
        }

        public Task UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken = default)
        {
            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            _context.Payments.Update(payment);
            return Task.CompletedTask;
        }
        public async Task<Order?> GetOrderByPaymentIntentIdAsync(
                 string paymentIntentId,
                        CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(paymentIntentId))
                throw new ArgumentException("Payment intent ID cannot be null or empty", nameof(paymentIntentId));

            _logger.LogDebug("Fetching order with payment intent: {PaymentIntentId}", paymentIntentId);

            return await GetOrdersWithIncludes()
                .FirstOrDefaultAsync(
                    o => o.Payment != null && o.Payment.StripePaymentIntentId == paymentIntentId,
                    cancellationToken);
        }

        public async Task<Order?> GetOrderByChargeIdAsync(
            string chargeId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(chargeId))
                throw new ArgumentException("Charge ID cannot be null or empty", nameof(chargeId));

            _logger.LogDebug("Fetching order with charge: {ChargeId}", chargeId);

            return await GetOrdersWithIncludes()
                .FirstOrDefaultAsync(
                    o => o.Payment != null && o.Payment.TransactionId == chargeId,
                    cancellationToken);
        }

        public async Task AddRatingAsync(Rating rating, CancellationToken cancellationToken = default)
        {
            if (rating == null)
                throw new ArgumentNullException(nameof(rating));

            await _context.Ratings.AddAsync(rating, cancellationToken);
        }

        public async Task<bool> HasUserRatedOrderItemAsync(
            Guid userId,
            int orderItemId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Ratings
                .AnyAsync(r => r.UserId == userId && r.OrderItemId == orderItemId, cancellationToken);
        }

        public async Task<OrderItem?> GetOrderItemWithOrderAsync(
            int orderItemId,
            CancellationToken cancellationToken = default)
        {
            return await _context.OrderItems
                .Include(i => i.Order)
                    .ThenInclude(o => o.Payment)
                .Include(i => i.Ratings)
                .FirstOrDefaultAsync(i => i.Id == orderItemId, cancellationToken);
        }

        public Task UpdateOrderItemAsync(OrderItem orderItem, CancellationToken cancellationToken = default)
        {
            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            _context.OrderItems.Update(orderItem);
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<Rating>> GetProductRatingsAsync(
            int productId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Ratings
                .Include(r => r.OrderItem)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.RatedAt)
                .ToListAsync(cancellationToken);
        }


        public async Task<int> CountUserOrdersAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Orders
                .CountAsync(o => o.UserId == userId, cancellationToken);
        }

        public async Task<decimal> GetUserTotalSpentAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var total = await _context.Orders
                .Where(o => o.UserId == userId && o.PaymentStatus == PaymentStatus.Completed)
                .SumAsync(o => (decimal?)o.Total, cancellationToken);

            return total ?? 0;
        }


        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _context.SaveChangesAsync(cancellationToken);
                return result > 0;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict occurred while saving changes");
                throw new InvalidOperationException("The record you attempted to edit was modified by another user. Please refresh and try again.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error occurred");

                if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx)
                {
                    HandleSqlException(sqlEx);
                }

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

                if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx)
                {
                    HandleSqlException(sqlEx);
                }

                throw;
            }
        }


        private void HandleSqlException(Microsoft.Data.SqlClient.SqlException sqlEx)
        {
            switch (sqlEx.Number)
            {
                case 2627: 
                case 2601:
                    throw new InvalidOperationException("A record with this key already exists.");

                case 547: 
                    throw new InvalidOperationException("This operation violates a foreign key constraint. Please check related data.");

                case -1: 
                case -2:
                    throw new TimeoutException("The database operation has timed out. Please try again.");

                case 1205: 
                    throw new InvalidOperationException("A database deadlock occurred. Please try again.");

                default:
                    _logger.LogWarning("Unhandled SQL error number: {ErrorNumber}", sqlEx.Number);
                    break;
            }
        }


        public async Task AddRangeAsync(
            IEnumerable<Order> orders,
            CancellationToken cancellationToken = default)
        {
            if (orders == null)
                throw new ArgumentNullException(nameof(orders));

            var orderList = orders.ToList();
            if (!orderList.Any())
                return;

            _logger.LogDebug("Adding {Count} orders in batch", orderList.Count);

            await _context.Orders.AddRangeAsync(orderList, cancellationToken);
        }

        public Task UpdateRangeAsync(
            IEnumerable<Order> orders,
            CancellationToken cancellationToken = default)
        {
            if (orders == null)
                throw new ArgumentNullException(nameof(orders));

            var orderList = orders.ToList();
            if (!orderList.Any())
                return Task.CompletedTask;

            _logger.LogDebug("Updating {Count} orders in batch", orderList.Count);

            _context.Orders.UpdateRange(orderList);
            return Task.CompletedTask;
        }
    }
}
