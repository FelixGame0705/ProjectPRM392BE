using System.Collections.Generic;
using System.Threading.Tasks;
using SalesApp.Models.Entities; // Replace with the actual namespace of Notification
using SalesApp.Models.DTOs; // Replace with the actual namespace of NotificationDto

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync();
    Task<NotificationDto?> GetNotificationByIdAsync(int id);
    Task CreateNotificationAsync(CreateNotificationDto notification);
    Task UpdateNotificationAsync(int id, UpdateNotificationDto dto);
    Task DeleteNotificationAsync(int id);
}