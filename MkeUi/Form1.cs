namespace MkeUi
{
    using System;
    using System.IO;
    using System.Diagnostics;
    using System.Windows.Forms;
    using Mke.Interfaces;

    public partial class Form1 : Form
    {
        private readonly ISolution<SolutionParams> _solution;
        private readonly SolutionParams _solutionParams;

        public Form1()
        {
            InitializeComponent();

            _solution = new Solution { SolutionParams = new SolutionParams() };
            _solutionParams = _solution.SolutionParams;
        }

        private void EdgeChanged(object sender, EventArgs e)
        {
            if (sender is CheckBox edge)
            {
                var name = edge.Name.ToLower();

                if (name.Contains("top") && name.Contains("first"))
                {
                    _solutionParams.TopFirst = edge.Checked;
                }

                if (name.Contains("top") && name.Contains("second"))
                {
                    _solutionParams.TopSecond = edge.Checked;
                }

                if (name.Contains("top") && name.Contains("third"))
                {
                    _solutionParams.TopThird = edge.Checked;
                }

                if (name.Contains("bottom") && name.Contains("first"))
                {
                    _solutionParams.BottomFirst = edge.Checked;
                }

                if (name.Contains("bottom") && name.Contains("second"))
                {
                    _solutionParams.BottomSecond= edge.Checked;
                }

                if (name.Contains("bottom") && name.Contains("third"))
                {
                    _solutionParams.BottomThird = edge.Checked;
                }

                if (name.Contains("left") && name.Contains("first"))
                {
                    _solutionParams.LeftFirst = edge.Checked;
                }

                if (name.Contains("left") && name.Contains("second"))
                {
                    _solutionParams.LeftSecond = edge.Checked;
                }

                if (name.Contains("left") && name.Contains("third"))
                {
                    _solutionParams.LeftThird = edge.Checked;
                }

                if (name.Contains("right") && name.Contains("first"))
                {
                    _solutionParams.RightFirst = edge.Checked;
                }

                if (name.Contains("right") && name.Contains("second"))
                {
                    _solutionParams.RightSecond = edge.Checked;
                }

                if (name.Contains("right") && name.Contains("third"))
                {
                    _solutionParams.RightThird = edge.Checked;
                }
            }
        }

        private void calculateBtn_Click(object sender, EventArgs e)
        {
            var (q, u) = _solution.Calculate();

            var fileName = "LOS.txt";

            using (var file = File.OpenWrite(fileName))
            {
                using (var sw = new StreamWriter(file))
                {
                    for (var i = 0; i < q.Length; i++)
                    {
                        sw.WriteLine($"{q[i]:0.########}\t{u[i]:0.###}");
                    }
                }
            }

            if (File.Exists(fileName))
            {
                Process.Start(new ProcessStartInfo(fileName));
            }
        }
    }
}
