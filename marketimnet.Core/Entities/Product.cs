using System.ComponentModel.DataAnnotations;

namespace marketimnet.Core.Entities
{
    public class Product : IEntity
    {
        public Product()
        {
            Name = string.Empty;
            Description = string.Empty;
            ImageUrl = string.Empty;
            ProductCode = string.Empty;
        }

        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün adı zorunludur")]
        [StringLength(100, ErrorMessage = "Ürün adı en fazla 100 karakter olabilir")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Fiyat zorunludur")]
        [Range(0.01, 1000000, ErrorMessage = "Fiyat 0'dan büyük olmalıdır")]
        public decimal Price { get; set; }

        [Range(0, 1000000, ErrorMessage = "İndirimli fiyat 0'dan küçük olamaz")]
        public decimal? DiscountedPrice { get; set; }

        [StringLength(50, ErrorMessage = "Ürün kodu en fazla 50 karakter olabilir")]
        public string ProductCode { get; set; }

        public int? CategoryId { get; set; }
        public Category Category { get; set; }
        
        [StringLength(100, ErrorMessage = "Resim yolu en fazla 100 karakter olabilir")]
        public string ImageUrl { get; set; }
        
        [StringLength(100, ErrorMessage = "Resim yolu en fazla 100 karakter olabilir")]
        public string Image { get => ImageUrl; set => ImageUrl = value; }
        
        public bool IsActive { get; set; } = true;
        public bool IsHome { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
    }
}
