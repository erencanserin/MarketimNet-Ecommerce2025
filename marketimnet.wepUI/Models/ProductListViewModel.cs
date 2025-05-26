using marketimnet.Core.Entities;

namespace marketimnet.wepUI.Models
{
    public class ProductListViewModel
    {
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public Category? CurrentCategory { get; set; }
        public string? SearchTerm { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public bool IsDiscountedPage { get; set; }
    }
} 