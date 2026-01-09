using AutoMapper;
using webshop_back.Data.Models;
using webshop_back.DTOs;
using webshop_back.DTOs.User;

namespace webshop_back.Data.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User -> UserDto
            CreateMap<User, UserDto>()
                .ForMember(d => d.ProfilePictureBase64, opt => opt.MapFrom(s => s.ProfilePicture))
                .ForMember(d => d.Role, opt => opt.MapFrom(s => s.Role.ToString()))
                .ForSourceMember(s => s.PasswordHash, opt => opt.DoNotValidate());

            // Vehicle -> VehicleDto
            CreateMap<Vehicle, VehicleDto>()
                .ForMember(d => d.ImageBase64, opt => opt.MapFrom(s => s.Image));

            // Order -> OrderDto (if OrderDto exists in your DTOs)
            CreateMap<Order, OrderDto>()
                .ForMember(d => d.OrderId, opt => opt.MapFrom(s => s.OrderId))
                .ForMember(d => d.UserId, opt => opt.MapFrom(s => s.UserId))
                .ForMember(d => d.Amount, opt => opt.MapFrom(s => s.Amount))
                .ForMember(d => d.Currency, opt => opt.MapFrom(s => s.Currency))
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status))
                .ForMember(d => d.CreatedAt, opt => opt.MapFrom(s => s.CreatedAt))
                .ForMember(d => d.ExpiresAt, opt => opt.MapFrom(s => s.ExpiresAt));


            CreateMap<UserDto, User>()
                .ForMember(e => e.PasswordHash, opt => opt.Ignore());
            CreateMap<VehicleDto, Vehicle>()
                .ForMember(e => e.Image, opt => opt.MapFrom(d => d.ImageBase64));
        }
    }
}
