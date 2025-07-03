using SalesApp.DAL.Repositories;
using SalesApp.Models.Entities;

namespace SalesApp.DAL.Repositories
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        // Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(string userId);
        // Task MarkNotificationAsReadAsync(int notificationId);
        // Task DeleteNotificationAsync(int notificationId);
        // Task AddNotificationAsync(Notification notification);
    }
}

