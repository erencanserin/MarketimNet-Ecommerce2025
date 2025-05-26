using System.ComponentModel.DataAnnotations;

namespace marketimnet.Core.Entities
{
    public class Category : IEntity
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı zorunludur")]
        [StringLength(50, ErrorMessage = "Kategori adı en fazla 50 karakter olabilir")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string? Image { get; set; }

        public bool IsActive { get; set; } = true;
        
        public bool IsTopMenu { get; set; } = false;
        
        public int OrderNo { get; set; } = 0;
        
        public int? ParentId { get; set; }
        
        public Category? Parent { get; set; }
        
        public ICollection<Category> SubCategories { get; set; } = new List<Category>();
        
        public List<Product> Products { get; set; } = new List<Product>();
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedDate { get; set; }
    }
}
