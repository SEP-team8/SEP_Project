
using webshop_back.DTOs.Auth;

public interface IAuthService
{
    Task<ResponsePayload<AuthResponse>> Register(RegisterUserRequest request);
    Task<ResponsePayload<AuthResponse>> Login(LoginUserRequest request);
}
