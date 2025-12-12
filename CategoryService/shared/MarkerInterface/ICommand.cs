using MediatR;
namespace CategoryService.shared.MarkerInterface;






// A marker interface to identify commands that should be wrapped in a transaction
public interface ICommand<out TResponse> : IRequest<TResponse> { } 


