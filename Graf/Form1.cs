using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Graf
{
    public partial class Form1 : Form
    {
        private DataTable dataTable;
        public Form1()
        {
            InitializeComponent();
        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            if (revenueBindingSource.DataSource == null)
            {
                return;
            }
            UpdateGraph();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            revenueBindingSource.DataSource = new List<Revenue>();
            cartesianChart1.AxisX.Add(new LiveCharts.Wpf.Axis
            {
                Title = "Month",
                Labels = new[] { "Leden", "Únor", "Březen", "Duben", "Květen", "Červen", "Červenec", "Srpen", "Září", "Říjen", "Listopad", "Prosinec" }
            });

            cartesianChart1.AxisY.Add(new LiveCharts.Wpf.Axis
            {
                Title = "Revenue",
                LabelFormatter = value => value.ToString(),
            });
            cartesianChart1.LegendLocation = LiveCharts.LegendLocation.Right;
        }

        private void exportButton_Click_1(object sender, EventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "JPEG Image|*.jpg";
            saveFileDialog.Title = "Save an Image File";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (Bitmap bmp = new Bitmap(cartesianChart1.Width, cartesianChart1.Height))
                {
                    cartesianChart1.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                    bmp.Save(saveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                }
            }
        }

        private string GetMonthName(int month)
        {
            DateTimeFormatInfo dtInfo = new DateTimeFormatInfo();
            return dtInfo.GetMonthName(month);
        }

        private void UpdateGraph()
        {
            cartesianChart1.Series.Clear();
            LiveCharts.SeriesCollection series = new LiveCharts.SeriesCollection();
            var years = (from o in revenueBindingSource.DataSource as List<Revenue> select new { Year = o.Year }).Distinct();

            foreach (var year in years)
            {
                List<Double> values = new List<Double>();
                for (int month = 1; month <= 12; month++)
                {
                    double value = 0;
                    var data = from o in revenueBindingSource.DataSource as List<Revenue>
                               where o.Year.Equals(year.Year) && o.Month.Equals(month)
                               orderby o.Month ascending
                               select new { o.Value, o.Month };
                    if (data.SingleOrDefault() != null)
                        value = data.SingleOrDefault().Value;
                    values.Add(value);
                }
                series.Add(new LineSeries() { Title = year.Year.ToString(), Values = new ChartValues<double>(values) });
            }
            cartesianChart1.Series = series;
        }

        private void importCSV_Click_1(object sender, EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "CSV File|*.csv";
            openFileDialog.Title = "Open CSV File";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    dataTable = new DataTable();

                    dataTable.Columns.Add("Year", typeof(string));
                    dataTable.Columns.Add("Month", typeof(string));
                    dataTable.Columns.Add("Value", typeof(double));

                    using (var reader = new StreamReader(openFileDialog.FileName))
                    {
                        reader.ReadLine();

                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            var values = line.Split(',');

                            string year = values[0];
                            string monthName = values[1];
                            double value = double.Parse(values[2]);

                            dataTable.Rows.Add(year, monthName, value);
                        }
                    }

                    dataGridView1.DataSource = dataTable;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading CSV file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void exportCSV_Click_1(object sender, EventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "CSV File|*.csv";
            saveFileDialog.Title = "Save CSV File";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (var writer = new StreamWriter(saveFileDialog.FileName))
                {
                    // Write headers
                    writer.WriteLine("Year,Month,Value");

                    foreach (var series in cartesianChart1.Series)
                    {
                        string year = series.Title;
                        for (int month = 0; month < series.Values.Count; month++)
                        {
                            string monthName = GetMonthName(month + 1);
                            double value = (double)series.Values[month];
                            writer.WriteLine($"{year},{monthName},{value}");
                        }
                    }
                }
            }
        }

        private void dataGridView1_CellValueChanged_1(object sender, DataGridViewCellEventArgs e)
        {
            if (revenueBindingSource.DataSource == null)
            {
                return;
            }
            UpdateGraph();
        }
    }
}
