using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Store.Models
{
    public class OrderDetail
    {
        [Key]
        public int OrderDetailId { get; set; }

        [Required]
        [ForeignKey("Order")]
        public int OrderId { get; set; }

        [Required]
        [ForeignKey("Product")]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        // Quan hệ n-1: Chi tiết đơn hàng thuộc về một đơn hàng
        public virtual Order Order { get; set; } = null!;

        // Quan hệ n-1: Chi tiết đơn hàng có 1 sản phẩm
        public virtual Product Product { get; set; } = null!;
    }
}
