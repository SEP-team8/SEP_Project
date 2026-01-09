using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _tenantProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));
            _configuration = configuration;
            _env = env;
        }

        private Guid? ResolveMerchantId()
        {
            return _tenantProvider?.CurrentMerchantId ?? _tenantProvider?.CurrentMerchantId;
            // simpler: return _tenantProvider?.CurrentMerchantId;
        }

        public async Task<ResponsePayload<AuthResponse>> Register(RegisterUserRequest request)
        {
            try
            {
                var currentMerchant = ResolveMerchantId();

                if (!currentMerchant.HasValue)
                {
                    return new ResponsePayload<AuthResponse>
                    {
                        Status = ResponseStatus.BadRequest,
                        Message = "Merchant context is missing"
                    };
                }

                var merchantId = currentMerchant.Value;

                if (await _context.Users.AnyAsync(u => u.Email == request.Email && u.MerchantId == merchantId))
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
                    MerchantId = merchantId
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

                if (!currentMerchant.HasValue)
                {
                    return new ResponsePayload<AuthResponse>
                    {
                        Status = ResponseStatus.BadRequest,
                        Message = "Merchant context is missing"
                    };
                }

                var merchantId = currentMerchant.Value;

                var user = await _context.Users.AsNoTracking()
                    .SingleOrDefaultAsync(u =>
                        u.Email == request.Email &&
                        u.MerchantId == merchantId);

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
