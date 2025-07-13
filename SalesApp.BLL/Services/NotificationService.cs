using AutoMapper;
using GoEStores.Services.Interface;
using SalesApp.DAL.UnitOfWork;
using SalesApp.DAL.Repositories;
using SalesApp.Models.Entities;
using SalesApp.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SalesApp.BLL.HubRealTime;
using Microsoft.Extensions.Logging;

namespace SalesApp.BLL.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationRepository _notificationRepository;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _notificationHubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IUnitOfWork unitOfWork, INotificationRepository notificationRepository,
            IMapper mapper, IHubContext<NotificationHub> notificationHubContext, ILogger<NotificationService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _notificationHubContext = notificationHubContext ?? throw new ArgumentNullException(nameof(notificationHubContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync()
        {
            try
            {
                var notifications = await _notificationRepository.GetAllAsync();
                return _mapper.Map<IEnumerable<NotificationDto>>(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all notifications.");
                throw;
            }
        }

        public async Task<NotificationDto?> GetNotificationByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new ArgumentException("Notification ID must be greater than zero.", nameof(id));
                }
                var notification = await _notificationRepository.GetByIdAsync(id);
                if (notification == null)
                {
                    throw new KeyNotFoundException($"Notification with ID {id} not found.");
                }
                return _mapper.Map<NotificationDto>(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification with ID {Id}.", id);
                throw;
            }
        }

        public async Task CreateNotificationAsync(CreateNotificationDto notificationDto)
        {
            try
            {
                if (notificationDto == null)
                {
                    throw new ArgumentNullException(nameof(notificationDto), "Notification cannot be null.");
                }
                if (string.IsNullOrWhiteSpace(notificationDto.Message))
                {
                    throw new ArgumentException("Notification message cannot be null or empty.", nameof(notificationDto.Message));
                }
                if (notificationDto.UserID <= 0)
                {
                    throw new ArgumentException("User ID must be greater than zero.", nameof(notificationDto.UserID));
                }

                await _unitOfWork.BeginTransactionAsync();
                var notification = _mapper.Map<Notification>(notificationDto);
                notification.IsRead = false;
                notification.CreatedAt = DateTime.Now;

                await _notificationRepository.AddAsync(notification);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // G?i thông báo qua SignalR
                await _notificationHubContext.Clients.User(notification.UserID.ToString())
                    .SendAsync("ReceiveNotification", notification.Message);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error creating notification.");
                throw;
            }
        }

        public async Task UpdateNotificationAsync(int id, UpdateNotificationDto notificationDto)
        {
            try
            {
                if (notificationDto == null)
                {
                    throw new ArgumentNullException(nameof(notificationDto), "Notification update data cannot be null.");
                }

                await _unitOfWork.BeginTransactionAsync();
                var notification = await _notificationRepository.GetByIdAsync(id);
                if (notification == null)
                {
                    throw new KeyNotFoundException($"Notification with ID {id} not found.");
                }

                _mapper.Map(notificationDto, notification); // Ánh x? ng??c
                _notificationRepository.Update(notification);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // G?i thông báo c?p nh?t (n?u c?n)
                if (!string.IsNullOrEmpty(notification.Message))
                {
                    await _notificationHubContext.Clients.User(notification.UserID.ToString())
                        .SendAsync("ReceiveNotification", $"Notification {id} updated: {notification.Message}");
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error updating notification with ID {Id}.", id);
                throw;
            }
        }

        public async Task DeleteNotificationAsync(int id)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();
                var notification = await _notificationRepository.GetByIdAsync(id);
                if (notification == null)
                {
                    throw new KeyNotFoundException($"Notification with ID {id} not found.");
                }

                _notificationRepository.Delete(notification);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                // G?i thông báo xóa (n?u c?n)
                await _notificationHubContext.Clients.User(notification.UserID.ToString())
                    .SendAsync("ReceiveNotification", $"Notification {id} deleted.");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Error deleting notification with ID {Id}.", id);
                throw;
            }
        }
    }
}