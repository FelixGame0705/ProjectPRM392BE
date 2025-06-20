using AutoMapper;
using SalesApp.DAL.UnitOfWork;
using SalesApp.Models.DTOs;
using SalesApp.Models.Entities;

namespace SalesApp.BLL.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _unitOfWork.ProductRepository.GetProductsWithCategoryAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            return product != null ? _mapper.Map<ProductDto>(product) : null;
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto createDto)
        {
            var product = _mapper.Map<Product>(createDto);

            await _unitOfWork.Repository<Product>().AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto?> UpdateAsync(int id, CreateProductDto updateDto)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            if (product == null) return null;

            _mapper.Map(updateDto, product);
            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            if (product == null) return false;

            _unitOfWork.Repository<Product>().Delete(product);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _unitOfWork.Repository<Product>().ExistsAsync(id);
        }

        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId)
        {
            var products = await _unitOfWork.ProductRepository.GetProductsByCategoryAsync(categoryId);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }
    }
}