using System;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Core.Entities;
using ChatApp.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Api.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITimeHelper _timeHelper;

        public ChatHub(IUnitOfWork unitOfWork, ITimeHelper timeHelper)
        {
            _unitOfWork = unitOfWork;
            _timeHelper = timeHelper;
        }

        public async Task SendMessage(Guid chatId, string content, string? mediaUrl, string? mediaType)
        {
            var userId = Context.UserIdentifier;
            if (userId == null) return;

            var parsedUserId = Guid.Parse(userId);

            // Verify membership
            var members = await _unitOfWork.ChatMembers.FindAsync(cm => cm.ChatId == chatId && cm.UserId == parsedUserId);
            if (!members.Any()) return; // Not a member

            var message = new Message
            {
                ChatId = chatId,
                SenderId = parsedUserId,
                Content = content,
                MediaUrl = mediaUrl,
                MediaType = mediaType,
                CreatedAt = _timeHelper.GetIstTime(),
                UpdatedAt = _timeHelper.GetIstTime()
            };

            await _unitOfWork.Messages.AddAsync(message);
            await _unitOfWork.CompleteAsync();

            await Clients.Group(chatId.ToString()).SendAsync("ReceiveMessage", new
            {
                message.Id,
                message.ChatId,
                message.SenderId,
                message.Content,
                message.MediaUrl,
                message.MediaType,
                message.CreatedAt
            });
        }

        public async Task JoinChatGroup(Guid chatId)
        {
            var userId = Context.UserIdentifier;
            if (userId == null) return;
            var parsedUserId = Guid.Parse(userId);
            
            var members = await _unitOfWork.ChatMembers.FindAsync(cm => cm.ChatId == chatId && cm.UserId == parsedUserId);
            if (members.Any())
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
            }
        }
        
        public async Task LeaveChatGroup(Guid chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
        }
    }
}
