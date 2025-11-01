namespace Store.Models.ViewModels
{
    public class ProductListViewModel
    {
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public IEnumerable<Brand> Brands { get; set; } = new List<Brand>();

        public PagingInfo PagingInfo { get; set; } = new PagingInfo();
        public int CurrentCategory { get; set; }

    }
}
