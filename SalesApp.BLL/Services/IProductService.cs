using SalesApp.Models.DTOs;

namespace SalesApp.BLL.Services
{
    public interface IProductService : IGenericService<ProductDto, CreateProductDto, CreateProductDto>
    {
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm);
        Task<IEnumerable<ProductDto>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<IEnumerable<ProductDto>> GetProductsFilteredAsync(int? categoryId, decimal? minPrice, decimal? maxPrice, string? searchTerm);
    }
}