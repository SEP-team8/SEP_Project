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
                // never map back password hash to DTO
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
                // optional additional mappings if DTO has them:
                .ForMember(d => d.PaymentId, opt => opt.MapFrom(s => s.PaymentId))
                .ForMember(d => d.MerchantId, opt => opt.MapFrom(s => s.MerchantId))
                .ForMember(d => d.PaymentUrl, opt => opt.MapFrom(s => s.PaymentUrl))
                .ForMember(d => d.GlobalTransactionId, opt => opt.MapFrom(s => s.GlobalTransactionId))
                .ForMember(d => d.ExpiresAt, opt => opt.MapFrom(s => s.ExpiresAt));

            // Reverse maps if you need to accept DTO -> Entity (careful with binary data)
            CreateMap<UserDto, User>()
                .ForMember(e => e.PasswordHash, opt => opt.Ignore()); // don't overwrite hash
            CreateMap<VehicleDto, Vehicle>()
                .ForMember(e => e.Image, opt => opt.MapFrom(d => d.ImageBase64));
        }
    }
}
