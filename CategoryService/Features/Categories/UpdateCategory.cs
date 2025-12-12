using CategoryService.Contracts;
using CategoryService.Models;
using CategoryService.Models.Enums;
using CategoryService.shared.MarkerInterface;
using FluentValidation;
using MediatR;
using Shared.ApiResultResponse;

namespace CategoryService.Features.Categories;



public static class UpdateCategory
{
    public sealed record Command(int Id, string Name, string? ImageUrl, bool IsActive) : ICommand<Result<bool>>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        }
    }

    internal sealed class Handler : IRequestHandler<Command, Result<bool>>
    {
        private readonly IGenericRepository<Category, int> _repository;
        private readonly IValidator<Command> _validator;

        public Handler(IGenericRepository<Category, int> repository, IValidator<Command> validator)
        {
            _repository = repository;
            _validator = validator;
        }

        

        public async Task<Result<bool>> Handle(Command request, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
                return Result.Failure<bool>(new Error("UpdateCategory.Validation", validationResult.ToString()));


            var category = await _repository.GetByIdAsync(request.Id, trackChanges: true);
            if (category is null)
                return Result.Failure<bool>(new Error("Category.NotFound", $"Category with Id {request.Id} was not found."));



            bool nameExists = await _repository.AnyAsync(c => c.Name == request.Name && c.Id != request.Id, cancellationToken);
            if (nameExists)
            {
                return Result.Failure<bool>(new Error("Category.NameNotUnique", $"The category name '{request.Name}' is already taken."));
            }

            category.Name = request.Name;
            category.ImageUrl = request.ImageUrl;
            category.Status = request.IsActive ? CategoryStatus.Active : CategoryStatus.InActive;
            
            string[] UpdatedValues = [nameof(category.Name), nameof(category.ImageUrl), nameof(category.Status)];
            _repository.SaveInclude(category , UpdatedValues);


            return Result.Success(true);
        }
    }
}