using System;

namespace marketimnet.Core.ViewModels
{
    public class OrderListViewModel
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
    }
} 