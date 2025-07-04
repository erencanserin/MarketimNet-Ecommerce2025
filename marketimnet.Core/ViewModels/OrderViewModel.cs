using System.ComponentModel.DataAnnotations;
using marketimnet.Core.Entities;

namespace marketimnet.Core.ViewModels
{
    public class OrderViewModel
    {
        [Display(Name = "Sepet")]
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Telefon numarası zorunludur")]
        [Display(Name = "Telefon Numarası")]
        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Geçerli bir telefon numarası giriniz (5XX XXX XXXX)")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [Display(Name = "E-posta Adresi")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Teslimat adresi zorunludur")]
        [Display(Name = "Teslimat Adresi")]
        public string ShippingAddress { get; set; }

        [Display(Name = "Sipariş Notu")]
        public string Notes { get; set; }

        [Required(ErrorMessage = "Kart sahibinin adı soyadı zorunludur")]
        [Display(Name = "Kart Üzerindeki İsim")]
        public string CardHolderName { get; set; }

        [Required(ErrorMessage = "Kart numarası zorunludur")]
        [Display(Name = "Kart Numarası")]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Geçerli bir kart numarası giriniz")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Son kullanma tarihi zorunludur")]
        [Display(Name = "Son Kullanma Tarihi (AA/YY)")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/([0-9]{2})$", ErrorMessage = "Geçerli bir son kullanma tarihi giriniz (AA/YY)")]
        public string ExpiryDate { get; set; }

        [Required(ErrorMessage = "CVC kodu zorunludur")]
        [Display(Name = "CVC")]
        [RegularExpression(@"^\d{3}$", ErrorMessage = "Geçerli bir CVC kodu giriniz")]
        public string Cvc { get; set; }
    }
} 