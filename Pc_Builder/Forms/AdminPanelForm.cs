using System.ComponentModel;
using System.Data;
using Microsoft.EntityFrameworkCore;
using PC_Builder.Core.Models;
using PC_Builder.Data;

namespace PC_Builder.WinForms.Forms
{
    public class AdminPanelForm : Form
    {
        private User _currentUser;
        private ApplicationDbContext _context;

        private TabControl _tabControl;

        // Вкладка компонентов
        private DataGridView _dgvComponents;
        private Button _btnAddComponent;
        private Button _btnEditComponent;
        private Button _btnDeleteComponent;
        private Button _btnRefreshComponents;

        // Вкладка заказов
        private DataGridView _dgvOrders;
        private Button _btnProcessOrder;
        private Button _btnCompleteOrder;
        private Button _btnCancelOrder;
        private Button _btnRefreshOrders;
        private ComboBox _cmbOrderFilter;

        // Вкладка пользователей
        private DataGridView _dgvUsers;
        private Button _btnRefreshUsers;

        private System.ComponentModel.IContainer components = null;
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
            SuspendLayout();
            // 
            // AdminPanelForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Name = "AdminPanelForm";
            Text = "AdminPanelForm";
            Load += AdminPanelForm_Load;
            ResumeLayout(false);
        }

        public AdminPanelForm(User user, ApplicationDbContext context)
        {
            _currentUser = user;
            _context = context;
            InitializeComponent();
            SetupForm();
            LoadAllData();
        }

        private void SetupForm()
        {
            this.Text = $"Панель администратора - {_currentUser.Username}";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(900, 600);

            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill
            };

            // Вкладка 1: Компоненты
            var tabComponents = new TabPage("📦 Компоненты");
            SetupComponentsTab(tabComponents);
            _tabControl.TabPages.Add(tabComponents);

            // Вкладка 2: Заказы
            var tabOrders = new TabPage("📋 Заказы");
            SetupOrdersTab(tabOrders);
            _tabControl.TabPages.Add(tabOrders);

            // Вкладка 3: Пользователи
            var tabUsers = new TabPage("👥 Пользователи");
            SetupUsersTab(tabUsers);
            _tabControl.TabPages.Add(tabUsers);

            // Вкладка 4: Статистика
            var tabStats = new TabPage("📊 Статистика");
            SetupStatsTab(tabStats);
            _tabControl.TabPages.Add(tabStats);

            this.Controls.Add(_tabControl);
        }

        private void SetupComponentsTab(TabPage tab)
        {
            var mainPanel = new Panel { Dock = DockStyle.Fill };

            // Панель управления
            var controlPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5)
            };

            _btnAddComponent = new Button
            {
                Text = "➕ Добавить",
                Location = new Point(10, 10),
                Size = new Size(100, 30),
                BackColor = Color.LightGreen
            };
            _btnAddComponent.Click += BtnAddComponent_Click;

            _btnEditComponent = new Button
            {
                Text = "✏️ Редактировать",
                Location = new Point(120, 10),
                Size = new Size(120, 30),
                BackColor = Color.LightBlue
            };
            _btnEditComponent.Click += BtnEditComponent_Click;

            _btnDeleteComponent = new Button
            {
                Text = "🗑 Удалить",
                Location = new Point(250, 10),
                Size = new Size(100, 30),
                BackColor = Color.LightCoral
            };
            _btnDeleteComponent.Click += BtnDeleteComponent_Click;

            _btnRefreshComponents = new Button
            {
                Text = "🔄 Обновить",
                Location = new Point(360, 10),
                Size = new Size(100, 30)
            };
            _btnRefreshComponents.Click += (s, e) => LoadComponents();

            controlPanel.Controls.AddRange(new Control[]
            {
                _btnAddComponent, _btnEditComponent,
                _btnDeleteComponent, _btnRefreshComponents
            });

            // Таблица компонентов
            _dgvComponents = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = SystemColors.Window
            };

            SetupComponentsGrid();

            mainPanel.Controls.Add(_dgvComponents);
            mainPanel.Controls.Add(controlPanel);
            tab.Controls.Add(mainPanel);
        }

        private void SetupComponentsGrid()
        {
            _dgvComponents.Columns.Clear();

            var columns = new[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 40 },
                new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "Категория", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Название", Width = 200 },
                new DataGridViewTextBoxColumn { Name = "Manufacturer", HeaderText = "Производитель", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "Price", HeaderText = "Цена", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "Кол-во", Width = 60 },
                new DataGridViewTextBoxColumn { Name = "Socket", HeaderText = "Сокет", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "FormFactor", HeaderText = "Форм-фактор", Width = 90 },
                new DataGridViewTextBoxColumn { Name = "MemoryType", HeaderText = "Тип памяти", Width = 80 }
            };

            _dgvComponents.Columns.AddRange(columns);

            _dgvComponents.Columns["Price"].DefaultCellStyle.Format = "C0";
            _dgvComponents.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _dgvComponents.Columns["Quantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Подсветка строк с малым количеством
            _dgvComponents.RowPostPaint += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < _dgvComponents.Rows.Count)
                {
                    var quantityCell = _dgvComponents.Rows[e.RowIndex].Cells["Quantity"];
                    if (quantityCell.Value != null && int.TryParse(quantityCell.Value.ToString(), out int quantity))
                    {
                        if (quantity <= 0)
                        {
                            _dgvComponents.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightCoral;
                        }
                        else if (quantity < 5)
                        {
                            _dgvComponents.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                        }
                    }
                }
            };
        }

        private void SetupOrdersTab(TabPage tab)
        {
            var mainPanel = new Panel { Dock = DockStyle.Fill };

            // Панель управления
            var controlPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5)
            };

            var lblFilter = new Label
            {
                Text = "Фильтр по статусу:",
                Location = new Point(10, 15),
                Size = new Size(100, 20)
            };

            _cmbOrderFilter = new ComboBox
            {
                Location = new Point(120, 12),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbOrderFilter.Items.AddRange(new string[]
            {
                "Все заказы", "Pending", "Processing", "Completed", "Cancelled"
            });
            _cmbOrderFilter.SelectedIndex = 0;
            _cmbOrderFilter.SelectedIndexChanged += (s, e) => LoadOrders();

            _btnProcessOrder = new Button
            {
                Text = "🔄 В обработку",
                Location = new Point(280, 10),
                Size = new Size(120, 30),
                BackColor = Color.LightBlue
            };
            _btnProcessOrder.Click += BtnProcessOrder_Click;

            _btnCompleteOrder = new Button
            {
                Text = "✅ Выполнить",
                Location = new Point(410, 10),
                Size = new Size(100, 30),
                BackColor = Color.LightGreen
            };
            _btnCompleteOrder.Click += BtnCompleteOrder_Click;

            _btnCancelOrder = new Button
            {
                Text = "❌ Отменить",
                Location = new Point(520, 10),
                Size = new Size(100, 30),
                BackColor = Color.LightCoral
            };
            _btnCancelOrder.Click += BtnCancelOrder_Click;

            _btnRefreshOrders = new Button
            {
                Text = "🔄 Обновить",
                Location = new Point(630, 10),
                Size = new Size(100, 30)
            };
            _btnRefreshOrders.Click += (s, e) => LoadOrders();

            controlPanel.Controls.AddRange(new Control[]
            {
                lblFilter, _cmbOrderFilter,
                _btnProcessOrder, _btnCompleteOrder, _btnCancelOrder, _btnRefreshOrders
            });

            // Таблица заказов
            _dgvOrders = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = SystemColors.Window
            };

            SetupOrdersGrid();

            mainPanel.Controls.Add(_dgvOrders);
            mainPanel.Controls.Add(controlPanel);
            tab.Controls.Add(mainPanel);
        }

        private void SetupOrdersGrid()
        {
            _dgvOrders.Columns.Clear();

            var columns = new[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 50 },
                new DataGridViewTextBoxColumn { Name = "buildid", HeaderText = "ID сборки", Width = 70 },
                new DataGridViewTextBoxColumn { Name = "UserId", HeaderText = "ID клиента", Width = 70 },
                new DataGridViewTextBoxColumn { Name = "TotalPrice", HeaderText = "Сумма", Width = 90 },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Статус", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "CreatedAt", HeaderText = "Дата создания", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "UpdatedAt", HeaderText = "Последнее изменение", Width = 120 }
            };

            _dgvOrders.Columns.AddRange(columns);

            _dgvOrders.Columns["TotalPrice"].DefaultCellStyle.Format = "C0";
            _dgvOrders.Columns["TotalPrice"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _dgvOrders.Columns["CreatedAt"].DefaultCellStyle.Format = "dd.MM.yyyy HH:mm";
            _dgvOrders.Columns["UpdatedAt"].DefaultCellStyle.Format = "dd.MM.yyyy HH:mm";

            // Подсветка по статусу
            _dgvOrders.RowPostPaint += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.RowIndex < _dgvOrders.Rows.Count)
                {
                    var statusCell = _dgvOrders.Rows[e.RowIndex].Cells["Status"];
                    if (statusCell.Value != null)
                    {
                        string status = statusCell.Value.ToString();
                        switch (status)
                        {
                            case "Pending":
                                _dgvOrders.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                                break;
                            case "Processing":
                                _dgvOrders.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightBlue;
                                break;
                            case "Completed":
                                _dgvOrders.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
                                break;
                            case "Cancelled":
                                _dgvOrders.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.LightCoral;
                                break;
                        }
                    }
                }
            };
        }

        private void SetupUsersTab(TabPage tab)
        {
            var mainPanel = new Panel { Dock = DockStyle.Fill };

            // Панель управления
            var controlPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5)
            };

            _btnRefreshUsers = new Button
            {
                Text = "🔄 Обновить",
                Location = new Point(10, 7),
                Size = new Size(100, 25)
            };
            _btnRefreshUsers.Click += (s, e) => LoadUsers();

            controlPanel.Controls.Add(_btnRefreshUsers);

            // Таблица пользователей
            _dgvUsers = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = SystemColors.Window
            };

            SetupUsersGrid();

            mainPanel.Controls.Add(_dgvUsers);
            mainPanel.Controls.Add(controlPanel);
            tab.Controls.Add(mainPanel);
        }

        private void SetupUsersGrid()
        {
            _dgvUsers.Columns.Clear();

            var columns = new[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 50 },
                new DataGridViewTextBoxColumn { Name = "Username", HeaderText = "Логин", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "Role", HeaderText = "Роль", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "CreatedAt", HeaderText = "Дата регистрации", Width = 120 },
                new DataGridViewTextBoxColumn { Name = "IsActive", HeaderText = "Активен", Width = 70 }
            };

            _dgvUsers.Columns.AddRange(columns);

            _dgvUsers.Columns["CreatedAt"].DefaultCellStyle.Format = "dd.MM.yyyy";
            _dgvUsers.Columns["IsActive"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void SetupStatsTab(TabPage tab)
        {
            var mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            var lblStatsTitle = new Label
            {
                Text = "📊 Статистика системы",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(400, 30)
            };

            var rtbStats = new RichTextBox
            {
                Location = new Point(20, 60),
                Size = new Size(600, 400),
                ReadOnly = true,
                Font = new Font("Consolas", 10),
                BackColor = Color.WhiteSmoke
            };

            var btnRefreshStats = new Button
            {
                Text = "🔄 Обновить статистику",
                Location = new Point(20, 470),
                Size = new Size(200, 30),
                BackColor = Color.LightBlue
            };
            btnRefreshStats.Click += (s, e) => LoadStatistics(rtbStats);

            mainPanel.Controls.AddRange(new Control[] { lblStatsTitle, rtbStats, btnRefreshStats });
            tab.Controls.Add(mainPanel);

            // Загружаем статистику при открытии вкладки
            LoadStatistics(rtbStats);
        }

        private void LoadAllData()
        {
            LoadComponents();
            LoadOrders();
            LoadUsers();
        }

        private void LoadComponents()
        {
            try
            {
                var components = _context.Components
                    .Include(c => c.Category) // Добавляем загрузку категории
                    .OrderBy(c => c.Category.Name) // Сортируем по имени категории
                    .ThenBy(c => c.Name)
                    .ToList();

                _dgvComponents.Rows.Clear();
                foreach (var component in components)
                {
                    _dgvComponents.Rows.Add(
                        component.Id,
                        component.Category?.Name ?? "Без категории", // Используем Name категории
                        component.Name,
                        component.Manufacturer,
                        component.Price,
                        component.Quantity,
                        component.Socket,
                        component.FormFactor,
                        component.MemoryType
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки компонентов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadOrders()
        {
            try
            {
                IQueryable<Order> query = _context.Orders.OrderByDescending(o => o.CreatedAt);

                // Применяем фильтр по статусу
                if (_cmbOrderFilter.SelectedIndex > 0)
                {
                    string selectedStatus = _cmbOrderFilter.SelectedItem.ToString();
                    query = query.Where(o => o.Status == selectedStatus);
                }

                var orders = query.ToList();

                _dgvOrders.Rows.Clear();
                foreach (var order in orders)
                {
                    _dgvOrders.Rows.Add(
                        order.Id,
                        order.BuildId,
                        order.UserId,
                        order.TotalPrice,
                        order.Status,
                        order.CreatedAt,
                        order.UpdatedAt
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadUsers()
        {
            try
            {
                var users = _context.Users
                    .OrderBy(u => u.Role)
                    .ThenBy(u => u.Username)
                    .ToList();

                _dgvUsers.Rows.Clear();
                foreach (var user in users)
                {
                    _dgvUsers.Rows.Add(
                        user.Id,
                        user.Username,
                        user.Role,
                        user.CreatedAt,
                        user.IsActive ? "Да" : "Нет"
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStatistics(RichTextBox rtbStats)
        {
            try
            {
                rtbStats.Clear();

                // Основная статистика
                int totalUsers = _context.Users.Count();
                int totalComponents = _context.Components.Count();
                int totalBuilds = _context.Builds.Count();
                int totalOrders = _context.Orders.Count();

                decimal totalRevenue = _context.Orders
                    .Where(o => o.Status == "Completed")
                    .Sum(o => o.TotalPrice);

                // Статистика по заказам
                var ordersByStatus = _context.Orders
                    .GroupBy(o => o.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToList();

                // Популярные компоненты - исправленный
                var popularComponents = _context.BuildComponents
                    .GroupBy(bc => bc.ComponentId)
                    .Select(g => new
                    {
                        componentid = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .Take(5)
                    .ToList();

                rtbStats.SelectionColor = Color.Blue;
                rtbStats.SelectionFont = new Font("Consolas", 12, FontStyle.Bold);
                rtbStats.AppendText("=== ОБЩАЯ СТАТИСТИКА ===\n\n");

                rtbStats.SelectionFont = new Font("Consolas", 10, FontStyle.Regular);
                rtbStats.SelectionColor = Color.Black;
                rtbStats.AppendText($"Всего пользователей: {totalUsers}\n");
                rtbStats.AppendText($"Всего компонентов в каталоге: {totalComponents}\n");
                rtbStats.AppendText($"Всего созданных сборок: {totalBuilds}\n");
                rtbStats.AppendText($"Всего заказов: {totalOrders}\n");
                rtbStats.AppendText($"Общая выручка: {totalRevenue:C0}\n\n");

                rtbStats.SelectionColor = Color.DarkGreen;
                rtbStats.SelectionFont = new Font("Consolas", 11, FontStyle.Bold);
                rtbStats.AppendText("=== СТАТУСЫ ЗАКАЗОВ ===\n\n");

                rtbStats.SelectionFont = new Font("Consolas", 10, FontStyle.Regular);
                foreach (var stat in ordersByStatus)
                {
                    rtbStats.AppendText($"{stat.Status}: {stat.Count} заказов\n");
                }

                rtbStats.SelectionColor = Color.Purple;
                rtbStats.SelectionFont = new Font("Consolas", 11, FontStyle.Bold);
                rtbStats.AppendText("\n=== ПОПУЛЯРНЫЕ КОМПОНЕНТЫ ===\n\n");

                rtbStats.SelectionFont = new Font("Consolas", 10, FontStyle.Regular);
                foreach (var pop in popularComponents)
                {
                    var component = _context.Components.Find(pop.componentid);
                    if (component != null)
                    {
                        rtbStats.AppendText($"{component.Name}: {pop.Count} раз\n");
                    }
                }

                // Статистика по категориям - исправленный (через Category)
                try
                {
                    var componentsByCategory = _context.Components
                        .Include(c => c.Category) // Загружаем категорию
                        .AsEnumerable()
                        .GroupBy(c => c.Category?.Name ?? "Без категории")
                        .Select(g => new
                        {
                            Category = g.Key,
                            Count = g.Count(),
                            Total = g.Sum(c => c.Quantity)
                        })
                        .ToList();

                    rtbStats.SelectionColor = Color.DarkOrange;
                    rtbStats.SelectionFont = new Font("Consolas", 11, FontStyle.Bold);
                    rtbStats.AppendText("\n=== КОМПОНЕНТЫ ПО КАТЕГОРИЯМ ===\n\n");

                    rtbStats.SelectionFont = new Font("Consolas", 10, FontStyle.Regular);
                    foreach (var cat in componentsByCategory)
                    {
                        rtbStats.AppendText($"{cat.Category}: {cat.Count} позиций, {cat.Total} шт. на складе\n");
                    }
                }
                catch (Exception ex)
                {
                    rtbStats.AppendText($"\nНе удалось загрузить статистику по категориям: {ex.Message}\n");
                }
            }
            catch (Exception ex)
            {
                rtbStats.AppendText($"Ошибка загрузки статистики: {ex.Message}");
            }
        }
        private void BtnAddComponent_Click(object sender, EventArgs e)
        {
            try
            {
                var editForm = new ComponentEditForm(null, _context); // Передаем тот же контекст
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    // Принудительно перезагружаем компоненты
                    LoadComponents();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии формы: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnEditComponent_Click(object sender, EventArgs e)
        {
            if (_dgvComponents.CurrentRow != null && _dgvComponents.CurrentRow.Index >= 0)
            {
                try
                {
                    var componentId = (int)_dgvComponents.CurrentRow.Cells["Id"].Value;

                    // Загружаем компонент с категорией
                    var component = _context.Components
                        .Include(c => c.Category) // Важно: включаем категорию
                        .FirstOrDefault(c => c.Id == componentId);

                    if (component != null)
                    {
                        var editForm = new ComponentEditForm(component, _context); // Передаем тот же контекст
                        if (editForm.ShowDialog() == DialogResult.OK)
                        {
                            // Принудительно перезагружаем компоненты
                            LoadComponents();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Компонент не найден", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при редактировании: {ex.Message}\n\nДетали: {ex.InnerException?.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите компонент для редактирования", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void BtnDeleteComponent_Click(object sender, EventArgs e)
        {
            if (_dgvComponents.CurrentRow != null && _dgvComponents.CurrentRow.Index >= 0)
            {
                var componentid = (int)_dgvComponents.CurrentRow.Cells["Id"].Value;
                var componentName = _dgvComponents.CurrentRow.Cells["Name"].Value.ToString();

                var result = MessageBox.Show($"Удалить компонент '{componentName}'?\n\n" +
                    "Внимание: это действие нельзя отменить!",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        var component = _context.Components.Find(componentid);
                        if (component != null)
                        {
                            // Проверяем, используется ли компонент в сборках
                            var usedInBuilds = _context.BuildComponents.Any(bc => bc.ComponentId == componentid);

                            if (usedInBuilds)
                            {
                                MessageBox.Show("Невозможно удалить компонент, так как он используется в сборках.",
                                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }

                            _context.Components.Remove(component);
                            await _context.SaveChangesAsync();

                            MessageBox.Show("Компонент успешно удален", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                            LoadComponents();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void BtnProcessOrder_Click(object sender, EventArgs e)
        {
            await UpdateOrderStatus("Processing");
        }

        private async void BtnCompleteOrder_Click(object sender, EventArgs e)
        {
            await UpdateOrderStatus("Completed");
        }

        private async void BtnCancelOrder_Click(object sender, EventArgs e)
        {
            await UpdateOrderStatus("Cancelled");
        }

        private async Task UpdateOrderStatus(string newStatus)
        {
            if (_dgvOrders.CurrentRow != null && _dgvOrders.CurrentRow.Index >= 0)
            {
                var orderId = (int)_dgvOrders.CurrentRow.Cells["Id"].Value;
                var currentStatus = _dgvOrders.CurrentRow.Cells["Status"].Value.ToString();

                if (currentStatus == newStatus)
                {
                    MessageBox.Show($"Заказ уже имеет статус '{newStatus}'", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                try
                {
                    var order = _context.Orders.Find(orderId);
                    if (order != null)
                    {
                        order.Status = newStatus;
                        order.UpdatedAt = DateTime.Now;

                        // Если отменяем заказ, возвращаем компоненты на склад
                        if (newStatus == "Cancelled" && currentStatus != "Cancelled")
                        {
                            var buildComponents = _context.BuildComponents
                                .Where(bc => bc.BuildId == order.BuildId)
                                .ToList();

                            foreach (var bc in buildComponents)
                            {
                                var component = _context.Components.Find(bc.ComponentId);
                                if (component != null)
                                {
                                    component.Quantity += bc.Quantity;
                                    _context.Components.Update(component);
                                }
                            }
                        }

                        _context.Orders.Update(order);
                        await _context.SaveChangesAsync();

                        MessageBox.Show($"Статус заказа №{orderId} изменен на '{newStatus}'", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadOrders();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления статуса: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для изменения статуса", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void AdminPanelForm_Load(object sender, EventArgs e)
        {

        }
    }

    // Форма редактирования компонента
    public class ComponentEditForm : Form
    {
        private Core.Models.Component _component;
        private ApplicationDbContext _context;
        private bool _isNew;

        private TextBox _txtName;
        private ComboBox _cmbCategory;
        private TextBox _txtManufacturer;
        private NumericUpDown _nudPrice;
        private NumericUpDown _nudQuantity;
        private TextBox _txtSocket;
        private TextBox _txtFormFactor;
        private TextBox _txtMemoryType;
        private NumericUpDown _nudPowerSupply;
        private NumericUpDown _nudMaxMemory;
        private NumericUpDown _nudTDP;
        private NumericUpDown _nudLength;
        private NumericUpDown _nudWidth;
        private NumericUpDown _nudHeight;

        public ComponentEditForm(Core.Models.Component component, ApplicationDbContext context)
        {
            _component = component;
            _context = context; // Используем переданный контекст
            _isNew = component == null;

            // Уберите InitializeComponent();
            SetupForm();
            LoadComponentData();
        }

        private void SetupForm()
        {
            this.Text = _isNew ? "Добавить компонент" : "Редактировать компонент";
            this.Size = new Size(500, 450); // Увеличим высоту
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 11, // Увеличили на 1
                Padding = new Padding(10),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));

            // Категория
            tableLayout.Controls.Add(new Label { Text = "Категория*:", TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            _cmbCategory = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbCategory.Items.AddRange(new string[]
            {
        "CPU", "Motherboard", "RAM", "GPU", "PSU", "Case"
            });
            tableLayout.Controls.Add(_cmbCategory, 1, 0);

            // Название
            tableLayout.Controls.Add(new Label { Text = "Название*:", TextAlign = ContentAlignment.MiddleRight }, 0, 1);
            _txtName = new TextBox { Dock = DockStyle.Fill };
            tableLayout.Controls.Add(_txtName, 1, 1);

            // Производитель
            tableLayout.Controls.Add(new Label { Text = "Производитель:", TextAlign = ContentAlignment.MiddleRight }, 0, 2);
            _txtManufacturer = new TextBox { Dock = DockStyle.Fill };
            tableLayout.Controls.Add(_txtManufacturer, 1, 2);

            // Цена
            tableLayout.Controls.Add(new Label { Text = "Цена* (руб):", TextAlign = ContentAlignment.MiddleRight }, 0, 3);
            _nudPrice = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 0, Maximum = 1000000, DecimalPlaces = 0 };
            tableLayout.Controls.Add(_nudPrice, 1, 3);

            // Количество
            tableLayout.Controls.Add(new Label { Text = "Количество*:", TextAlign = ContentAlignment.MiddleRight }, 0, 4);
            _nudQuantity = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 0, Maximum = 10000, DecimalPlaces = 0 };
            tableLayout.Controls.Add(_nudQuantity, 1, 4);

            // Сокет
            tableLayout.Controls.Add(new Label { Text = "Сокет:", TextAlign = ContentAlignment.MiddleRight }, 0, 5);
            _txtSocket = new TextBox { Dock = DockStyle.Fill };
            tableLayout.Controls.Add(_txtSocket, 1, 5);

            // Форм-фактор
            tableLayout.Controls.Add(new Label { Text = "Форм-фактор:", TextAlign = ContentAlignment.MiddleRight }, 0, 6);
            _txtFormFactor = new TextBox { Dock = DockStyle.Fill };
            tableLayout.Controls.Add(_txtFormFactor, 1, 6);

            // Тип памяти - ВСЕГДА заполняем!
            tableLayout.Controls.Add(new Label { Text = "Тип памяти*:", TextAlign = ContentAlignment.MiddleRight }, 0, 7);
            _txtMemoryType = new TextBox { Dock = DockStyle.Fill };
            tableLayout.Controls.Add(_txtMemoryType, 1, 7);

            // Мощность
            tableLayout.Controls.Add(new Label { Text = "Мощность (Вт):", TextAlign = ContentAlignment.MiddleRight }, 0, 8);
            _nudPowerSupply = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 0, Maximum = 2000, DecimalPlaces = 0 };
            tableLayout.Controls.Add(_nudPowerSupply, 1, 8);

            // TDP
            tableLayout.Controls.Add(new Label { Text = "TDP (Вт):", TextAlign = ContentAlignment.MiddleRight }, 0, 9);
            var nudTDP = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 0, Maximum = 500, DecimalPlaces = 0, Name = "nudTDP" };
            tableLayout.Controls.Add(nudTDP, 1, 9);

            // Кнопки
            var buttonPanel = new Panel { Dock = DockStyle.Bottom, Height = 50 };
            var btnSave = new Button
            {
                Text = "Сохранить",
                DialogResult = DialogResult.OK,
                Size = new Size(100, 30),
                Location = new Point(150, 10),
                BackColor = Color.LightGreen
            };
            var btnCancel = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Size = new Size(100, 30),
                Location = new Point(260, 10)
            };

            buttonPanel.Controls.AddRange(new Control[] { btnSave, btnCancel });

            this.Controls.Add(tableLayout);
            this.Controls.Add(buttonPanel);

            this.AcceptButton = btnSave;
            this.CancelButton = btnCancel;
        }

        private void LoadComponentData()
        {
            if (!_isNew && _component != null)
            {
                _txtName.Text = _component.Name;

                var categoryName = _component.Category?.Name;
                if (!string.IsNullOrEmpty(categoryName) && _cmbCategory.Items.Contains(categoryName))
                {
                    _cmbCategory.SelectedItem = categoryName;
                }
                else if (_cmbCategory.Items.Count > 0)
                {
                    _cmbCategory.SelectedIndex = 0;
                }

                _txtManufacturer.Text = _component.Manufacturer;
                _nudPrice.Value = _component.Price;
                _nudQuantity.Value = _component.Quantity;
                _txtSocket.Text = _component.Socket ?? "";
                _txtFormFactor.Text = _component.FormFactor ?? "";
                _txtMemoryType.Text = _component.MemoryType ?? "";
                _nudPowerSupply.Value = _component.PowerSupply.GetValueOrDefault();

                // TDP
                var nudTDP = this.Controls.Find("nudTDP", true).FirstOrDefault() as NumericUpDown;
                if (nudTDP != null)
                {
                    nudTDP.Value = _component.TDP.GetValueOrDefault();
                }
            }
            else
            {
                _cmbCategory.SelectedIndex = 0;
                _nudPrice.Value = 1000;
                _nudQuantity.Value = 1;
                _nudPowerSupply.Value = 0;
                _txtMemoryType.Text = ""; // Пустая строка по умолчанию

                var nudTDP = this.Controls.Find("nudTDP", true).FirstOrDefault() as NumericUpDown;
                if (nudTDP != null)
                {
                    nudTDP.Value = 0;
                }
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(_txtName.Text))
            {
                MessageBox.Show("Введите название компонента", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtName.Focus();
                return false;
            }

            if (_cmbCategory.SelectedIndex < 0)
            {
                MessageBox.Show("Выберите категорию компонента", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _cmbCategory.Focus();
                return false;
            }

            if (_nudPrice.Value <= 0)
            {
                MessageBox.Show("Цена должна быть больше 0", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _nudPrice.Focus();
                return false;
            }

            if (_nudQuantity.Value < 0)
            {
                MessageBox.Show("Количество не может быть отрицательным", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _nudQuantity.Focus();
                return false;
            }

            // Тип памяти ВСЕГДА должен быть заполнен (хотя бы пустой строкой)
            // Валидация не нужна, так как мы всегда установим пустую строку если null

            return true;
        }

        protected override async void OnFormClosing(FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                if (!ValidateInput())
                {
                    e.Cancel = true;
                    return;
                }

                try
                {
                    using var localContext = new ApplicationDbContext(); // Новый контекст

                    var selectedCategoryName = _cmbCategory.SelectedItem.ToString();
                    var category = await localContext.Categories
                        .FirstOrDefaultAsync(c => c.Name == selectedCategoryName);

                    if (category == null)
                    {
                        MessageBox.Show("Выбранная категория не найдена", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        e.Cancel = true;
                        return;
                    }

                    if (_isNew)
                    {
                        _component = new Core.Models.Component();
                    }
                    else
                    {
                        // Загружаем существующий компонент из БД
                        _component = await localContext.Components
                            .FirstOrDefaultAsync(c => c.Id == _component.Id);

                        if (_component == null)
                        {
                            MessageBox.Show("Компонент не найден в базе данных", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            e.Cancel = true;
                            return;
                        }
                    }

                    // Заполняем поля
                    _component.Name = _txtName.Text.Trim();
                    _component.CategoryId = category.Id;
                    _component.Category = category;
                    _component.Manufacturer = _txtManufacturer.Text.Trim();
                    _component.Price = _nudPrice.Value;
                    _component.Quantity = (int)_nudQuantity.Value;
                    _component.Socket = _txtSocket.Text.Trim();
                    _component.FormFactor = _txtFormFactor.Text.Trim();
                    _component.MemoryType = _txtMemoryType.Text.Trim(); // ВСЕГДА заполняем!
                    _component.PowerSupply = _nudPowerSupply.Value > 0 ? (int?)_nudPowerSupply.Value : null;

                    // TDP
                    var nudTDP = this.Controls.Find("nudTDP", true).FirstOrDefault() as NumericUpDown;
                    if (nudTDP != null)
                    {
                        _component.TDP = nudTDP.Value > 0 ? (int?)nudTDP.Value : null;
                    }

                    if (_isNew)
                    {
                        localContext.Components.Add(_component);
                    }
                    else
                    {
                        localContext.Components.Update(_component);
                    }

                    await localContext.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения: {ex.Message}\n\n" +
                                   $"Детали: {ex.InnerException?.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Cancel = true;
                }
            }
            base.OnFormClosing(e);
        }
    }
}
