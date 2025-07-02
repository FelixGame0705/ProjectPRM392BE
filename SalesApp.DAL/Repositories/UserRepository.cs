using Microsoft.EntityFrameworkCore;
using SalesApp.DAL.Data;
using SalesApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesApp.DAL.Repositories
{
    public class UserRepository : GenericRepository<User> ,IUserRepository
    {
        private readonly SalesAppDbContext _context;
        public UserRepository(SalesAppDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<User?> GetUserByNameAsync(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) Console.WriteLine("User not found");
            else Console.WriteLine($"Id: {user.UserID}, Username: {user.Username}");
            return user;
        }
    }
}
