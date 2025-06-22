using SalesApp.Models.DTOs;

namespace SalesApp.BLL.Services
{
    public interface ICategoryService : IGenericService<CategoryDto, CreateCategoryDto, UpdateCategoryDto>
    {
        Task<IEnumerable<CategoryDto>> GetCategoriesWithProductCountAsync();
    }
}