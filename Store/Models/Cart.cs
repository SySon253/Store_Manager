using System.Text.Json.Serialization;

namespace Store.Models
{
    public class Cart
    {
        public List<CartLine> Lines { get; set; } = new List<CartLine>();
        public void AddItem(Product product, int quantity)
        {
            var line = Lines.FirstOrDefault(p => p.ProductId == product.ProductId);
            if (line == null)
            {
                Lines.Add(new CartLine
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    ProductPrice = (decimal)product.ProductPrice,
                    ProductDiscount = (decimal)product.ProductDiscount,
                    ProductImage = product.ProductImage,
                    Quantity = quantity
                });
            }
            else
            {
                line.Quantity += quantity;
            }
        }

        public void RemoveLine(Product product) =>
            Lines.RemoveAll(l => l.ProductId == product.ProductId);

        public decimal ComputeTotalValues()
        {
            return Lines.Sum(e => e.ProductPrice * (1 - e.ProductDiscount) * e.Quantity);
        }

        public void Clear() => Lines.Clear();
    }
    public class CartLine
    {
        public int CartLineId { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public decimal ProductDiscount { get; set; }
        public string ProductImage { get; set; }

        public int Quantity { get; set; }

        // Không serialize Product để tránh vòng lặp khi lưu session
        [JsonIgnore]
        public Product Product { get; set; }
    }
}
