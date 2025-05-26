using marketimnet.Core.Entities;

namespace marketimnet.wepUI.Models
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; }
        public IEnumerable<Product> RelatedProducts { get; set; } = new List<Product>();
    }
} 