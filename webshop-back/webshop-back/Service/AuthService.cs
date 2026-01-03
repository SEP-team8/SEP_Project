using Microsoft.AspNetCore.Identity;
using webshop_back.Data;
using webshop_back.Data.Models;
using webshop_back.DTOs;
using webshop_back.DTOs.Auth;
using webshop_back.Helpers;
using Microsoft.EntityFrameworkCore;
using webshop_back.DTOs.User;


namespace webshop_back.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly TokenProvider _tokenProvider;

        public AuthService(AppDbContext context, TokenProvider tokenProvider)
        {
            _context = context;
            _tokenProvider = tokenProvider;
        }

        public async Task<ResponsePayload<AuthResponse>> Register(RegisterUserRequest request)
        {
            try
            {
                // check existing email or username
                if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                {
                    return new ResponsePayload<AuthResponse>
                    {
                        Status = ResponseStatus.BadRequest,
                        Message = "The email is already in use"
                    };
                }

                var user = new User
                {
                    Name = request.Name,
                    Email = request.Email,
                    //PasswordHash = hasher.HashPassword(user, request.Password),
                    Role = UserRole.User
                };

                var hasher = new PasswordHasher<User>();
                user.PasswordHash = hasher.HashPassword(user, request.Password);

                _context.Users.Add(user);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    var inner = dbEx.InnerException?.Message ?? dbEx.Message;
                    var entriesInfo = string.Join("; ", dbEx.Entries.Select(e => e.Entity.GetType().Name + (e.State.ToString())));
                    return new ResponsePayload<AuthResponse>
                    {
                        Status = ResponseStatus.InternalServerError,
                        Message = $"An error occurred while saving the entity changes. DB message: {inner}. Entries: {entriesInfo}"
                    };
                }

                // create token
                string token = _tokenProvider.Create(user);

                var authResponse = new AuthResponse
                {
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        Role = user.Role.ToString()
                    }
                };

                return new ResponsePayload<AuthResponse>
                {
                    Data = authResponse,
                    Status = ResponseStatus.Created,
                    Message = "Registration successful"
                };
            }
            catch (Exception ex)
            {
                return new ResponsePayload<AuthResponse>
                {
                    Status = ResponseStatus.InternalServerError,
                    Message = $"An error occurred while processing your request: {ex.Message} {ex.InnerException?.Message}"
                };
            }
        }

        public async Task<ResponsePayload<AuthResponse>> Login(LoginUserRequest request)
        {
            try
            {
                User? user = await _context.Users.AsNoTracking().SingleOrDefaultAsync(
                    u => u.Email == request.Email);

                if (user is null)
                {
                    return new ResponsePayload<AuthResponse>
                    {
                        Status = ResponseStatus.NotFound,
                        Message = "User not found"
                    };
                }

                var verification = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password);
                if (verification == PasswordVerificationResult.Failed)
                {
                    return new ResponsePayload<AuthResponse>
                    {
                        Status = ResponseStatus.Unauthorized,
                        Message = "Invalid password"
                    };
                }

                string token = _tokenProvider.Create(user);

                var authResponse = new AuthResponse
                {
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Email = user.Email,
                        Role = user.Role.ToString()
                    }
                };

                return new ResponsePayload<AuthResponse>
                {
                    Data = authResponse,
                    Status = ResponseStatus.OK,
                    Message = "Login successful"
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
