namespace MkeXyzUi
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Diagnostics;
    using System.Windows.Forms;
    using System.Runtime.Serialization.Json;

    using Mke.Interfaces;
    using System.Windows.Forms.DataVisualization.Charting;

    public partial class Form1 : Form
    {
        private readonly ISolution<SolutionParams> _solution;

        private readonly SolutionParams _solutionParams;

        private readonly int _middle;

        public Form1()
        {
            InitializeComponent();

            _solutionParams = ReadParamsFromJson();
            var (x, middle) = BuildGrid();
            _solutionParams.x = x;
            _middle = middle;

            _solution = new Solution { SolutionParams = _solutionParams };
        }

        private void calculateBtn_Click(object sender, EventArgs e)
        {
            try
            {
                dataTable.Rows.Clear();
                var (q, u) = _solution.Calculate();

                var x = _solutionParams.x;

                var xLen = _solutionParams.x.Length;
                var yLen = _solutionParams.y.Length;
                var zLen = _solutionParams.z.Length;

                var startNode = (zLen - 1) * xLen * yLen + yLen / 2 * xLen + _middle;
                var endNode = startNode + _middle;

                using (var sw = new StreamWriter("C:\\Users\\Arthur\\Desktop\\test.txt", false, System.Text.Encoding.Default))
                {
                    sw.WriteLine();
                    sw.WriteLine();

                    for (int i = startNode, j = _middle; i < endNode; i++, j++)
                    {
                        sw.WriteLine($"{x[j]} {q[i]}");
                    }
                }

                MessageBox.Show(this, @"Запись в файл произведена", @"Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

//                var series = new Series
//                {
//                    ChartType = SeriesChartType.Line,
//                    BorderWidth = 10
//                };
//                for (int i = 0; i < q.Length; ++i)
//                {
//                    dataTable.Rows.Add($"{q[i]}", $"{u[i]}", $"{q[i] - u[i]}");
//                }
//
//                for (int i = 0; i < 500; i++)
//                {
//                    series.Points.AddXY(i, q[i] - u[i]);
//                }
//
//                chart1.Series.Clear();
//                chart1.Series.Add(series);
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, @"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void readParamsButton_Click(object sender, EventArgs e)
        {
            _solution.SolutionParams = ReadParamsFromJson();
        }

        private SolutionParams ReadParamsFromJson()
        {
            var jsonFormatter = new DataContractJsonSerializer(typeof(SolutionParams));

            SolutionParams solutionParams;

            using (var fs = new FileStream("SolutionParams.json", FileMode.OpenOrCreate))
            {
                solutionParams = (SolutionParams)jsonFormatter.ReadObject(fs);
            }

            var size = 21;
            var half = size / 2;

            var y = new double[size];
            var z = new double[2];

            for (int i = 0; i < size; i++)
            {
                y[i] = i - half;
            }

            z[0] = 0;
            z[1] = 1;

            solutionParams.y = y;
            solutionParams.z = z;

            return solutionParams;
        }

        private (double[], int) BuildGrid()
        {
            const double kr = 1.1;

            var xList = new List<double>();
            var coord = -500.0;
            var h = 70.0;

            do
            {
                xList.Add(coord);
                h /= kr;
                coord += h;
            } while (coord < 0);

            var middle = xList.Count;

            xList.Add(coord - h / 2);

            do
            {
                xList.Add(coord);
                h *= kr;
                coord += h;
            } while (coord < 500);

            return (xList.ToArray(), middle);
        }

        private void openParamsFileMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("SolutionParams.json");
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
