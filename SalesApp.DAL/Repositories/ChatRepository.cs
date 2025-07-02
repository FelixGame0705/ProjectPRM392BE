using SalesApp.DAL.Data;
using SalesApp.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesApp.DAL.Repositories
{
    public class ChatRepository : GenericRepository<ChatMessage>, IChatRepository
    {
        public ChatRepository(SalesAppDbContext context) : base(context)
        {
        }
        public IQueryable<ChatMessage> GetQueryable()
        {
            return _dbSet.AsQueryable(); // _dbSet kế thừa từ GenericRepository
        }
    }
}
