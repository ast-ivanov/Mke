using System;
using System.Windows.Forms;

namespace MkeXyzUi
{
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Serialization.Json;
    using System.Xml.Serialization;

    public partial class Form1 : Form
    {
        private readonly ISolution _solution;
        public Form1()
        {
            InitializeComponent();

            _solution = new Solution { SolutionParams = ReadParamsFromJson() };
        }

        private void calculateBtn_Click(object sender, EventArgs e)
        {
            try
            {
                dataTable.Rows.Clear();
                var (q, u) = _solution.Calculate();
                for (int i = 0; i < q.Length; ++i)
                {
                    dataTable.Rows.Add($"{q[i]}", $"{u[i]}");
                }
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

            return solutionParams;
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
