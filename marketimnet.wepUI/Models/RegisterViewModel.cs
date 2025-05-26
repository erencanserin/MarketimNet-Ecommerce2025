using System.ComponentModel.DataAnnotations;

namespace marketimnet.wepUI.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad zorunludur")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Soyad zorunludur")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir")]
        public string Surname { get; set; }

        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [StringLength(50, ErrorMessage = "E-posta en fazla 50 karakter olabilir")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [StringLength(50, ErrorMessage = "Telefon numarası en fazla 50 karakter olabilir")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Şifre zorunludur")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Şifre en az 6, en fazla 50 karakter olabilir")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Şifre tekrarı zorunludur")]
        [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor")]
        [DataType(DataType.Password)]
        public string RepeatPassword { get; set; }
    }
} 