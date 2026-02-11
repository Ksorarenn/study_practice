using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PC_Builder.Core.Models
{
    public class Component
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; }
        public string Name { get; set; }
        public string Description { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string Socket { get; set; } = "";
        public string FormFactor { get; set; } = "";
        public string MemoryType { get; set; } = "";

        // Сделайте эти поля nullable (int?) для опциональных значений
        public int? MaxMemory { get; set; }
        public int? PowerSupply { get; set; }  // <-- ИЗМЕНИТЬ на int?
        public int? MemorySlots { get; set; }
        public int? MaxMemoryPerSlot { get; set; }
        public int? TDP { get; set; }
        public int? Length { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }

        [NotMapped]
        public string Specifications => GetSpecifications();

        private string GetSpecifications()
        {
            var specs = new System.Text.StringBuilder();

            if (!string.IsNullOrEmpty(Socket) && Socket.Trim() != "")
                specs.Append($"Сокет: {Socket} | ");

            if (!string.IsNullOrEmpty(FormFactor) && FormFactor.Trim() != "")
                specs.Append($"Форм-фактор: {FormFactor} | ");

            if (!string.IsNullOrEmpty(MemoryType) && MemoryType.Trim() != "")
                specs.Append($"Тип памяти: {MemoryType} | ");

            if (PowerSupply.HasValue && PowerSupply > 0)  // <-- Используем .HasValue
                specs.Append($"Питание: {PowerSupply}Вт | ");

            if (MaxMemory.HasValue && MaxMemory > 0)
                specs.Append($"Макс. память: {MaxMemory}GB | ");

            if (TDP.HasValue && TDP > 0)
                specs.Append($"TDP: {TDP}Вт | ");

            if (MemorySlots.HasValue && MemorySlots > 0)
                specs.Append($"Слотов памяти: {MemorySlots}");

            string result = specs.ToString().TrimEnd(' ', '|');

            // Убираем лишние разделители если они остались
            result = result.Replace(" |  | ", " | ");
            result = result.Replace("  ", " ");

            return string.IsNullOrWhiteSpace(result) ? "Характеристики не указаны" : result;
        }
    }
}