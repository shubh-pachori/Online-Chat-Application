using System.Linq;
using System.Threading.Tasks;
using ChatApp.Core.Entities;
using ChatApp.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;

        public AuthController(IUnitOfWork unitOfWork, IAuthService authService)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Email == request.Email && u.PasswordHash == request.Password);
            var user = users.FirstOrDefault();
            
            if (user == null || !user.IsActive)
                return Unauthorized("Invalid credentials or inactive account.");

            var token = _authService.GenerateJwtToken(user);
            return Ok(new { Token = token, User = new { user.Id, user.Email, user.Username, user.Role } });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Email == request.Email);
            if (users.Any()) return BadRequest("Email already exists.");

            var newUser = new User
            {
                Email = request.Email,
                Username = request.Username,
                PasswordHash = request.Password, // Simple hash for demo
                Role = "User",
                IsActive = true
            };

            await _unitOfWork.Users.AddAsync(newUser);
            await _unitOfWork.CompleteAsync();

            return Ok(new { Message = "User registered successfully." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto request)
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Email == request.Email);
            var user = users.FirstOrDefault();
            if (user == null) return NotFound("User not found.");

            user.OtpCode = _authService.GenerateOtp();
            user.OtpExpiry = System.DateTime.UtcNow.AddMinutes(15);
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            // Typically send email here, for demo we just return the OTP
            return Ok(new { Message = "OTP sent to email.", Otp = user.OtpCode });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Email == request.Email);
            var user = users.FirstOrDefault();
            if (user == null) return NotFound("User not found.");

            if (user.OtpCode != request.Otp || user.OtpExpiry < System.DateTime.UtcNow)
                return BadRequest("Invalid or expired OTP.");

            user.PasswordHash = request.NewPassword;
            user.OtpCode = null;
            user.OtpExpiry = null;
            
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            return Ok(new { Message = "Password reset successfully." });
        }

        [HttpPost("request-reactivation")]
        public async Task<IActionResult> RequestReactivation([FromBody] ForgotPasswordDto request)
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Email == request.Email);
            var user = users.FirstOrDefault();
            if (user == null) return NotFound("User not found.");
            if (user.IsActive) return BadRequest("User is already active.");

            user.OtpCode = _authService.GenerateOtp();
            user.OtpExpiry = System.DateTime.UtcNow.AddMinutes(15);
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            return Ok(new { Message = "OTP sent to email for reactivation.", Otp = user.OtpCode });
        }

        [HttpPost("reactivate")]
        public async Task<IActionResult> Reactivate([FromBody] ReactivateDto request)
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Email == request.Email);
            var user = users.FirstOrDefault();
            if (user == null) return NotFound("User not found.");

            if (user.OtpCode != request.Otp || user.OtpExpiry < System.DateTime.UtcNow)
                return BadRequest("Invalid or expired OTP.");

            user.IsActive = true;
            user.OtpCode = null;
            user.OtpExpiry = null;
            
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            return Ok(new { Message = "Account reactivated successfully." });
        }
    }

    public class LoginDto { public string Email { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
    public class RegisterDto { public string Email { get; set; } = string.Empty; public string Username { get; set; } = string.Empty; public string Password { get; set; } = string.Empty; }
    public class ForgotPasswordDto { public string Email { get; set; } = string.Empty; }
    public class ResetPasswordDto { public string Email { get; set; } = string.Empty; public string Otp { get; set; } = string.Empty; public string NewPassword { get; set; } = string.Empty; }
    public class ReactivateDto { public string Email { get; set; } = string.Empty; public string Otp { get; set; } = string.Empty; }
}
