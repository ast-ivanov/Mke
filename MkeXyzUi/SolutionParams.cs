namespace MkeXyzUi
{
    public class SolutionParams
    {
        public int N => x.Length * y.Length * z.Length;

        public double Lambda { get; set; } = 1;

        public double Gamma { get; set; } = 1;

        public double Beta { get; set; } = 1;

        public double[] x { get; set; } = { 1, 2, 3, 4, 5 };

        public double[] y { get; set; } = { 1, 2, 3, 4, 5 };

        public double[] z { get; set; } = { 1, 2, 3, 4, 5 };

        public bool TopFirst { get; set; }

        public bool TopSecond { get; set; }

        public bool TopThird { get; set; }

        public bool BottomFirst { get; set; }

        public bool BottomSecond { get; set; }

        public bool BottomThird { get; set; }

        public bool LeftFirst { get; set; }

        public bool LeftSecond { get; set; }

        public bool LeftThird { get; set; }

        public bool RightFirst { get; set; }

        public bool RightSecond { get; set; }

        public bool RightThird { get; set; }
    }
}
