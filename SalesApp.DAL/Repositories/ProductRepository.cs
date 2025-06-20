using Microsoft.EntityFrameworkCore;
using SalesApp.DAL.Data;
using SalesApp.Models.Entities;

namespace SalesApp.DAL.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        public ProductRepository(SalesAppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Where(p => p.CategoryID == categoryId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsWithCategoryAsync()
        {
            return await _dbSet
                .Include(p => p.Category)
                .ToListAsync();
        }
    }
}