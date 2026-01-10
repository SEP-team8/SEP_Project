using PSPbackend.Models.Enums;

namespace PSPbackend.DTOs
{
    public class OrderResponseDto
    {
        public decimal Amount { get; set; }
        public Currency Currency { get; set; }
    }
}
