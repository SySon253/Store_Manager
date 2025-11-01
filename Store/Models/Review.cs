using System.ComponentModel.DataAnnotations;

namespace Store.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        public string UserName { get; set; } = string.Empty;

        [Required]
        public int Rating { get; set; }

        [Required]
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
