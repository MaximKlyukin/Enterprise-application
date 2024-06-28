using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

namespace Диплом
{
    public partial class LoginForm : Form
    {
        private AccessDatabase db;

        public LoginForm()
        {
            InitializeComponent();
            // Получение строки подключения из конфигурационного файла
            string connectionString = ConfigurationManager.ConnectionStrings["AccessDbConnectionString"].ConnectionString;
            // Создание экземпляра класса AccessDatabase с использованием строки подключения из конфигурационного файла
            db = new AccessDatabase(connectionString);
            this.AcceptButton = loginButton;
            // Центрирование формы
            this.StartPosition = FormStartPosition.CenterScreen; 
        }

        private void showPasswordCheckBox_Click(object sender, EventArgs e)
        {
            passwordTextBox.UseSystemPasswordChar = !showPasswordCheckBox.Checked;
        }        
        
        private void passwordTextBox_TextChanged(object sender, EventArgs e)
        {
            // Получаем текущее состояние чекбокса
            bool isChecked = showPasswordCheckBox.Checked;

            // Если чекбокс отмечен (показ пароля), используем открытый текст
            if (isChecked)
            {
                passwordTextBox.UseSystemPasswordChar = false; // Отображаем открытый текст
            }
            else // В противном случае (чекбокс не отмечен), скрываем текст пароля
            {
                passwordTextBox.UseSystemPasswordChar = true; // Скрываем текст пароля
            }
        }

        private void LoginForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Проверяем, была ли нажата клавиша Enter
            if (e.KeyChar == (char)Keys.Enter)
            {
                // Вызываем обработчик события для кнопки loginButton
                loginButton.PerformClick();
            }
        }

        private void loginButton_Click(object sender, EventArgs e)
        {
            string username = usernameTextBox.Text;
            string password = passwordTextBox.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, введите имя пользователя и пароль.");
                return;
            }

            // Хеширование пароля
            string hashedPassword = HashPassword(password);

            // Проверяем аутентификацию пользователя через базу данных
            if (db.VerifyUserCredentials(username, hashedPassword))
            {
                MessageBox.Show("Вход выполнен успешно!");

                if (username == "admin")
                {
                    OpenAdminForm(); // Перенаправляем администратора на форму AdminForm
                }
                else
                {
                    OpenMainForm(username); // Открываем главную форму для остальных пользователей
                }
            }
            else
            {
                MessageBox.Show("Неправильное имя пользователя или пароль.");
            }
        }
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < hashedBytes.Length; i++)
                {
                    builder.Append(hashedBytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        private void OpenAdminForm()
        {
            AdminForm adminForm = new AdminForm();
            adminForm.Show();
            this.Hide();
        }

        private void OpenMainForm(string username)
        {
            MainForm mainForm = new MainForm(username);
            mainForm.Show();
            this.Hide();
        }
    }
}
