namespace Store.Models.ViewModels
{
    public class CheckoutViewModel
    {
        public Cart Cart { get; set; } = new Cart();
        public User User { get; set; } = new User();
        public decimal TotalAmount { get; set; }
    }
}
