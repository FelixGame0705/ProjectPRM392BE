using SalesApp.DAL.UnitOfWork; // Add this line if IUnitOfWork is in this namespace
using SalesApp.Models.Entities;
using SalesApp.DAL.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using SalesApp.Models.DTOs;

using AutoMapper;
namespace SalesApp.BLL.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;

        public NotificationService(IUnitOfWork unitOfWork, INotificationRepository notificationRepository, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _notificationRepository = notificationRepository;
            _mapper = mapper;
        }
        public async Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync()
        {

            var notifications = await _notificationRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
        }

        public async Task<NotificationDto?> GetNotificationByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new ArgumentException("Notification ID must be greater than zero.", nameof(id));
                }
            }
            catch (ArgumentException ex)
            {
                // Log the exception or handle it as needed
                throw;
            }

            return _mapper.Map<NotificationDto>(await _notificationRepository.GetByIdAsync(id));
        }

        public async Task<IEnumerable<NotificationDto>> GetNotificationByUserIdAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User ID must be greater than zero.", nameof(userId));
            }
            var notifications = await _notificationRepository.GetNotificationsByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
        }

        public async Task CreateNotificationAsync(CreateNotificationDto notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification), "Notification cannot be null.");
            }
            if (string.IsNullOrWhiteSpace(notification.Message))
            {
                throw new ArgumentException("Notification message cannot be null or empty.", nameof(notification.Message));
            }
            if (notification.UserID <= 0)
            {
                throw new ArgumentException("User ID must be greater than zero.", nameof(notification.UserID));
            }
            var entity = _mapper.Map<Notification>(notification);
            await _notificationRepository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateNotificationAsync(int id, UpdateNotificationDto d)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null)
            {
                throw new KeyNotFoundException($"Notification with ID {id} not found.");
            }
            _notificationRepository.Update(notification);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteNotificationAsync(int id)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null)
            {
                throw new KeyNotFoundException($"Notification with ID {id} not found.");
            }
            _notificationRepository.Delete(notification);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
