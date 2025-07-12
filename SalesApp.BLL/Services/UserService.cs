using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using SalesApp.BLL.Services;
using SalesApp.DAL.Repositories;
using SalesApp.DAL.UnitOfWork;
using SalesApp.Models.DTOs;
using SalesApp.Models.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SalesApp.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly IUserRepository _accountRepository;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, IConfiguration configuration, IUserRepository userRepository, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _accountRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<LoginResponseDTO?> LoginAsync(LoginDto loginDto)
        {
            var user = await _accountRepository.GetUserByNameAsync(loginDto.Username);
            if (user == null || !VerifyPassword(loginDto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials");


            var issuer = _configuration["JwtConfig:Issuer"];
            var audience = _configuration["JwtConfig:Audience"];
            var key = _configuration["JwtConfig:Key"];
            var tokenValidityMinues = _configuration.GetValue<int>("JwtConfig:TokenValidityMinutes", 30);
            var tokenExpiryTimeStamp = DateTime.UtcNow.AddMinutes(tokenValidityMinues);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                    new Claim(ClaimTypes.Role, user.Role.ToString()) // Assuming User has a Role property
                }),
                Expires = tokenExpiryTimeStamp,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key))
                , SecurityAlgorithms.HmacSha256Signature),
            };

            var jwtTokenHandler = new JwtSecurityTokenHandler(); // Renamed variable to avoid conflict
            var securityToken = jwtTokenHandler.CreateToken(tokenDescriptor);
            var assertedToken = jwtTokenHandler.WriteToken(securityToken);

            return new LoginResponseDTO
            {
                AccessToken = assertedToken,
                Username = loginDto.Username,
                ExpiresIn = (int)(tokenExpiryTimeStamp - DateTime.UtcNow).TotalSeconds,
                Role = user.Role.ToString() // Fix: Assign the RoleEnum directly instead of converting to string
            };
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

        public async Task<User> GetCurrentAccountAsync()
        {
            try
            {
                string userId = GetUserId();
                var account = await _unitOfWork.UserRepository.GetByIdAsync(int.Parse(userId));
                return account;
            }
            catch
            {
                throw;
            }

        }
        public string GetUserId()
        {
            try
            {
                return _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new();
            }
            catch
            {
                throw new UnauthorizedAccessException("User is not authenticated or does not have a valid user ID.");
            }
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