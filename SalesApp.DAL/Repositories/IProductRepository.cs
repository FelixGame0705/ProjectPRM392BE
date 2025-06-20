using SalesApp.Models.Entities;

namespace SalesApp.DAL.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    {
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> GetProductsWithCategoryAsync();
    }
}