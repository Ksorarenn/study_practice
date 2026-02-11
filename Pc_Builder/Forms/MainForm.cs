using PC_Builder.Core.Models;
using PC_Builder.Data;
using PC_Builder.Data.SeedData;

namespace PC_Builder.WinForms.Forms
{
    public partial class MainForm : Form
    {
        private User _currentUser;
        private ApplicationDbContext _context;

        public MainForm(User user)
        {
            _currentUser = user;
            _context = new ApplicationDbContext();
            InitializeComponent();
            DatabaseInitializer.Initialize(_context);
            SetupForm();
        }

        private System.ComponentModel.IContainer components = null;

        public AutoScaleMode AutoScaleMode { get; private set; }
        public Size ClientSize { get; private set; }
        public string Text { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Text = "MainForm";
        }

        private void SetupForm()
        {
            this.Text = $"PC Builder - {_currentUser.Username} ({_currentUser.Role})";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Меню
            var menuStrip = new MenuStrip();

            // Общие пункты меню
            var fileMenu = new ToolStripMenuItem("Файл");
            var exitItem = new ToolStripMenuItem("Выход");
            exitItem.Click += (s, e) => Application.Exit();
            fileMenu.DropDownItems.Add(exitItem);
            menuStrip.Items.Add(fileMenu);

            // Меню для клиента
            if (_currentUser.Role == "Client")
            {
                var catalogMenu = new ToolStripMenuItem("Каталог");
                catalogMenu.Click += (s, e) => OpenCatalog();
                menuStrip.Items.Add(catalogMenu);

                var myBuildsMenu = new ToolStripMenuItem("Мои сборки");
                myBuildsMenu.Click += (s, e) => OpenMyBuilds();
                menuStrip.Items.Add(myBuildsMenu);

                var ordersMenu = new ToolStripMenuItem("Мои заказы");
                ordersMenu.Click += (s, e) => OpenMyOrders();
                menuStrip.Items.Add(ordersMenu);
            }

            // Меню для администратора
            if (_currentUser.Role == "Admin")
            {
                var adminMenu = new ToolStripMenuItem("Администрирование");

                var manageComponents = new ToolStripMenuItem("Управление компонентами");
                manageComponents.Click += (s, e) => OpenComponentManagement();
                adminMenu.DropDownItems.Add(manageComponents);

                var manageOrders = new ToolStripMenuItem("Управление заказами");
                manageOrders.Click += (s, e) => OpenOrderManagement();
                adminMenu.DropDownItems.Add(manageOrders);

                menuStrip.Items.Add(adminMenu);
            }

            var accountMenu = new ToolStripMenuItem("Аккаунт");
            var logoutItem = new ToolStripMenuItem("Выйти");
            logoutItem.Click += (s, e) => Logout();
            accountMenu.DropDownItems.Add(logoutItem);
            menuStrip.Items.Add(accountMenu);

            this.Controls.Add(menuStrip);

            // Статус бар
            var statusStrip = new StatusStrip();
            var statusLabel = new ToolStripStatusLabel();
            statusLabel.Text = $"Пользователь: {_currentUser.Username} | Роль: {_currentUser.Role}";
            statusStrip.Items.Add(statusLabel);

            this.Controls.Add(statusStrip);

            // Приветственное сообщение
            var welcomeLabel = new Label
            {
                Text = $"Добро пожаловать, {_currentUser.Username}!",
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(50, 50),
                Size = new Size(500, 30)
            };

            var infoLabel = new Label
            {
                Text = _currentUser.Role == "Admin"
                    ? "Используйте меню 'Администрирование' для управления системой."
                    : "Используйте меню 'Каталог' для начала сборки ПК.",
                Location = new Point(50, 100),
                Size = new Size(500, 50)
            };

            this.Controls.Add(welcomeLabel);
            this.Controls.Add(infoLabel);
        }

        private void OpenCatalog()
        {
            var catalogForm = new CatalogForm(_currentUser, _context);
            catalogForm.ShowDialog();
        }

        private void OpenMyBuilds()
        {
            var buildForm = new BuildForm(_currentUser, _context);
            buildForm.ShowDialog();
        }

        private void OpenComponentManagement()
        {
            var adminForm = new AdminPanelForm(_currentUser, _context);
            adminForm.ShowDialog();
        }

        private void OpenMyOrders()
        {
            MessageBox.Show("Функция 'Мои заказы' в разработке", "Информация",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void OpenOrderManagement()
        {
            MessageBox.Show("Функция 'Управление заказами' в разработке", "Информация",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Logout()
        {
            if (MessageBox.Show("Вы уверены, что хотите выйти?", "Выход",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _context?.Dispose();
                Application.Restart();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _context?.Dispose();
            base.OnFormClosing(e);
        }
    }
}