using Microsoft.EntityFrameworkCore;
using PC_Builder.Data;
using PC_Builder.Data.SeedData;
using PC_Builder.WinForms.Forms;
using System.Diagnostics;

namespace PC_Builder.WinForms
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            try
            {
                using var context = new ApplicationDbContext();

                try
                {
                    // Создаем базу если нет
                    context.Database.EnsureCreated();
                    
                    // Инициализируем данные если база пустая
                    if (!context.Users.Any())
                    {
                        DatabaseInitializer.Initialize(context);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка инициализации базы данных: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Application.Run(new LoginForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Критическая ошибка запуска: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}