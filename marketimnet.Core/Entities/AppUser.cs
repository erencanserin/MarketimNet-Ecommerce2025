using System.ComponentModel.DataAnnotations;

namespace marketimnet.Core.Entities
{
    public class AppUser : IEntity
    {
        public AppUser()
        {
            Name = string.Empty;
            Surname = string.Empty;
            Email = string.Empty;
            Password = string.Empty;
        }

        public int Id { get; set; }
        public Guid UserGuid { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(50)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(50)]
        public string Surname { get; set; }

        [Required(ErrorMessage = "Email alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        [StringLength(100)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        public string Password { get; set; }

        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
    }
}
