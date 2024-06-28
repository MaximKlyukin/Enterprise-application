using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

namespace Диплом
{
    public partial class AdminForm : Form
    {
        private string connectionString;
        public AdminForm()
        {
            InitializeComponent();
            // Получение строки подключения из конфигурационного файла
            connectionString = ConfigurationManager.ConnectionStrings["AccessDbConnectionString"].ConnectionString;
            LoadMessagesForAdmin();
            // Центрирование формы
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void AdminForm_Load(object sender, EventArgs e)
        {
            dataGridView.CellContentClick += dataGridView_CellContentClick;
            dataGridView.RowPrePaint += dataGridView_RowPrePaint;

        }

        public void LoadMessagesForAdmin()
        {
            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT MessageID, SenderUsername, RoomNumber, MessageText, SentDateTime, Importance, IsRead FROM Messages_db WHERE ReceiverUsername = 'Admin'";
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection))
                    {
                        DataSet dataSet = new DataSet();
                        adapter.Fill(dataSet);

                        dataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

                        dataGridView.DataSource = dataSet.Tables[0];

                        // Установка переноса текста
                        dataGridView.Columns["MessageText"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                        dataGridView.Columns["SenderUsername"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;

                        // Установка ширины столбцов
                        dataGridView.Columns["MessageID"].Width = 70;
                        dataGridView.Columns["SenderUsername"].Width = 100;
                        dataGridView.Columns["RoomNumber"].Width = 80;
                        dataGridView.Columns["MessageText"].Width = 200;
                        dataGridView.Columns["SentDateTime"].Width = 110;
                        dataGridView.Columns["Importance"].Width = 80;
                        dataGridView.Columns["IsRead"].Width = 50;

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке сообщений: {ex.Message}");
            }
        }

        private void dataGridView_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            var row = dataGridView.Rows[e.RowIndex];
            bool isRead = Convert.ToBoolean(row.Cells["IsRead"].Value);
            DateTime sentDateTime = Convert.ToDateTime(row.Cells["SentDateTime"].Value);
            if (!isRead && (DateTime.Now - sentDateTime).TotalDays > 3)
            {
                row.DefaultCellStyle.BackColor = Color.LightCoral;
            }
        }

        private void dataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Проверяем, был ли кликнут столбец с чекбоксом прочтения
            if (e.ColumnIndex == dataGridView.Columns["IsRead"].Index && e.RowIndex >= 0)
            {
                // Получаем значение MessageID из выделенной строки
                int messageId = Convert.ToInt32(dataGridView.Rows[e.RowIndex].Cells["MessageID"].Value);

                // Получаем текущее значение чекбокса IsRead
                bool isRead = !Convert.ToBoolean(dataGridView.Rows[e.RowIndex].Cells["IsRead"].Value);

                // Обновляем статус прочтения в базе данных
                UpdateReadStatus(messageId, isRead);
            }
        }

        private void UpdateReadStatus(int messageId, bool isRead)
        {
            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();
                    string updateQuery = "UPDATE Messages_db SET IsRead = ? WHERE MessageID = ?";
                    using (OleDbCommand command = new OleDbCommand(updateQuery, connection))
                    {
                        // Правильно устанавливаем параметры команды
                        command.Parameters.AddWithValue("?", isRead);
                        command.Parameters.AddWithValue("?", messageId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении статуса прочтения: {ex.Message}");
            }
        }

        private void dataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView.Columns[e.ColumnIndex].Name == "Importance" && e.Value != null)
            {
                string importance = e.Value.ToString();

                // Определяем цвет текста ячейки в зависимости от важности сообщения
                switch (importance)
                {
                    case "Обычное":
                        e.CellStyle.ForeColor = Color.Green;
                        break;
                    case "Важное":
                        e.CellStyle.ForeColor = Color.Blue;
                        break;
                    case "Срочное":
                        e.CellStyle.ForeColor = Color.Red;
                        break;
                    default:
                        e.CellStyle.ForeColor = Color.Green;
                        break;
                }
            }
        }

       

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                DialogResult result = MessageBox.Show("Вы уверены, что хотите удалить выбранные сообщения?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    foreach (DataGridViewRow row in dataGridView.SelectedRows)
                    {
                        int messageId = Convert.ToInt32(row.Cells["MessageID"].Value);

                        try
                        {
                            using (OleDbConnection connection = new OleDbConnection(connectionString))
                            {
                                connection.Open();
                                string deleteQuery = "DELETE FROM Messages_db WHERE MessageID = ?";
                                using (OleDbCommand command = new OleDbCommand(deleteQuery, connection))
                                {
                                    command.Parameters.AddWithValue("?", messageId);
                                    command.ExecuteNonQuery();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при удалении сообщения: {ex.Message}");
                        }
                    }

                    LoadMessagesForAdmin(); // Обновляем отображение после удаления
                }
            }
            else
            {
                MessageBox.Show("Выберите сообщения для удаления.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void addUserButton_Click(object sender, EventArgs e)
        {
            // Форма добавления нового пользователя
            AddUserForm addUserForm = new AddUserForm();
            addUserForm.ShowDialog();
        } 
        
        private void reportButton_Click(object sender, EventArgs e)
        {
            AdminReportForm adminReportForm = new AdminReportForm();
            adminReportForm.ShowDialog();
        }

        private void buttonDeleteUser_Click(object sender, EventArgs e)
        {
            DeleteUserForm deleteUserForm = new DeleteUserForm();
            deleteUserForm.ShowDialog();
        }
    }
}
