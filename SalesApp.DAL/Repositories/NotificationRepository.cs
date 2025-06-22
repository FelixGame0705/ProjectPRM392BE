using SalesApp.DAL.Data;
using SalesApp.Models.Entities;

namespace SalesApp.DAL.Repositories
{

    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(SalesAppDbContext context) : base(context)
        {
        }
        // Implement any specific methods for notifications if needed
    }
}