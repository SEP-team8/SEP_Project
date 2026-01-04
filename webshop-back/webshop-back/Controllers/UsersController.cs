using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using webshop_back.DTOs.User;
using webshop_back.Service.Interfaces;

namespace webshop_back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public UsersController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        // Helper: extract user id from token claims (tries multiple claim names)
        private int? GetUserIdFromClaims()
        {
            var user = HttpContext.User;
            if (user == null) return null;

            // common possibilities: "id", ClaimTypes.NameIdentifier, "sub"
            var claim = user.FindFirst("id") ?? user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("sub");
            if (claim == null) return null;

            if (int.TryParse(claim.Value, out var id)) return id;
            return null;
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<PublicUserDto>> Me()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _profileService.GetProfileAsync(userId);
            if (user == null) return NotFound();

            return new PublicUserDto
            {
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                ProfilePictureBase64 = user.ProfilePictureBase64
            };
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null) return Unauthorized();

            try
            {
                var dto = await _profileService.UpdateProfileAsync(userId.Value, request);
                if (dto == null) return NotFound();
                return Ok(dto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null) return Unauthorized();

            var ok = await _profileService.ChangePasswordAsync(userId.Value, request);
            if (!ok) return BadRequest(new { error = "Current password is incorrect." });

            return Ok(new { message = "Password changed" });
        }

        [Authorize]
        [HttpPost("picture")]
        [RequestSizeLimit(6 * 1024 * 1024)] // limit ~6MB
        public async Task<IActionResult> UploadPicture([FromForm] IFormFile file)
        {
            var userId = GetUserIdFromClaims();
            if (userId == null) return Unauthorized();

            try
            {
                var dto = await _profileService.UpdateProfilePictureAsync(userId.Value, file);
                if (dto == null) return NotFound();
                return Ok(dto);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}