using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Диплом
{
    public partial class MainForm : Form
    {
        private string connectionString;
        private string currentUser;

        public MainForm(string username)
        {
            InitializeComponent();
            currentUser = username;
            // Получение строки подключения из конфигурационного файла
            connectionString = ConfigurationManager.ConnectionStrings["AccessDbConnectionString"].ConnectionString;
            // Центрирование формы
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Добавляем варианты степеней важности
            importanceComboBox.Items.AddRange(new object[] { "Обычное", "Важное", "Срочное" });
            // По умолчанию устанавливаем "Обычное"
            importanceComboBox.SelectedIndex = 0;
        }


        private void logoutButton_Click(object sender, EventArgs e)
        {
            string message = messageTextBox.Text;
            string importance = importanceComboBox.SelectedItem.ToString();
            string roomNumber = roomNumberTextBox.Text;

            // Проверка на пустые поля
            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(importance) || string.IsNullOrEmpty(roomNumber))
            {
                MessageBox.Show("Пожалуйста, заполните все поля перед отправкой сообщения.");
                return;
            }

            SendMessageToAdmin(roomNumber,message, importance); // Вызываем метод SendMessageToAdmin

            // Очистка полей ввода после отправки сообщения
            messageTextBox.Clear();
            roomNumberTextBox.Clear();
        }

        public void SaveMessageToDatabase(string roomNumber, string message, string importance )
        {
            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO Messages_db ([SenderUsername], [ReceiverUsername], [RoomNumber], [MessageText], [Importance], [SentDateTime]) VALUES (?, ?, ?, ?, ?, ?)";
                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("?", currentUser);
                        command.Parameters.AddWithValue("?", "Admin");
                        command.Parameters.AddWithValue("?", roomNumber);
                        command.Parameters.AddWithValue("?", message);
                        command.Parameters.AddWithValue("?", importance);
                        command.Parameters.AddWithValue("?", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Сообщение сохранено в базе данных!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении сообщения в базе данных: " + ex.Message);
            }
        }

        public void SendMessageToAdmin(string roomNumber, string message, string importance)
        {
            try
            {
                // Сохранение сообщения в базе данных
                SaveMessageToDatabase(roomNumber, message, importance);

                // Обновление DataGridView на форме AdminForm
                AdminForm adminForm = Application.OpenForms.OfType<AdminForm>().FirstOrDefault();
                adminForm?.LoadMessagesForAdmin(); // Перезагрузка сообщений на форме администратора
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении сообщения в базе данных: " + ex.Message);
            }
        }

        private void sentMessagesButton_Click(object sender, EventArgs e)
        {
            SentMessagesForm sentMessagesForm = new SentMessagesForm(currentUser);
            sentMessagesForm.Show();
        }
    }
}
