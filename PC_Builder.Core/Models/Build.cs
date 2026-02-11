using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PC_Builder.Core.Models
{
    public class Build
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Draft";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [NotMapped] // Это свойство не хранится в БД
        public List<Component> Components { get; set; } = new List<Component>();
    }
}