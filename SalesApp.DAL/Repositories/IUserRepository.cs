using Microsoft.EntityFrameworkCore;
using SalesApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesApp.DAL.Repositories
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetUserByNameAsync(string username);
      
    }
}
