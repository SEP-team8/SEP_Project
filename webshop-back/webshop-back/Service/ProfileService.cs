using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using webshop_back.Data;
using webshop_back.Data.Models;
using webshop_back.DTOs.User;
using webshop_back.Service.Interfaces;

namespace webshop_back.Service
{
    public class ProfileService : IProfileService
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<User> _hasher;

        public ProfileService(AppDbContext context)
        {
            _context = context;
            _hasher = new PasswordHasher<User>();
        }

        public async Task<UserDto?> GetProfileAsync(int userId)
        {
            var user = await _context.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;
            return MapToDto(user);
        }

        public async Task<UserDto?> UpdateProfileAsync(int userId, UpdateUserRequest request)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            if (!string.IsNullOrWhiteSpace(request.Name))
                user.Name = request.Name;

            await _context.SaveChangesAsync();
            return MapToDto(user);
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null) return false;

            var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
            if (verify == PasswordVerificationResult.Failed)
                return false;

            user.PasswordHash = _hasher.HashPassword(user, request.NewPassword);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserDto?> UpdateProfilePictureAsync(int userId, IFormFile file)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == userId);
            if (user == null) return null;

            if (file == null || file.Length == 0) return MapToDto(user);

            const long maxBytes = 5 * 1024 * 1024;
            if (file.Length > maxBytes)
                throw new InvalidOperationException("File too large. Max 5MB.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            user.ProfilePicture = ms.ToArray();

            await _context.SaveChangesAsync();
            return MapToDto(user);
        }

        private UserDto MapToDto(User user)
        {
            string? base64 = null;
            if (user.ProfilePicture != null && user.ProfilePicture.Length > 0)
            {
                base64 = Convert.ToBase64String(user.ProfilePicture);
            }

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                ProfilePictureBase64 = base64,
                Role = user.Role.ToString()
            };
        }
    }
}

