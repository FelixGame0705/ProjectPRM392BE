using Microsoft.AspNetCore.Mvc;
using SalesApp.BLL.Services;
using SalesApp.Models.DTOs;
using SalesApp.Models.Entities;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetAllNotifications()
    {
        var notifications = await _notificationService.GetAllNotificationsAsync();
        return Ok(notifications);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<NotificationDto?>> GetNotificationById(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Notification ID must be greater than zero.");
        }
        var notification = await _notificationService.GetNotificationByIdAsync(id);
        if (notification == null)
        {
            return NotFound();
        }
        return Ok(notification);
    }

    [HttpPost]
    public async Task<ActionResult> CreateNotification(NotificationDto notification)
    {
        if (notification == null)
        {
            return BadRequest("Notification cannot be null.");
        }
        if (string.IsNullOrEmpty(notification.Message) || notification.UserID <= 0)
        {
            return BadRequest("Notification message and UserID cannot be empty.");
        }
        await _notificationService.CreateNotificationAsync(notification);
        return CreatedAtAction(nameof(GetNotificationById), new { id = notification.NotificationID }, notification);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateNotification(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Notification ID must be greater than zero.");
        }
        var notification = await _notificationService.GetNotificationByIdAsync(id);
        if (notification == null || notification.NotificationID != id)      
        {
            return BadRequest("Notification ID mismatch.");
        }
        if (notification == null || id <= 0)
        {
            return BadRequest();
        }
        await _notificationService.UpdateNotificationAsync(id);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteNotification(int id)
    {
        if (id <= 0)
        {
            return BadRequest("Notification ID must be greater than zero.");
        }   
        await _notificationService.DeleteNotificationAsync(id);
        return NoContent();
    }
}
