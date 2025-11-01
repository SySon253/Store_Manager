using Store.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Store.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập Tên sản phẩm")]
        public string ProductName { get; set; }
        [Required, MinLength(4, ErrorMessage = "Yêu cầu nhập Mô tả sản phẩm")]
        public string ProductDescription { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập Giá sản phẩm")]
        [Range(0.01, double.MaxValue)]
        [Column(TypeName = "decimal(8,2)")]
        public decimal ProductPrice { get; set; }
        [Column(TypeName = "decimal(2,2)")]
        public decimal? ProductDiscount { get; set; }
        [Required, Range(1, int.MaxValue, ErrorMessage = "Chọn một thương hiệu")]
        public int BrandId { get; set; }
        [Required, Range(1, int.MaxValue, ErrorMessage = "Chọn một danh mục")]
        public int CategoryId { get; set; }
        public virtual Brand Brand { get; set; }
        public virtual Category Category { get; set; }
        public string ProductImage { get; set; } = "noimage.jpg";
        [NotMapped]
        [FileExtension]
        public IFormFile ImageUpload { get; set; }
        public bool IsTopSelling { get; set; }
        public bool IsArrived { get; set; }

    }
}
