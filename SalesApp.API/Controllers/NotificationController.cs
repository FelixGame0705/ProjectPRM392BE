using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SalesApp.API.SignalR;
using SalesApp.BLL.Services;
using SalesApp.Models.DTOs;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IHubContext<ChatHub, IChatClient> _hubContext;
    private readonly IUserService _userService;
    public NotificationController(INotificationService notificationService, IHubContext<ChatHub, IChatClient> hubContext, IUserService userService)
    {
        _notificationService = notificationService;
        _hubContext = hubContext;
        _userService = userService;
    }

    // Gửi notification đơn giản tới tất cả client (test SignalR)
    [HttpPost("broadcast")]
    public async Task<IActionResult> Broadcast(string message)
    {
        await _hubContext.Clients.All.RecieveMessage(message);
        return NoContent();
    }

    // Tạo notification và gửi tới client
    [HttpPost]
    public async Task<ActionResult> CreateNotification([FromBody] CreateNotificationDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Message) || dto.UserID <= 0)
            return BadRequest("Invalid data.");
        var user = await _userService.GetByIdAsync(dto.UserID);
        if (user == null)
        {
            return BadRequest("UserID không tồn tại.");
        }


        // Lưu vào DB
        await _notificationService.CreateNotificationAsync(dto);

        // Gửi real-time (tùy chỉnh theo user nếu muốn)
        await _hubContext.Clients.All.RecieveMessage(dto.Message);

        return NoContent();
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
        if (id <= 0) return BadRequest("ID must be greater than 0");

        var notification = await _notificationService.GetNotificationByIdAsync(id);
        if (notification == null) return NotFound();

        return Ok(notification);
    }
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotificationsByUserId(int userId)
    {
        if (userId <= 0) return BadRequest("User ID must be greater than 0");
        var notifications = await _notificationService.GetNotificationByUserIdAsync(userId);
        if (notifications == null || !notifications.Any()) return NotFound("No notifications found for this user.");
        return Ok(notifications);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateNotification(int id, [FromBody] UpdateNotificationDto dto)
    {
        if (id <= 0 || dto == null)
            return BadRequest("Invalid request.");

        var exists = await _notificationService.GetNotificationByIdAsync(id);
        if (exists == null)
            return NotFound("Notification not found.");

        await _notificationService.UpdateNotificationAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteNotification(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid ID.");

        var exists = await _notificationService.GetNotificationByIdAsync(id);
        if (exists == null)
            return NotFound("Notification not found.");

        await _notificationService.DeleteNotificationAsync(id);
        return NoContent();
    }

}
