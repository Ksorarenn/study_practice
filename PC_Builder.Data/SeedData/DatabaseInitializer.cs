using PC_Builder.Core.Models;
using PC_Builder.Data;
using System.Security.Cryptography;
using System.Text;
namespace PC_Builder.Data.SeedData
{
    public class DatabaseInitializer
    {
    public static void Initialize(ApplicationDbContext context)
        {
            try
            {
                context.Database.EnsureCreated();
                Console.WriteLine("База данных проверена/создана");

                // Сначала создаем категории если их нет
                if (!context.Categories.Any())
                {
                    Console.WriteLine("Создание категорий...");
                    var categories = new[]
                    {
                new Category { Name = "CPU", Description = "Процессоры" },
                new Category { Name = "Motherboard", Description = "Материнские платы" },
                new Category { Name = "RAM", Description = "Оперативная память" },
                new Category { Name = "GPU", Description = "Видеокарты" },
                new Category { Name = "PSU", Description = "Блоки питания" },
                new Category { Name = "Case", Description = "Корпуса" }
            };

                    context.Categories.AddRange(categories);
                    context.SaveChanges();
                    Console.WriteLine("Категории созданы");
                }

                // Получаем категории из базы
                var cpuCategory = context.Categories.First(c => c.Name == "CPU");
                var motherboardCategory = context.Categories.First(c => c.Name == "Motherboard");
                var ramCategory = context.Categories.First(c => c.Name == "RAM");
                var gpuCategory = context.Categories.First(c => c.Name == "GPU");
                var psuCategory = context.Categories.First(c => c.Name == "PSU");
                var caseCategory = context.Categories.First(c => c.Name == "Case");

                // Добавляем пользователей если их нет
                if (!context.Users.Any())
                {
                    Console.WriteLine("Создание пользователей...");
                    var users = new[]
                    {
                new User { Username = "admin", FullName = "Administrator", Email = "admin@pcbuilder.com", PasswordHash = HashPassword("admin123"), Role = "Admin" },
                new User { Username = "client", FullName = "Test Client", Email = "client@pcbuilder.com", PasswordHash = HashPassword("client123"), Role = "Client" }
            };

                    context.Users.AddRange(users);
                    context.SaveChanges();
                    Console.WriteLine("Пользователи созданы");
                }

                // Добавляем тестовые компоненты если их нет
                if (!context.Components.Any())
                {
                    Console.WriteLine("Создание тестовых компонентов...");

                    var components = new List<Component>();

                    // Процессоры
                    components.Add(new Component
                    {
                        CategoryId = cpuCategory.Id,
                        Name = "Intel Core i7-13700K",
                        Description = "16-ядерный (8P+8E) процессор 13-го поколения с тактовой частотой до 5.4 GHz",
                        Manufacturer = "Intel",
                        Price = 42000,
                        Quantity = 10,
                        Socket = "LGA1700",
                        FormFactor = "LGA",
                        MemoryType = "DDR5",
                        MaxMemory = 128,
                        PowerSupply = 125,
                        MemorySlots = 0,
                        MaxMemoryPerSlot = 0,
                        TDP = 125,
                        Length = 37,
                        Width = 45,
                        Height = 5
                    });

                    components.Add(new Component
                    {
                        CategoryId = cpuCategory.Id,
                        Name = "AMD Ryzen 7 5800X",
                        Description = "8-ядерный 16-поточный процессор с частотой до 4.7 GHz, техпроцесс 7 нм",
                        Manufacturer = "AMD",
                        Price = 30000,
                        Quantity = 8,
                        Socket = "AM4",
                        FormFactor = "PGA",
                        MemoryType = "DDR4",
                        MaxMemory = 128,
                        PowerSupply = 105,
                        MemorySlots = 0,
                        MaxMemoryPerSlot = 0,
                        TDP = 105,
                        Length = 40,
                        Width = 40,
                        Height = 5
                    });

                    components.Add(new Component
                    {
                        CategoryId = cpuCategory.Id,
                        Name = "Intel Core i5-12600K",
                        Description = "10-ядерный (6P+4E) процессор 12-го поколения, частота до 4.9 GHz",
                        Manufacturer = "Intel",
                        Price = 25000,
                        Quantity = 12,
                        Socket = "LGA1700",
                        FormFactor = "LGA",
                        MemoryType = "DDR5",
                        MaxMemory = 128,
                        PowerSupply = 125,
                        MemorySlots = 0,
                        MaxMemoryPerSlot = 0,
                        TDP = 125,
                        Length = 37,
                        Width = 45,
                        Height = 5
                    });

                    // Материнские платы
                    components.Add(new Component
                    {
                        CategoryId = motherboardCategory.Id,
                        Name = "ASUS ROG Strix Z690-E Gaming WiFi",
                        Description = "Материнская плата ATX для процессоров Intel 12-13 поколения, WiFi 6E, 4 слота DDR5",
                        Manufacturer = "ASUS",
                        Price = 35000,
                        Quantity = 5,
                        Socket = "LGA1700",
                        FormFactor = "ATX",
                        MemoryType = "DDR5",
                        MaxMemory = 128,
                        PowerSupply = 0,
                        MemorySlots = 4,
                        MaxMemoryPerSlot = 32,
                        TDP = 0,
                        Length = 305,
                        Width = 244,
                        Height = 44
                    });

                    components.Add(new Component
                    {
                        CategoryId = motherboardCategory.Id,
                        Name = "Gigabyte B550 AORUS ELITE V2",
                        Description = "Материнская плата ATX для процессоров AMD AM4, поддержка PCIe 4.0, 4 слота DDR4",
                        Manufacturer = "Gigabyte",
                        Price = 15000,
                        Quantity = 7,
                        Socket = "AM4",
                        FormFactor = "ATX",
                        MemoryType = "DDR4",
                        MaxMemory = 128,
                        PowerSupply = 0,
                        MemorySlots = 4,
                        MaxMemoryPerSlot = 32,
                        TDP = 0,
                        Length = 305,
                        Width = 244,
                        Height = 44
                    });

                    components.Add(new Component
                    {
                        CategoryId = motherboardCategory.Id,
                        Name = "ASRock B660M Pro RS",
                        Description = "Компактная материнская плата Micro-ATX для процессоров Intel 12-го поколения",
                        Manufacturer = "ASRock",
                        Price = 12000,
                        Quantity = 6,
                        Socket = "LGA1700",
                        FormFactor = "Micro-ATX",
                        MemoryType = "DDR4",
                        MaxMemory = 64,
                        PowerSupply = 0,
                        MemorySlots = 2,
                        MaxMemoryPerSlot = 32,
                        TDP = 0,
                        Length = 244,
                        Width = 244,
                        Height = 35
                    });

                    // Оперативная память
                    components.Add(new Component
                    {
                        CategoryId = ramCategory.Id,
                        Name = "Corsair Vengeance RGB 32GB (2x16GB) DDR5 6000MHz",
                        Description = "Набор из двух модулей DDR5 32GB с RGB подсветкой и низкими таймингами CL36",
                        Manufacturer = "Corsair",
                        Price = 14000,
                        Quantity = 15,
                        Socket = "DIMM",
                        FormFactor = "DIMM",
                        MemoryType = "DDR5",
                        MaxMemory = 32,
                        PowerSupply = 0,
                        MemorySlots = 0,
                        MaxMemoryPerSlot = 0,
                        TDP = 0,
                        Length = 133,
                        Width = 8,
                        Height = 43
                    });

                    components.Add(new Component
                    {
                        CategoryId = ramCategory.Id,
                        Name = "Kingston Fury Beast 16GB (2x8GB) DDR4 3200MHz",
                        Description = "Набор из двух модулей DDR4 16GB с радиаторами и частотой 3200MHz",
                        Manufacturer = "Kingston",
                        Price = 6500,
                        Quantity = 20,
                        Socket = "DIMM",
                        FormFactor = "DIMM",
                        MemoryType = "DDR4",
                        MaxMemory = 16,
                        PowerSupply = 0,
                        MemorySlots = 0,
                        MaxMemoryPerSlot = 0,
                        TDP = 0,
                        Length = 133,
                        Width = 8,
                        Height = 34
                    });

                    components.Add(new Component
                    {
                        CategoryId = ramCategory.Id,
                        Name = "G.Skill Trident Z5 RGB 64GB (2x32GB) DDR5 6400MHz",
                        Description = "Высокопроизводительная память DDR5 64GB с RGB подсветкой для игровых ПК",
                        Manufacturer = "G.Skill",
                        Price = 28000,
                        Quantity = 4,
                        Socket = "DIMM",
                        FormFactor = "DIMM",
                        MemoryType = "DDR5",
                        MaxMemory = 64,
                        PowerSupply = 0,
                        MemorySlots = 0,
                        MaxMemoryPerSlot = 0,
                        TDP = 0,
                        Length = 133,
                        Width = 8,
                        Height = 44
                    });

                    // Видеокарты
                    components.Add(new Component
                    {
                        CategoryId = gpuCategory.Id,
                        Name = "NVIDIA GeForce RTX 4070 Ti 12GB GDDR6X",
                        Description = "Игровая видеокарта с 12GB памяти, поддержкой DLSS 3 и трассировки лучей",
                        Manufacturer = "NVIDIA",
                        Price = 85000,
                        Quantity = 3,
                        Socket = "PCIe 4.0",
                        FormFactor = "ATX",
                        MemoryType = "GDDR6X",
                        MaxMemory = 12,
                        PowerSupply = 285,
                        MemorySlots = 0,
                        MaxMemoryPerSlot = 0,
                        TDP = 285,
                        Length = 336,
                        Width = 140,
                        Height = 61
                    });

                    components.Add(new Component
                    {
                        CategoryId = gpuCategory.Id,
                        Name = "AMD Radeon RX 7800 XT 16GB GDDR6",
                        Description = "Игровая видеокарта AMD с 16GB памяти и архитектурой RDNA 3",
                        Manufacturer = "AMD",
                        Price = 65000,
                        Quantity = 5,
                        Socket = "PCIe 4.0",
                        FormFactor = "ATX",
                        MemoryType = "GDDR6",
                        MaxMemory = 16,
                        PowerSupply = 263,
                        MemorySlots = 0,
                        MaxMemoryPerSlot = 0,
                        TDP = 263,
                        Length = 267,
                        Width = 120,
                        Height = 50
                    });

                    components.Add(new Component
                    {
                        CategoryId = gpuCategory.Id,
                        Name = "NVIDIA GeForce RTX 3060 12GB GDDR6",
                        Description = "Бюджетная игровая видеокарта с 12GB памяти для Full HD игр",
                        Manufacturer = "NVIDIA",
                        Price = 35000,
                        Quantity = 8,
                        Socket = "PCIe 4.0",
                        FormFactor = "ATX",
                        MemoryType = "GDDR6",
                        MaxMemory = 12,
                        PowerSupply = 170,
                        MemorySlots = 0,
                        MaxMemoryPerSlot = 0,
                        TDP = 170,
                        Length = 242,
                        Width = 112,
                        Height = 40
                    });

                    // Блоки питания
                    components.Add(new Component
                    {
                        CategoryId = psuCategory.Id,
                        Name = "Seasonic Focus GX-750 750W 80+ Gold",
                        Description = "Модульный блок питания 750W с сертификатом 80+ Gold, 10 лет гарантии",
                        Manufacturer = "Seasonic",
                        Price = 12000,
                        Quantity = 12,
                        Socket = "ATX",
                        FormFactor = "ATX",
                        MemoryType = "",
                        MaxMemory = 0,
                        PowerSupply = 750,
                        MemorySlots = 0,
                        MaxMemoryPerSlot = 0,
                        TDP = 0,
                        Length = 160,
                        Width = 150,
                        Height = 86
                    });

                    components.Add(new Component
                    {
                        CategoryId = psuCategory.Id,
                        Name = "Corsair RM850x 850W 80+ Gold",
                        Description = "Полностью модульный блок питания 850W с бесшумным вентилятором 135mm",
                        Manufacturer = "Corsair",
                        Price = 15000,
                        Quantity = 9,
                        Socket = "ATX",
                        FormFactor = "ATX",
                        MemoryType = "",
                        MaxMemory = 0,
                        PowerSupply = 850,
                        MemorySlots = 0,
                        MaxMemoryPerSlot = 0,
                        TDP = 0,
                        Length = 180,
                        Width = 150,
                        Height = 86
                    });

                    components.Add(new Component
                    {
                        CategoryId = psuCategory.Id,
                        Name = "be quiet! Straight Power 11 1500W 80+ Platinum",
                        Description = "Топовый блок питания 1500W с сертификатом 80+ Platinum и пассивным режимом",
                        Manufacturer = "be quiet!",
                        Price = 30000,
                        Quantity = 4,
                        Socket = "ATX",
                        FormFactor = "ATX",
                        MemoryType = "",
                        MaxMemory = 0,
                        PowerSupply = 1500,
                        MemorySlots = 0,
                        MaxMemoryPerSlot = 0,
                        TDP = 0,
                        Length = 190,
                        Width = 150,
                        Height = 86
                    });
                    components.Add(new Component
                    {
                        CategoryId = psuCategory.Id,
                        Name = "be quiet! Straight Power 11 1000W 80+ Platinum",
                        Description = "Топовый блок питания 1000W с сертификатом 80+ Platinum и пассивным режимом",
                        Manufacturer = "be quiet!",
                        Price = 25000,
                        Quantity = 4,
                        Socket = "ATX",
                        FormFactor = "ATX",
                        MemoryType = "",
                        MaxMemory = 0,
                        PowerSupply = 1000,
                        MemorySlots = 0,
                        MaxMemoryPerSlot = 0,
                        TDP = 0,
                        Length = 190,
                        Width = 150,
                        Height = 86
                    });

                    // Корпуса
                    components.Add(new Component
                    {
                        CategoryId = caseCategory.Id,
                        Name = "NZXT H510 Flow Black",
                        Description = "Среднебашенный корпус ATX с перфорированной передней панелью для улучшенного воздушного потока",
                        Manufacturer = "NZXT",
                        Price = 9000,
                        Quantity = 6,
                        Socket = "",
                        FormFactor = "ATX",
                        MemoryType = "",
                        MaxMemory = 0,
                        PowerSupply = 0,
                        MemorySlots = 0,
                        MaxMemoryPerSlot = 0,
                        TDP = 0,
                        Length = 460,
                        Width = 210,
                        Height = 428
                    });

                    components.Add(new Component
                    {
                        CategoryId = caseCategory.Id,
                        Name = "Fractal Design Meshify 2 Compact",
                        Description = "Компактный корпус ATX с сетчатой передней панелью и отличной системой вентиляции",
                        Manufacturer = "Fractal Design",
                        Price = 11000,
                        Quantity = 7,
                        Socket = "",
                        FormFactor = "ATX",
                        MemoryType = "",
                        MaxMemory = 0,
                        PowerSupply = 0,
                        MemorySlots = 0,
                        MaxMemoryPerSlot = 0,
                        TDP = 0,
                        Length = 424,
                        Width = 210,
                        Height = 475
                    });

                    components.Add(new Component
                    {
                        CategoryId = caseCategory.Id,
                        Name = "Lian Li Lancool 216 RGB Black",
                        Description = "Корпус ATX с двумя вентиляторами 160mm спереди и RGB подсветкой",
                        Manufacturer = "Lian Li",
                        Price = 9500,
                        Quantity = 5,
                        Socket = "",
                        FormFactor = "ATX",
                        MemoryType = "",
                        MaxMemory = 0,
                        PowerSupply = 0,
                        MemorySlots = 0,
                        MaxMemoryPerSlot = 0,
                        TDP = 0,
                        Length = 480,
                        Width = 235,
                        Height = 491
                    });

                    context.Components.AddRange(components);
                    context.SaveChanges();
                    Console.WriteLine($"Создано {components.Count} компонентов");
                }

                Console.WriteLine("Инициализация базы данных завершена успешно");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка инициализации БД: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"InnerException: {ex.InnerException.Message}");
                }
                throw;
            }
        }
        public static string HashPassword(string password)
        {   
            // Использование BCrypt (рекомендуется)
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
