using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Store.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        [Required]
        public string? UserName { get; set; }
        [Required]
        public string? UserEmail { get; set; }
        [Required]
        public string? UserPassword { get; set; }

        [NotMapped]
        [Compare("UserPassword", ErrorMessage = "Mật khẩu xác nhận không khớp!")]
        public string? ConfirmPassword { get; set; }

        public string? UserRole { get; set; }
    }
}
