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

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Adres alanı zorunludur")]
        [StringLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir")]
        public string Address { get; set; }

        [StringLength(50, ErrorMessage = "İl en fazla 50 karakter olabilir")]
        public string? City { get; set; }

        [StringLength(50, ErrorMessage = "İlçe en fazla 50 karakter olabilir")]
        public string? District { get; set; }

        [StringLength(10, ErrorMessage = "Posta kodu en fazla 10 karakter olabilir")]
        public string? ZipCode { get; set; }

        public string? Note { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public DateTime? ShippingDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Beklemede";

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Ödeme bilgileri - Veritabanında saklanmayacak
        [NotMapped]
        public PaymentInformation PaymentInformation { get; set; }
    }
} 