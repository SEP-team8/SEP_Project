namespace webshop_back.DTOs.User
{
    public class PublicUserDto
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string? ProfilePictureBase64 { get; set; }
        public string Role { get; set; } = "User";
    }
}
