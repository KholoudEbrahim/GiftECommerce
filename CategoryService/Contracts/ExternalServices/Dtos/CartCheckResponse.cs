namespace CategoryService.Contracts.ExternalServices.Dtos
{
    public record CartCheckResponse(
    bool IsInCart,
    int ReservedQuantity
);

}
