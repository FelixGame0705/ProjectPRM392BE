using SalesApp.Models.DTOs;

namespace SalesApp.BLL.Services
{
    public interface IProductService : IGenericService<ProductDto, CreateProductDto, CreateProductDto>
    {
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId);
    }
}