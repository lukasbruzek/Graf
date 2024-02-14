using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Graf
{
    public partial class Form1 : Form
    {
        private System.Drawing.Printing.PrintDocument printDocument = new System.Drawing.Printing.PrintDocument();
        public Form1()
        {
            InitializeComponent();
            printDocument.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(printDocument_PrintPage);
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
            SaveFileDialog saveFileDialog = new SaveFileDialog();
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
                List<double> values = new List<double>();
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
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV File|*.csv";
            openFileDialog.Title = "Open CSV File";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    List<Revenue> revenueData = new List<Revenue>();

                    using (var reader = new StreamReader(openFileDialog.FileName))
                    {
                        reader.ReadLine(); // Skip headers

                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            var values = line.Split(',');

                            int year = int.Parse(values[0]);
                            string monthName = values[1];
                            double value = double.Parse(values[2]);

                            int month = GetMonthNumber(monthName);

                            revenueData.Add(new Revenue { Year = year, Month = month, Value = value });
                        }
                    }

                    revenueBindingSource.DataSource = revenueData;
                    dataGridView1.DataSource = revenueBindingSource;

                    UpdateGraph();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading CSV file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private int GetMonthNumber(string monthName)
        {
            DateTimeFormatInfo dtInfo = new DateTimeFormatInfo();
            return dtInfo.MonthNames.ToList().IndexOf(monthName) + 1;
        }

        private void exportCSV_Click_1(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
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

        private void printDocument_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            using (Bitmap bmp = new Bitmap(cartesianChart1.Width, cartesianChart1.Height))
            {
                cartesianChart1.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                e.Graphics.DrawImage(bmp, 0, 0, 1000, bmp.Height);
            }
            
        }

        private void printButton_Click(object sender, EventArgs e)
        {
            printDocument.DefaultPageSettings.Landscape = true;
            PrintPreviewDialog printPreviewDialog = new PrintPreviewDialog();
            printPreviewDialog.Document = printDocument;
            printPreviewDialog.ShowDialog();
        }
    }
}
