using Microsoft.EntityFrameworkCore;
using PC_Builder.Core.Models;
using PC_Builder.Data;
using System.Data;

namespace PC_Builder.WinForms.Forms
{
    public class BuildForm : Form
    {
        private User _currentUser;
        private ApplicationDbContext _context;
        private DataGridView _dgvBuilds;
        private Button _btnViewDetails;
        private Button _btnDelete;
        private Button _btnRefresh;
        private RichTextBox _rtbBuildDetails;
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
            this.Text = "BuildForm";
        }

        public BuildForm(User user, ApplicationDbContext context)
        {
            _currentUser = user;
            _context = context;
            InitializeComponent();
            SetupForm();
            LoadBuilds();
        }

        private void SetupForm()
        {
            this.Text = $"Мои сборки - {_currentUser.Username}";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            // Разделение на две части
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 300
            };

            // Верхняя панель: список сборок
            var topPanel = new Panel { Dock = DockStyle.Fill };

            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lblTitle = new Label
            {
                Text = "Мои сборки",
                Font = new Font("Arial", 11, FontStyle.Bold),
                Location = new Point(10, 10),
                Size = new Size(200, 20)
            };

            _btnRefresh = new Button
            {
                Text = "Обновить",
                Location = new Point(220, 7),
                Size = new Size(100, 25)
            };
            _btnRefresh.Click += (s, e) => LoadBuilds();

            headerPanel.Controls.AddRange(new Control[] { lblTitle, _btnRefresh });

            _dgvBuilds = new DataGridView
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

            // Панель кнопок
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BorderStyle = BorderStyle.FixedSingle
            };

            _btnViewDetails = new Button
            {
                Text = "Просмотреть детали",
                Location = new Point(10, 7),
                Size = new Size(150, 25),
                BackColor = Color.LightBlue
            };
            _btnViewDetails.Click += BtnViewDetails_Click;

            _btnDelete = new Button
            {
                Text = "Удалить сборку",
                Location = new Point(170, 7),
                Size = new Size(150, 25),
                BackColor = Color.LightCoral,
                Enabled = false
            };
            _btnDelete.Click += BtnDelete_Click;

            buttonPanel.Controls.AddRange(new Control[] { _btnViewDetails, _btnDelete });

            topPanel.Controls.Add(_dgvBuilds);
            topPanel.Controls.Add(buttonPanel);
            topPanel.Controls.Add(headerPanel);

            // Нижняя панель: детали сборки
            var bottomPanel = new Panel { Dock = DockStyle.Fill };

            var lblDetails = new Label
            {
                Text = "Детали сборки",
                Font = new Font("Arial", 11, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.LightGray
            };

            _rtbBuildDetails = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.WhiteSmoke,
                Font = new Font("Consolas", 9)
            };

            bottomPanel.Controls.Add(_rtbBuildDetails);
            bottomPanel.Controls.Add(lblDetails);

            splitContainer.Panel1.Controls.Add(topPanel);
            splitContainer.Panel2.Controls.Add(bottomPanel);

            this.Controls.Add(splitContainer);

            // Настройка DataGridView
            SetupBuildsGrid();
        }

        private void SetupBuildsGrid()
        {
            _dgvBuilds.Columns.Clear();

            var columns = new[]
            {
                new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "ID", Width = 50 },
                new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Название", Width = 200 },
                new DataGridViewTextBoxColumn { Name = "TotalPrice", HeaderText = "Стоимость", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Статус", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "CreatedAt", HeaderText = "Дата создания", Width = 120 }
            };

            _dgvBuilds.Columns.AddRange(columns);

            _dgvBuilds.Columns["TotalPrice"].DefaultCellStyle.Format = "C0";
            _dgvBuilds.Columns["TotalPrice"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            _dgvBuilds.Columns["CreatedAt"].DefaultCellStyle.Format = "dd.MM.yyyy";

            _dgvBuilds.SelectionChanged += (s, e) =>
            {
                _btnDelete.Enabled = _dgvBuilds.SelectedRows.Count > 0;
            };
        }

        private void LoadBuilds()
        {
            try
            {
                var builds = _context.Builds
                    .Where(b => b.UserId == _currentUser.Id)
                    .OrderByDescending(b => b.CreatedAt)
                    .ToList();

                _dgvBuilds.Rows.Clear();
                foreach (var build in builds)
                {
                    _dgvBuilds.Rows.Add(
                        build.Id,
                        build.Name,
                        build.TotalPrice,
                        build.Status,
                        build.CreatedAt
                    );

                    // Подсветка в зависимости от статуса
                    int rowIndex = _dgvBuilds.Rows.Count - 1;
                    switch (build.Status)
                    {
                        case "Completed":
                            _dgvBuilds.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightGreen;
                            break;
                        case "Cancelled":
                            _dgvBuilds.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightCoral;
                            break;
                        case "Processing":
                            _dgvBuilds.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightBlue;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сборок: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnViewDetails_Click(object sender, EventArgs e)
        {
            if (_dgvBuilds.CurrentRow != null && _dgvBuilds.CurrentRow.Index >= 0)
            {
                try
                {
                    var buildid = (int)_dgvBuilds.CurrentRow.Cells["Id"].Value;

                    // Получаем сборку с компонентами
                    var build = _context.Builds.Find(buildid);
                    var buildComponents = _context.BuildComponents
                        .Where(bc => bc.BuildId == buildid)
                        .Include(bc => bc.Component)
                        .ToList();

                    _rtbBuildDetails.Clear();

                    _rtbBuildDetails.SelectionColor = Color.Blue;
                    _rtbBuildDetails.SelectionFont = new Font("Consolas", 10, FontStyle.Bold);
                    _rtbBuildDetails.AppendText($"=== СБОРКА: {build.Name} ===\n\n");

                    _rtbBuildDetails.SelectionFont = new Font("Consolas", 9, FontStyle.Regular);
                    _rtbBuildDetails.AppendText($"ID: {build.Id}\n");
                    _rtbBuildDetails.AppendText($"Статус: {build.Status}\n");
                    _rtbBuildDetails.AppendText($"Дата создания: {build.CreatedAt:dd.MM.yyyy HH:mm}\n");
                    _rtbBuildDetails.AppendText($"Общая стоимость: {build.TotalPrice:C0}\n\n");

                    _rtbBuildDetails.SelectionColor = Color.DarkGreen;
                    _rtbBuildDetails.AppendText("=== КОМПОНЕНТЫ ===\n\n");

                    if (buildComponents.Any())
                    {
                        foreach (var bc in buildComponents)
                        {
                            _rtbBuildDetails.SelectionColor = Color.Black;
                            _rtbBuildDetails.AppendText($"• {bc.Component.Category}: {bc.Component.Name}\n");
                            _rtbBuildDetails.SelectionColor = Color.Gray;
                            _rtbBuildDetails.AppendText($"  Цена: {bc.Component.Price:C0}\n");
                            _rtbBuildDetails.AppendText($"  Характеристики: {bc.Component.Specifications}\n\n");
                        }
                    }
                    else
                    {
                        _rtbBuildDetails.SelectionColor = Color.Red;
                        _rtbBuildDetails.AppendText("Компоненты не найдены\n");
                    }

                    // Проверяем есть ли заказ для этой сборки
                    var order = _context.Orders.FirstOrDefault(o => o.BuildId == buildid);
                    if (order != null)
                    {
                        _rtbBuildDetails.SelectionColor = Color.Purple;
                        _rtbBuildDetails.AppendText("\n=== ИНФОРМАЦИЯ О ЗАКАЗЕ ===\n\n");
                        _rtbBuildDetails.SelectionColor = Color.Black;
                        _rtbBuildDetails.AppendText($"Номер заказа: {order.Id}\n");
                        _rtbBuildDetails.AppendText($"Статус заказа: {order.Status}\n");
                        _rtbBuildDetails.AppendText($"Дата заказа: {order.CreatedAt:dd.MM.yyyy HH:mm}\n");
                        if (order.UpdatedAt.HasValue)
                        {
                            _rtbBuildDetails.AppendText($"Последнее обновление: {order.UpdatedAt.Value:dd.MM.yyyy HH:mm}\n");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки деталей: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите сборку для просмотра деталей", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_dgvBuilds.CurrentRow != null && _dgvBuilds.CurrentRow.Index >= 0)
            {
                var buildid = (int)_dgvBuilds.CurrentRow.Cells["Id"].Value;
                var buildName = _dgvBuilds.CurrentRow.Cells["Name"].Value.ToString();

                var result = MessageBox.Show($"Удалить сборку '{buildName}'?\n\n" +
                    "Примечание: удалятся только данные сборки, заказы останутся.",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        // Удаляем связи BuildComponent
                        var buildComponents = _context.BuildComponents.Where(bc => bc.BuildId == buildid);
                        _context.BuildComponents.RemoveRange(buildComponents);

                        // Удаляем сборку
                        var build = _context.Builds.Find(buildid);
                        _context.Builds.Remove(build);

                        await _context.SaveChangesAsync();

                        MessageBox.Show("Сборка успешно удалена", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadBuilds();
                        _rtbBuildDetails.Clear();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}