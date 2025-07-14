using Microsoft.EntityFrameworkCore;
using SalesApp.DAL.Data;
using SalesApp.Models.Entities;

namespace SalesApp.DAL.Repositories
{

    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(SalesAppDbContext context) : base(context)
        {
        }
        public async Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(int userId)
        {
            return await _dbSet.Where(n => n.UserID == userId).ToListAsync();
        }
        // Implement any specific methods for notifications if needed
    }
}