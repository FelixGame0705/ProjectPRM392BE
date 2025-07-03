using SalesApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesApp.DAL.Repositories
{
    public interface IChatRepository : IGenericRepository<ChatMessage>
    {
        IQueryable<ChatMessage> GetQueryable(); // Để truy vấn ở tầng Service

    }
}
