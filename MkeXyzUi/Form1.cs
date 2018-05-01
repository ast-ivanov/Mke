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
            const string filename = "result.txt";
            try
            {
                var (q, u) = _solution.Calculate();
                var fileStream = new FileStream(filename, FileMode.Create);
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.WriteLine("q");
                    foreach (var qVar in q)
                    {
                        streamWriter.WriteLine(qVar);
                    }
                    streamWriter.WriteLine("u");
                    foreach (var uVar in u)
                    {
                        streamWriter.WriteLine(uVar);
                    }
                }
                Process.Start(filename);
            }
            catch (Exception exception)
            {
                MessageBox.Show(this, exception.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
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
    }
}
