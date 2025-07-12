using GoEStores.Core.DTO.Requests;
using GoEStores.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace GoEStores.Api.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("hub")]
        public async Task<IActionResult> CreateChatHub(Guid secondUserId)
        {
            try
            {
                var result = await _chatService.CreateChatHup(secondUserId);
                return Ok(result);
            }
            

            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpGet("hub/{id}")]
        public async Task<IActionResult> GetChatHubById(Guid id)
        {
            try
            {
                var result = await _chatService.GetChatHupById(id);
                return Ok(result);
            }

            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("hubs/user/{id}")]
        public async Task<IActionResult> GetChatHubs(Guid id)
        {
            try
            {
                var result = await _chatService.GetAllChatHupsByUserId(id);
                return Ok(result);
            }
  
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("message")]
        public async Task<IActionResult> CreateMessage([FromBody] CreateMessageRequest model)
        {
            try
            {
                var type = model.Type.ToString();
                await _chatService.CreateChatMessage(model.ChatHubId, model.Content, type);
                return Ok("Saved successfull.");
            }
          
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
