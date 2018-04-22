using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MkeXyzUi
{
    public partial class Form1 : Form
    {
        private readonly ISolution _solution;
        public Form1()
        {
            InitializeComponent();

            _solution = new Solution { SolutionParams = new SolutionParams() };
        }

        private void calculateBtn_Click(object sender, EventArgs e)
        {
            var (q, u) = _solution.Calculate();
        }
    }
}
