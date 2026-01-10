namespace webshop_back.Data.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public byte[]? ProfilePicture { get; set; }
        public UserRole Role { get; set; } = UserRole.User;

        public Guid MerchantId { get; set; }
    }
}
