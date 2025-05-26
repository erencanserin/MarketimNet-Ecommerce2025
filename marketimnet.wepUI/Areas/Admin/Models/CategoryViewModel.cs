using Microsoft.AspNetCore.Mvc.Rendering;using System.ComponentModel.DataAnnotations;using Microsoft.AspNetCore.Http;

namespace marketimnet.wepUI.Areas.Admin.Models
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı zorunludur")]
        [StringLength(50, ErrorMessage = "Kategori adı en fazla 50 karakter olabilir")]
        [Display(Name = "Kategori Adı")]
        public string Name { get; set; }

        [Display(Name = "Açıklama")]
        public string Description { get; set; }

        [Display(Name = "Üst Kategori")]
        public int? ParentId { get; set; }

        [Display(Name = "Sıra No")]
        public int OrderNo { get; set; } = 0;

        [Display(Name = "Resim")]
        public string? Image { get; set; }

        [Display(Name = "Aktif Mi?")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Üst Menüde Göster")]
        public bool IsTopMenu { get; set; } = false;

        public SelectList? ParentCategories { get; set; }
        
        [Display(Name = "Resim")]
        public IFormFile? ImageFile { get; set; }
    }
} 