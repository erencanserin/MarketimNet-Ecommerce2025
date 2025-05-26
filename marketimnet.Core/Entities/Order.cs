using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace marketimnet.Core.Entities
{
    public class Order : BaseEntity
    {
        public string OrderNumber { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
        
        [Required(ErrorMessage = "Ad Soyad alanı zorunludur")]
        [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "E-posta alanı zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Telefon alanı zorunludur")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Adres alanı zorunludur")]
        [StringLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir")]
        public string Address { get; set; }

        [Required(ErrorMessage = "İl alanı zorunludur")]
        public string City { get; set; }

        [Required(ErrorMessage = "İlçe alanı zorunludur")]
        public string District { get; set; }

        [Required(ErrorMessage = "Posta kodu zorunludur")]
        public string ZipCode { get; set; }

        public string? Note { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public DateTime? ShippingDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        public string Status { get; set; } = "Beklemede";

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Ödeme bilgileri - Veritabanında saklanmayacak
        [NotMapped]
        public PaymentInformation PaymentInformation { get; set; }

        [Required(ErrorMessage = "Kart üzerindeki isim zorunludur")]
        [NotMapped]
        public string CardHolderName { get; set; }

        [Required(ErrorMessage = "Kart numarası zorunludur")]
        [CreditCard(ErrorMessage = "Geçerli bir kredi kartı numarası giriniz")]
        [NotMapped]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "Son kullanma ayı zorunludur")]
        [Range(1, 12, ErrorMessage = "Geçerli bir ay giriniz (1-12)")]
        [NotMapped]
        public int ExpiryMonth { get; set; }

        [Required(ErrorMessage = "Son kullanma yılı zorunludur")]
        [Range(2024, 2034, ErrorMessage = "Geçerli bir yıl giriniz (2024-2034)")]
        [NotMapped]
        public int ExpiryYear { get; set; }

        [Required(ErrorMessage = "CVC kodu zorunludur")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "CVC kodu 3 haneli olmalıdır")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "CVC kodu sadece rakamlardan oluşmalıdır")]
        [NotMapped]
        public string Cvc { get; set; }
    }
} 