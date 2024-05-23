using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Calculate_Click(object sender, RoutedEventArgs e)
        {
            string str1 = TextBox1.Text.Trim();
            string str2 = TextBox2.Text.Trim();

            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
            {
                ErrorText.Text = "Both strings must be non-empty.";
                DistanceGrid.ItemsSource = null;
                return;
            }

            ErrorText.Text = string.Empty;

            var distance = await Task.Run(() => CalculateLevenshteinDistance(str1, str2));

            DataTable table = new DataTable();

            // Create columns with unique names
            table.Columns.Add("");
            for (int i = 0; i <= str2.Length; i++)
            {
                string columnName = i == 0 ? "" : str2[i - 1].ToString() + "_" + i;
                table.Columns.Add(columnName);
            }

            // Create rows
            for (int i = 0; i <= str1.Length; i++)
            {
                DataRow row = table.NewRow();
                row[0] = i == 0 ? "" : str1[i - 1].ToString() + "_" + i;
                for (int j = 0; j <= str2.Length; j++)
                {
                    row[j + 1] = distance[i, j];
                }
                table.Rows.Add(row);
            }

            DistanceGrid.ItemsSource = table.DefaultView;
        }

        private int[,] CalculateLevenshteinDistance(string str1, string str2)
        {
            int[,] d = new int[str1.Length + 1, str2.Length + 1];

            for (int i = 0; i <= str1.Length; i++)
            {
                d[i, 0] = i;
            }

            for (int j = 0; j <= str2.Length; j++)
            {
                d[0, j] = j;
            }

            for (int i = 1; i <= str1.Length; i++)
            {
                for (int j = 1; j <= str2.Length; j++)
                {
                    int cost = (str1[i - 1] == str2[j - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }

            return d;
        }

        private void ExportToCSV_Click(object sender, RoutedEventArgs e)
        {
            if (DistanceGrid.ItemsSource == null)
            {
                ErrorText.Text = "No data to export.";
                return;
            }

            ErrorText.Text = string.Empty;

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV file (*.csv)|*.csv",
                FileName = "LevenshteinDistance.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                DataTable dataTable = ((DataView)DistanceGrid.ItemsSource).ToTable();
                ExportDataTableToCSV(dataTable, saveFileDialog.FileName);
            }
        }

        private void ExportDataTableToCSV(DataTable dataTable, string filePath)
        {
            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = dataTable.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in dataTable.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }

            File.WriteAllText(filePath, sb.ToString());
        }
    }
}
   
