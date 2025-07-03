using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesApp.BLL.Services;
using SalesApp.Models.DTOs;
using System.Security.Claims;

namespace SalesApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // đảm bảo có JWT để lấy UserID
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendChat([FromBody] ChatDto chatDto)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized("User not authenticated");

            if (string.IsNullOrWhiteSpace(chatDto.Message))
                return BadRequest("Message is required");

            chatDto.UserID = userId.Value;
            chatDto.SentAt = DateTime.Now;

            await _chatService.SaveChatMessageAsync(chatDto);
            return Ok(new { message = "Chat saved successfully", data = chatDto });
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetChatHistory()
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized();

            var messages = await _chatService.GetChatHistoryAsync(userId.Value);
            return Ok(messages);
        }

        private int? GetUserId()
        {
            var userIdStr = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdStr, out var userId) ? userId : null;
        }
    }
}
