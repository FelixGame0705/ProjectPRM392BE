using SalesApp.Models.DTOs;

namespace SalesApp.BLL.Services
{
    public interface IUserService : IGenericService<UserDto, CreateUserDto, UpdateUserDto>
    {
        Task<UserDto?> GetUserByUsernameAsync(string username);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<bool> ValidateUserCredentialsAsync(string username, string password);
    }
}