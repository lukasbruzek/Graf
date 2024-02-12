using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Graf
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            cartesianChart1.Series.Clear();
            LiveCharts.SeriesCollection series = new LiveCharts.SeriesCollection();
            var years = (from o in revenueBindingSource.DataSource as List<Revenue> select new {Year = o.Year}).Distinct();

            foreach (var year in years)
            {
                List<Double> values = new List<Double>();   
                for(int month = 1; month <= 12; month++)
                {
                    double value = 0;
                    var data = from o in revenueBindingSource.DataSource as List<Revenue>
                               where o.Year.Equals(year.Year) && o.Month.Equals(month)
                               orderby o.Month ascending
                               select new { o.Value, o.Month };
                    if(data.SingleOrDefault() != null)
                        value = data.SingleOrDefault().Value;
                    values.Add(value);  
                }
                series.Add(new LineSeries() { Title = year.Year.ToString(), Values = new ChartValues<double>(values) });
            }
            cartesianChart1.Series = series;
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
    }
}
