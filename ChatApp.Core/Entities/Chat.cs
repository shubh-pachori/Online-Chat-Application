using System;
using System.Collections.Generic;

namespace ChatApp.Core.Entities
{
    public class Chat : BaseEntity
    {
        public bool IsGroupChat { get; set; }
        public string? GroupName { get; set; }
        public string? GroupDescription { get; set; }
        public string? GroupProfilePictureUrl { get; set; }
        public Guid? AdminUserId { get; set; } // For groups

        public ICollection<ChatMember> Members { get; set; } = new List<ChatMember>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
