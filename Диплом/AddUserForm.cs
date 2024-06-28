using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Диплом
{
    public partial class AddUserForm : Form
    {
        private AccessDatabase db;

        public AddUserForm()
        {
            InitializeComponent();
            // Получение строки подключения из конфигурационного файла
            string connectionString = ConfigurationManager.ConnectionStrings["AccessDbConnectionString"].ConnectionString;
            // Экземпляр класса AccessDatabase с использованием строки подключения из конфигурационного файла
            db = new AccessDatabase(connectionString);
            // Центрирование формы
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            string username = usernameTextBox.Text;
            string password = passwordTextBox.Text;

            // Проверяем, не пустые ли поля
            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                // Хэшируем пароль перед добавлением в базу данных
                string hashedPassword = HashPassword(password);

                // Вызываем метод для добавления пользователя в базу данных с хэшированным паролем
                db.InsertUser(username, hashedPassword);

                // Оповещаем пользователя об успешном добавлении
                MessageBox.Show("Новый пользователь успешно добавлен.");

                // Очищаем поля ввода
                usernameTextBox.Clear();
                passwordTextBox.Clear();
            }
            else
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
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
    }
}
