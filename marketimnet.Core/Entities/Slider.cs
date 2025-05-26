using System.ComponentModel.DataAnnotations;

namespace marketimnet.Core.Entities
{
    public class Slider : IEntity
    {
        public Slider()
        {
            Title = string.Empty;
            Description = string.Empty;
            Image = string.Empty;
            Link = string.Empty;
        }

        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(100)]
        public string Image { get; set; }

        [MaxLength(100)]
        public string? Link { get; set; }
        public int OrderNo { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
    }
}
