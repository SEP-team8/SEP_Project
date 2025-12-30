namespace webshop_back.Models
{
    public class User
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Name { get; set; }
    }
}
