using System.ComponentModel.DataAnnotations;

namespace Store.Models
{
    public class Brand
    {
        [Key]
        public int BrandId { get; set; }
        [Required]
        [StringLength(50)]
        public string BrandName { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
