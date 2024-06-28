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
    public partial class DeleteUserForm : Form
    {
        private string connectionString;
        private OleDbConnection connection;
        public DeleteUserForm()
        {
            InitializeComponent();
            // Получение строки подключения из конфигурационного файла
            connectionString = ConfigurationManager.ConnectionStrings["AccessDbConnectionString"].ConnectionString;
            connection = new OleDbConnection(connectionString);
            LoadUsers();
            // Центрирование формы
            this.StartPosition = FormStartPosition.CenterScreen;
        }
        private void LoadUsers()
        {
            try
            {
                connection.Open();
                string query = "SELECT Username FROM Users_db";
                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            comboBoxUsers.Items.Add(reader["Username"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (comboBoxUsers.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите пользователя для удаления.");
                return;
            }

            string selectedUser = comboBoxUsers.SelectedItem.ToString();
            try
            {
                connection.Open();
                string query = "DELETE FROM Users_db WHERE Username = @Username";
                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", selectedUser);
                    command.ExecuteNonQuery();
                }
                MessageBox.Show("Пользователь успешно удален.");
                comboBoxUsers.Items.Remove(selectedUser);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении пользователя: {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }
    }
}
