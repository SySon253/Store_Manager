namespace Store.Models.ViewModels
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; }
        public List<Review> Reviews { get; set; } = new List<Review>();
        public Review NewReview { get; set; } = new Review();

    }
}
