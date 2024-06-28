using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Диплом
{
    public partial class AdminReportForm : Form
    {
        private string connectionString;

        public AdminReportForm()
        {
            InitializeComponent();

            // Получение строки подключения из конфигурационного файла
            connectionString = ConfigurationManager.ConnectionStrings["AccessDbConnectionString"].ConnectionString;

            // Центрирование формы
            this.StartPosition = FormStartPosition.CenterScreen;
        }
        private void generateButton_Click(object sender, EventArgs e)
        {
            // Получаем начальную и конечную даты из DateTimePicker
            DateTime startDate = startDatePicker.Value;
            DateTime endDate = endDatePicker.Value;

            // Проверяем, чтобы начальная дата была меньше или равна конечной
            if (startDate > endDate)
            {
                MessageBox.Show("Начальная дата должна быть меньше или равна конечной дате.");
                return;
            }

            // Вызываем метод GenerateReport, передавая начальную и конечную даты
            GenerateReport(startDate, endDate);
        }

        public void GenerateReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();

                    // Запрос для общего количества сообщений
                    string totalCountQuery = "SELECT COUNT(*) AS TotalCount FROM Messages_db WHERE ReceiverUsername = 'Admin' AND SentDateTime BETWEEN @StartDate AND @EndDate";
                    using (OleDbCommand totalCountCommand = new OleDbCommand(totalCountQuery, connection))
                    {
                        totalCountCommand.Parameters.AddWithValue("@StartDate", startDate);
                        totalCountCommand.Parameters.AddWithValue("@EndDate", endDate);
                        int totalCount = (int)totalCountCommand.ExecuteScalar(); // Получаем общее количество сообщений

                        // Запрос для получения данных о количестве сообщений каждого отправителя с разбивкой по степени важности
                        string query = @"
                    SELECT m.SenderUsername, 
                           COUNT(*) AS TotalMessages,
                           (SELECT COUNT(*) FROM Messages_db WHERE ReceiverUsername = 'Admin' AND SentDateTime BETWEEN @StartDate AND @EndDate AND Importance = 'Обычное' AND SenderUsername = m.SenderUsername) AS NormalMessages,
                           (SELECT COUNT(*) FROM Messages_db WHERE ReceiverUsername = 'Admin' AND SentDateTime BETWEEN @StartDate AND @EndDate AND Importance = 'Важное' AND SenderUsername = m.SenderUsername) AS ImportantMessages,
                           (SELECT COUNT(*) FROM Messages_db WHERE ReceiverUsername = 'Admin' AND SentDateTime BETWEEN @StartDate AND @EndDate AND Importance = 'Срочное' AND SenderUsername = m.SenderUsername) AS UrgentMessages
                    FROM Messages_db AS m
                    WHERE ReceiverUsername = 'Admin' AND SentDateTime BETWEEN @StartDate AND @EndDate 
                    GROUP BY SenderUsername";

                        using (OleDbCommand command = new OleDbCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@StartDate", startDate);
                            command.Parameters.AddWithValue("@EndDate", endDate);

                            using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
                            {
                                DataTable dataTable = new DataTable();
                                adapter.Fill(dataTable);

                                // Добавляем строку с общим количеством сообщений в DataTable
                                DataRow totalRow = dataTable.NewRow();
                                totalRow["SenderUsername"] = "Всего:";
                                totalRow["TotalMessages"] = totalCount;
                                dataTable.Rows.Add(totalRow);

                                // Привязываем результаты к источнику данных dataGridView
                                dataGridView1.DataSource = dataTable;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при формировании отчета: {ex.Message}");
            }
        }

        private void printButton_Click(object sender, EventArgs e)
        {
            PrintDocument pd = new PrintDocument();
            pd.PrintPage += new PrintPageEventHandler(PrintPage);
            PrintDialog pdi = new PrintDialog();
            pdi.Document = pd;
            if (pdi.ShowDialog() == DialogResult.OK)
            {
                pd.Print();
            }
        }

        private void PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            Font font = new Font("Arial", 12);
            Brush brush = new SolidBrush(Color.Black);
            Pen pen = new Pen(Color.Black);
            int x = 10;
            int y = 10;

            // Рисуем заголовок отчета
            g.DrawString("Отчет о сообщениях", font, brush, x, y);
            y += 20;

            // Рисуем таблицу с данными отчета
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    g.DrawString(cell.Value.ToString(), font, brush, x, y);
                    x += 150; 
                }
                y += 20;
                x = 10;
            }
        }
    }
}
