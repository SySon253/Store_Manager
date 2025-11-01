using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Store.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public int? UserId { get; set; }   // ✅ Sửa: cho phép null

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [StringLength(50)]
        public string? Status { get; set; } = "Đang xử lý";

        public virtual List<OrderDetail> Details { get; set; } = new();

        public virtual User? User { get; set; }  // ✅ cũng cho phép null
    }

}
