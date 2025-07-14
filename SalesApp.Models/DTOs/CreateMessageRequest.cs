using System.ComponentModel.DataAnnotations;

namespace GoEStores.Core.DTO.Requests
{
    public class CreateMessageRequest
    {
        [Required]
        public Guid ChatHubId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; }
        [Required]
        public ChatMessageTypeEnum Type { get; set; }
    }
    public enum ChatMessageTypeEnum
    {
        Text,
        Image,
    }
}
