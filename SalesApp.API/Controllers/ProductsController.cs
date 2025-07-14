// ProductsController.cs
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
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
        private readonly Cloudinary _cloudinary;

        public ProductsController(
            IProductService productService,
            IWebHostEnvironment environment,
            Cloudinary cloudinary
        )
        {
            _productService = productService;
            _environment = environment;
            _cloudinary = cloudinary;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(
            [FromQuery] int? categoryId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? search
        )
        {
            try
            {
                IEnumerable<ProductDto> products;

                if (
                    categoryId.HasValue
                    || minPrice.HasValue
                    || maxPrice.HasValue
                    || !string.IsNullOrEmpty(search)
                )
                {
                    products = await _productService.GetProductsFilteredAsync(
                        categoryId,
                        minPrice,
                        maxPrice,
                        search
                    );
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
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(
            int categoryId
        )
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
        public async Task<ActionResult<IEnumerable<ProductDto>>> SearchProducts(
            [FromQuery] string searchTerm
        )
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
            [FromQuery] decimal maxPrice
        )
        {
            try
            {
                if (minPrice < 0 || maxPrice < 0 || minPrice > maxPrice)
                    return BadRequest("Invalid price range.");

                var products = await _productService.GetProductsByPriceRangeAsync(
                    minPrice,
                    maxPrice
                );
                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct(
            [FromForm] CreateProductWithImageDto createProductDto
        )
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
                    var fileExtension = Path.GetExtension(createProductDto.ImageFile.FileName)
                        .ToLowerInvariant();

                    if (Array.IndexOf(allowedExtensions, fileExtension) == -1)
                    {
                        return BadRequest(
                            "Only image files (jpg, jpeg, png, gif, webp) are allowed."
                        );
                    }

                    // Kiểm tra kích thước file (5MB)
                    if (createProductDto.ImageFile.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest("File size cannot exceed 5MB.");
                    }

                    // Tạo thư mục uploads nếu chưa có
                    //var uploadsFolder = Path.Combine("uploads", "products");
                    //if (!Directory.Exists(uploadsFolder))
                    //{
                    //    Directory.CreateDirectory(uploadsFolder);
                    //}

                    // Tạo tên file unique
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(
                            createProductDto.ImageFile.FileName,
                            createProductDto.ImageFile.OpenReadStream()
                        ),
                    };
                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        imageUrl = uploadResult.SecureUrl.ToString();
                    }
                    //var filePath = Path.Combine(uploadsFolder, fileName);

                    // Lưu file
                    //using (var fileStream = new FileStream(filePath, FileMode.Create))
                    //{
                    //    await createProductDto.ImageFile.CopyToAsync(fileStream);
                    //}

                    // Tạo URL cho ảnh
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
                    ImageURL = imageUrl,
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
        public async Task<ActionResult<ProductDto>> UpdateProduct(
            int id,
            [FromForm] CreateProductWithImageDto updateProductDto
        )
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Lấy thông tin product hiện tại để có URL ảnh cũ
                var existingProduct = await _productService.GetByIdAsync(id);
                if (existingProduct == null)
                    return NotFound($"Product with ID {id} not found.");

                string? imageUrl = existingProduct.ImageURL; // Giữ ảnh cũ nếu không upload ảnh mới

                // Xử lý upload ảnh nếu có
                if (updateProductDto.ImageFile != null)
                {
                    // Kiểm tra định dạng file
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var fileExtension = Path.GetExtension(updateProductDto.ImageFile.FileName)
                        .ToLowerInvariant();

                    if (Array.IndexOf(allowedExtensions, fileExtension) == -1)
                    {
                        return BadRequest(
                            "Only image files (jpg, jpeg, png, gif, webp) are allowed."
                        );
                    }

                    // Kiểm tra kích thước file (5MB)
                    if (updateProductDto.ImageFile.Length > 5 * 1024 * 1024)
                    {
                        return BadRequest("File size cannot exceed 5MB.");
                    }

                    // Upload ảnh mới
                    var fileName = $"{Guid.NewGuid()}{fileExtension}";
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(
                            updateProductDto.ImageFile.FileName,
                            updateProductDto.ImageFile.OpenReadStream()
                        ),
                    };

                    var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                    if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var newImageUrl = uploadResult.SecureUrl.ToString();

                        // Xóa ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(existingProduct.ImageURL))
                        {
                            try
                            {
                                // Lấy public_id từ URL (phần sau dấu / cuối cùng, bỏ extension)
                                var publicId = GetPublicIdFromUrl(existingProduct.ImageURL);
                                if (!string.IsNullOrEmpty(publicId))
                                {
                                    var deleteParams = new DeletionParams(publicId);
                                    await _cloudinary.DestroyAsync(deleteParams);
                                }
                            }
                            catch (Exception ex)
                            {
                                // Log lỗi nhưng không làm fail toàn bộ request
                                Console.WriteLine($"Error deleting old image: {ex.Message}");
                            }
                        }

                        imageUrl = newImageUrl;
                    }
                }

                // Tạo DTO để gửi tới service
                var productDto = new CreateProductDto
                {
                    ProductName = updateProductDto.ProductName,
                    BriefDescription = updateProductDto.BriefDescription,
                    FullDescription = updateProductDto.FullDescription,
                    TechnicalSpecifications = updateProductDto.TechnicalSpecifications,
                    Price = updateProductDto.Price,
                    CategoryID = updateProductDto.CategoryID,
                    ImageURL = imageUrl,
                };

                var updatedProduct = await _productService.UpdateAsync(id, productDto);
                return Ok(updatedProduct);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Helper method để lấy public_id từ Cloudinary URL
        private string GetPublicIdFromUrl(string cloudinaryUrl)
        {
            try
            {
                // URL format: https://res.cloudinary.com/[cloud_name]/image/upload/v[version]/[public_id].[extension]
                var uri = new Uri(cloudinaryUrl);
                var segments = uri.Segments;

                // Lấy segment cuối cùng và bỏ extension
                var lastSegment = segments[segments.Length - 1];
                var publicId = Path.GetFileNameWithoutExtension(lastSegment);

                return publicId;
            }
            catch
            {
                return string.Empty;
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
