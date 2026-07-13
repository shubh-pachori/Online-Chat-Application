using System;

namespace ChatApp.Core.Entities
{
    public class Message : BaseEntity
    {
        public Guid ChatId { get; set; }
        public Chat Chat { get; set; } = null!;
        public Guid SenderId { get; set; }
        public User Sender { get; set; } = null!;
        
        public string? Content { get; set; }
        public string? MediaUrl { get; set; }
        public string? MediaType { get; set; } // Text, Image, Video, Audio
    }
}
