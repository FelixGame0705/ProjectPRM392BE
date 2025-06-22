// ProductsController.cs
using Microsoft.AspNetCore.Mvc;
using SalesApp.BLL.Services;
using SalesApp.Models.DTOs;

namespace SalesApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(IProductService productService, IWebHostEnvironment environment)
        {
            _productService = productService;
            _environment = environment;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(
            [FromQuery] int? categoryId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? search)
        {
            try
            {
                IEnumerable<ProductDto> products;

                if (categoryId.HasValue || minPrice.HasValue || maxPrice.HasValue || !string.IsNullOrEmpty(search))
                {
                    products = await _productService.GetProductsFilteredAsync(categoryId, minPrice, maxPrice, search);
                }
                else
                {
                    products = await _productService.GetAllAsync();
                }

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                    return NotFound($"Product with ID {id} not found.");

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(int categoryId)
        {
            try
            {
                var products = await _productService.GetProductsByCategoryAsync(categoryId);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> SearchProducts([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrEmpty(searchTerm))
                    return BadRequest("Search term is required.");

                var products = await _productService.SearchProductsAsync(searchTerm);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("price-range")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByPriceRange(
            [FromQuery] decimal minPrice,
            [FromQuery] decimal maxPrice)
        {
            try
            {
                if (minPrice < 0 || maxPrice < 0 || minPrice > maxPrice)
                    return BadRequest("Invalid price range.");

                var products = await _productService.GetProductsByPriceRangeAsync(minPrice, maxPrice);
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromForm] CreateProductWithImageDto createProductDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                string? imageUrl = null;

                // Xử lý upload ảnh nếu có
                if (createProductDto.ImageFile != null)
                {
                    // Kiểm tra định dạng file
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var fileExtension = Path.GetExtension(createProductDto.ImageFile.FileName).ToLowerInvariant();

                    if (Array.IndexOf(allowedExtensions, fileExtension) == -1)
                    {
                        return BadRequest("Only image files (jpg, jpeg, png, gif, webp) are allowed.");
                    }

                    // Kiểm tra kích thước file (5MB)
                    if (createProductDto.ImageFile.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest("File size cannot exceed 5MB.");
                    }

                    // Tạo thư mục uploads nếu chưa có
                    var uploadsFolder = Path.Combine("uploads", "products");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Tạo tên file unique
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    // Lưu file
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await createProductDto.ImageFile.CopyToAsync(fileStream);
                    }

                    // Tạo URL cho ảnh
                    imageUrl = $"/uploads/products/{fileName}";
                }

                // Tạo DTO để gửi tới service
                var productDto = new CreateProductDto
                {
                    ProductName = createProductDto.ProductName,
                    BriefDescription = createProductDto.BriefDescription,
                    FullDescription = createProductDto.FullDescription,
                    TechnicalSpecifications = createProductDto.TechnicalSpecifications,
                    Price = createProductDto.Price,
                    CategoryID = createProductDto.CategoryID,
                    ImageURL = imageUrl
                };

                var product = await _productService.CreateAsync(productDto);
                return CreatedAtAction(nameof(GetProduct), new { id = product.ProductID }, product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProductDto>> UpdateProduct(int id, CreateProductDto updateProductDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updatedProduct = await _productService.UpdateAsync(id, updateProductDto);
                if (updatedProduct == null)
                    return NotFound($"Product with ID {id} not found.");

                return Ok(updatedProduct);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var deleted = await _productService.DeleteAsync(id);
                if (!deleted)
                    return NotFound($"Product with ID {id} not found.");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpHead("{id}")]
        public async Task<IActionResult> ProductExists(int id)
        {
            try
            {
                var exists = await _productService.ExistsAsync(id);
                return exists ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}