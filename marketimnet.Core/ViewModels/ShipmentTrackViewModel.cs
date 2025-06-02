namespace marketimnet.Core.ViewModels
{
    public class ShipmentTrackViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
    }
} 