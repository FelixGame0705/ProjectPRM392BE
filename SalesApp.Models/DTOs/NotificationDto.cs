namespace SalesApp.Models.DTOs
{
    public class NotificationDto
    {
        public int NotificationID { get; set; }
        public string? Message { get; set; }
        public int UserID { get; set; }
    }

    public class CreateNotificationDto
    {
        public string? Message { get; set; }
        public int UserID { get; set; }
    }
    public class UpdateNotificationDto
    {
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
    }


}
