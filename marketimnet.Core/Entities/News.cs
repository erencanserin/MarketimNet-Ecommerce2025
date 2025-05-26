using System.ComponentModel.DataAnnotations;

namespace marketimnet.Core.Entities
{
    public class News : IEntity
    {
        public News()
        {
            Title = string.Empty;
            Description = string.Empty;
            Image = string.Empty;
        }

        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public string Image { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
    }
}
