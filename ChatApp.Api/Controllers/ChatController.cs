using System;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Core.Entities;
using ChatApp.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITimeHelper _timeHelper;
        private readonly IS3Service _s3Service;

        public ChatController(IUnitOfWork unitOfWork, ITimeHelper timeHelper, IS3Service s3Service)
        {
            _unitOfWork = unitOfWork;
            _timeHelper = timeHelper;
            _s3Service = s3Service;
        }

        private Guid GetUserId() => Guid.Parse(User.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value);

        [HttpPost("group")]
        public async Task<IActionResult> CreateGroupChat([FromBody] CreateGroupDto request)
        {
            var userId = GetUserId();
            var now = _timeHelper.GetIstTime();

            var chat = new Chat
            {
                IsGroupChat = true,
                GroupName = request.Name,
                GroupDescription = request.Description,
                AdminUserId = userId,
                CreatedAt = now,
                UpdatedAt = now
            };

            await _unitOfWork.Chats.AddAsync(chat);

            var member = new ChatMember
            {
                ChatId = chat.Id,
                UserId = userId,
                JoinedAt = now
            };

            await _unitOfWork.ChatMembers.AddAsync(member);
            await _unitOfWork.CompleteAsync();

            return Ok(new { chat.Id, chat.GroupName });
        }

        [HttpPut("group/{chatId}")]
        public async Task<IActionResult> UpdateGroupProfile(Guid chatId, [FromBody] UpdateGroupDto request)
        {
            var userId = GetUserId();
            var members = await _unitOfWork.ChatMembers.FindAsync(cm => cm.ChatId == chatId && cm.UserId == userId);
            if (!members.Any()) return Forbid("Only members can edit the group.");

            var chat = await _unitOfWork.Chats.GetByIdAsync(chatId);
            if (chat == null || !chat.IsGroupChat) return NotFound("Group not found.");

            chat.GroupName = request.GroupName ?? chat.GroupName;
            chat.GroupDescription = request.GroupDescription ?? chat.GroupDescription;
            chat.GroupProfilePictureUrl = request.GroupProfilePictureUrl ?? chat.GroupProfilePictureUrl;
            chat.UpdatedAt = _timeHelper.GetIstTime();

            _unitOfWork.Chats.Update(chat);
            await _unitOfWork.CompleteAsync();

            return Ok(new { Message = "Group profile updated." });
        }

        [HttpPost("group/{chatId}/members")]
        public async Task<IActionResult> AddMember(Guid chatId, [FromBody] AddMemberDto request)
        {
            var userId = GetUserId();
            var chat = await _unitOfWork.Chats.GetByIdAsync(chatId);
            if (chat == null || chat.AdminUserId != userId) return Forbid("Only admin can add members.");

            var existing = await _unitOfWork.ChatMembers.FindAsync(cm => cm.ChatId == chatId && cm.UserId == request.UserId);
            if (existing.Any()) return BadRequest("User is already a member.");

            var newMember = new ChatMember
            {
                ChatId = chatId,
                UserId = request.UserId,
                JoinedAt = _timeHelper.GetIstTime()
            };

            await _unitOfWork.ChatMembers.AddAsync(newMember);
            await _unitOfWork.CompleteAsync();

            return Ok(new { Message = "Member added." });
        }
        
        [HttpGet("{chatId}/messages")]
        public async Task<IActionResult> GetMessages(Guid chatId)
        {
            var userId = GetUserId();
            var members = await _unitOfWork.ChatMembers.FindAsync(cm => cm.ChatId == chatId && cm.UserId == userId);
            if (!members.Any()) return Forbid();

            var messages = await _unitOfWork.Messages.FindAsync(m => m.ChatId == chatId);
            
            var result = messages.OrderBy(m => m.CreatedAt).Select(m => new {
                m.Id, m.SenderId, m.Content,
                MediaUrl = m.MediaUrl != null ? _s3Service.GetPresignedUrl(m.MediaUrl, TimeSpan.FromHours(1)) : null,
                m.MediaType, m.CreatedAt
            });

            return Ok(result);
        }

        [HttpPost("media")]
        public async Task<IActionResult> UploadMedia(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("File is empty.");

            var key = await _s3Service.UploadFileAsync(file.OpenReadStream(), file.FileName, file.ContentType);
            return Ok(new { MediaUrl = key }); // Return the key, the hub will use it to broadcast.
        }
    }

    public class CreateGroupDto { public string Name { get; set; } = string.Empty; public string? Description { get; set; } }
    public class AddMemberDto { public Guid UserId { get; set; } }
    public class UpdateGroupDto { public string? GroupName { get; set; } public string? GroupDescription { get; set; } public string? GroupProfilePictureUrl { get; set; } }
}
