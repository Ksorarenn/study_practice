using PC_Builder.Core.Models;
using PC_Builder.WinForms.Services;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PC_Builder.WinForms.Forms
{
    public partial class RegisterForm : Form
    {
        private readonly AuthService _authService;

        public RegisterForm(AuthService authService)
        {
            _authService = authService;
            InitializeComponent();
            SetupForm();
        }

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
            this.ClientSize = new System.Drawing.Size(400, 450);
            this.Text = "RegisterForm";
        }

        private void SetupForm()
        {
            this.Text = "PC Builder - Регистрация";
            this.Size = new Size(400, 450);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var lblTitle = new Label
            {
                Text = "Регистрация",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Location = new Point(130, 20),
                Size = new Size(200, 30)
            };

            // Full Name
            var lblFullName = new Label { Text = "ФИО:", Location = new Point(50, 70), Size = new Size(100, 25) };
            var txtFullName = new TextBox { Location = new Point(150, 70), Size = new Size(200, 25) };

            // Username
            var lblUsername = new Label { Text = "Логин:", Location = new Point(50, 110), Size = new Size(100, 25) };
            var txtUsername = new TextBox { Location = new Point(150, 110), Size = new Size(200, 25) };

            // Email
            var lblEmail = new Label { Text = "Email:", Location = new Point(50, 150), Size = new Size(100, 25) };
            var txtEmail = new TextBox { Location = new Point(150, 150), Size = new Size(200, 25) };

            // Password
            var lblPassword = new Label { Text = "Пароль:", Location = new Point(50, 190), Size = new Size(100, 25) };
            var txtPassword = new TextBox { Location = new Point(150, 190), Size = new Size(200, 25), PasswordChar = '*' };

            // Confirm Password
            var lblConfirm = new Label { Text = "Повтор:", Location = new Point(50, 230), Size = new Size(100, 25) };
            var txtConfirm = new TextBox { Location = new Point(150, 230), Size = new Size(200, 25), PasswordChar = '*' };

            // Buttons
            var btnRegister = new Button
            {
                Text = "Зарегистрироваться",
                Location = new Point(50, 280),
                Size = new Size(300, 35),
                BackColor = Color.LightGreen
            };
            btnRegister.Click += (s, e) => Register(txtFullName.Text, txtUsername.Text, txtEmail.Text, txtPassword.Text, txtConfirm.Text);

            var btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(150, 330),
                Size = new Size(100, 30)
            };
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] {
                lblTitle,
                lblFullName, txtFullName,
                lblUsername, txtUsername,
                lblEmail, txtEmail,
                lblPassword, txtPassword,
                lblConfirm, txtConfirm,
                btnRegister, btnCancel
            });
        }

        private void Register(string fullName, string username, string email, string password, string confirm)
        {
            if (password != confirm)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_authService.Register(fullName, username, email, password, out string error))
            {
                MessageBox.Show("Регистрация успешна! Теперь вы можете войти.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            else
            {
                MessageBox.Show(error, "Ошибка регистрации", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
