using webshop_back.Data.Models;
using webshop_back.DTOs;
using webshop_back.DTOs.User;

namespace webshop_back.Data.Mapping
{
    public static class MapperExtensions
    {
        public static UserDto ToDto(this User u) => new UserDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Role = u.Role.ToString(),
            ProfilePictureBase64 = u.ProfilePicture != null ? Convert.ToBase64String(u.ProfilePicture) : null
        };

        public static VehicleDto ToDto(this Vehicle v) => new VehicleDto
        {
            Id = v.Id,
            Make = v.Make,
            Model = v.Model,
            Description = v.Description,
            Price = v.Price,
            ImageBase64 = v.Image != null ? Convert.ToBase64String(v.Image) : null
        };

        public static OrderDto ToDto(this Order o) => new OrderDto
        {
            OrderId = o.OrderId,
            UserId = o.UserId,
            Amount = o.Amount,
            Currency = o.Currency,
            Status = o.Status,
            CreatedAt = o.CreatedAt
        };
    }
}
