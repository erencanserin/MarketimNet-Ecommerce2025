using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace marketimnet.Core.Entities
{
    public class OrderItem : IEntity
    {
        public int Id { get; set; }
        
        [Required]
        public int OrderId { get; set; }
        
        public virtual Order Order { get; set; }
        
        [Required]
        public int ProductId { get; set; }
        
        public virtual Product Product { get; set; }

        [NotMapped]
        public string ProductName => Product?.Name;
        
        public int Quantity { get; set; } = 1;
        
        public decimal UnitPrice { get; set; }
        
        public decimal TotalPrice { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public DateTime? UpdatedDate { get; set; }
    }
} 