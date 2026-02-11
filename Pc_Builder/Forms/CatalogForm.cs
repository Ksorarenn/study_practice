using Microsoft.EntityFrameworkCore;
using PC_Builder.Core.Models;
using PC_Builder.Core.Services;
using PC_Builder.Data;
using System.Data;

namespace PC_Builder.WinForms.Forms
{
    public class CatalogForm : Form
    {
        private User _currentUser;
        private ApplicationDbContext _context;
        private CompatibilityService _compatibilityService;
        private List<Component> _selectedComponents = new List<Component>();
        private Build _currentBuild = new Build();

        private DataGridView _dgvComponents;
        private DataGridView _dgvSelected;
        private Button _btnAdd;
        private Button _btnRemove;
        private Button _btnCheckCompatibility;
        private Button _btnCreateOrder;
        private Button _btnClearBuild;
        private Label _lblTotalPrice;
        private RichTextBox _rtbCompatibility;
        private ComboBox _cmbCategoryFilter;

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
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Text = "CatalogForm";
        }


        public CatalogForm(User user, ApplicationDbContext context)
        {
            _currentUser = user;
            _context = context;
            _compatibilityService = new CompatibilityService();
            InitializeComponent();
            SetupForm();
            LoadComponents();
        }

        private void SetupForm()
        {
            this.Text = "Каталог комплектующих";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(800, 600);

            // Создаем главный контейнер с разделением
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 600
            };

            // Левая панель: каталог
            var leftPanel = new Panel { Dock = DockStyle.Fill };

            // Панель фильтрации
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblFilter = new Label
            {
                Text = "Фильтр по категории:",
                Location = new Point(10, 10),
                Size = new Size(120, 20)
            };

            _cmbCategoryFilter = new ComboBox
            {
                Location = new Point(140, 7),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            _cmbCategoryFilter.Items.AddRange(new string[]
            {
                "Все категории", "CPU", "Motherboard", "RAM", "GPU", "PSU", "Case"
            });
            _cmbCategoryFilter.SelectedIndex = 0;
            _cmbCategoryFilter.SelectedIndexChanged += CmbCategoryFilter_SelectedIndexChanged;

            var btnRefresh = new Button
            {
                Text = "Обновить",
                Location = new Point(300, 7),
                Size = new Size(80, 25)
            };
            btnRefresh.Click += (s, e) => LoadComponents();

            filterPanel.Controls.AddRange(new Control[] { lblFilter, _cmbCategoryFilter, btnRefresh });

            var lblCatalog = new Label
            {
                Text = "Каталог комплектующих (двойной клик для добавления)",
                Font = new Font("Arial", 10, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.LightGray
            };

            _dgvComponents = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToResizeRows = false,
                BackgroundColor = SystemColors.Window
            };
            _dgvComponents.CellDoubleClick += DgvComponents_CellDoubleClick;

            _btnAdd = new Button
            {
                Text = "Добавить в сборку →",
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.LightGreen,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            _btnAdd.Click += BtnAdd_Click;

            leftPanel.Controls.Add(_dgvComponents);
            leftPanel.Controls.Add(_btnAdd);
            leftPanel.Controls.Add(lblCatalog);
            leftPanel.Controls.Add(filterPanel);

            // Правая панель: сборка
            var rightPanel = new Panel { Dock = DockStyle.Fill };

            var lblSelected = new Label
            {
                Text = "Текущая сборка (выбрано: 0 компонентов)",
                Font = new Font("Arial", 10, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.LightGray,
                Name = "lblSelectedCount"
            };

            _dgvSelected = new DataGridView
            {
                Dock = DockStyle.Fill,
                Height = 200,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = SystemColors.Window
            };

            // Панель управления сборкой
            var buildControlPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 120
            };

            _btnRemove = new Button
            {
                Text = "✗ Удалить выбранное",
                Location = new Point(10, 10),
                Size = new Size(150, 30),
                BackColor = Color.LightCoral
            };
            _btnRemove.Click += BtnRemove_Click;

            _btnClearBuild = new Button
            {
                Text = "🗑 Очистить сборку",
                Location = new Point(170, 10),
                Size = new Size(150, 30)
            };
            _btnClearBuild.Click += BtnClearBuild_Click;

            _lblTotalPrice = new Label
            {
                Text = "Итого: 0 ₽",
                Location = new Point(330, 15),
                Size = new Size(200, 25),
                Font = new Font("Arial", 12, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.Green
            };

            _btnCheckCompatibility = new Button
            {
                Text = "🔍 Проверить совместимость",
                Location = new Point(10, 50),
                Size = new Size(200, 35),
                BackColor = Color.LightBlue,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            _btnCheckCompatibility.Click += BtnCheckCompatibility_Click;

            _btnCreateOrder = new Button
            {
                Text = "✅ Создать заказ",
                Location = new Point(220, 50),
                Size = new Size(200, 35),
                BackColor = Color.LightGreen,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Enabled = false
            };
            _btnCreateOrder.Click += BtnCreateOrder_Click;

            buildControlPanel.Controls.AddRange(new Control[]
            {
                _btnRemove, _btnClearBuild, _lblTotalPrice,
                _btnCheckCompatibility, _btnCreateOrder
            });

            // Панель результатов проверки
            var resultPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 150,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblResults = new Label
            {
                Text = "Результаты проверки совместимости:",
                Dock = DockStyle.Top,
                Height = 25,
                Font = new Font("Arial", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.LightGray
            };

            _rtbCompatibility = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.WhiteSmoke,
                Font = new Font("Consolas", 9)
            };

            resultPanel.Controls.Add(_rtbCompatibility);
            resultPanel.Controls.Add(lblResults);

            rightPanel.Controls.Add(_dgvSelected);
            rightPanel.Controls.Add(buildControlPanel);
            rightPanel.Controls.Add(resultPanel);
            rightPanel.Controls.Add(lblSelected);

            splitContainer.Panel1.Controls.Add(leftPanel);
            splitContainer.Panel2.Controls.Add(rightPanel);

            this.Controls.Add(splitContainer);

            // Настройка DataGridView каталога
            SetupComponentsGrid();
            SetupSelectedGrid();
        }

        private void SetupComponentsGrid()
        {
            _dgvComponents.Columns.Clear();

            var columns = new[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 40 },
                new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "Категория", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Название", Width = 200 },
                new DataGridViewTextBoxColumn { Name = "Manufacturer", HeaderText = "Производитель", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Price", HeaderText = "Цена", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "Quantity", HeaderText = "В наличии", Width = 70 },
                new DataGridViewTextBoxColumn { Name = "Specs", HeaderText = "Характеристики", Width = 250 }
            };

            _dgvComponents.Columns.AddRange(columns);
            _dgvComponents.Columns["Id"].Visible = false;
            _dgvComponents.Columns["Price"].DefaultCellStyle.Format = "C0";
            _dgvComponents.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _dgvComponents.Columns["Quantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        private void SetupSelectedGrid()
        {
            _dgvSelected.Columns.Clear();

            var columns = new[]
            {
                new DataGridViewTextBoxColumn { Name = "Category", HeaderText = "Категория", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Название", Width = 250 },
                new DataGridViewTextBoxColumn { Name = "Price", HeaderText = "Цена", Width = 80 }
            };

            _dgvSelected.Columns.AddRange(columns);
            _dgvSelected.Columns["Price"].DefaultCellStyle.Format = "C0";
            _dgvSelected.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        private async void LoadComponents()
        {
            try
            {
                // Используем новый контекст для этой операции
                using var localContext = new ApplicationDbContext();

                IQueryable<Component> query = localContext.Components
                    .Include(c => c.Category)
                    .Where(c => c.Quantity > 0);

                // Применяем фильтр по категории
                if (_cmbCategoryFilter.SelectedIndex > 0)
                {
                    string selectedCategory = _cmbCategoryFilter.SelectedItem.ToString();
                    query = query.Where(c => c.Category.Name == selectedCategory);
                }

                var components = await query
                    .OrderBy(c => c.Category.Name)
                    .ThenBy(c => c.Name)
                    .ToListAsync(); // Используем асинхронный вызов

                // Обновляем UI в основном потоке
                this.Invoke((MethodInvoker)delegate
                {
                    _dgvComponents.Rows.Clear();
                    foreach (var component in components)
                    {
                        _dgvComponents.Rows.Add(
                            component.Id,
                            component.Category?.Name ?? "Без категории",
                            component.Name,
                            component.Manufacturer ?? "",
                            component.Price,
                            component.Quantity,
                            component.Specifications
                        );

                        int rowIndex = _dgvComponents.Rows.Count - 1;
                        if (component.Quantity < 3)
                        {
                            _dgvComponents.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightYellow;
                        }
                    }

                    _btnAdd.Enabled = _dgvComponents.Rows.Count > 0;
                });
            }
            catch (Exception ex)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    MessageBox.Show($"Ошибка загрузки компонентов: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
        }

        private void CmbCategoryFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadComponents();
        }

        private void DgvComponents_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                AddSelectedComponent();
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            AddSelectedComponent();
        }

        private void AddSelectedComponent()
        {
            if (_dgvComponents.CurrentRow != null && _dgvComponents.CurrentRow.Index >= 0)
            {
                var componentId = (int)_dgvComponents.CurrentRow.Cells["Id"].Value;
                var component = _context.Components.Find(componentId);

                if (component != null)
                {
                    // Проверяем наличие
                    if (component.Quantity <= 0)
                    {
                        MessageBox.Show("Данный компонент отсутствует на складе", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Проверяем, есть ли уже такой компонент (кроме RAM)
                    var existing = _selectedComponents.FirstOrDefault(c => c.Id == component.Id);
                    if (existing != null && component.Category?.Name != "RAM")
                    {
                        MessageBox.Show("Этот компонент уже добавлен в сборку", "Информация",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    _selectedComponents.Add(component);
                    UpdateSelectedGrid();
                    UpdateTotalPrice();
                    ClearCompatibilityResults();

                    // Обновляем количество в таблице
                    int currentRow = _dgvComponents.CurrentRow.Index;
                    int newQuantity = component.Quantity - 1;
                    _dgvComponents.Rows[currentRow].Cells["Quantity"].Value = newQuantity;

                    // Подсветка если мало осталось
                    if (newQuantity < 3)
                    {
                        _dgvComponents.Rows[currentRow].DefaultCellStyle.BackColor = Color.LightYellow;
                    }
                }
            }
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            if (_dgvSelected.CurrentRow != null && _dgvSelected.CurrentRow.Index >= 0)
            {
                int index = _dgvSelected.CurrentRow.Index;
                if (index < _selectedComponents.Count)
                {
                    var removedComponent = _selectedComponents[index];

                    // Возвращаем количество в таблице каталога
                    foreach (DataGridViewRow row in _dgvComponents.Rows)
                    {
                        if (row.Cells["Id"].Value is int id && id == removedComponent.Id)
                        {
                            int currentQuantity = (int)row.Cells["Quantity"].Value;
                            row.Cells["Quantity"].Value = currentQuantity + 1;

                            if (currentQuantity + 1 < 3)
                            {
                                row.DefaultCellStyle.BackColor = Color.LightYellow;
                            }
                            else
                            {
                                row.DefaultCellStyle.BackColor = SystemColors.Window;
                            }
                            break;
                        }
                    }

                    _selectedComponents.RemoveAt(index);
                    UpdateSelectedGrid();
                    UpdateTotalPrice();
                    ClearCompatibilityResults();
                }
            }
        }

        private void BtnClearBuild_Click(object sender, EventArgs e)
        {
            if (_selectedComponents.Count > 0)
            {
                var result = MessageBox.Show("Очистить всю сборку?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Возвращаем все количества в каталог
                    foreach (var component in _selectedComponents)
                    {
                        foreach (DataGridViewRow row in _dgvComponents.Rows)
                        {
                            if (row.Cells["Id"].Value is int id && id == component.Id)
                            {
                                int currentQuantity = (int)row.Cells["Quantity"].Value;
                                row.Cells["Quantity"].Value = currentQuantity + 1;

                                if (currentQuantity + 1 < 3)
                                {
                                    row.DefaultCellStyle.BackColor = Color.LightYellow;
                                }
                                else
                                {
                                    row.DefaultCellStyle.BackColor = SystemColors.Window;
                                }
                                break;
                            }
                        }
                    }

                    _selectedComponents.Clear();
                    UpdateSelectedGrid();
                    UpdateTotalPrice();
                    ClearCompatibilityResults();
                }
            }
        }

        private void UpdateSelectedGrid()
        {
            _dgvSelected.Rows.Clear();

            foreach (var component in _selectedComponents)
            {
                _dgvSelected.Rows.Add(
                    component.Category,
                    component.Name,
                    component.Price
                );
            }

            // Обновляем заголовок с количеством
            var lblSelectedCount = this.Controls.Find("lblSelectedCount", true).FirstOrDefault() as Label;
            if (lblSelectedCount != null)
            {
                lblSelectedCount.Text = $"Текущая сборка (выбрано: {_selectedComponents.Count} компонентов)";
            }
        }

        private void UpdateTotalPrice()
        {
            var total = _selectedComponents.Sum(c => c.Price);
            _lblTotalPrice.Text = $"Итого: {total:C0}";
            _currentBuild.TotalPrice = total;

            // Подсветка если дорого
            if (total > 100000)
            {
                _lblTotalPrice.ForeColor = Color.Red;
            }
            else if (total > 50000)
            {
                _lblTotalPrice.ForeColor = Color.Orange;
            }
            else
            {
                _lblTotalPrice.ForeColor = Color.Green;
            }
        }

        private void BtnCheckCompatibility_Click(object sender, EventArgs e)
        {
            if (_selectedComponents.Count == 0)
            {
                MessageBox.Show("Добавьте компоненты для проверки совместимости", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var result = _compatibilityService.CheckCompatibility(_selectedComponents);

                _rtbCompatibility.Clear();

                if (result.IsCompatible && !result.Errors.Any() && !result.Warnings.Any())
                {
                    _rtbCompatibility.SelectionColor = Color.Green;
                    _rtbCompatibility.AppendText("✅ ВСЕ КОМПОНЕНТЫ СОВМЕСТИМЫ!\n\n");
                    _rtbCompatibility.AppendText("Сборка готова к оформлению заказа.\n");
                    _rtbCompatibility.AppendText($"Общая стоимость: {_currentBuild.TotalPrice:C0}\n");
                    _btnCreateOrder.Enabled = true;
                }
                else
                {
                    if (result.Errors.Any())
                    {
                        _rtbCompatibility.SelectionColor = Color.Red;
                        _rtbCompatibility.AppendText("❌ КРИТИЧЕСКИЕ ОШИБКИ:\n");
                        foreach (var error in result.Errors)
                        {
                            _rtbCompatibility.AppendText($"• {error}\n");
                        }
                        _rtbCompatibility.AppendText("\n");
                        _btnCreateOrder.Enabled = false;
                    }

                    if (result.Warnings.Any())
                    {
                        _rtbCompatibility.SelectionColor = Color.DarkOrange;
                        _rtbCompatibility.AppendText("⚠ ПРЕДУПРЕЖДЕНИЯ:\n");
                        foreach (var warning in result.Warnings)
                        {
                            _rtbCompatibility.AppendText($"• {warning}\n");
                        }
                        _rtbCompatibility.AppendText("\n");
                        _btnCreateOrder.Enabled = true; // С предупреждениями можно создать заказ
                    }

                    if (result.IsCompatible && result.Errors.Count == 0)
                    {
                        _rtbCompatibility.SelectionColor = Color.Green;
                        _rtbCompatibility.AppendText("✅ Сборка возможна с учетом предупреждений.\n");
                    }
                }

                // Добавляем сводку по компонентам
                _rtbCompatibility.SelectionColor = Color.Blue;
                _rtbCompatibility.AppendText("\n--- СОСТАВ СБОРКИ ---\n");
                foreach (var component in _selectedComponents)
                {
                    _rtbCompatibility.AppendText($"• {component.Category}: {component.Name}\n");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке совместимости: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearCompatibilityResults()
        {
            _rtbCompatibility.Clear();
            _btnCreateOrder.Enabled = false;
        }

        private async void BtnCreateOrder_Click(object sender, EventArgs e)
        {
            if (_selectedComponents.Count == 0)
            {
                MessageBox.Show("Добавьте компоненты в сборку", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Используем транзакцию для целостности данных
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Проверяем наличие всех компонентов
                foreach (var component in _selectedComponents)
                {
                    var dbComponent = await _context.Components.FindAsync(component.Id);
                    if (dbComponent == null || dbComponent.Quantity <= 0)
                    {
                        MessageBox.Show($"Компонент '{component.Name}' больше не доступен", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                // Запрашиваем название сборки
                using (var inputForm = new InputForm("Введите название сборки:", "Название сборки"))
                {
                    if (inputForm.ShowDialog() == DialogResult.OK)
                    {
                        _currentBuild.Name = string.IsNullOrWhiteSpace(inputForm.InputText)
                            ? $"Сборка от {DateTime.Now:dd.MM.yyyy HH:mm}"
                            : inputForm.InputText;
                    }
                    else
                    {
                        return; // Пользователь отменил
                    }
                }

                // Сохраняем сборку
                _currentBuild.UserId = _currentUser.Id;
                _currentBuild.Status = "Draft";
                _currentBuild.CreatedAt = DateTime.Now;
                _currentBuild.TotalPrice = _selectedComponents.Sum(c => c.Price);

                _context.Builds.Add(_currentBuild);
                await _context.SaveChangesAsync(); // Сохраняем чтобы получить Id

                // Создаем связи BuildComponent
                foreach (var component in _selectedComponents)
                {
                    var buildComponent = new BuildComponent
                    {
                        BuildId = _currentBuild.Id,
                        ComponentId = component.Id,
                        Quantity = 1,
                        AddedAt = DateTime.Now
                    };

                    _context.BuildComponents.Add(buildComponent);

                    // Уменьшаем количество на складе
                    var dbComponent = await _context.Components.FindAsync(component.Id);
                    if (dbComponent != null)
                    {
                        dbComponent.Quantity--;
                        _context.Components.Update(dbComponent);
                    }
                }

                // Создаем заказ
                var order = new Order
                {
                    BuildId = _currentBuild.Id,
                    UserId = _currentUser.Id,
                    TotalPrice = _currentBuild.TotalPrice,
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };

                _context.Orders.Add(order);

                // Сохраняем все изменения
                await _context.SaveChangesAsync();

                // Фиксируем транзакцию
                await transaction.CommitAsync();

                MessageBox.Show($"Заказ №{order.Id} успешно создан!\n\n" +
                              $"Название: {_currentBuild.Name}\n" +
                              $"Стоимость: {order.TotalPrice:C0}\n" +
                              $"Статус: {order.Status}\n\n" +
                              $"Заказ отправлен на обработку администратору.",
                              "Заказ создан",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Очищаем текущую сборку
                _selectedComponents.Clear();
                UpdateSelectedGrid();
                UpdateTotalPrice();
                ClearCompatibilityResults();

                // Перезагружаем каталог
                LoadComponents();

                // Создаем новую сборку
                _currentBuild = new Build();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                string errorDetails = ex.Message;
                if (ex.InnerException != null)
                {
                    errorDetails += $"\n\nДетали: {ex.InnerException.Message}";
                }

                MessageBox.Show($"Ошибка при создании заказа: {errorDetails}",
                               "Ошибка",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Выводим в консоль для отладки
                Console.WriteLine($"ОШИБКА: {ex}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_selectedComponents.Count > 0)
            {
                var result = MessageBox.Show("В сборке есть несохраненные компоненты. Закрыть форму?",
                    "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
            base.OnFormClosing(e);
        }
    }

    // Вспомогательная форма для ввода текста
    public class InputForm : Form
    {
        private TextBox _txtInput;
        public string InputText => _txtInput.Text;

        public InputForm(string prompt, string title)
        {
            this.Text = title;
            this.Size = new Size(400, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            var lblPrompt = new Label
            {
                Text = prompt,
                Location = new Point(20, 20),
                Size = new Size(350, 30)
            };

            _txtInput = new TextBox
            {
                Location = new Point(20, 60),
                Size = new Size(340, 25)
            };

            var btnOk = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(180, 95),
                Size = new Size(80, 30)
            };

            var btnCancel = new Button
            {
                Text = "Отмена",
                DialogResult = DialogResult.Cancel,
                Location = new Point(270, 95),
                Size = new Size(80, 30)
            };

            this.Controls.AddRange(new Control[] { lblPrompt, _txtInput, btnOk, btnCancel });
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }
    }
}