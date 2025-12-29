using System;

namespace ExchangeWebsite.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string UserId { get; set; } // Ng??i nh?n thông báo
        public User User { get; set; }
        public string Message { get; set; }
        public string Link { get; set; } // ???ng d?n ??n bài vi?t, comment, v.v.
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}   