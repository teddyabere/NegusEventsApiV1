
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Win32;
using NegusEventsApi.DTO;
using NegusEventsApi.Models.User;
using NegusEventsApi.Services;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NegusEventsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        public UserController(IUserService userService, IConfiguration configuration)
        {
            _userService = userService;
            _configuration = configuration;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="user">The user object containing user details.</param>
        /// <returns>A success message if registration is successful.</returns>
        [HttpPost("sign-up/register")]
        public async Task<IActionResult> Register(Users user)
        {
            var existingUser = await _userService.GetByEmailAsync(user.Email);
            if (existingUser != null)
                return BadRequest("Email is already in use.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            await _userService.CreateUserAsync(user);
            return Ok("User registered successfully.");
        }


        /// <summary>
        /// Logs in a user and generates a JWT token.
        /// </summary>
        /// <param name="loginRequest">The login request containing email and password.</param>
        /// <returns>A JWT token if login is successful.</returns>
        [HttpPost("user-login")]
        public async Task<IActionResult> LogIn([FromBody] NLoginRequest loginRequest)
        {
            var existingUser = await _userService.GetByEmailAsync(loginRequest.Email);
            if (existingUser != null && BCrypt.Net.BCrypt.Verify(loginRequest.Password, existingUser.Password))
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Email, loginRequest.Email),
                    new Claim(ClaimTypes.Role, existingUser.Role),
                    new Claim("UserId", existingUser.Id),
                    new Claim("Name", existingUser.Name),
                    new Claim("Status", existingUser.Status)
                };

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Issuer"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: credentials
                ));

                return Ok(token);
            }
            return Unauthorized("Invalid username or password");
        }

        /// <summary>
        /// Gets a user by email.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <returns>The user object if found.</returns>
        [HttpGet("get-user-by-email")]
        [Authorize(Roles = "Admin,Organizer")]
        public async Task<IActionResult> GetUserbyEmailid(string email)
        {
            var existingUser = await _userService.GetByEmailAsync(email);
            if (existingUser == null)
                throw new Exception("User with the specified email doesn't exist.");

            return Ok(existingUser);
        }

        /// <summary>
        /// Gets a list of pending organizers.
        /// </summary>
        /// <returns>A list of pending organizers.</returns>
        [HttpGet("get-pending-organizers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingOrganizers()
        {
            var pendingOrganizers = await _userService.GetPendingOrganizers();
            return Ok(pendingOrganizers);
        }

        /// <summary>
        /// Gets a list of approved organizers.
        /// </summary>
        /// <returns>A list of approved organizers.</returns>
        [HttpGet("get-approved-organizers")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetApprovedOrganizers()
        {
            var approvedOrganizers = await _userService.GetApprovedOrganizers();
            return Ok(approvedOrganizers);
        }

        /// <summary>
        /// Gets a list of approved users.
        /// </summary>
        /// <returns>A list of approved organizers.</returns>
        [HttpGet("get-all-approved-users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsersOrganizers()
        {
            var approvedOrganizers = await _userService.GetAllUsersOrganizers();
            return Ok(approvedOrganizers);
        }

        /// <summary>
        /// Gets a list of attendee users.
        /// </summary>
        /// <returns>A list of attendees.</returns>
        [HttpGet("get-all-attendees")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAttendees()
        {
            var approvedOrganizers = await _userService.GetAllAttendees();
            return Ok(approvedOrganizers);
        }
        /// <summary>
        /// Approves an organizer.
        /// </summary>
        /// <param name="userId">The user ID of the organizer to approve.</param>
        /// <returns>A success message if approval is successful.</returns>
        [HttpPost("approve-Organizer")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveOrganizer(string userId)
        {
            var user = await _userService.GetByIdAsync(userId);
            if (user == null || user.Role != "Organizer")
                return BadRequest("Invalid user or role.");

            await _userService.ApproveOrgannizer(user);
            return Ok("Organizer approved successfully.");
        }

        /// <summary>
        /// Resets a user's password.
        /// </summary>
        /// <param name="model">The reset password request containing email, token, and new password.</param>
        /// <returns>A success message if the password is reset successfully.</returns>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetForgotPasswordRequest model)
        {
            var user = await _userService.GetByEmailAsync(model.Email);
            if (user == null)
                return NotFound("User not found.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            await _userService.ResetPasswordAsync(user.Id, user, model.Token, user.Password);

            return Ok("Password has been reset successfully.");
        }
    }
}

