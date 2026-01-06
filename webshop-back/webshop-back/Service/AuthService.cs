using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using webshop_back.Data;
using webshop_back.Data.Models;
using webshop_back.DTOs.Auth;
using webshop_back.Helpers;
using webshop_back.DTOs.User;

namespace webshop_back.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly TokenProvider _tokenProvider;
        private readonly ITenantProvider _tenantProvider;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _env;

        public AuthService(
            AppDbContext context,
            TokenProvider tokenProvider,
            ITenantProvider tenantProvider,
            IConfiguration configuration,
            IHostEnvironment env)
        {
            _context = context;
            _tokenProvider = tokenProvider;
            _tenantProvider = tenantProvider;
            _configuration = configuration;
            _env = env;
        }

        private string? ResolveMerchantId()
        {
            // 1. pokušaj iz headera
            var merchantId = _tenantProvider?.CurrentMerchantId;

            if (!string.IsNullOrWhiteSpace(merchantId))
                return merchantId;

            // 2. fallback samo u Development okruženju
            if (_env.IsDevelopment())
            {
                return _configuration["Dev:DefaultMerchantId"];
            }

            return null;
        }

        public async Task<ResponsePayload<AuthResponse>> Register(RegisterUserRequest request)
        {
            try
            {
                var currentMerchant = ResolveMerchantId();
                if (string.IsNullOrEmpty(currentMerchant))
                {
                    return new ResponsePayload<AuthResponse>
                    {
                        Status = ResponseStatus.BadRequest,
                        Message = "Merchant context is missing"
                    };
                }

                if (await _context.Users.AnyAsync(
                        u => u.Email == request.Email && u.MerchantId == currentMerchant))
                {
                    return new ResponsePayload<AuthResponse>
                    {
                        Status = ResponseStatus.BadRequest,
                        Message = "The email is already in use for this merchant"
                    };
                }

                var user = new User
                {
                    Name = request.Name,
                    Email = request.Email,
                    Role = UserRole.User,
                    MerchantId = currentMerchant
                };

                var hasher = new PasswordHasher<User>();
                user.PasswordHash = hasher.HashPassword(user, request.Password);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                string token = _tokenProvider.Create(user);

                return new ResponsePayload<AuthResponse>
                {
                    Status = ResponseStatus.Created,
                    Message = "Registration successful",
                    Data = new AuthResponse
                    {
                        Token = token,
                        User = new UserDto
                        {
                            Id = user.Id,
                            Name = user.Name,
                            Email = user.Email,
                            Role = user.Role.ToString()
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponsePayload<AuthResponse>
                {
                    Status = ResponseStatus.InternalServerError,
                    Message = $"An error occurred while processing your request: {ex.Message}"
                };
            }
        }

        public async Task<ResponsePayload<AuthResponse>> Login(LoginUserRequest request)
        {
            try
            {
                var currentMerchant = ResolveMerchantId();
                if (string.IsNullOrEmpty(currentMerchant))
                {
                    return new ResponsePayload<AuthResponse>
                    {
                        Status = ResponseStatus.BadRequest,
                        Message = "Merchant context is missing"
                    };
                }

                var user = await _context.Users.AsNoTracking()
                    .SingleOrDefaultAsync(u =>
                        u.Email == request.Email &&
                        u.MerchantId == currentMerchant);

                if (user is null)
                {
                    return new ResponsePayload<AuthResponse>
                    {
                        Status = ResponseStatus.NotFound,
                        Message = "User not found for this merchant"
                    };
                }

                var verification = new PasswordHasher<User>()
                    .VerifyHashedPassword(user, user.PasswordHash, request.Password);

                if (verification == PasswordVerificationResult.Failed)
                {
                    return new ResponsePayload<AuthResponse>
                    {
                        Status = ResponseStatus.Unauthorized,
                        Message = "Invalid password"
                    };
                }

                string token = _tokenProvider.Create(user);

                return new ResponsePayload<AuthResponse>
                {
                    Status = ResponseStatus.OK,
                    Message = "Login successful",
                    Data = new AuthResponse
                    {
                        Token = token,
                        User = new UserDto
                        {
                            Id = user.Id,
                            Name = user.Name,
                            Email = user.Email,
                            Role = user.Role.ToString()
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponsePayload<AuthResponse>
                {
                    Status = ResponseStatus.InternalServerError,
                    Message = $"An error occurred while processing your request: {ex.Message}"
                };
            }
        }
    }
}
