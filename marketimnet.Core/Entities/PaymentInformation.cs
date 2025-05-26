using System.ComponentModel.DataAnnotations;

namespace marketimnet.Core.Entities
{
    public class PaymentInformation : IEntity
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kart sahibinin adı soyadı zorunludur")]
        [Display(Name = "Kart Sahibinin Adı Soyadı")]
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

        public int OrderId { get; set; }
        public virtual Order Order { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
    }
} 