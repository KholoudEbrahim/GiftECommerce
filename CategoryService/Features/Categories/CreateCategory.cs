using CategoryService.Contracts;
using CategoryService.Models;
using CategoryService.Models.Enums;
using CategoryService.shared.MarkerInterface;
using FluentValidation;
using MediatR;
using Shared.ApiResultResponse;

namespace CategoryService.Features.Categories;

public static class CreateCategory
{
    public sealed record Command(string Name,string? ImageUrl,bool IsActive) : ICommand<Result<int>>;


    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        }
    }

    internal sealed class Handler : IRequestHandler<Command, Result<int>>
    {
        private readonly IGenericRepository<Category, int> _repository;
        private readonly IValidator<Command> _validator;

        public Handler(IGenericRepository<Category, int> repository, IValidator<Command> validator)
        {
            _repository = repository;
            _validator = validator;
        }

        public async Task<Result<int>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return Result.Failure<int>(new Error("CreateCategory.Validation", validationResult.ToString()));

            bool nameExists = await _repository.AnyAsync(c => c.Name == request.Name, cancellationToken);

            if (nameExists)
            {
                return Result.Failure<int>(new Error("Category.NameNotUnique",$"The category name '{request.Name}' already exists."));
            }

            
            var category = new Category
            {
                Name = request.Name,
                ImageUrl = request.ImageUrl,
                Status = request.IsActive ? CategoryStatus.Active : CategoryStatus.InActive,
                IsDeleted = false
            };
            await _repository.AddAsync(category);
            await _repository.SaveChangesAsync();

            return Result.Success(category.Id);
        }
    }

}
