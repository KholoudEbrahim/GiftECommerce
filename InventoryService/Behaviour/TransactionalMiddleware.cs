using InventoryService.DataBase;
using InventoryService.shared.MarkerInterface;
using MediatR;
using Shared.ApiResultResponse;
using System.Threading;

namespace InventoryService.Behaviour
{
    public static class TransactionalMiddleware
    {
        public class TransactionPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
            where TRequest : ICommand<TResponse>
            where TResponse : Shared.ApiResultResponse.IResult
        {

            private readonly InventoryDbContext _context;

            public TransactionPipelineBehavior(InventoryDbContext context)
            {
                _context = context;
            }


            public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
            {
                if (_context.Database.CurrentTransaction is not null)
                    return await next();


                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    var response = await next();

                    if (response.IsFailure)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return response;
                    }

                    await _context.SaveChangesAsync(cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    return response;
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
        }
    }
}
