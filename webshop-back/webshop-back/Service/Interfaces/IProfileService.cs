using webshop_back.DTOs.User;

namespace webshop_back.Service.Interfaces
{
    public interface IProfileService
    {
        Task<UserDto?> GetProfileAsync(int userId);
        Task<UserDto?> UpdateProfileAsync(int userId, UpdateUserRequest request);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);
        Task<UserDto?> UpdateProfilePictureAsync(int userId, IFormFile file);
    }
}
