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
    public partial class SentMessagesForm : Form
    {
        private string connectionString;
        private string currentUser;

        public SentMessagesForm(string currentUser)
        {
            InitializeComponent();
            this.currentUser = currentUser;

            // Получение строки подключения из конфигурационного файла
            connectionString = ConfigurationManager.ConnectionStrings["AccessDbConnectionString"].ConnectionString;

            // Центрирование формы
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        public void DisplaySentMessages(string currentUser)
        {
            dataGridView.Columns.Clear();
            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT MessageID, SenderUsername, RoomNumber, MessageText, SentDateTime, IsRead FROM Messages_db WHERE SenderUsername = @Username OR ReceiverUsername = @Username";
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(query, connection))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@Username", currentUser);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        dataGridView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
                        dataGridView.DataSource = dataTable;

                        dataGridView.Columns["MessageText"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;

                        // Скрываем столбец
                        dataGridView.Columns["MessageID"].Visible = false;

                        // Установка ширины столбцов
                        dataGridView.Columns["SenderUsername"].Width = 100;
                        dataGridView.Columns["RoomNumber"].Width = 80;
                        dataGridView.Columns["MessageText"].Width = 200;
                        dataGridView.Columns["SentDateTime"].Width = 120;

                        foreach (DataGridViewRow row in dataGridView.Rows)
                        {
                            bool isRead = Convert.ToBoolean(row.Cells["IsRead"].Value);
                            DateTime sentDateTime = Convert.ToDateTime(row.Cells["SentDateTime"].Value);
                            if (isRead)
                            {
                                row.DefaultCellStyle.BackColor = Color.LightBlue;
                            }
                            else if (!isRead && (DateTime.Now - sentDateTime).TotalDays > 3)
                            {
                                row.DefaultCellStyle.BackColor = Color.LightCoral;
                            }
                        }
                        dataGridView.Columns["IsRead"].Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке сообщений: {ex.Message}");
            }
        }

        private void SentMessagesForm_Load(object sender, EventArgs e)
        {
            DisplaySentMessages(currentUser);
        }


        private void DeleteButton_Click(object sender, EventArgs e)
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

                    DisplaySentMessages(currentUser); // Обновляем отображение после удаления
                }
            }
            else
            {
                MessageBox.Show("Выберите сообщения для удаления.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
