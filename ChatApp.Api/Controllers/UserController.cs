using System;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IS3Service _s3Service;

        public UserController(IUnitOfWork unitOfWork, IS3Service s3Service)
        {
            _unitOfWork = unitOfWork;
            _s3Service = s3Service;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var email = HttpContext.Items["UserEmail"] as string;
            var users = await _unitOfWork.Users.FindAsync(u => u.Email == email);
            var user = users.FirstOrDefault();
            if (user == null) return NotFound();

            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                user.ProfilePictureUrl = _s3Service.GetPresignedUrl(user.ProfilePictureUrl, TimeSpan.FromHours(1));
            }

            return Ok(new { 
                user.Id, user.Email, user.Username, user.Role, 
                user.ProfilePictureUrl, user.PhoneNumber, user.DateOfBirth,
                user.CreatedAt, user.UpdatedAt 
            });
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto request)
        {
            var email = HttpContext.Items["UserEmail"] as string;
            var users = await _unitOfWork.Users.FindAsync(u => u.Email == email);
            var user = users.FirstOrDefault();
            if (user == null) return NotFound();

            user.Username = request.Username ?? user.Username;
            user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
            user.DateOfBirth = request.DateOfBirth ?? user.DateOfBirth;
            user.ProfilePictureUrl = request.ProfilePictureUrl ?? user.ProfilePictureUrl;
            
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            return Ok(new { Message = "Profile updated." });
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchUser([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return BadRequest("Email is required.");
            
            var users = await _unitOfWork.Users.FindAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);
            var user = users.FirstOrDefault();
            
            if (user == null) return NotFound("User not found.");

            string? picUrl = null;
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                picUrl = _s3Service.GetPresignedUrl(user.ProfilePictureUrl, TimeSpan.FromHours(1));
            }

            return Ok(new { user.Id, user.Email, user.Username, ProfilePictureUrl = picUrl });
        }

        [HttpPost("deactivate")]
        public async Task<IActionResult> DeactivateAccount()
        {
            var email = HttpContext.Items["UserEmail"] as string;
            var users = await _unitOfWork.Users.FindAsync(u => u.Email == email);
            var user = users.FirstOrDefault();
            if (user == null) return NotFound();

            user.IsActive = false;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            return Ok(new { Message = "Account deactivated." });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            return Ok(users.Select(u => new { u.Id, u.Email, u.Username, u.Role, u.IsActive }));
        }
    }

    public class UpdateProfileDto { 
        public string? Username { get; set; } 
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}
