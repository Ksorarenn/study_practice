using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PC_Builder.Core.Models;

namespace PC_Builder.Core.Services
{
    public class CompatibilityService
    {
        public class CompatibilityResult
        {
            public bool IsCompatible { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
            public List<string> Warnings { get; set; } = new List<string>();
        }

        public CompatibilityResult CheckCompatibility(List<Component> components)
        {
            var result = new CompatibilityResult { IsCompatible = true };

            // Используем Category.Name вместо Category
            var motherboard = components.FirstOrDefault(c => c.Category?.Name == "Motherboard");
            var cpu = components.FirstOrDefault(c => c.Category?.Name == "CPU");
            var ram = components.FirstOrDefault(c => c.Category?.Name == "RAM");
            var caseComp = components.FirstOrDefault(c => c.Category?.Name == "Case");
            var psu = components.FirstOrDefault(c => c.Category?.Name == "PSU");

            // Проверка обязательных компонентов
            if (cpu == null) result.Errors.Add("Не выбран процессор");
            if (motherboard == null) result.Errors.Add("Не выбрана материнская плата");
            if (ram == null) result.Errors.Add("Не выбрана оперативная память");
            if (psu == null) result.Errors.Add("Не выбран блок питания");

            if (result.Errors.Any())
            {
                result.IsCompatible = false;
                return result;
            }

            // Проверка CPU + Motherboard
            if (cpu != null && motherboard != null)
            {
                if (cpu.Socket != motherboard.Socket)
                {
                    result.Errors.Add($"Несовместимость сокетов: CPU ({cpu.Socket}) ≠ Motherboard ({motherboard.Socket})");
                }
            }

            // Проверка RAM + Motherboard
            if (ram != null && motherboard != null)
            {
                if (ram.MemoryType != motherboard.MemoryType)
                {
                    result.Errors.Add($"Несовместимость памяти: RAM ({ram.MemoryType}) ≠ Motherboard ({motherboard.MemoryType})");
                }
            }

            // Проверка форм-фактора
            if (motherboard != null && caseComp != null)
            {
                if (!IsFormFactorCompatible(motherboard.FormFactor, caseComp.FormFactor))
                {
                    result.Errors.Add($"Несовместимость форм-факторов: {motherboard.FormFactor} → {caseComp.FormFactor}");
                }
            }

            // Проверка блока питания
            if (psu != null)
            {
                var totalPower = components.Sum(c => c.PowerSupply ?? 0);
                var psuPower = GetPSUPower(psu.Name);

                if (totalPower > psuPower)
                {
                    result.Errors.Add($"Недостаточная мощность БП: требуется {totalPower}Вт, доступно {psuPower}Вт");
                }
                else if (totalPower > psuPower * 0.8)
                {
                    result.Warnings.Add($"БП работает на 80% мощности. Рекомендуется более мощный блок.");
                }
            }

            result.IsCompatible = !result.Errors.Any();
            return result;
        }

        private bool IsFormFactorCompatible(string motherboardFF, string caseFF)
        {
            var compatibility = new Dictionary<string, List<string>>
            {
                ["Mini-ITX"] = new List<string> { "Mini-ITX", "Micro-ATX", "ATX", "E-ATX" },
                ["Micro-ATX"] = new List<string> { "Micro-ATX", "ATX", "E-ATX" },
                ["ATX"] = new List<string> { "ATX", "E-ATX" },
                ["E-ATX"] = new List<string> { "E-ATX" }
            };

            return compatibility.ContainsKey(motherboardFF) &&
                   compatibility[motherboardFF].Contains(caseFF);
        }

        private int GetPSUPower(string psuName)
        {
            var match = System.Text.RegularExpressions.Regex.Match(psuName, @"(\d+)\s*W");
            return match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }
    }
}
