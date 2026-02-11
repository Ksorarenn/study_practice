using Microsoft.EntityFrameworkCore;
using PC_Builder.Core.Models;

namespace PC_Builder.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Component> Components { get; set; }
        public DbSet<Category> Categories { get; set; } // ДОБАВИТЬ ЭТУ СТРОЧКУ
        public DbSet<Build> Builds { get; set; }
        public DbSet<BuildComponent> BuildComponents { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                Console.WriteLine("Настройка подключения к PostgreSQL...");

                var connectionString = "Host=localhost;Database=pc_builder;Username=postgres;Password=dL_t82sc1";
                Console.WriteLine($"Строка подключения: {connectionString.Replace("Password=dL_t82sc1", "Password=****")}");

                optionsBuilder.UseNpgsql(connectionString, options =>
                {
                    options.EnableRetryOnFailure();
                });

                // Включите логирование для отладки
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Создаем ValueConverter для автоматического преобразования DateTime в UTC
            var dateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
                v => v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            var nullableDateTimeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? v.Value.ToUniversalTime() : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);

            // Применяем конвертер ко всем свойствам DateTime во всех сущностях
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(dateTimeConverter);
                        property.SetColumnType("timestamp without time zone");
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(nullableDateTimeConverter);
                        property.SetColumnType("timestamp without time zone");
                    }
                }
            }

            // Настройка таблицы Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("categories");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(200);
                entity.Property(e => e.ParentId).HasColumnName("parent_id");
            });

            // Настройка таблицы Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Username).HasColumnName("username").IsRequired().HasMaxLength(50);
                entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100).HasDefaultValue("");
                entity.Property(e => e.PasswordHash).HasColumnName("passwordhash").IsRequired();
                entity.Property(e => e.Role).HasColumnName("role").HasDefaultValue("Client").HasMaxLength(20);
                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp with time zone") // ИЗМЕНИТЕ на with time zone
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);

                entity.HasIndex(e => e.Username).IsUnique().HasDatabaseName("ix_users_username");
            });

            modelBuilder.Entity<Component>(entity =>
            {
                entity.ToTable("components");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.CategoryId).HasColumnName("category_id").IsRequired();
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasColumnName("description"); // с маленькой буквы
                entity.Property(e => e.Manufacturer).HasColumnName("manufacturer").HasMaxLength(50);
                entity.Property(e => e.Price).HasColumnName("price").HasPrecision(10, 2);
                entity.Property(e => e.Quantity).HasColumnName("quantity").HasDefaultValue(0);
                entity.Property(e => e.Socket).HasColumnName("socket").HasMaxLength(30);
                entity.Property(e => e.FormFactor).HasColumnName("form_factor").HasMaxLength(30);
                entity.Property(e => e.MemoryType).HasColumnName("memory_type").HasMaxLength(20);
                entity.Property(e => e.MaxMemory).HasColumnName("max_memory");
                entity.Property(e => e.PowerSupply).HasColumnName("power_supply");
                entity.Property(e => e.MemorySlots).HasColumnName("memory_slots");
                entity.Property(e => e.MaxMemoryPerSlot).HasColumnName("max_memory_per_slot");
                entity.Property(e => e.TDP).HasColumnName("tdp");
                entity.Property(e => e.Length).HasColumnName("length");
                entity.Property(e => e.Width).HasColumnName("width");
                entity.Property(e => e.Height).HasColumnName("height");

                entity.HasOne(c => c.Category)
                      .WithMany(cat => cat.Components)
                      .HasForeignKey(c => c.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Настройка таблицы Builds
            modelBuilder.Entity<Build>(entity =>
            {
                entity.ToTable("builds");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
                entity.Property(e => e.TotalPrice).HasColumnName("total_price").HasPrecision(10, 2);
                entity.Property(e => e.Status).HasColumnName("status").HasDefaultValue("Draft").HasMaxLength(20);
                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp with time zone") // ИЗМЕНИТЕ на with time zone
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne<User>().WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
            });

            // Настройка таблицы BuildComponents
            modelBuilder.Entity<BuildComponent>(entity =>
            {
                entity.ToTable("build_components");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.BuildId).HasColumnName("build_id");
                entity.Property(e => e.ComponentId).HasColumnName("component_id");
                entity.Property(e => e.Quantity).HasColumnName("quantity").HasDefaultValue(1);
                entity.Property(e => e.AddedAt)
                    .HasColumnName("added_at")
                    .HasColumnType("timestamp with time zone") // ИЗМЕНИТЕ на with time zone
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne<Build>().WithMany().HasForeignKey(e => e.BuildId).OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<Component>().WithMany().HasForeignKey(e => e.ComponentId).OnDelete(DeleteBehavior.Cascade);
            });

            // Настройка таблицы Orders
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.BuildId).HasColumnName("build_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.Status).HasColumnName("status").HasDefaultValue("Pending").HasMaxLength(20);
                entity.Property(e => e.TotalPrice).HasColumnName("total_price").HasPrecision(10, 2);
                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp with time zone") // ИЗМЕНИТЕ на with time zone
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("timestamp without time zone");

                entity.HasOne<Build>().WithMany().HasForeignKey(e => e.BuildId);
                entity.HasOne<User>().WithMany().HasForeignKey(e => e.UserId);
            });
        }
    }
}