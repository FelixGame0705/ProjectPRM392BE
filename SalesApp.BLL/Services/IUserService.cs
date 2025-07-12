using SalesApp.Models.DTOs;
using SalesApp.Models.Entities;

namespace SalesApp.BLL.Services
{
    public interface IUserService : IGenericService<UserDto, CreateUserDto, UpdateUserDto>
    {
        Task<UserDto?> GetUserByUsernameAsync(string username);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<bool> ValidateUserCredentialsAsync(string username, string password);
        Task<LoginResponseDTO?> LoginAsync(LoginDto loginDto);
        Task<User> GetCurrentAccountAsync();
        string GetUserId();

    }
}