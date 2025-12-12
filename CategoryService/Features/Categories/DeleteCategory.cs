using CategoryService.Contracts;
using CategoryService.Models;
using CategoryService.shared.MarkerInterface;
using FluentValidation;
using MediatR;
using Shared.ApiResultResponse;

namespace CategoryService.Features.Categories;



public static class DeleteCategory
{
    public sealed record Command(int Id) : ICommand<Result<bool>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
        }
    }

    internal sealed class Handler : IRequestHandler<Command, Result<bool>>
    {
        private readonly IGenericRepository<Category, int> _categoryRepository;
        private readonly IGenericRepository<Product, int> _productRepository;
        private readonly IValidator<Command> _validator;

        

        public Handler(IGenericRepository<Category, int> categoryRepository, IValidator<Command> validator, IGenericRepository<Product, int> productRepository)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _validator = validator;
        }

        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return Result.Failure<bool>(new Error("DeleteCategory.Validation", validationResult.ToString()));


            var category = await _categoryRepository.GetByIdAsync(request.Id);
            if (category is null)
            {
                return Result.Failure<bool>(new Error("Category.NotFound", $"Category with Id {request.Id} was not found."));
            }

            bool hasProducts = await _productRepository.AnyAsync(p => p.CategoryId == request.Id && !p.IsDeleted, cancellationToken);
            if (hasProducts)
            {
                return Result.Failure<bool>(new Error("Category.HasDependencies", "Cannot delete category because it has assigned products."));
            }

            await _categoryRepository.DeleteAsync(request.Id);

            return Result.Success(true);
        }
    }
}