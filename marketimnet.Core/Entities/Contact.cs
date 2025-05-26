using System.ComponentModel.DataAnnotations;

namespace marketimnet.Core.Entities
{
    public class Contact : IEntity
    {
        public Contact()
        {
            Name = string.Empty;
            Email = string.Empty;
            Message = string.Empty;
            Subject = string.Empty;
            Phone = string.Empty;
        }

        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [Required]
        [MaxLength(50)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(500)]
        public string Message { get; set; }

        [MaxLength(20)]
        public string Phone { get; set; }

        [MaxLength(100)]
        public string Subject { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
    }
}
