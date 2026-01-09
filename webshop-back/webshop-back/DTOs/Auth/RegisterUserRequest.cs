using System.ComponentModel.DataAnnotations;

public class RegisterUserRequest
{

    [Required]
    [MaxLength(255)]
    public required string Name { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public required string Email { get; set; }
    
    [Required]
    [MinLength(6)]
    public required string Password { get; set; }

}