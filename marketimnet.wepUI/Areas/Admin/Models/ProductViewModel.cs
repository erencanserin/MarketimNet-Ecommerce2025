using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace marketimnet.wepUI.Areas.Admin.Models
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün adı zorunludur")]
        [StringLength(100, ErrorMessage = "Ürün adı en fazla 100 karakter olabilir")]
        [Display(Name = "Ürün Adı")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Fiyat zorunludur")]
        [Range(0.01, 1000000, ErrorMessage = "Fiyat 0'dan büyük olmalıdır")]
        [Display(Name = "Fiyat")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Ürün kodu zorunludur")]
        [StringLength(50, ErrorMessage = "Ürün kodu en fazla 50 karakter olabilir")]
        [Display(Name = "Ürün Kodu")]
        public string ProductCode { get; set; }

        [Display(Name = "Kategori")]
        public int? CategoryId { get; set; }

        [Display(Name = "Aktif Mi?")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Anasayfada Göster")]
        public bool IsHome { get; set; }

        [Range(0, 1000000, ErrorMessage = "İndirimli fiyat 0'dan küçük olamaz")]
        [Display(Name = "İndirimli Fiyat")]
        public decimal? DiscountedPrice { get; set; }

        [Display(Name = "Ürün Resmi")]
        public IFormFile? Image { get; set; }

        public string? ImageUrl { get; set; }

        public SelectList? Categories { get; set; }
    }
} 