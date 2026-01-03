using System.ComponentModel.DataAnnotations;

public class LoginUserRequest
{
    [MaxLength(255)]
    public required string Email { get; set; }

    [Required]
    [MinLength(6)]
    public required string Password { get; set; }
}