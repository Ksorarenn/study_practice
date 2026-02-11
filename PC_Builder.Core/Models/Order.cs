using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PC_Builder.Core.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int BuildId { get; set; } // Исправлено с buildid на BuildId
        public int UserId { get; set; }
        public string Status { get; set; } = "Pending";
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
