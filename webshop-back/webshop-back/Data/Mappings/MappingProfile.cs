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

            // Reverse maps if you need to accept DTO -> Entity (careful with binary data)
            CreateMap<UserDto, User>()
                .ForMember(e => e.PasswordHash, opt => opt.Ignore()); // don't overwrite hash
            CreateMap<VehicleDto, Vehicle>()
                .ForMember(e => e.Image, opt => opt.MapFrom(d => d.ImageBase64));
        }
    }
}
