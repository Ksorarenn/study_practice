using PC_Builder.Core.Models;
using PC_Builder.WinForms.Services;
using PC_Builder.Data;
using System.Security.Cryptography;
using System.Text;


namespace PC_Builder.WinForms.Forms
{
    public partial class LoginForm : Form
    {
        // Удаляем прямое поле _context, добавляем сервис
        private readonly AuthService _authService;
        
        // Временно создаем контекст здесь, чтобы передать в сервис (в идеале нужно внедрение зависимостей)
        private ApplicationDbContext _context;

        public User CurrentUser { get; private set; }

        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            // Освобождаем контекст при закрытии формы
            _context?.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Text = "LoginForm";
        }

        public LoginForm()
        {
            // Инициализируем зависимости вручную (так как нет DI контейнера)
            _context = new ApplicationDbContext();
            _authService = new AuthService(_context);

            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            this.Text = "PC Builder - Вход";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Элементы формы
            var lblTitle = new Label
            {
                Text = "Вход в систему",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Location = new Point(150, 20),
                Size = new Size(200, 30)
            };

            var lblUsername = new Label
            {
                Text = "Логин:",
                Location = new Point(50, 70),
                Size = new Size(100, 25)
            };

            var txtUsername = new TextBox
            {
                Location = new Point(150, 70),
                Size = new Size(200, 25),
                Text = "admin" // Для теста
            };

            var lblPassword = new Label
            {
                Text = "Пароль:",
                Location = new Point(50, 110),
                Size = new Size(100, 25)
            };

            var txtPassword = new TextBox
            {
                Location = new Point(150, 110),
                Size = new Size(200, 25),
                PasswordChar = '*',
                Text = "admin123" // Для теста
            };

            var btnLogin = new Button
            {
                Text = "Войти",
                Location = new Point(150, 160),
                Size = new Size(100, 30),
                BackColor = Color.LightBlue
            };
            btnLogin.Click += (s, e) => Login(txtUsername.Text, txtPassword.Text);

            var btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(260, 160),
                Size = new Size(100, 30)
            };
            btnCancel.Click += (s, e) => Application.Exit();

            var btnRegister = new Button
            {
                Text = "Регистрация",
                Location = new Point(150, 210), // Ниже кнопки входа
                Size = new Size(210, 30),
                //BackColor = Color.LightYellow
            };
            btnRegister.Click += (s, e) => OpenRegisterForm();

            // Добавляем элементы на форму
            this.Controls.AddRange(new Control[]
            {
                lblTitle, lblUsername, txtUsername,
                lblPassword, txtPassword, btnLogin, btnCancel, 
                btnRegister // Добавляем новую кнопку
            });

            // Enter для входа
            this.AcceptButton = btnLogin;
        }

        private void OpenRegisterForm()
        {
            var regForm = new RegisterForm(_authService);
            regForm.ShowDialog();
        }

        private void Login(string username, string password)
        {
            try
            {
                var user = _authService.Authenticate(username, password);

                if (user == null)
                {
                    MessageBox.Show("Неверный логин или пароль", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                CurrentUser = user;

                // Открываем главную форму
                var mainForm = new MainForm(CurrentUser);
                mainForm.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка входа: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}