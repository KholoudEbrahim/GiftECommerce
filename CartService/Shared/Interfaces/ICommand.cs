using MediatR;
namespace CategoryService.shared.MarkerInterface;
public interface ICommand<out TResponse> : IRequest<TResponse> { }


