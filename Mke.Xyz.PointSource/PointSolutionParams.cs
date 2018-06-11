namespace Mke.Xyz.PointSource
{
    using System.Runtime.Serialization;

    [DataContract]
    public class PointSolutionParams
    {
        public int N => x.Length * y.Length * z.Length;

        [DataMember]
        public int FunctionNumber { get; set; }

        [DataMember]
        public ESolutionMethod SolutionMethod { get; set; }

        [DataMember]
        public double Lambda { get; set; }

        [DataMember]
        public double Gamma { get; set; }

        [DataMember]
        public double Beta { get; set; }

        [DataMember]
        public double Ro { get; set; }

        public int Middle { get; set; }

        public double[] x { get; set; }

        public double[] y { get; set; }

        public double[] z { get; set; }

        [DataMember]
        public bool TopFirst { get; set; }

        [DataMember]
        public bool TopSecond { get; set; }

        [DataMember]
        public bool TopThird { get; set; }

        [DataMember]
        public bool BottomFirst { get; set; }

        [DataMember]
        public bool BottomSecond { get; set; }

        [DataMember]
        public bool BottomThird { get; set; }

        [DataMember]
        public bool LeftFirst { get; set; }

        [DataMember]
        public bool LeftSecond { get; set; }

        [DataMember]
        public bool LeftThird { get; set; }

        [DataMember]
        public bool RightFirst { get; set; }

        [DataMember]
        public bool RightSecond { get; set; }

        [DataMember]
        public bool RightThird { get; set; }

        [DataMember]
        public bool FrontFirst { get; set; }

        [DataMember]
        public bool FrontSecond { get; set; }

        [DataMember]
        public bool FrontThird { get; set; }

        [DataMember]
        public bool BackFirst { get; set; }

        [DataMember]
        public bool BackSecond { get; set; }

        [DataMember]
        public bool BackThird { get; set; }
    }
}