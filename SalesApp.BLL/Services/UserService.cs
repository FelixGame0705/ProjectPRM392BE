using AutoMapper;
using SalesApp.BLL.Services;
using SalesApp.DAL.UnitOfWork;
using SalesApp.Models.DTOs;
using SalesApp.Models.Entities;
using System.Security.Cryptography;
using System.Text;

namespace SalesApp.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDto>> GetAllAsync()
        {
            var users = await _unitOfWork.Repository<User>().GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        public async Task<UserDto> CreateAsync(CreateUserDto createDto)
        {
            var user = _mapper.Map<User>(createDto);
            user.PasswordHash = HashPassword(createDto.Password);

            await _unitOfWork.Repository<User>().AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> UpdateAsync(int id, UpdateUserDto updateDto)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
            if (user == null) return null;

            _mapper.Map(updateDto, user);
            _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _unitOfWork.Repository<User>().GetByIdAsync(id);
            if (user == null) return false;

            _unitOfWork.Repository<User>().Delete(user);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _unitOfWork.Repository<User>().ExistsAsync(id);
        }

        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            var users = await _unitOfWork.Repository<User>().FindAsync(u => u.Username == username);
            var user = users.FirstOrDefault();
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var users = await _unitOfWork.Repository<User>().FindAsync(u => u.Email == email);
            var user = users.FirstOrDefault();
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        public async Task<bool> ValidateUserCredentialsAsync(string username, string password)
        {
            var users = await _unitOfWork.Repository<User>().FindAsync(u => u.Username == username);
            var user = users.FirstOrDefault();

            if (user == null) return false;

            return VerifyPassword(password, user.PasswordHash);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }
    }
}